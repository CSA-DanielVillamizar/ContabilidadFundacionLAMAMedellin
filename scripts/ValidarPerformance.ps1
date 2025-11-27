# Script de Validación de Performance
# Mide tiempos de respuesta de endpoints clave antes/después de optimizaciones

param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$Iteraciones = 10,
    [switch]$UseDevMirror
)

Write-Host "VALIDACION DE PERFORMANCE - LAMA Medellin" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Función para medir tiempo de endpoint
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Nombre
    )
    
    $tiempos = @()
    
    Write-Host "Probando: $Nombre" -ForegroundColor Yellow
    Write-Host "   URL: $Url"
    
    for ($i = 1; $i -le $Iteraciones; $i++) {
        try {
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            $response = Invoke-WebRequest -Uri $Url -Method GET -UseBasicParsing -ErrorAction Stop
            $stopwatch.Stop()
            
            $tiempos += $stopwatch.ElapsedMilliseconds
            
            $contentLength = $response.Content.Length
            $compressed = $response.Headers["Content-Encoding"]
            
            if ($i -eq 1) {
                Write-Host "   Primera petición: $($stopwatch.ElapsedMilliseconds)ms | Tamaño: $contentLength bytes" -ForegroundColor Gray
                if ($compressed) {
                    Write-Host "   Compresion activa: $compressed" -ForegroundColor Green
                }
            }
        }
        catch {
            Write-Host "   Error en iteracion $i : $_" -ForegroundColor Red
        }
        
        # Pequeña pausa entre requests
        Start-Sleep -Milliseconds 100
    }
    
    if ($tiempos.Count -gt 0) {
        $promedio = ($tiempos | Measure-Object -Average).Average
        $minimo = ($tiempos | Measure-Object -Minimum).Minimum
        $maximo = ($tiempos | Measure-Object -Maximum).Maximum
        
    Write-Host "   Resultados ($($tiempos.Count) peticiones):" -ForegroundColor Green
        Write-Host "      Promedio: $([math]::Round($promedio, 2))ms"
        Write-Host "      Mínimo: $minimo ms"
        Write-Host "      Máximo: $maximo ms"
        
        return [pscustomobject]@{
            Nombre = $Nombre
            Promedio = [math]::Round($promedio, 2)
            Minimo = $minimo
            Maximo = $maximo
        }
    }
    
    Write-Host ""
    return $null
}

# Verificar que el servidor esté corriendo
try {
    $healthCheck = Invoke-WebRequest -Uri "$BaseUrl" -Method GET -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Servidor alcanzable en $BaseUrl" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "❌ Error: No se puede conectar al servidor en $BaseUrl" -ForegroundColor Red
    Write-Host "   Asegúrate de que la aplicación esté corriendo." -ForegroundColor Yellow
    Write-Host "   Usa: dotnet run --project src/Server" -ForegroundColor Yellow
    exit 1
}

# Selector de endpoint (permite usar mirrors públicos en Development)
function Build-Url {
    param(
        [string]$Relative
    )
    if ($UseDevMirror) {
        return "$BaseUrl/dev$Relative"
    }
    return "$BaseUrl$Relative"
}

# Array para almacenar resultados
$resultados = @()

# Endpoints a probar (ajusta según tus rutas reales)
Write-Host "Probando endpoints optimizados..." -ForegroundColor Cyan
Write-Host ""

$resultados += Test-Endpoint -Url (Build-Url "/api/conceptos") -Nombre "Conceptos (con cache)"
$resultados += Test-Endpoint -Url (Build-Url "/api/conceptos/simples") -Nombre "Conceptos Simples (con cache)"
$resultados += Test-Endpoint -Url (Build-Url "/api/productos") -Nombre "Productos - Todos (con cache)"
$resultados += Test-Endpoint -Url (Build-Url "/api/productos/activos") -Nombre "Productos - Activos (con cache)"
$resultados += Test-Endpoint -Url (Build-Url "/api/productos/bajo-stock") -Nombre "Productos - Bajo Stock"

# Resumen final
Write-Host "" 
Write-Host ("".PadLeft(60,'=')) -ForegroundColor Cyan
Write-Host "RESUMEN DE RESULTADOS" -ForegroundColor Cyan
Write-Host ("".PadLeft(60,'=')) -ForegroundColor Cyan
Write-Host ""

$resultados | Where-Object { $_ -ne $null } | ForEach-Object {
    Write-Host "  $($_.Nombre)"
    Write-Host "    Promedio: $($_.Promedio)ms | Mín: $($_.Minimo)ms | Máx: $($_.Maximo)ms"
    Write-Host ""
}

# Métricas agregadas
$promedioGeneral = ((($resultados | Where-Object { $_ -ne $null }) | ForEach-Object { $_.Promedio }) | Measure-Object -Average).Average
Write-Host "Tiempo promedio general: $([math]::Round($promedioGeneral, 2))ms" -ForegroundColor Green
Write-Host ""

# Recomendaciones
Write-Host "RECOMENDACIONES:" -ForegroundColor Yellow
Write-Host ""

if ($promedioGeneral -lt 100) {
    Write-Host "  Excelente performance (< 100ms)" -ForegroundColor Green
}
elseif ($promedioGeneral -lt 200) {
    Write-Host "  Buena performance (< 200ms)" -ForegroundColor Green
}
elseif ($promedioGeneral -lt 500) {
    Write-Host "  Performance aceptable (< 500ms)" -ForegroundColor Yellow
    Write-Host "     Considera aplicar los índices SQL si aún no lo has hecho" -ForegroundColor Yellow
}
else {
    Write-Host "  Performance mejorable (> 500ms)" -ForegroundColor Red
    Write-Host "     1. Aplica la migración de índices: dotnet ef database update" -ForegroundColor Yellow
    Write-Host "     2. Verifica que Response Compression esté activo" -ForegroundColor Yellow
    Write-Host "     3. Revisa logs de SQL Server para queries lentas" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Para mas detalles, consulta: OPTIMIZACIONES_PERFORMANCE.md" -ForegroundColor Cyan
Write-Host ""
