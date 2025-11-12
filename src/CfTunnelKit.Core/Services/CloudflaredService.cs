using System.Diagnostics;
using System.ServiceProcess;
using CfTunnel.Core.Utilities;

namespace CfTunnel.Core.Services;

/// <summary>
/// Serviço para gerenciar a instalação e configuração do cloudflared no Windows.
/// </summary>
public class CloudflaredService
{
    /// <summary>
    /// Verifica se o usuário atual tem privilégios de administrador (Windows).
    /// </summary>
    public static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Garante que o binário cloudflared está instalado, fazendo download se necessário.
    /// </summary>
    public static async Task<string> EnsureBinaryAsync(string downloadUrl)
    {
        var exeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "cloudflared");
        var exePath = Path.Combine(exeDir, "cloudflared.exe");
        
        Directory.CreateDirectory(exeDir);
        
        if (!File.Exists(exePath))
        {
            Logger.Info("Baixando cloudflared...");
            using var httpClient = new HttpClient();
            var binaryData = await httpClient.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(exePath, binaryData);
            Logger.Success($"cloudflared baixado em: {exePath}");
        }
        else
        {
            Logger.Info($"cloudflared já existe em: {exePath}");
        }
        
        return exePath;
    }

    /// <summary>
    /// Instala o cloudflared como serviço do Windows.
    /// </summary>
    public static void InstallService(string exePath, string tunnelToken)
    {
        Logger.Info("Instalando serviço cloudflared...");
        
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"service install {tunnelToken}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        using var process = Process.Start(psi);
        if (process == null)
            throw new Exception("Falha ao iniciar processo de instalação do serviço.");
        
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Falha ao instalar serviço cloudflared (exit {process.ExitCode}): {error}");
        }
        
        Logger.Success("Serviço cloudflared instalado.");
    }

    /// <summary>
    /// Inicia o serviço cloudflared e aguarda até que esteja em execução.
    /// </summary>
    public static void StartService(string serviceName = "cloudflared", TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        
        Logger.Info($"Iniciando serviço {serviceName}...");
        
        using var serviceController = new ServiceController(serviceName);
        
        if (serviceController.Status == ServiceControllerStatus.Running)
        {
            Logger.Info($"Serviço {serviceName} já está em execução.");
            return;
        }
        
        serviceController.Start();
        serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout.Value);
        
        Logger.Success($"Serviço {serviceName} iniciado.");
    }

    /// <summary>
    /// Configura políticas de recuperação automática do serviço.
    /// </summary>
    public static void ConfigureServiceRecovery(string serviceName = "cloudflared")
    {
        Logger.Info("Configurando recuperação automática do serviço...");
        
        void RunScCommand(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(psi);
            process?.WaitForExit();
        }
        
        // Configura restart em caso de falha: 5s, 30s, 60s
        RunScCommand($@"failure {serviceName} reset= 86400 actions= restart/5000/restart/30000/restart/60000");
        
        // Habilita flag de recuperação
        RunScCommand($@"failureflag {serviceName} 1");
        
        Logger.Success("Recuperação automática configurada.");
    }

    /// <summary>
    /// Obtém o status atual do serviço cloudflared.
    /// </summary>
    public static string? GetServiceStatus(string serviceName = "cloudflared")
    {
        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            using var serviceController = new ServiceController(serviceName);
            return serviceController.Status.ToString();
        }
        catch
        {
            return null;
        }
    }
}
