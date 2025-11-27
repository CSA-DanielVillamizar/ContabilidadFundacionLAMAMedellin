# Script para migrar [Authorize(Roles=...)] a [Authorize(Policy=...)]
# Ejecutar desde la raíz del proyecto

$ErrorActionPreference = "Stop"

Write-Host "🔧 Migrando atributos Authorize(Roles) a Authorize(Policy)..." -ForegroundColor Cyan

# Definir mapeos (orden importa: del más específico al más general)
$mappings = @(
    @{
        Pattern = '\[Authorize\(Roles\s*=\s*"Tesorero,Junta,Consulta"\)\]'
        Replacement = '[Authorize(Policy = "TesoreroJuntaConsulta")]'
        Name = 'TesoreroJuntaConsulta'
    },
    @{
        Pattern = '\[Authorize\(Roles\s*=\s*"Admin,Gerente,Tesorero"\)\]'
        Replacement = '[Authorize(Policy = "AdminGerenteTesorero")]'
        Name = 'AdminGerenteTesorero'
    },
    @{
        Pattern = '\[Authorize\(Roles\s*=\s*"Admin,Tesorero"\)\]'
        Replacement = '[Authorize(Policy = "AdminTesorero")]'
        Name = 'AdminTesorero'
    },
    @{
        Pattern = '\[Authorize\(Roles\s*=\s*"Admin,Gerente"\)\]'
        Replacement = '[Authorize(Policy = "AdminGerente")]'
        Name = 'AdminGerente'
    },
    @{
        Pattern = '\[Authorize\(Roles\s*=\s*"Tesorero,Junta"\)\]'
        Replacement = '[Authorize(Policy = "TesoreroJunta")]'
        Name = 'TesoreroJunta'
    }
)

$totalReplacements = 0
$filesChanged = @()

# Buscar todos los archivos .cs y .razor en src/Server
$files = Get-ChildItem -Path "src\Server" -Recurse -Include *.cs,*.razor | Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if (-not $content) { continue }
    
    $originalContent = $content
    $fileReplacements = 0
    
    foreach ($mapping in $mappings) {
        $matches = [regex]::Matches($content, $mapping.Pattern)
        if ($matches.Count -gt 0) {
            $content = $content -replace $mapping.Pattern, $mapping.Replacement
            $fileReplacements += $matches.Count
            Write-Host "  ✓ $($file.Name): $($matches.Count) x $($mapping.Name)" -ForegroundColor Green
        }
    }
    
    if ($fileReplacements -gt 0) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $filesChanged += $file.FullName
        $totalReplacements += $fileReplacements
    }
}

Write-Host "`n📊 Resumen:" -ForegroundColor Yellow
Write-Host "  • Archivos modificados: $($filesChanged.Count)" -ForegroundColor White
Write-Host "  • Reemplazos totales: $totalReplacements" -ForegroundColor White

if ($filesChanged.Count -gt 0) {
    Write-Host "`n📁 Archivos modificados:" -ForegroundColor Yellow
    $filesChanged | ForEach-Object {
        $relativePath = $_.Replace((Get-Location).Path + '\', '')
        Write-Host "  - $relativePath" -ForegroundColor Gray
    }
}

Write-Host "`n✅ Migración completada." -ForegroundColor Green
Write-Host "👉 Ejecuta 'dotnet build' para verificar compilación." -ForegroundColor Cyan
