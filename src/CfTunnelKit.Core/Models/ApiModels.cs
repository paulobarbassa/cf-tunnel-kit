namespace CfTunnel.Core.Models;

/// <summary>
/// Representa o resultado padrão da API do Cloudflare.
/// </summary>
/// <typeparam name="T">Tipo do resultado</typeparam>
public record ApiResult<T>(bool Success, T? Result, ApiError[]? Errors)
{
    public T? Result { get; init; } = Result;
}

/// <summary>
/// Representa um erro retornado pela API do Cloudflare.
/// </summary>
public record ApiError(string Code, string Message);

/// <summary>
/// Representa um túnel do Cloudflare.
/// </summary>
public record Tunnel(string Id, string Name);

/// <summary>
/// Representa um token de túnel do Cloudflare.
/// </summary>
public record TunnelToken(string Token);

/// <summary>
/// Representa uma lista de túneis.
/// </summary>
public record TunnelList(List<Tunnel> Tunnels)
{
    public List<Tunnel> Tunnels { get; init; } = Tunnels;
}

/// <summary>
/// Representa um registro DNS.
/// </summary>
public record DnsRecord(string Id, string Name, string Type, string Content);

/// <summary>
/// Representa uma lista de registros DNS.
/// </summary>
public record DnsList(List<DnsRecord> Result)
{
    public List<DnsRecord> Result { get; init; } = Result;
    
    public DnsRecord? FirstOrDefault() => Result.Count > 0 ? Result[0] : null;
}
