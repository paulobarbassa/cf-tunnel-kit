using System.CommandLine;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text.Json;
using Polly;
using Polly.Retry;

var accountIdOpt = new Option<string>("--account-id", "Cloudflare Account ID") { IsRequired = true };
var zoneIdOpt    = new Option<string>("--zone-id",    "Cloudflare Zone ID")    { IsRequired = true };
var hostOpt      = new Option<string>("--hostname",   "Hostname público (ex.: app-host.example.com)") { IsRequired = true };
var originOpt    = new Option<string>("--origin",     ()=>"http://127.0.0.1:8080", "URL do serviço local a expor");
var proxiedOpt   = new Option<bool>("--proxied",      ()=> true, "CNAME proxied (laranja)");
var ttlOpt       = new Option<string>("--ttl",        ()=> "auto", "TTL (segundos ou 'auto')");
var prefixOpt    = new Option<string>("--tunnel-prefix", ()=> "tunnel-", "Prefixo do nome do túnel");
var downloadOpt  = new Option<string>("--cloudflared-url", ()=> "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe", "URL binário cloudflared (Windows)");
var fallbackOpt  = new Option<string>("--fallback",   ()=> "http_status:404", "Regra de fallback do ingress");
var skipSvcOpt   = new Option<bool>("--skip-service", ()=> false, "Pula a instalação do serviço (útil em Linux ou quando já existe)");

var root = new RootCommand("Provisionador autônomo de Cloudflare Tunnel");
root.AddOption(accountIdOpt);
root.AddOption(zoneIdOpt);
root.AddOption(hostOpt);
root.AddOption(originOpt);
root.AddOption(proxiedOpt);
root.AddOption(ttlOpt);
root.AddOption(prefixOpt);
root.AddOption(downloadOpt);
root.AddOption(fallbackOpt);
root.AddOption(skipSvcOpt);

root.SetHandler(async (context) =>
{
    var accountId = context.ParseResult.GetValueForOption(accountIdOpt)!;
    var zoneId = context.ParseResult.GetValueForOption(zoneIdOpt)!;
    var hostname = context.ParseResult.GetValueForOption(hostOpt)!;
    var origin = context.ParseResult.GetValueForOption(originOpt)!;
    var proxied = context.ParseResult.GetValueForOption(proxiedOpt);
    var ttl = context.ParseResult.GetValueForOption(ttlOpt)!;
    var prefix = context.ParseResult.GetValueForOption(prefixOpt)!;
    var cloudflaredUrl = context.ParseResult.GetValueForOption(downloadOpt)!;
    var fallback = context.ParseResult.GetValueForOption(fallbackOpt)!;
    var skipService = context.ParseResult.GetValueForOption(skipSvcOpt);
    
    var apiToken = Environment.GetEnvironmentVariable("CF_API_TOKEN");
    if (string.IsNullOrWhiteSpace(apiToken))
        throw new InvalidOperationException("A variável de ambiente CF_API_TOKEN não está definida.");

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

    var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    // Política de retry/backoff com jitter
    var retry = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || (int)r.StatusCode == 429)
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250)),
            onRetry: (outcome, ts, attempt, _) => Log.Warn($"Retry {attempt} em {ts.TotalMilliseconds:F0}ms"));

    async Task<T> PostJson<T>(string url, object body)
    {
        var resp = await retry.ExecuteAsync(() => http.PostAsJsonAsync(url, body));
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Status {(int)resp.StatusCode}: {error}");
        }
        var doc = await resp.Content.ReadFromJsonAsync<T>(jsonOpts);
        return doc!;
    }
    async Task<T> PutJson<T>(string url, object body)
    {
        var resp = await retry.ExecuteAsync(() => http.PutAsJsonAsync(url, body));
        resp.EnsureSuccessStatusCode();
        var doc = await resp.Content.ReadFromJsonAsync<T>(jsonOpts);
        return doc!;
    }
    async Task<T> GetJson<T>(string url)
    {
        var resp = await retry.ExecuteAsync(() => http.GetAsync(url));
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Status {(int)resp.StatusCode}: {error}");
        }
        var doc = await resp.Content.ReadFromJsonAsync<T>(jsonOpts);
        return doc!;
    }

    // 0) Verificar Token (opcional, falha rápida)
    try
    {
        await GetJson<ApiResult<object>>("https://api.cloudflare.com/client/v4/user/tokens/verify");
        Log.Ok("CF_API_TOKEN verificado.");
    }
    catch
    {
        throw new Exception("Falha ao verificar CF_API_TOKEN. Permissões: Account(Tunnel:Edit) + Zone(DNS:Edit).");
    }

    // 1) Criar ou obter túnel existente
    Log.Info("Criando túnel...");
    var secretBytes = RandomNumberGenerator.GetBytes(32);
    var secret = Convert.ToBase64String(secretBytes);
    var tunnelName = $"{prefix}{Environment.MachineName}";
    string tunnelId;
    
    try
    {
        var create = await PostJson<ApiResult<Tunnel>>(
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel",
            new { name = tunnelName, tunnel_secret = secretBytes });
        tunnelId = create.Result!.Id;
        if (string.IsNullOrWhiteSpace(tunnelId)) throw new Exception("Sem Tunnel ID.");
        Log.Ok($"Tunnel ID: {tunnelId}");
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("\"code\":1013"))
    {
        // Túnel já existe, buscar ID pelo nome
        Log.Warn($"Túnel '{tunnelName}' já existe. Usando túnel existente...");
        var tunnels = await GetJson<ApiResult<List<Tunnel>>>(
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel?name={Uri.EscapeDataString(tunnelName)}");
        var existing = tunnels.Result!.FirstOrDefault(t => t.Name == tunnelName);
        if (existing == null) throw new Exception($"Não foi possível localizar o túnel '{tunnelName}'.");
        tunnelId = existing.Id;
        Log.Ok($"Tunnel ID: {tunnelId}");
    }

    // 2) Token do túnel
    Log.Info("Obtendo token do túnel...");
    var tok = await GetJson<ApiResult<string>>(
        $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/token");
    var tunnelToken = tok.Result!;

    // 3) Instalar serviço (Windows)
    if (!skipService && OperatingSystem.IsWindows())
    {
        EnsureAdmin(); // exige elevação
        var exe = await EnsureCloudflaredBinary(cloudflaredUrl);
        InstallService(exe, tunnelToken);
        StartAndWaitService("cloudflared", TimeSpan.FromSeconds(30));
        ApplyServiceRecovery("cloudflared");
        Log.Ok("Serviço cloudflared instalado e em execução.");
    }
    else if (!OperatingSystem.IsWindows() && !skipService)
    {
        Log.Warn("Sistema não é Windows. Pulei a instalação do serviço (use --skip-service para esconder este aviso).");
    }

    // 4) Config remota (ingress)
    Log.Info("Aplicando configuração remota (ingress)...");
    var cfg = new
    {
        config = new
        {
            ingress = new object[] {
                new { hostname = hostname, service = origin },
                new { service = fallback }
            },
            warp_routing = new { enabled = false }
        }
    };
    await PutJson<ApiResult<object>>(
        $"https://api.cloudflare.com/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations", cfg);

    // 5) DNS CNAME -> <UUID>.cfargotunnel.com
    Log.Info("Criando/atualizando DNS (CNAME)...");
    var target = $"{tunnelId}.cfargotunnel.com";

    var list = await GetJson<ApiResult<List<DnsRecord>>>(
        $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records?type=CNAME&name={Uri.EscapeDataString(hostname)}");
    var recId = list.Result!.FirstOrDefault()?.Id;

    var dnsPayload = new
    {
        type = "CNAME",
        name = hostname,
        content = target,
        proxied = proxied,
        ttl = ttl.Equals("auto", StringComparison.OrdinalIgnoreCase) ? 1 : int.Parse(ttl)
    };

    if (!string.IsNullOrEmpty(recId))
    {
        await PutJson<ApiResult<object>>(
            $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recId}", dnsPayload);
    }
    else
    {
        await PostJson<ApiResult<object>>(
            $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records", dnsPayload);
    }

    Console.WriteLine();
    Log.Ok("Túnel criado e ativo!");
    Console.WriteLine($" - Tunnel Name : {tunnelName}");
    Console.WriteLine($" - Tunnel ID   : {tunnelId}");
    Console.WriteLine($" - Hostname    : https://{hostname}");
    Console.WriteLine($" - Origem      : {origin}");
    if (OperatingSystem.IsWindows() && !skipService)
    {
        using var sc = new ServiceController("cloudflared");
        Console.WriteLine($" - Serviço     : cloudflared (status: {sc.Status})");
    }
});

// ======= Local helper functions =======

static void EnsureAdmin()
{
    try
    {
        using var id = System.Security.Principal.WindowsIdentity.GetCurrent();
        var p = new System.Security.Principal.WindowsPrincipal(id);
        if (!p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            throw new InvalidOperationException("Execute este programa como Administrador (UAC) para instalar o serviço cloudflared.");
    }
    catch (PlatformNotSupportedException) { } // não-Windows
}

static async Task<string> EnsureCloudflaredBinary(string url)
{
    var exeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "cloudflared");
    var exe = Path.Combine(exeDir, "cloudflared.exe");
    Directory.CreateDirectory(exeDir);
    if (!File.Exists(exe))
    {
        Log.Info("Baixando cloudflared...");
        using var hc = new HttpClient();
        var bin = await hc.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(exe, bin);
    }
    return exe;
}

static void InstallService(string exe, string tunnelToken)
{
    var psi = new ProcessStartInfo
    {
        FileName = exe,
        Arguments = $"service install {tunnelToken}",
        UseShellExecute = false,
        CreateNoWindow = true
    };
    using var p = Process.Start(psi)!;
    p.WaitForExit();
    if (p.ExitCode != 0) throw new Exception($"Falha ao instalar o serviço cloudflared (exit {p.ExitCode}).");
}

static void StartAndWaitService(string name, TimeSpan timeout)
{
    using var sc = new ServiceController(name);
    if (sc.Status != ServiceControllerStatus.Running)
    {
        sc.Start();
        sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
    }
}

static void ApplyServiceRecovery(string name)
{
    void Run(string args)
    {
        using var p = Process.Start(new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;
        p.WaitForExit();
    }
    Run($@"failure {name} reset= 86400 actions= restart/5000/restart/30000/restart/60000");
    Run($@"failureflag {name} 1");
}

await root.InvokeAsync(args);

// ======= Type declarations =======

record ApiResult<T>(bool Success, T? Result, ApiError[]? Errors)
{
    public T? Result { get; init; } = Result;
}
record ApiError(string Code, string Message);
record Tunnel(string Id, string Name);
record TunnelToken(string Token);
record TunnelList(List<Tunnel> Tunnels)
{
    public List<Tunnel> Tunnels { get; init; } = Tunnels;
}
record DnsRecord(string Id, string Name, string Type, string Content);
record DnsList(List<DnsRecord> Result)
{
    public List<DnsRecord> Result { get; init; } = Result;
    public DnsRecord? FirstOrDefault() => Result.Count > 0 ? Result[0] : null;
}

static class Log
{
    public static void Info(string msg)  => Console.WriteLine($"[INFO] {msg}");
    public static void Warn(string msg)  => Console.WriteLine($"[WARN] {msg}");
    public static void Error(string msg) => Console.Error.WriteLine($"[ERROR] {msg}");
    public static void Ok(string msg)    => Console.WriteLine($"[ OK ] {msg}");
}
