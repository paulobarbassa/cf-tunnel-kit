namespace CfTunnel.Core.Models;

/// <summary>
/// Configuração completa para provisionamento de um túnel Cloudflare.
/// </summary>
public class TunnelConfiguration
{
    /// <summary>
    /// ID da conta do Cloudflare (obrigatório).
    /// </summary>
    public required string AccountId { get; init; }
    
    /// <summary>
    /// ID da zona DNS do Cloudflare (obrigatório).
    /// </summary>
    public required string ZoneId { get; init; }
    
    /// <summary>
    /// Hostname público (ex.: app.example.com) - obrigatório.
    /// </summary>
    public required string Hostname { get; init; }
    
    /// <summary>
    /// URL do serviço local a ser exposto (padrão: http://127.0.0.1:8080).
    /// </summary>
    public string Origin { get; init; } = "http://127.0.0.1:8080";
    
    /// <summary>
    /// Se o CNAME deve ser proxied (laranja) - padrão: true.
    /// </summary>
    public bool Proxied { get; init; } = true;
    
    /// <summary>
    /// TTL do DNS em segundos ou "auto" - padrão: "auto".
    /// </summary>
    public string Ttl { get; init; } = "auto";
    
    /// <summary>
    /// Prefixo do nome do túnel (padrão: "tunnel-").
    /// </summary>
    public string TunnelPrefix { get; init; } = "tunnel-";
    
    /// <summary>
    /// URL para download do binário cloudflared (Windows).
    /// </summary>
    public string CloudflaredUrl { get; init; } = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";
    
    /// <summary>
    /// Regra de fallback do ingress (padrão: http_status:404).
    /// </summary>
    public string Fallback { get; init; } = "http_status:404";
    
    /// <summary>
    /// Se deve pular a instalação do serviço Windows.
    /// </summary>
    public bool SkipService { get; init; } = false;
    
    /// <summary>
    /// Token da API do Cloudflare (obrigatório).
    /// </summary>
    public required string ApiToken { get; init; }
}
