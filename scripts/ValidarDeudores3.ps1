# Validacion simplificada
Write-Host 'SQL: 26 deudores' -ForegroundColor Cyan
Write-Host 'Validando endpoint...' -ForegroundColor Yellow

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-WebRequest -Uri 'http://localhost:5000/Identity/Account/Login' -SessionVariable s -UseBasicParsing
$token = ($login.InputFields | Where-Object { $_.name -eq '__RequestVerificationToken' }).value

$body = @{
    'Input.Email' = 'admin@lamamed.org'
    'Input.Password' = 'Admin123!'
    'Input.RememberMe' = 'false'
    '__RequestVerificationToken' = $token
}

try {
    Invoke-WebRequest -Uri 'http://localhost:5000/Identity/Account/Login' -Method POST -Body $body -WebSession $s -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue | Out-Null
} catch {}

$result = Invoke-RestMethod -Uri 'http://localhost:5000/api/deudores' -Method GET -WebSession $s
$count = ($result | Measure-Object).Count

Write-Host \"APP: $count deudores\" -ForegroundColor Cyan

if ($count -eq 26) {
    Write-Host 'VALIDACION EXITOSA!' -ForegroundColor Green
    $result | Select-Object -First 5 | ForEach-Object {
        Write-Host \"  $($_.Nombre): $($_.MesesPendientes.Count) meses\"
    }
} else {
    Write-Host 'DISCREPANCIA!' -ForegroundColor Red
}
