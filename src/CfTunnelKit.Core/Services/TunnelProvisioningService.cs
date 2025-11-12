using System.Security.Cryptography;
using CfTunnel.Core.Models;
using CfTunnel.Core.Utilities;

namespace CfTunnel.Core.Services;

/// <summary>
/// Serviço principal para provisionamento completo de túneis Cloudflare.
/// </summary>
public class TunnelProvisioningService : IDisposable
{
    private readonly CloudflareApiClient _apiClient;
    private readonly TunnelConfiguration _config;

    public TunnelProvisioningService(TunnelConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _apiClient = new CloudflareApiClient(config.ApiToken);
    }

    /// <summary>
    /// Provisiona um túnel Cloudflare completo: criação, configuração, DNS e serviço.
    /// </summary>
    public async Task<TunnelProvisionResult> ProvisionTunnelAsync()
    {
        // 0. Verificar token da API
        Logger.Info("Verificando CF_API_TOKEN...");
        if (!await _apiClient.VerifyTokenAsync())
        {
            throw new Exception(
                "Falha ao verificar CF_API_TOKEN. Certifique-se de que o token possui permissões: " +
                "Account(Tunnel:Edit) + Zone(DNS:Edit).");
        }
        Logger.Success("CF_API_TOKEN verificado.");

        // 1. Criar ou obter túnel existente
        Logger.Info("Criando túnel...");
        var secretBytes = RandomNumberGenerator.GetBytes(32);
        var tunnelName = $"{_config.TunnelPrefix}{Environment.MachineName}";
        string tunnelId;

        try
        {
            var tunnel = await _apiClient.CreateTunnelAsync(_config.AccountId, tunnelName, secretBytes);
            tunnelId = tunnel.Id;
            Logger.Success($"Túnel criado: {tunnelId}");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("\"code\":1013"))
        {
            // Túnel já existe
            Logger.Warn($"Túnel '{tunnelName}' já existe. Usando túnel existente...");
            var tunnels = await _apiClient.GetTunnelsAsync(_config.AccountId, tunnelName);
            var existing = tunnels.FirstOrDefault(t => t.Name == tunnelName);
            
            if (existing == null)
                throw new Exception($"Não foi possível localizar o túnel '{tunnelName}'.");
            
            tunnelId = existing.Id;
            Logger.Success($"Túnel existente: {tunnelId}");
        }

        // 2. Obter token do túnel
        Logger.Info("Obtendo token do túnel...");
        var tunnelToken = await _apiClient.GetTunnelTokenAsync(_config.AccountId, tunnelId);
        Logger.Success("Token obtido.");

        // 3. Instalar serviço (Windows)
        string? serviceStatus = null;
        if (!_config.SkipService && OperatingSystem.IsWindows())
        {
            if (!CloudflaredService.IsAdministrator())
            {
                throw new InvalidOperationException(
                    "Execute este programa como Administrador (UAC) para instalar o serviço cloudflared.");
            }

            var exePath = await CloudflaredService.EnsureBinaryAsync(_config.CloudflaredUrl);
            CloudflaredService.InstallService(exePath, tunnelToken);
            CloudflaredService.StartService("cloudflared", TimeSpan.FromSeconds(30));
            CloudflaredService.ConfigureServiceRecovery("cloudflared");
            
            serviceStatus = CloudflaredService.GetServiceStatus("cloudflared");
            Logger.Success($"Serviço cloudflared instalado e em execução: {serviceStatus}");
        }
        else if (!OperatingSystem.IsWindows() && !_config.SkipService)
        {
            Logger.Warn("Sistema não é Windows. Instalação do serviço ignorada (use --skip-service para ocultar este aviso).");
        }

        // 4. Configurar ingress remoto
        Logger.Info("Aplicando configuração remota (ingress)...");
        await _apiClient.UpdateTunnelConfigurationAsync(
            _config.AccountId,
            tunnelId,
            _config.Hostname,
            _config.Origin,
            _config.Fallback);
        Logger.Success("Configuração aplicada.");

        // 5. Configurar DNS (CNAME)
        Logger.Info("Criando/atualizando DNS (CNAME)...");
        var target = $"{tunnelId}.cfargotunnel.com";
        var ttlValue = _config.Ttl.Equals("auto", StringComparison.OrdinalIgnoreCase) ? 1 : int.Parse(_config.Ttl);

        var existingRecords = await _apiClient.GetDnsRecordsAsync(_config.ZoneId, "CNAME", _config.Hostname);
        var existingRecord = existingRecords.FirstOrDefault();

        if (existingRecord != null)
        {
            await _apiClient.UpdateDnsRecordAsync(
                _config.ZoneId,
                existingRecord.Id,
                "CNAME",
                _config.Hostname,
                target,
                _config.Proxied,
                ttlValue);
            Logger.Success("DNS atualizado.");
        }
        else
        {
            await _apiClient.CreateDnsRecordAsync(
                _config.ZoneId,
                "CNAME",
                _config.Hostname,
                target,
                _config.Proxied,
                ttlValue);
            Logger.Success("DNS criado.");
        }

        return new TunnelProvisionResult
        {
            TunnelName = tunnelName,
            TunnelId = tunnelId,
            Hostname = _config.Hostname,
            Origin = _config.Origin,
            TunnelToken = tunnelToken,
            ServiceStatus = serviceStatus
        };
    }

    public void Dispose()
    {
        _apiClient?.Dispose();
    }
}
