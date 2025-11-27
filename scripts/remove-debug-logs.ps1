$rootPath = "c:\Users\DanielVillamizar\ContabilidadLAMAMedellin\src\Server"
$patterns = @(
    "Pages\Tesoreria\*.razor",
    "Pages\GerenciaNegocios\*.razor",
    "Pages\Admin\*.razor"
)

$totalRemoved = 0

foreach ($pattern in $patterns) {
    $fullPattern = Join-Path $rootPath $pattern
    $files = Get-ChildItem -Path $fullPattern -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $countBefore = ([regex]::Matches($content, 'Console\.WriteLine')).Count
        
        if ($countBefore -gt 0) {
            $newContent = $content -replace '(?m)^\s*Console\.WriteLine\([^)]*\);\s*[\r\n]*', ''
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
            $totalRemoved += $countBefore
            Write-Host "Procesado: $($file.Name) - $countBefore eliminados" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "Total eliminados: $totalRemoved" -ForegroundColor Cyan
