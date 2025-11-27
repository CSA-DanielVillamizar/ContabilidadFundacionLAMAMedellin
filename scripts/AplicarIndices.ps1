# Script para aplicar migración de índices de performance
# Ejecutar en ventana de mantenimiento programado

param(
    [switch]$Produccion,
    [switch]$Desarrollo
)

Write-Host "🗄️ APLICACIÓN DE ÍNDICES SQL - LAMA Medellín" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$serverPath = Join-Path $PSScriptRoot "..\src\Server"

if (-not (Test-Path $serverPath)) {
    Write-Host "❌ Error: No se encuentra el directorio del servidor" -ForegroundColor Red
    Write-Host "   Ruta esperada: $serverPath" -ForegroundColor Yellow
    exit 1
}

Set-Location $serverPath

# Verificar que existe la migración
$migracionPath = Join-Path $serverPath "Migrations\20251112212910_PerformanceIndexes.cs"
if (-not (Test-Path $migracionPath)) {
    Write-Host "❌ Error: No se encuentra la migración PerformanceIndexes" -ForegroundColor Red
    Write-Host "   Ejecuta primero: dotnet ef migrations add PerformanceIndexes" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Migración encontrada: PerformanceIndexes" -ForegroundColor Green
Write-Host ""

# Mostrar índices que se van a crear
Write-Host "📋 Índices que se aplicarán:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Recibos:" -ForegroundColor White
Write-Host "    • IX_Recibos_FechaEmision"
Write-Host "    • IX_Recibos_Estado"
Write-Host "    • IX_Recibos_FechaEmision_Estado (compuesto)"
Write-Host ""
Write-Host "  Egresos:" -ForegroundColor White
Write-Host "    • IX_Egresos_Fecha"
Write-Host "    • IX_Egresos_Categoria"
Write-Host "    • IX_Egresos_Fecha_Categoria (compuesto)"
Write-Host ""
Write-Host "  Miembros:" -ForegroundColor White
Write-Host "    • IX_Miembros_NumeroIdentificacion"
Write-Host "    • IX_Miembros_Estado"
Write-Host ""
Write-Host "  Ventas:" -ForegroundColor White
Write-Host "    • IX_Ventas_Estado"
Write-Host "    • IX_Ventas_FechaVenta"
Write-Host ""
Write-Host "  Compras:" -ForegroundColor White
Write-Host "    • IX_Compras_Estado"
Write-Host "    • IX_Compras_FechaCompra"
Write-Host ""
Write-Host "  Productos e Inventario:" -ForegroundColor White
Write-Host "    • IX_Productos_Sku"
Write-Host "    • IX_MovimientosInventario_Tipo"
Write-Host "    • IX_MovimientosInventario_FechaMovimiento"
Write-Host ""

# Advertencia para producción
if ($Produccion) {
    Write-Host "⚠️  ADVERTENCIA: Aplicando en PRODUCCIÓN" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Esto creará índices en la base de datos de producción." -ForegroundColor Yellow
    Write-Host "   El proceso puede tardar varios minutos dependiendo del volumen de datos." -ForegroundColor Yellow
    Write-Host ""
    
    $confirmacion = Read-Host "¿Deseas continuar? (escribe 'SI' para confirmar)"
    
    if ($confirmacion -ne "SI") {
        Write-Host "❌ Operación cancelada" -ForegroundColor Red
        exit 0
    }
    
    Write-Host ""
    Write-Host "🚀 Aplicando migración en PRODUCCIÓN..." -ForegroundColor Cyan
    
    # Backup recomendado
    Write-Host "💡 RECOMENDACIÓN: Asegúrate de tener un backup reciente de la base de datos" -ForegroundColor Yellow
    $backupConfirm = Read-Host "¿Tienes un backup reciente? (S/N)"
    
    if ($backupConfirm -ne "S") {
        Write-Host "⚠️  Por favor crea un backup antes de continuar" -ForegroundColor Yellow
        exit 0
    }
    
    try {
        dotnet ef database update --verbose
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Migración aplicada exitosamente en PRODUCCIÓN" -ForegroundColor Green
            Write-Host ""
            Write-Host "📊 Próximos pasos:" -ForegroundColor Cyan
            Write-Host "   1. Ejecuta el script de validación: .\ValidarPerformance.ps1" -ForegroundColor White
            Write-Host "   2. Monitorea SQL Server para verificar uso de índices" -ForegroundColor White
            Write-Host "   3. Compara tiempos de respuesta antes/después" -ForegroundColor White
        }
        else {
            Write-Host ""
            Write-Host "❌ Error al aplicar migración" -ForegroundColor Red
            Write-Host "   Revisa los logs de Entity Framework arriba" -ForegroundColor Yellow
            exit 1
        }
    }
    catch {
        Write-Host ""
        Write-Host "❌ Error inesperado: $_" -ForegroundColor Red
        exit 1
    }
}
elseif ($Desarrollo) {
    Write-Host "🔧 Aplicando en ambiente de DESARROLLO..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        dotnet ef database update --verbose
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Migración aplicada exitosamente en DESARROLLO" -ForegroundColor Green
            Write-Host ""
            Write-Host "📊 Próximos pasos:" -ForegroundColor Cyan
            Write-Host "   1. Ejecuta el script de validación: .\ValidarPerformance.ps1" -ForegroundColor White
            Write-Host "   2. Prueba los módulos principales de la aplicación" -ForegroundColor White
            Write-Host "   3. Si todo funciona OK, programa aplicación en PRODUCCIÓN" -ForegroundColor White
        }
        else {
            Write-Host ""
            Write-Host "❌ Error al aplicar migración" -ForegroundColor Red
            exit 1
        }
    }
    catch {
        Write-Host ""
        Write-Host "❌ Error inesperado: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "❌ Error: Debes especificar el ambiente" -ForegroundColor Red
    Write-Host ""
    Write-Host "Uso:" -ForegroundColor Yellow
    Write-Host "  .\AplicarIndices.ps1 -Desarrollo    # Aplica en ambiente de desarrollo" -ForegroundColor White
    Write-Host "  .\AplicarIndices.ps1 -Produccion    # Aplica en producción (con confirmación)" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""
