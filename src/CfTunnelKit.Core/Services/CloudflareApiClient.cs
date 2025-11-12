using System.Net.Http.Json;
using System.Text.Json;
using CfTunnel.Core.Models;
using Polly;
using Polly.Retry;

namespace CfTunnel.Core.Services;

/// <summary>
/// Cliente para interação com a API do Cloudflare.
/// </summary>
public class CloudflareApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline;

    public CloudflareApiClient(string apiToken)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Política de retry com backoff exponencial e jitter
        _retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500 || (int)r.StatusCode == 429),
                OnRetry = args =>
                {
                    Console.WriteLine($"[WARN] Retry {args.AttemptNumber} em {args.RetryDelay.TotalMilliseconds:F0}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Verifica se o token da API é válido.
    /// </summary>
    public async Task<bool> VerifyTokenAsync()
    {
        try
        {
            var response = await GetJsonAsync<ApiResult<object>>("https://api.cloudflare.com/client/v4/user/tokens/verify");
            Console.WriteLine($"[DEBUG] Token verification response: Success={response.Success}");
            return response.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Token verification failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Cria um novo túnel Cloudflare.
    /// </summary>
    public async Task<Tunnel> CreateTunnelAsync(string accountId, string tunnelName, byte[] secretBytes)
    {
        var result = await PostJsonAsync<ApiResult<Tunnel>>(
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel",
            new { name = tunnelName, tunnel_secret = secretBytes });
        
        if (result.Result == null || string.IsNullOrWhiteSpace(result.Result.Id))
            throw new Exception("Falha ao criar túnel: ID não retornado.");
        
        return result.Result;
    }

    /// <summary>
    /// Busca túneis existentes pelo nome.
    /// </summary>
    public async Task<List<Tunnel>> GetTunnelsAsync(string accountId, string? nameFilter = null)
    {
        var url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel";
        if (!string.IsNullOrWhiteSpace(nameFilter))
            url += $"?name={Uri.EscapeDataString(nameFilter)}";

        var result = await GetJsonAsync<ApiResult<List<Tunnel>>>(url);
        return result.Result ?? new List<Tunnel>();
    }

    /// <summary>
    /// Obtém o token de autenticação de um túnel.
    /// </summary>
    public async Task<string> GetTunnelTokenAsync(string accountId, string tunnelId)
    {
        var result = await GetJsonAsync<ApiResult<string>>(
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/token");
        
        if (string.IsNullOrWhiteSpace(result.Result))
            throw new Exception("Token do túnel não retornado.");
        
        return result.Result;
    }

    /// <summary>
    /// Atualiza a configuração remota do túnel (ingress rules).
    /// </summary>
    public async Task UpdateTunnelConfigurationAsync(string accountId, string tunnelId, string hostname, string origin, string fallback)
    {
        var config = new
        {
            config = new
            {
                ingress = new object[]
                {
                    new { hostname, service = origin },
                    new { service = fallback }
                },
                warp_routing = new { enabled = false }
            }
        };

        await PutJsonAsync<ApiResult<object>>(
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations",
            config);
    }

    /// <summary>
    /// Lista registros DNS de uma zona.
    /// </summary>
    public async Task<List<DnsRecord>> GetDnsRecordsAsync(string zoneId, string? type = null, string? name = null)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records";
        var query = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(type))
            query.Add($"type={Uri.EscapeDataString(type)}");
        if (!string.IsNullOrWhiteSpace(name))
            query.Add($"name={Uri.EscapeDataString(name)}");
        
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        var result = await GetJsonAsync<ApiResult<List<DnsRecord>>>(url);
        return result.Result ?? new List<DnsRecord>();
    }

    /// <summary>
    /// Cria um novo registro DNS.
    /// </summary>
    public async Task<DnsRecord> CreateDnsRecordAsync(string zoneId, string type, string name, string content, bool proxied, int ttl)
    {
        var payload = new { type, name, content, proxied, ttl };
        var result = await PostJsonAsync<ApiResult<DnsRecord>>(
            $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records",
            payload);
        
        if (result.Result == null)
            throw new Exception("Falha ao criar registro DNS.");
        
        return result.Result;
    }

    /// <summary>
    /// Atualiza um registro DNS existente.
    /// </summary>
    public async Task<DnsRecord> UpdateDnsRecordAsync(string zoneId, string recordId, string type, string name, string content, bool proxied, int ttl)
    {
        var payload = new { type, name, content, proxied, ttl };
        var result = await PutJsonAsync<ApiResult<DnsRecord>>(
            $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}",
            payload);
        
        if (result.Result == null)
            throw new Exception("Falha ao atualizar registro DNS.");
        
        return result.Result;
    }

    private async Task<T> PostJsonAsync<T>(string url, object body)
    {
        var response = await _retryPipeline.ExecuteAsync(
            async ct => await _httpClient.PostAsJsonAsync(url, body, ct));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Status {(int)response.StatusCode}: {error}");
        }
        
        return (await response.Content.ReadFromJsonAsync<T>(_jsonOptions))!;
    }

    private async Task<T> PutJsonAsync<T>(string url, object body)
    {
        var response = await _retryPipeline.ExecuteAsync(
            async ct => await _httpClient.PutAsJsonAsync(url, body, ct));
        
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(_jsonOptions))!;
    }

    private async Task<T> GetJsonAsync<T>(string url)
    {
        var response = await _retryPipeline.ExecuteAsync(
            async ct => await _httpClient.GetAsync(url, ct));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Status {(int)response.StatusCode}: {error}");
        }
        
        return (await response.Content.ReadFromJsonAsync<T>(_jsonOptions))!;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
