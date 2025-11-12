@echo off
REM ====================================================================
REM Script de Exemplo - CF Tunnel Kit
REM ====================================================================
REM Este script demonstra como usar o CF Tunnel Kit para provisionar
REM um túnel Cloudflare que expõe uma aplicação local para a internet.
REM ====================================================================

REM --------------------------------------------------------------------
REM 1. CONFIGURAR CREDENCIAIS CLOUDFLARE
REM --------------------------------------------------------------------
REM Obtenha estas credenciais no painel Cloudflare:
REM - Account ID: My Account > Account ID
REM - Zone ID: Seu domínio > Overview > Zone ID (lado direito)
REM - API Token: My Profile > API Tokens > Create Token
REM   Permissões necessárias:
REM     * Account > Cloudflare Tunnel > Edit
REM     * Zone > DNS > Edit

set CF_API_TOKEN=RV9OjKNAS2_PRM02Glm6KWEVvhh1pASnvBJ3WDil
set CF_ACCOUNT_ID=10cea26fba8337c073187c2f02b2e042
set CF_ZONE_ID=7eb5982b680055b30e51bd2b29af3a7c

REM --------------------------------------------------------------------
REM 2. CONFIGURAR PARÂMETROS DO TÚNEL
REM --------------------------------------------------------------------
REM - hostname: O domínio público que você quer usar (ex: app.seudominio.com)
REM - origin: O endereço local da sua aplicação (ex: http://localhost:3000)

set HOSTNAME=app.seudominio.com
set ORIGIN=http://localhost:8080

REM --------------------------------------------------------------------
REM 3. OPÇÕES AVANÇADAS (Opcional)
REM --------------------------------------------------------------------
REM Estas opções têm valores padrão, mas podem ser personalizadas:

REM Prefixo do nome do túnel (padrão: tunnel-)
set TUNNEL_PREFIX=meu-app-

REM DNS proxied (laranja) - true ou false (padrão: true)
set PROXIED=true

REM TTL do registro DNS em segundos ou "auto" (padrão: auto)
set TTL=auto

REM Regra de fallback do ingress (padrão: http_status:404)
set FALLBACK=http_status:404

REM --------------------------------------------------------------------
REM 4. EXECUTAR O PROVISIONAMENTO
REM --------------------------------------------------------------------
REM Navegue até o diretório do projeto Console e execute

cd /d "%~dp0src\CfTunnel.Console"

echo.
echo ====================================================================
echo Provisionando Cloudflare Tunnel
echo ====================================================================
echo Hostname: %HOSTNAME%
echo Origin: %ORIGIN%
echo ====================================================================
echo.

REM Executar sem instalação de serviço Windows (use --skip-service)
REM Remove --skip-service se quiser instalar como serviço (requer Admin)
dotnet run -- ^
  --account-id "%CF_ACCOUNT_ID%" ^
  --zone-id "%CF_ZONE_ID%" ^
  --hostname "%HOSTNAME%" ^
  --origin "%ORIGIN%" ^
  --proxied %PROXIED% ^
  --ttl "%TTL%" ^
  --tunnel-prefix "%TUNNEL_PREFIX%" ^
  --fallback "%FALLBACK%" ^
  --skip-service

REM --------------------------------------------------------------------
REM 5. VERIFICAR RESULTADO
REM --------------------------------------------------------------------
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ====================================================================
    echo SUCESSO! Tunel criado e configurado.
    echo ====================================================================
    echo Agora voce pode acessar: https://%HOSTNAME%
    echo A aplicacao local %ORIGIN% estara acessivel publicamente.
    echo ====================================================================
) else (
    echo.
    echo ====================================================================
    echo ERRO! Falha ao provisionar o tunel.
    echo ====================================================================
    echo Verifique:
    echo - Se o CF_API_TOKEN esta correto e tem as permissoes necessarias
    echo - Se o CF_ACCOUNT_ID e CF_ZONE_ID estao corretos
    echo - Se o hostname pertence a zona DNS configurada
    echo ====================================================================
)

echo.
pause
