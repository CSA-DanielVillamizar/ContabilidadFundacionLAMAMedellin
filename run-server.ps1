# Script para mantener el servidor Blazor activo
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5179"

Write-Host "🚀 Iniciando servidor Blazor en http://localhost:5179" -ForegroundColor Green
Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Yellow
Write-Host ""

Set-Location -Path $PSScriptRoot
dotnet run --project src\Server\Server.csproj

# Si el servidor se cierra, esperar antes de salir
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ El servidor se cerró con error. Código: $LASTEXITCODE" -ForegroundColor Red
    Write-Host "Revisa los logs arriba para más detalles." -ForegroundColor Yellow
    pause
}
