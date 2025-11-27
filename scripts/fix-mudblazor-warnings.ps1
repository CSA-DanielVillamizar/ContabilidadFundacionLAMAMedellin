$rootPath = "c:\Users\DanielVillamizar\ContabilidadLAMAMedellin\src\Server"

# Archivos con @bind-Open (MudDialog y MudDrawer)
$filesWithBindOpen = @(
    "Pages\Tesoreria\Egresos.razor",
    "Pages\Shared\MainLayout.razor",
    "Pages\Recibos.razor",
    "Pages\Tesoreria\RecibosForm.razor",
    "Pages\GerenciaNegocios\Inventario.razor",
    "Pages\GerenciaNegocios\Clientes.razor"
)

$totalFixed = 0

foreach ($file in $filesWithBindOpen) {
    $fullPath = Join-Path $rootPath $file
    
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw
        $newContent = $content -replace '@bind-Open=', 'Open='
        
        if ($content -ne $newContent) {
            Set-Content -Path $fullPath -Value $newContent -NoNewline
            $totalFixed++
            Write-Host "Corregido: $file" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "Total archivos corregidos: $totalFixed" -ForegroundColor Cyan
