using System.CommandLine;
using CfTunnel.Core.Models;
using CfTunnel.Core.Services;
using CfTunnel.Core.Utilities;

// Definição das opções de linha de comando
var accountIdOpt = new Option<string>("--account-id", "Cloudflare Account ID") { IsRequired = true };
var zoneIdOpt = new Option<string>("--zone-id", "Cloudflare Zone ID") { IsRequired = true };
var hostOpt = new Option<string>("--hostname", "Hostname público (ex.: app-host.example.com)") { IsRequired = true };
var originOpt = new Option<string>("--origin", () => "http://127.0.0.1:8080", "URL do serviço local a expor");
var proxiedOpt = new Option<bool>("--proxied", () => true, "CNAME proxied (laranja)");
var ttlOpt = new Option<string>("--ttl", () => "auto", "TTL (segundos ou 'auto')");
var prefixOpt = new Option<string>("--tunnel-prefix", () => "tunnel-", "Prefixo do nome do túnel");
var downloadOpt = new Option<string>(
    "--cloudflared-url",
    () => "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe",
    "URL binário cloudflared (Windows)");
var fallbackOpt = new Option<string>("--fallback", () => "http_status:404", "Regra de fallback do ingress");
var skipSvcOpt = new Option<bool>("--skip-service", () => false, "Pula a instalação do serviço (útil em Linux ou quando já existe)");

// Comando raiz
var rootCommand = new RootCommand("Provisionador autônomo de Cloudflare Tunnel");
rootCommand.AddOption(accountIdOpt);
rootCommand.AddOption(zoneIdOpt);
rootCommand.AddOption(hostOpt);
rootCommand.AddOption(originOpt);
rootCommand.AddOption(proxiedOpt);
rootCommand.AddOption(ttlOpt);
rootCommand.AddOption(prefixOpt);
rootCommand.AddOption(downloadOpt);
rootCommand.AddOption(fallbackOpt);
rootCommand.AddOption(skipSvcOpt);

// Handler do comando
rootCommand.SetHandler(async (context) =>
{
    try
    {
        // Obter CF_API_TOKEN da variável de ambiente
        var apiToken = Environment.GetEnvironmentVariable("CF_API_TOKEN");
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            Logger.Error("A variável de ambiente CF_API_TOKEN não está definida.");
            context.ExitCode = 1;
            return;
        }

        // Criar configuração a partir dos argumentos
        var config = new TunnelConfiguration
        {
            AccountId = context.ParseResult.GetValueForOption(accountIdOpt)!,
            ZoneId = context.ParseResult.GetValueForOption(zoneIdOpt)!,
            Hostname = context.ParseResult.GetValueForOption(hostOpt)!,
            Origin = context.ParseResult.GetValueForOption(originOpt)!,
            Proxied = context.ParseResult.GetValueForOption(proxiedOpt),
            Ttl = context.ParseResult.GetValueForOption(ttlOpt)!,
            TunnelPrefix = context.ParseResult.GetValueForOption(prefixOpt)!,
            CloudflaredUrl = context.ParseResult.GetValueForOption(downloadOpt)!,
            Fallback = context.ParseResult.GetValueForOption(fallbackOpt)!,
            SkipService = context.ParseResult.GetValueForOption(skipSvcOpt),
            ApiToken = apiToken
        };

        // Provisionar túnel usando o serviço do Core
        using var provisioningService = new TunnelProvisioningService(config);
        var result = await provisioningService.ProvisionTunnelAsync();

        // Exibir resultado
        Console.WriteLine();
        Logger.Success("Túnel criado e ativo!");
        Console.WriteLine($" - Tunnel Name : {result.TunnelName}");
        Console.WriteLine($" - Tunnel ID   : {result.TunnelId}");
        Console.WriteLine($" - Hostname    : https://{result.Hostname}");
        Console.WriteLine($" - Origem      : {result.Origin}");
        
        if (!string.IsNullOrEmpty(result.ServiceStatus))
        {
            Console.WriteLine($" - Serviço     : cloudflared (status: {result.ServiceStatus})");
        }

        context.ExitCode = 0;
    }
    catch (Exception ex)
    {
        Logger.Error($"Erro ao provisionar túnel: {ex.Message}");
        if (ex.InnerException != null)
        {
            Logger.Error($"  Detalhes: {ex.InnerException.Message}");
        }
        context.ExitCode = 1;
    }
});

// Executar o comando
return await rootCommand.InvokeAsync(args);
