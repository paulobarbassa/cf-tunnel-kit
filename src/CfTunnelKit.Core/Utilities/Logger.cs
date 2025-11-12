namespace CfTunnel.Core.Utilities;

/// <summary>
/// Utilitário simples para logging com diferentes níveis.
/// </summary>
public static class Logger
{
    public static void Info(string message) => Console.WriteLine($"[INFO] {message}");
    
    public static void Warn(string message) => Console.WriteLine($"[WARN] {message}");
    
    public static void Error(string message) => Console.Error.WriteLine($"[ERROR] {message}");
    
    public static void Success(string message) => Console.WriteLine($"[ OK ] {message}");
}
