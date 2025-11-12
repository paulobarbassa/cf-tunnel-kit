namespace CfTunnel.Core.Models;

/// <summary>
/// Resultado do provisionamento de um túnel Cloudflare.
/// </summary>
public class TunnelProvisionResult
{
    /// <summary>
    /// Nome do túnel criado ou existente.
    /// </summary>
    public required string TunnelName { get; init; }
    
    /// <summary>
    /// ID único do túnel.
    /// </summary>
    public required string TunnelId { get; init; }
    
    /// <summary>
    /// Hostname público configurado.
    /// </summary>
    public required string Hostname { get; init; }
    
    /// <summary>
    /// URL de origem (serviço local).
    /// </summary>
    public required string Origin { get; init; }
    
    /// <summary>
    /// Token do túnel para autenticação do cloudflared.
    /// </summary>
    public required string TunnelToken { get; init; }
    
    /// <summary>
    /// Status do serviço Windows (se aplicável).
    /// </summary>
    public string? ServiceStatus { get; init; }
}
