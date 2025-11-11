#!/usr/bin/env pwsh
# Script para corregir la codificación UTF-8 de los miembros

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  CORRECCIÓN DE CODIFICACIÓN UTF-8" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Paso 1: Ejecutar script SQL para corregir collation
Write-Host "📝 Paso 1/4: Corrigiendo collation en SQL Server..." -ForegroundColor Yellow
$sqlResult = & sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Collation corregida" -ForegroundColor Green
} else {
    Write-Host "   ❌ Error en SQL: $sqlResult" -ForegroundColor Red
    Write-Host "`n⚠️  Intenta ejecutar manualmente:" -ForegroundColor Yellow
    Write-Host "   sqlcmd -S localhost -d LamaMedellin -E -i `"src\Server\Scripts\FixCollation.sql`"`n" -ForegroundColor White
    exit 1
}

# Paso 2: Crear migración
Write-Host "`n📝 Paso 2/4: Creando migración de Entity Framework..." -ForegroundColor Yellow
Push-Location src\Server
$migrationResult = & dotnet ef migrations add UpdateMiembroModelWithUTF8Support 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Migración creada" -ForegroundColor Green
} else {
    Write-Host "   ❌ Error al crear migración: $migrationResult" -ForegroundColor Red
    Pop-Location
    exit 1
}

# Paso 3: Aplicar migración
Write-Host "`n📝 Paso 3/4: Aplicando migración a la base de datos..." -ForegroundColor Yellow
$updateResult = & dotnet ef database update 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Base de datos actualizada" -ForegroundColor Green
} else {
    Write-Host "   ❌ Error al aplicar migración: $updateResult" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

# Paso 4: Compilar y ejecutar
Write-Host "`n📝 Paso 4/4: Compilando aplicación..." -ForegroundColor Yellow
$buildResult = & dotnet build src\Server\Server.csproj --nologo 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Compilación exitosa" -ForegroundColor Green
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  ✅ CORRECCIÓN COMPLETADA" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    Write-Host "🚀 Ahora ejecuta la aplicación con:" -ForegroundColor Cyan
    Write-Host "   dotnet run --project src\Server\Server.csproj`n" -ForegroundColor White
    
    Write-Host "📊 Verás en los logs:" -ForegroundColor Cyan
    Write-Host "   ✅ Se cargaron 26 miembros desde el CSV" -ForegroundColor White
    Write-Host "   ✅ Logo copiado a: wwwroot\images\LogoLAMAMedellin.png`n" -ForegroundColor White
    
    Write-Host "🔍 Para verificar la importación:" -ForegroundColor Cyan
    Write-Host "   sqlcmd -S localhost -d LamaMedellin -E -i `"src\Server\Scripts\VerificarMiembros.sql`"`n" -ForegroundColor White
    
} else {
    Write-Host "   ❌ Error al compilar: $buildResult" -ForegroundColor Red
    exit 1
}
