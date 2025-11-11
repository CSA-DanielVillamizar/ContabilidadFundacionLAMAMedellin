# Script de validación: compara deudores SQL vs App
# Compila, arranca la app, consulta endpoint con autenticación, y compara resultados

$baseUrl = "http://localhost:5179"
$email = "admin@lamamed.org"
$password = "Admin123!"

Write-Host "=== VALIDACIÓN DE DEUDORES SQL vs APP ===" -ForegroundColor Cyan

# 1. Ejecutar script SQL y capturar resultado
Write-Host "`n1. Ejecutando script SQL..." -ForegroundColor Yellow
$sqlOutput = sqlcmd -S localhost -d LamaMedellin -E -i "c:\Users\DanielVillamizar\ContabilidadLAMAMedellin\scripts\ReporteDeudoresMensualidad2025.sql" -h -1 -W
$sqlLines = $sqlOutput | Select-String -Pattern "^\s*\d+\s+" | ForEach-Object { $_.Line.Trim() }
$sqlDeudoresCount = ($sqlLines | Measure-Object).Count

Write-Host "   ✓ SQL reporta $sqlDeudoresCount deudores" -ForegroundColor Green

# 2. Verificar si la app está corriendo
Write-Host "`n2. Verificando si la app está corriendo..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/" -Method GET -TimeoutSec 2 -UseBasicParsing
    Write-Host "   ✓ App está corriendo en $baseUrl" -ForegroundColor Green
} catch {
    Write-Host "   ✗ App no responde. Iniciando..." -ForegroundColor Red
    Start-Process powershell -ArgumentList "-Command", "cd c:\Users\DanielVillamizar\ContabilidadLAMAMedellin\src\Server; dotnet run --urls $baseUrl" -WindowStyle Hidden
    Write-Host "   Esperando 50 segundos para que la app arranque..." -ForegroundColor Yellow
    Start-Sleep -Seconds 50
}

# 3. Login y obtener cookie
Write-Host "`n3. Iniciando sesión como $email..." -ForegroundColor Yellow
$loginPage = Invoke-WebRequest -Uri "$baseUrl/Identity/Account/Login" -SessionVariable session -UseBasicParsing
$token = $loginPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" } | Select-Object -ExpandProperty value

$loginBody = @{
    "Input.Email" = $email
    "Input.Password" = $password
    "Input.RememberMe" = "false"
    "__RequestVerificationToken" = $token
}

try {
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/Identity/Account/Login" -Method POST -Body $loginBody -WebSession $session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Write-Host "   OK Sesion iniciada correctamente" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 302) {
        Write-Host "   OK Sesion iniciada correctamente (redirect)" -ForegroundColor Green
    } else {
        Write-Host "   ERROR al iniciar sesion: $_" -ForegroundColor Red
        exit 1
    }
}

# 4. Consultar endpoint de deudores
Write-Host "`n4. Consultando endpoint /api/deudores..." -ForegroundColor Yellow
try {
    $deudoresResponse = Invoke-RestMethod -Uri "$baseUrl/api/deudores" -Method GET -WebSession $session
    $appDeudoresCount = ($deudoresResponse | Measure-Object).Count
    Write-Host "   OK App reporta $appDeudoresCount deudores" -ForegroundColor Green
} catch {
    Write-Host "   ERROR al consultar endpoint: $_" -ForegroundColor Red
    exit 1
}

# 5. Comparar resultados
Write-Host "`n5. Comparando resultados..." -ForegroundColor Yellow
if ($sqlDeudoresCount -eq $appDeudoresCount) {
    Write-Host "   OK VALIDACION EXITOSA: SQL y APP coinciden ($sqlDeudoresCount deudores)" -ForegroundColor Green
    
    # Mostrar algunos nombres para verificacion visual
    Write-Host "`n   Primeros 5 deudores en la app:" -ForegroundColor Cyan
    $deudoresResponse | Select-Object -First 5 | ForEach-Object {
        $mesesCount = $_.MesesPendientes.Count
        Write-Host "      - $($_.Nombre): $mesesCount meses" -ForegroundColor White
    }
} else {
    Write-Host "   X DISCREPANCIA: SQL=$sqlDeudoresCount, APP=$appDeudoresCount" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== VALIDACION COMPLETA ===" -ForegroundColor Cyan
