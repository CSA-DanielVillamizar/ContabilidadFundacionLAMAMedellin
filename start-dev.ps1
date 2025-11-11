<#
 Script para iniciar la aplicación en modo Development y capturar el log.
 Simplificado para evitar problemas de parseo con comillas y backticks.
#>

$ErrorActionPreference = 'Continue'

Write-Host '========================================' -ForegroundColor Cyan
Write-Host 'Iniciando aplicación en modo Development' -ForegroundColor Cyan
Write-Host '========================================' -ForegroundColor Cyan

# Configurar variables de entorno
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_DETAILEDERRORS = 'true'
$env:Logging__LogLevel__Default = 'Debug'
$env:Logging__LogLevel__Microsoft = 'Information'

Write-Host ("Directorio actual: {0}" -f (Get-Location)) -ForegroundColor Yellow
Write-Host 'Ejecutando: dotnet run --project .\src\Server\Server.csproj' -ForegroundColor Yellow

# Ejecutar la aplicación capturando todo el output en archivo
try {
    # Redirigir toda la salida (stdout + stderr) a un archivo y a la consola
    dotnet run --project .\src\Server\Server.csproj *> .\app-log.txt
} catch {
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'ERROR AL INICIAR LA APLICACIÓN' -ForegroundColor Red
    Write-Host '========================================' -ForegroundColor Red
    Write-Host ($_.Exception.Message) -ForegroundColor Red
    Write-Host ($_.ScriptStackTrace) -ForegroundColor Red
}

Write-Host '========================================' -ForegroundColor Cyan
Write-Host 'La aplicación ha terminado' -ForegroundColor Cyan
Write-Host 'Revisa app-log.txt para ver el log completo' -ForegroundColor Cyan
Write-Host '========================================' -ForegroundColor Cyan
