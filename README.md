# 🚀 CF Tunnel Kit

<div align="center">

[![Build and Test](https://github.com/paulobarbassa/cf-tunnel-kit/actions/workflows/build.yml/badge.svg)](https://github.com/paulobarbassa/cf-tunnel-kit/actions/workflows/build.yml)
[![Release](https://github.com/paulobarbassa/cf-tunnel-kit/actions/workflows/release.yml/badge.svg)](https://github.com/paulobarbassa/cf-tunnel-kit/actions/workflows/release.yml)
[![GitHub release](https://img.shields.io/github/v/release/paulobarbassa/cf-tunnel-kit?style=flat-square)](https://github.com/paulobarbassa/cf-tunnel-kit/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=flat-square)](https://github.com/paulobarbassa/cf-tunnel-kit)

**Provisionador autônomo de Cloudflare Tunnel para .NET**

Exponha suas aplicações locais para a internet de forma segura e profissional, sem configuração manual de DNS ou túneis.

[Funcionalidades](#-funcionalidades) • [Instalação](#-instalação) • [Uso Rápido](#-uso-rápido) • [Documentação](#-documentação) • [Exemplos](#-exemplos)

</div>

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Arquitetura](#-arquitetura)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Uso Rápido](#-uso-rápido)
- [Documentação Completa](#-documentação-completa)
- [Exemplos](#-exemplos)
- [Integração Programática](#-integração-programática)
- [Desenvolvimento](#-desenvolvimento)
- [Roadmap](#-roadmap)
- [Contribuindo](#-contribuindo)
- [Licença](#-licença)

---

## 🎯 Sobre o Projeto

**CF Tunnel Kit** é uma solução completa para provisionamento automatizado de túneis Cloudflare, projetada para desenvolvedores que precisam expor aplicações locais para a internet de forma rápida, segura e profissional.

### Por que usar CF Tunnel Kit?

- ✅ **Zero configuração manual**: Cria túnel, configura DNS e aplica rotas automaticamente
- ✅ **Pronto para produção**: Retry automático, logging estruturado e gestão de erros
- ✅ **Flexível**: Use como CLI, biblioteca .NET ou integre em seus próprios projetos
- ✅ **Resiliente**: Política de retry com backoff exponencial e jitter
- ✅ **Multi-plataforma**: Windows, Linux e macOS

### Casos de Uso

- 🔧 Desenvolvimento local com webhooks externos (Stripe, PayPal, etc.)
- 🌐 Demonstrações de aplicações para clientes
- 🧪 Testes de integrações com serviços externos
- 📱 Desenvolvimento mobile com APIs locais
- 🏢 Exposição temporária de serviços internos

---

## ✨ Funcionalidades

### Provisionamento Completo

- 🔐 **Verificação automática** de credenciais da API Cloudflare
- 🚇 **Criação/reutilização** inteligente de túneis
- 🌍 **Configuração automática de DNS** (registros CNAME)
- ⚙️ **Aplicação de rotas de ingress** remotamente
- 🪟 **Instalação opcional de serviço Windows** (cloudflared)

### Robustez e Confiabilidade

- 🔄 **Retry automático** com backoff exponencial e jitter (Polly)
- 📝 **Logging estruturado** com níveis (Info, Warn, Error, Success)
- 🛡️ **Tratamento de erros** detalhado com mensagens claras
- ⏱️ **Timeout configurável** para operações críticas

### Flexibilidade

- 🎛️ **Múltiplas opções de configuração** (CLI arguments)
- 📦 **Biblioteca reutilizável** (`CfTunnel.Core`)
- 🔌 **Fácil integração** em projetos .NET existentes
- 🖥️ **Suporte multi-plataforma** (Windows, Linux, macOS)

---

## 🏗️ Arquitetura

O projeto é estruturado em camadas para máxima reutilização e testabilidade:

```
cf-tunnel-kit/
├── src/
│   ├── CfTunnel.Console/          # 🖥️ Aplicação CLI
│   │   ├── Program.cs             # Entry point com System.CommandLine
│   │   └── CfTunnel.Console.csproj
│   │
│   └── CfTunnelKit.Core/          # 📦 Biblioteca Core
│       ├── Models/                # 📄 Modelos de dados
│       │   ├── ApiModels.cs       # DTOs da API Cloudflare
│       │   ├── TunnelConfiguration.cs
│       │   └── TunnelProvisionResult.cs
│       │
│       ├── Services/              # ⚙️ Serviços principais
│       │   ├── CloudflareApiClient.cs        # Cliente HTTP com retry
│       │   ├── CloudflaredService.cs         # Gestão do binário/serviço
│       │   └── TunnelProvisioningService.cs  # Orquestração completa
│       │
│       ├── Utilities/             # 🛠️ Utilitários
│       │   └── Logger.cs          # Logger colorido
│       │
│       └── CfTunnel.Core.csproj
│
├── tunnel-install.sln             # Solução Visual Studio
├── exemplo-uso.bat                # Script de exemplo (Windows)
└── README.md
```

### Componentes Principais

#### 🖥️ **CfTunnel.Console**
Aplicação de linha de comando com interface amigável usando `System.CommandLine`.

#### 📦 **CfTunnelKit.Core**

- **`CloudflareApiClient`**: Cliente HTTP para API do Cloudflare com retry/backoff automático (Polly)
- **`CloudflaredService`**: Gerenciamento do binário cloudflared e instalação de serviço Windows
- **`TunnelProvisioningService`**: Orquestrador que executa todo o fluxo de provisionamento
- **`Logger`**: Sistema de logging colorido para melhor experiência do usuário

---

## 📋 Pré-requisitos

### Software

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** ou superior
- **Windows** (para instalação automática do serviço cloudflared)
  - *Linux/macOS: Use `--skip-service` para pular instalação do serviço*

### Credenciais Cloudflare

Você precisará de uma conta Cloudflare (gratuita) e um domínio configurado. Obtenha:

1. **Account ID**: [Dashboard](https://dash.cloudflare.com/) → Selecione sua conta → Account ID (barra lateral direita)

2. **Zone ID**: Dashboard → Selecione seu domínio → Overview → Zone ID (barra lateral direita)

3. **API Token**: Dashboard → My Profile → [API Tokens](https://dash.cloudflare.com/profile/api-tokens) → Create Token

   **Permissões necessárias:**
   - ✅ **Account** → Cloudflare Tunnel → **Edit**
   - ✅ **Zone** → DNS → **Edit**

---

## 🚀 Instalação

### Clonar o Repositório

```bash
git clone https://github.com/paulobarbassa/cf-tunnel-kit.git
cd cf-tunnel-kit
```

### Compilar o Projeto

```bash
dotnet build tunnel-install.sln
```

### Executar sem Compilação

```bash
cd src/CfTunnel.Console
dotnet run -- --help
```

### Criar Executável Standalone

```bash
dotnet publish src/CfTunnel.Console/CfTunnel.Console.csproj -c Release -o ./publish
```

O executável estará em `./publish/cf-tunnel.exe` (Windows) ou `./publish/cf-tunnel` (Linux/macOS).

---

## ⚡ Uso Rápido

### Exemplo Básico

```powershell
# Definir token da API
$env:CF_API_TOKEN = "seu-token-aqui"

# Navegar até o projeto
cd src/CfTunnel.Console

# Provisionar túnel
dotnet run -- `
  --account-id "sua-account-id" `
  --zone-id "sua-zone-id" `
  --hostname "app.seudominio.com" `
  --origin "http://localhost:3000" `
  --skip-service
```

### Usando o Script de Exemplo (Windows)

Edite o arquivo `exemplo-uso.bat` com suas credenciais e execute:

```batch
exemplo-uso.bat
```

### Saída Esperada

```
[INFO] Verificando CF_API_TOKEN...
[ OK ] CF_API_TOKEN verificado.
[INFO] Criando túnel...
[ OK ] Túnel criado: abc123def456
[INFO] Obtendo token do túnel...
[ OK ] Token obtido.
[INFO] Aplicando configuração remota (ingress)...
[ OK ] Configuração aplicada.
[INFO] Criando/atualizando DNS (CNAME)...
[ OK ] DNS criado.

[ OK ] Túnel criado e ativo!
 - Tunnel Name : tunnel-MYMACHINE
 - Tunnel ID   : abc123def456
 - Hostname    : https://app.seudominio.com
 - Origem      : http://localhost:3000
```

---

## 📚 Documentação Completa

### Opções da Linha de Comando

| Opção | Obrigatório | Padrão | Descrição |
|-------|-------------|--------|-----------|
| `--account-id` | ✅ Sim | - | Cloudflare Account ID |
| `--zone-id` | ✅ Sim | - | Cloudflare Zone ID da zona DNS |
| `--hostname` | ✅ Sim | - | Hostname público (ex: app.example.com) |
| `--origin` | ❌ Não | `http://127.0.0.1:8080` | URL do serviço local a expor |
| `--proxied` | ❌ Não | `true` | DNS proxied pela Cloudflare (proxy laranja) |
| `--ttl` | ❌ Não | `auto` | TTL do registro DNS em segundos ou "auto" |
| `--tunnel-prefix` | ❌ Não | `tunnel-` | Prefixo do nome do túnel |
| `--cloudflared-url` | ❌ Não | [link oficial] | URL para download do cloudflared |
| `--fallback` | ❌ Não | `http_status:404` | Regra de fallback do ingress |
| `--skip-service` | ❌ Não | `false` | Pula instalação do serviço Windows |

### Variável de Ambiente

- **`CF_API_TOKEN`** (obrigatória): Token da API Cloudflare

  ```powershell
  # PowerShell
  $env:CF_API_TOKEN = "seu-token"
  
  # Bash
  export CF_API_TOKEN="seu-token"
  ```

---

## 💡 Exemplos

### Exemplo 1: Expor Aplicação React Local

```powershell
$env:CF_API_TOKEN = "seu-token"

dotnet run -- `
  --account-id "abc123" `
  --zone-id "xyz789" `
  --hostname "app.meusite.com" `
  --origin "http://localhost:3000"
```

### Exemplo 2: API Node.js na Porta 8080

```powershell
$env:CF_API_TOKEN = "seu-token"

dotnet run -- `
  --account-id "abc123" `
  --zone-id "xyz789" `
  --hostname "api.meusite.com" `
  --origin "http://localhost:8080" `
  --tunnel-prefix "api-"
```

### Exemplo 3: Servidor .NET com DNS Não-Proxied

```powershell
$env:CF_API_TOKEN = "seu-token"

dotnet run -- `
  --account-id "abc123" `
  --zone-id "xyz789" `
  --hostname "direto.meusite.com" `
  --origin "http://localhost:5000" `
  --proxied false
```

### Exemplo 4: Instalar como Serviço Windows (requer Admin)

```powershell
# Execute como Administrador
$env:CF_API_TOKEN = "seu-token"

dotnet run -- `
  --account-id "abc123" `
  --zone-id "xyz789" `
  --hostname "app.meusite.com" `
  --origin "http://localhost:3000"
```

---

## 🔌 Integração Programática

Use `CfTunnel.Core` como biblioteca em seus próprios projetos .NET:

### 1. Adicionar Referência

```xml
<ItemGroup>
  <ProjectReference Include="caminho\para\CfTunnelKit.Core\CfTunnel.Core.csproj" />
</ItemGroup>
```

### 2. Usar no Código

```csharp
using CfTunnel.Core.Models;
using CfTunnel.Core.Services;

// Configurar
var config = new TunnelConfiguration
{
    AccountId = "abc123",
    ZoneId = "xyz789",
    Hostname = "app.example.com",
    Origin = "http://localhost:5000",
    ApiToken = Environment.GetEnvironmentVariable("CF_API_TOKEN"),
    SkipService = true  // Não instalar serviço Windows
};

// Provisionar
using var service = new TunnelProvisioningService(config);
var result = await service.ProvisionTunnelAsync();

// Usar resultado
Console.WriteLine($"✅ Túnel ativo: https://{result.Hostname}");
Console.WriteLine($"🆔 Tunnel ID: {result.TunnelId}");
Console.WriteLine($"📍 Origin: {result.Origin}");
```

### 3. Tratamento de Erros

```csharp
try
{
    using var service = new TunnelProvisioningService(config);
    var result = await service.ProvisionTunnelAsync();
    // Sucesso
}
catch (HttpRequestException ex)
{
    // Erro de comunicação com API
    Console.WriteLine($"Erro de API: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Erro de operação (ex: não é administrador)
    Console.WriteLine($"Erro de operação: {ex.Message}");
}
catch (Exception ex)
{
    // Outros erros
    Console.WriteLine($"Erro: {ex.Message}");
}
```

---

## 🛠️ Desenvolvimento

### Estrutura de Desenvolvimento

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar testes (se existirem)
dotnet test

# Publicar release
dotnet publish -c Release
```

### Dependências

- **[Polly](https://github.com/App-vNext/Polly)** (v8.4.0): Resilience e retry policies
- **[System.CommandLine](https://github.com/dotnet/command-line-api)** (v2.0.0-beta4): CLI parsing
- **[System.ServiceProcess.ServiceController](https://www.nuget.org/packages/System.ServiceProcess.ServiceController)** (v8.0.0): Gestão de serviços Windows

### Padrões de Código

- ✅ Async/await para operações I/O
- ✅ Using statements para IDisposable
- ✅ XML documentation em APIs públicas
- ✅ Records para DTOs imutáveis
- ✅ Nullable reference types habilitado

---

## 🗺️ Roadmap

- [ ] Suporte a múltiplos túneis simultâneos
- [ ] Interface web para gestão visual
- [ ] Configuração via arquivo YAML/JSON
- [ ] Logs estruturados (JSON) para integração
- [ ] Testes automatizados (unit + integration)
- [ ] Docker container pronto para uso
- [ ] CI/CD com GitHub Actions
- [ ] Publicação no NuGet (CfTunnel.Core)

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. 🍴 Fazer fork do projeto
2. 🌟 Criar uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. ✅ Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. 📤 Push para a branch (`git push origin feature/MinhaFeature`)
5. 🔀 Abrir um Pull Request

### Reportar Bugs

Abra uma [issue](https://github.com/paulobarbassa/cf-tunnel-kit/issues) com:
- Descrição do problema
- Passos para reproduzir
- Comportamento esperado vs atual
- Logs de erro (se houver)

---

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 🙏 Agradecimentos

- [Cloudflare](https://www.cloudflare.com/) pelo serviço de túneis gratuito
- [Polly](https://github.com/App-vNext/Polly) pela biblioteca de resilience
- Comunidade .NET pelo suporte e ferramentas

---

<div align="center">

**Desenvolvido com ❤️ por [Paulo Barbassa](https://github.com/paulobarbassa)**

⭐ Se este projeto foi útil, considere dar uma estrela!

</div>
