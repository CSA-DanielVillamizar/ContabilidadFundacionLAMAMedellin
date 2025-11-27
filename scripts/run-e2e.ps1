param(
  [int]$Port = 5001,
  [string]$BaseUrl = "http://localhost:5001",
  [string]$Filter = "",
  [switch]$Headed
)

$ErrorActionPreference = "Stop"

Write-Host "[E2E] Preparando entorno E2E en $BaseUrl..."

# 1) Liberar puerto si esta en uso
try {
  $existing = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
  if ($existing) {
    Write-Host "[E2E] Cerrando proceso en puerto $Port (PID=$existing)"
    Stop-Process -Id $existing -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
  }
} catch { }

# 2) Iniciar Server de forma desacoplada
$serverProject = Join-Path $PSScriptRoot "..\src\Server\Server.csproj"
$logDir = Join-Path $PSScriptRoot "..\logs"
$newItem = New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$serverLog = Join-Path $logDir ("server-e2e-" + (Get-Date -Format "yyyyMMdd_HHmmss") + ".log")

Write-Host "[E2E] Compilando servidor..."
dotnet build $serverProject --nologo
Write-Host "[E2E] Iniciando servidor en $BaseUrl... (log: $serverLog)"
# Configurar entorno Testing para que cargue appsettings.Test.json con Identity + DetailedErrors + 2FA deshabilitado
$env:ASPNETCORE_ENVIRONMENT = "Testing"
$server = Start-Process -FilePath "dotnet" -ArgumentList @("run","--urls", $BaseUrl,"--project", $serverProject) -RedirectStandardOutput $serverLog -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 1
if ($server.HasExited) {
  Write-Host "[E2E] ❌ El proceso del servidor terminó inmediatamente. Log parcial:"; Get-Content $serverLog | Select-Object -First 40; throw "Fallo al iniciar servidor"
}

# 3) Esperar a que esté listo
$loginUrl = "$BaseUrl/Identity/Account/Login"
$max = 40
for ($i=1; $i -le $max; $i++) {
  try {
    $resp = Invoke-WebRequest -Uri $loginUrl -UseBasicParsing -TimeoutSec 3
    if ($resp.StatusCode -eq 200) {
      Write-Host "[E2E] Server listo tras $i intentos"
      break
    }
    Write-Host "[E2E] Intento ${i} - codigo $($resp.StatusCode)"
  } catch {
    Write-Host "[E2E] Intento ${i} - $($_.Exception.Message)"
  }
  Start-Sleep -Milliseconds (300 + ($i*100))
  if ($i -eq $max) { throw "El servidor no estuvo disponible en $loginUrl" }
}

# 4) Ejecutar pruebas E2E apuntando al server externo
$testProj = Join-Path $PSScriptRoot "..\tests\E2E\ContabilidadLAMAMedellin.Tests.E2E.csproj"
$env:E2E_BASE_URL = $BaseUrl
if ($Headed) { $env:HEADED = "1" } else { Remove-Item Env:\HEADED -ErrorAction SilentlyContinue }

$logger = 'trx;LogFileName=e2e.trx'
$filterArgs = @()
if ($Filter) { $filterArgs = @("--filter", $Filter) }

Write-Host "[E2E] Ejecutando pruebas E2E..."
try {
  # Construir y ejecutar pruebas E2E para recoger cambios recientes en tests
  dotnet test $testProj --logger:$logger @filterArgs
} finally {
  Write-Host "[E2E] Deteniendo servidor (PID=$($server.Id))..."
  try { Stop-Process -Id $server.Id -Force -ErrorAction SilentlyContinue } catch { }
  Write-Host "[E2E] Logs del servidor: $serverLog"
  Write-Host "[E2E] Videos: tests\E2E\videos\"
  Write-Host "[E2E] Traces: tests\E2E\traces\"
}
