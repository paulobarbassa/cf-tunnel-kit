# 🚀 Guia de Releases no GitHub

Este documento explica como gerar releases e usar os recursos do GitHub para o projeto cf-tunnel-kit.

## 📋 Índice

- [Gerando Releases](#-gerando-releases)
- [GitHub Actions Configurados](#-github-actions-configurados)
- [Issues e Pull Requests](#-issues-e-pull-requests)
- [Recursos Adicionais do GitHub](#-recursos-adicionais-do-github)

---

## 🎯 Gerando Releases

### Método Automático (Recomendado)

O projeto está configurado com GitHub Actions para gerar releases automaticamente quando você criar uma tag.

#### Passo 1: Preparar o código

```bash
# Certifique-se de que está na branch main e atualizado
git checkout main
git pull origin main
```

#### Passo 2: Criar uma tag

Use [Semantic Versioning](https://semver.org/) (vX.Y.Z):

```bash
# Exemplo: versão 1.0.0
git tag -a v1.0.0 -m "Release v1.0.0 - Versão inicial"

# Verificar a tag
git tag -l
```

#### Passo 3: Fazer push da tag

```bash
git push origin v1.0.0
```

#### Passo 4: Aguardar o build

O GitHub Actions irá automaticamente:

1. ✅ Compilar o projeto para múltiplas plataformas
2. ✅ Criar arquivos ZIP para cada plataforma:
   - Windows x64
   - Linux x64
   - macOS x64 (Intel)
   - macOS ARM64 (Apple Silicon)
3. ✅ Criar a release no GitHub
4. ✅ Fazer upload dos binários
5. ✅ Gerar release notes automaticamente

Você pode acompanhar o progresso em:
`https://github.com/paulobarbassa/cf-tunnel-kit/actions`

### Método Manual

Se preferir criar releases manualmente:

1. Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/releases/new`

2. Preencha:
   - **Tag version**: `v1.0.0` (crie uma nova tag)
   - **Release title**: `Release v1.0.0`
   - **Description**: Descrição das mudanças
   
3. Anexe os binários compilados:
   ```bash
   # Compilar para Windows
   dotnet publish src/CfTunnel.Console/CfTunnel.Console.csproj -c Release -o release/win-x64 -r win-x64 --self-contained false
   
   # Compilar para Linux
   dotnet publish src/CfTunnel.Console/CfTunnel.Console.csproj -c Release -o release/linux-x64 -r linux-x64 --self-contained false
   
   # Criar ZIPs
   # Windows: Compress-Archive
   # Linux/Mac: zip -r cf-tunnel-win-x64.zip release/win-x64
   ```

4. Clique em **Publish release**

---

## ⚙️ GitHub Actions Configurados

### 1. Build and Test (`.github/workflows/build.yml`)

**Quando executa:**
- Push para `main` ou `develop`
- Pull requests para `main` ou `develop`

**O que faz:**
- ✅ Testa em Windows, Linux e macOS
- ✅ Compila o projeto
- ✅ Executa testes (se existirem)
- ✅ Faz upload de artifacts

### 2. Release (`.github/workflows/release.yml`)

**Quando executa:**
- Quando você cria uma tag `v*` (ex: v1.0.0)

**O que faz:**
- ✅ Compila para múltiplas plataformas
- ✅ Cria releases automaticamente
- ✅ Faz upload de binários
- ✅ Gera release notes

### Visualizar Status dos Workflows

Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/actions`

---

## 🐛 Issues e Pull Requests

### Criar Issues

O projeto tem templates configurados para facilitar a criação de issues:

#### Bug Report

1. Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/issues/new/choose`
2. Selecione **🐛 Bug Report**
3. Preencha o formulário:
   - Descrição do bug
   - Passos para reproduzir
   - Comportamento esperado
   - Sistema operacional
   - Versão do projeto
   - Logs

#### Feature Request

1. Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/issues/new/choose`
2. Selecione **✨ Feature Request**
3. Preencha o formulário:
   - Problema relacionado
   - Solução proposta
   - Alternativas consideradas
   - Prioridade

### Criar Pull Requests

O projeto tem um template automático para PRs:

1. Crie uma branch:
   ```bash
   git checkout -b feature/minha-funcionalidade
   ```

2. Faça suas mudanças e commit:
   ```bash
   git add .
   git commit -m "feat: adiciona nova funcionalidade"
   ```

3. Faça push:
   ```bash
   git push origin feature/minha-funcionalidade
   ```

4. Abra o PR no GitHub - o template será carregado automaticamente

5. Preencha:
   - ✅ Descrição das mudanças
   - ✅ Tipo de mudança
   - ✅ Issue relacionada
   - ✅ Checklist
   - ✅ Como testar

---

## 🎨 Recursos Adicionais do GitHub

### 1. GitHub Projects

Organize tarefas e planejamento:

1. Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/projects`
2. Clique em **New project**
3. Escolha um template (Board, Table, Roadmap)
4. Adicione issues e PRs ao projeto

**Sugestão de colunas:**
- 📋 Backlog
- 🔜 To Do
- 🏗️ In Progress
- 👀 In Review
- ✅ Done

### 2. GitHub Discussions

Crie um espaço para conversas da comunidade:

1. Acesse: Settings → Features → ✅ Discussions
2. Configure categorias:
   - 💡 Ideas (Ideias)
   - 🙏 Q&A (Perguntas)
   - 📣 Announcements (Anúncios)
   - 🎉 Show and tell (Mostre seu trabalho)

### 3. GitHub Wiki

Documente o projeto em detalhes:

1. Acesse: `https://github.com/paulobarbassa/cf-tunnel-kit/wiki`
2. Crie páginas:
   - Guia de instalação detalhado
   - Tutoriais
   - Troubleshooting
   - FAQ
   - Arquitetura do sistema

### 4. GitHub Pages

Hospede documentação estática:

1. Crie uma branch `gh-pages`:
   ```bash
   git checkout --orphan gh-pages
   ```

2. Adicione arquivos HTML/CSS/JS

3. Em Settings → Pages:
   - Source: `gh-pages` branch
   - Acesse em: `https://paulobarbassa.github.io/cf-tunnel-kit`

### 5. Branch Protection Rules

Proteja a branch main:

1. Acesse: Settings → Branches → Add rule
2. Configure:
   - ✅ Require pull request reviews
   - ✅ Require status checks (CI) to pass
   - ✅ Require branches to be up to date
   - ✅ Include administrators

### 6. Code Scanning (Security)

Ative análise de segurança automática:

1. Acesse: Security → Code scanning → Configure
2. Escolha **GitHub Actions**
3. O GitHub criará um workflow de análise

### 7. Dependabot

Mantenha dependências atualizadas:

Crie `.github/dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
```

### 8. Secrets para CI/CD

Armazene credenciais com segurança:

1. Acesse: Settings → Secrets and variables → Actions
2. Adicione secrets (ex: `CF_API_TOKEN`)
3. Use nos workflows:
   ```yaml
   env:
     CF_API_TOKEN: ${{ secrets.CF_API_TOKEN }}
   ```

### 9. Releases Notes Automáticas

Configure categorias de PR:

Crie `.github/release.yml`:

```yaml
changelog:
  categories:
    - title: 🚀 Features
      labels:
        - enhancement
        - feature
    - title: 🐛 Bug Fixes
      labels:
        - bug
        - fix
    - title: 📚 Documentation
      labels:
        - documentation
    - title: 🔧 Maintenance
      labels:
        - chore
        - dependencies
```

### 10. Social Preview

Configure a imagem de preview:

1. Crie uma imagem 1280x640px
2. Acesse: Settings → Social preview
3. Faça upload da imagem

---

## 📝 Exemplo de Fluxo de Trabalho Completo

### Lançar uma nova versão

```bash
# 1. Certifique-se de estar atualizado
git checkout main
git pull origin main

# 2. Verifique se tudo está funcionando
dotnet build
dotnet test

# 3. Atualize o CHANGELOG.md (se existir)
# Adicione as mudanças da versão

# 4. Commit as mudanças
git add .
git commit -m "chore: prepare release v1.1.0"
git push origin main

# 5. Crie e envie a tag
git tag -a v1.1.0 -m "Release v1.1.0

- Adiciona suporte para múltiplos túneis
- Corrige bug no DNS
- Melhora performance"

git push origin v1.1.0

# 6. Aguarde o GitHub Actions gerar a release
# Acesse: https://github.com/paulobarbassa/cf-tunnel-kit/actions

# 7. Quando concluído, verifique a release
# https://github.com/paulobarbassa/cf-tunnel-kit/releases
```

### Fazer uma correção rápida

```bash
# 1. Crie uma branch de hotfix
git checkout -b hotfix/corrige-bug-critico

# 2. Faça a correção
# Edite os arquivos necessários

# 3. Commit
git add .
git commit -m "fix: corrige bug crítico no DNS"

# 4. Push
git push origin hotfix/corrige-bug-critico

# 5. Abra um PR no GitHub
# O template será carregado automaticamente

# 6. Após aprovação e merge, crie uma nova tag
git checkout main
git pull origin main
git tag -a v1.0.1 -m "Hotfix v1.0.1 - Corrige bug crítico"
git push origin v1.0.1
```

---

## 🎓 Recursos de Aprendizado

### Documentação Oficial

- [GitHub Actions](https://docs.github.com/en/actions)
- [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)
- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)

### Tutoriais Recomendados

- [GitHub Learning Lab](https://lab.github.com/)
- [GitHub Actions Quickstart](https://docs.github.com/en/actions/quickstart)
- [Publishing to GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)

---

## ❓ Dúvidas Comuns

### Como cancelar uma release?

1. Acesse a release
2. Clique em **Delete**
3. Delete a tag:
   ```bash
   git tag -d v1.0.0
   git push origin :refs/tags/v1.0.0
   ```

### Como editar uma release existente?

1. Acesse: Releases → Selecione a release
2. Clique em **Edit release**
3. Faça as alterações
4. Clique em **Update release**

### Como fazer uma pre-release?

```bash
# Use uma tag com sufixo
git tag -a v1.0.0-beta.1 -m "Beta release"
git push origin v1.0.0-beta.1
```

Marque como **Pre-release** na página de criação.

### Como ver quem baixou as releases?

Acesse: Insights → Traffic → Releases downloads

---

**Pronto! 🎉** Agora você tem tudo configurado para usar todos os recursos do GitHub de forma profissional!
