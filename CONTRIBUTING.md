# 🤝 Guia de Contribuição

Obrigado por considerar contribuir para o cf-tunnel-kit! Este documento fornece diretrizes para contribuições.

## 📋 Índice

- [Código de Conduta](#código-de-conduta)
- [Como Posso Contribuir?](#como-posso-contribuir)
- [Configuração do Ambiente de Desenvolvimento](#configuração-do-ambiente-de-desenvolvimento)
- [Processo de Submissão](#processo-de-submissão)
- [Guias de Estilo](#guias-de-estilo)

## 📜 Código de Conduta

Este projeto e todos os participantes dele são regidos por um código de conduta. Ao participar, espera-se que você mantenha este código. Por favor, reporte comportamentos inaceitáveis.

## 🎯 Como Posso Contribuir?

### Reportando Bugs

Antes de criar um bug report:
- **Verifique a documentação** para ter certeza de que não é um mal-entendido
- **Procure por issues existentes** para ver se o bug já foi reportado

Quando criar um bug report:
- Use o template de bug report
- Inclua o máximo de detalhes possível
- Adicione logs e mensagens de erro
- Descreva os passos para reproduzir

### Sugerindo Melhorias

Para sugerir novas funcionalidades:
- Use o template de feature request
- Explique por que esta funcionalidade seria útil
- Forneça exemplos de uso

### Pull Requests

1. Fork o repositório
2. Crie uma branch a partir de `main`: `git checkout -b feature/minha-funcionalidade`
3. Faça suas mudanças
4. Commit suas mudanças: `git commit -m 'feat: adiciona nova funcionalidade'`
5. Push para a branch: `git push origin feature/minha-funcionalidade`
6. Abra um Pull Request

## 🛠️ Configuração do Ambiente de Desenvolvimento

### Pré-requisitos

- .NET 8.0 SDK ou superior
- Git
- Visual Studio 2022, VS Code, ou Rider

### Configuração

```bash
# Clone o repositório
git clone https://github.com/paulobarbassa/cf-tunnel-kit.git
cd cf-tunnel-kit

# Restaure as dependências
dotnet restore

# Build o projeto
dotnet build

# Execute os testes
dotnet test
```

## 📝 Processo de Submissão

### Convenção de Commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>(<escopo>): <descrição>

[corpo opcional]

[rodapé opcional]
```

**Tipos:**
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Mudanças na documentação
- `style`: Formatação, falta de ponto e vírgula, etc
- `refactor`: Refatoração de código
- `perf`: Melhorias de performance
- `test`: Adição ou correção de testes
- `chore`: Manutenção, tarefas de build, etc

**Exemplos:**
```bash
feat(api): adiciona suporte para múltiplos túneis
fix(console): corrige erro ao processar argumentos
docs(readme): atualiza instruções de instalação
```

### Branches

- `main`: Branch principal, código em produção
- `develop`: Branch de desenvolvimento
- `feature/`: Novas funcionalidades
- `fix/`: Correções de bugs
- `docs/`: Documentação
- `refactor/`: Refatorações

### Code Review

Todos os PRs passam por code review:
- Mantenha PRs focados e com escopo limitado
- Responda aos comentários de forma construtiva
- Faça updates baseados no feedback

## 🎨 Guias de Estilo

### Código C#

- Siga as [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use nomes descritivos para variáveis e métodos
- Adicione comentários XML para APIs públicas
- Mantenha métodos pequenos e focados

```csharp
/// <summary>
/// Cria um novo túnel Cloudflare
/// </summary>
/// <param name="name">Nome do túnel</param>
/// <returns>Resultado do provisionamento</returns>
public async Task<TunnelProvisionResult> CreateTunnelAsync(string name)
{
    // Implementação
}
```

### Documentação

- Use Markdown para documentação
- Mantenha a documentação atualizada com o código
- Adicione exemplos quando possível
- Use linguagem clara e concisa

## 🧪 Testes

- Escreva testes para novas funcionalidades
- Mantenha cobertura de testes alta
- Testes devem ser rápidos e determinísticos

```bash
# Executar testes
dotnet test

# Executar testes com cobertura
dotnet test /p:CollectCoverage=true
```

## 📦 Versionamento

Usamos [Semantic Versioning](https://semver.org/):
- MAJOR: Mudanças incompatíveis na API
- MINOR: Funcionalidades adicionadas de forma compatível
- PATCH: Correções de bugs compatíveis

## ❓ Dúvidas?

Se tiver dúvidas:
- Abra uma [issue](https://github.com/paulobarbassa/cf-tunnel-kit/issues)
- Entre em contato com os mantenedores

## 📄 Licença

Ao contribuir, você concorda que suas contribuições serão licenciadas sob a mesma licença do projeto.

---

**Obrigado por contribuir! 🎉**
