# Script para actualizar el modelo de Miembros y corregir la codificación

Write-Host "🔧 Paso 1: Limpiando y corrigiendo collation en SQL Server..." -ForegroundColor Cyan
sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Collation corregida exitosamente" -ForegroundColor Green
    
    Write-Host "`n🔧 Paso 2: Creando nueva migración..." -ForegroundColor Cyan
    Set-Location src\Server
    dotnet ef migrations add UpdateMiembroModelWithUTF8Support
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migración creada exitosamente" -ForegroundColor Green
        
        Write-Host "`n🔧 Paso 3: Aplicando migración..." -ForegroundColor Cyan
        dotnet ef database update
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Base de datos actualizada exitosamente" -ForegroundColor Green
            Write-Host "`n✅ ¡Todo listo! Los miembros se cargarán automáticamente al iniciar la aplicación" -ForegroundColor Green
        } else {
            Write-Host "❌ Error al aplicar migración" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Error al crear migración" -ForegroundColor Red
    }
    
    Set-Location ..\..
} else {
    Write-Host "❌ Error al ejecutar script de collation" -ForegroundColor Red
}
