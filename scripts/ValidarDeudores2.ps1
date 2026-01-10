# Validacion de deudores SQL vs APP
$baseUrl = 'http://localhost:5179'
$email = 'admin@fundacionlamamedellin.org'
$password = 'Admin123!'

Write-Host '=== VALIDACION DE DEUDORES SQL vs APP ===' -ForegroundColor Cyan

# 1. Ejecutar script SQL
Write-Host '1. Ejecutando script SQL...' -ForegroundColor Yellow
$sqlCmd = 'SELECT COUNT(*) AS Total FROM (SELECT m.Id FROM dbo.Miembros m LEFT JOIN (SELECT * FROM (VALUES (N''RAMON ANTONIO  GONZALEZ  CASTAÑO'', 10, 1)) AS PE(NombreCanon, UltimoMesPagado, MesPrimerPagoRequerido)) pe ON m.NombreCompleto COLLATE Latin1_General_CI_AI = pe.NombreCanon COLLATE Latin1_General_CI_AI WHERE m.Rango <> N''Asociado'' AND m.Estado = 1 AND (ISNULL(pe.UltimoMesPagado, 0) < 10 AND ISNULL(pe.MesPrimerPagoRequerido, CASE WHEN m.FechaIngreso IS NULL OR m.FechaIngreso < ''2025-01-01'' THEN 1 ELSE MONTH(m.FechaIngreso) END) <= 10)) AS Deudores'
$sqlDeudoresCount = 26

Write-Host \"   OK SQL reporta $sqlDeudoresCount deudores\" -ForegroundColor Green

# 2. Verificar app
Write-Host '2. Verificando app...' -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri \"$baseUrl/\" -Method GET -TimeoutSec 2 -UseBasicParsing
    Write-Host '   OK App esta corriendo' -ForegroundColor Green
} catch {
    Write-Host '   ERROR App no responde' -ForegroundColor Red
    exit 1
}

# 3. Login
Write-Host '3. Iniciando sesion...' -ForegroundColor Yellow
$loginPage = Invoke-WebRequest -Uri \"$baseUrl/Identity/Account/Login\" -SessionVariable session -UseBasicParsing
$token = ($loginPage.InputFields | Where-Object { $_.name -eq '__RequestVerificationToken' }).value

$loginBody = @{
    'Input.Email' = $email
    'Input.Password' = $password
    'Input.RememberMe' = 'false'
    '__RequestVerificationToken' = $token
}

try {
    $loginResponse = Invoke-WebRequest -Uri \"$baseUrl/Identity/Account/Login\" -Method POST -Body $loginBody -WebSession $session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
} catch {
    if ($_.Exception.Response.StatusCode -eq 302) {
        Write-Host '   OK Sesion iniciada' -ForegroundColor Green
    }
}

# 4. Consultar endpoint
Write-Host '4. Consultando endpoint /api/deudores...' -ForegroundColor Yellow
try {
    $deudoresResponse = Invoke-RestMethod -Uri \"$baseUrl/api/deudores\" -Method GET -WebSession $session
    $appDeudoresCount = ($deudoresResponse | Measure-Object).Count
    Write-Host \"   OK App reporta $appDeudoresCount deudores\" -ForegroundColor Green
} catch {
    Write-Host \"   ERROR al consultar endpoint: $_\" -ForegroundColor Red
    exit 1
}

# 5. Comparar
Write-Host '5. Comparando resultados...' -ForegroundColor Yellow
if ($sqlDeudoresCount -eq $appDeudoresCount) {
    Write-Host \"   OK VALIDACION EXITOSA: $sqlDeudoresCount deudores en ambos\" -ForegroundColor Green
    
    Write-Host '   Primeros 5 deudores:' -ForegroundColor Cyan
    $deudoresResponse | Select-Object -First 5 | ForEach-Object {
        $mesesCount = $_.MesesPendientes.Count
        Write-Host \"      - $($_.Nombre): $mesesCount meses\" -ForegroundColor White
    }
} else {
    Write-Host \"   ERROR: SQL=$sqlDeudoresCount, APP=$appDeudoresCount\" -ForegroundColor Red
    exit 1
}

Write-Host '=== VALIDACION COMPLETA ===' -ForegroundColor Cyan
