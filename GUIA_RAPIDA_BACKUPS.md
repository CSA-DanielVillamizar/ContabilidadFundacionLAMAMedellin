# Guía Rápida: Habilitación de Backups en Azure con Managed Identity

> **Fecha:** Enero 2026  
> **Propósito:** Referencia rápida para crear Storage Account y configurar backups con Managed Identity (RBAC)  
> **⭐ Novedad:** Sin connection strings - Autenticación con System Assigned MI

---

## Resumen Ejecutivo

**Paso 0-2: Crear infraestructura (5 min)**  
**Paso 3: Configurar RBAC + Managed Identity (3 min)**  
**Paso 4-8: Validar configuración (10 min)**  
**Total estimado: 18 minutos**

---

## Comandos CLI (Copy-Paste)

### Definir Variables

```powershell
# Personaliza estos valores
$resourceGroup = "RG-TesoreriaLAMAMedellin-Prod"
$storageAccountName = "lamaprodstorage2025"  # Ejemplo - validar disponibilidad primero
$containerName = "sql-backups"
$region = "centralus"  # Región confirmada de todos los recursos de producción
$appServiceName = "app-tesorerialamamedellin-prod"
```

### Paso 0: Validar Disponibilidad del Nombre (IMPORTANTE)

```powershell
Write-Host "=== Validar Disponibilidad del Nombre de Storage Account ==="
az storage account check-name --name $storageAccountName

# Verifica que retorne:
# {
#   "nameAvailable": true,
#   "reason": null,
#   "message": null
# }

# Si "nameAvailable": false, elige otro nombre y actualiza la variable:
# $storageAccountName = "lamaprodstorage2026"  # Ejemplo alternativo

Write-Host "✓ Nombre validado como disponible"
```

### Paso 1: Crear Storage Account

```powershell
Write-Host "=== Crear Storage Account ==="
az storage account create `
  --resource-group $resourceGroup `
  --name $storageAccountName `
  --location $region `
  --sku Standard_LRS `
  --kind StorageV2 `
  --access-tier Hot `
  --https-only true `
  --min-tls-version TLS1_2 `
  --default-action Deny

Write-Host "✓ Storage Account creado: $storageAccountName"
```

### Paso 2: Crear Contenedor sql-backups

```powershell
Write-Host "=== Crear Contenedor sql-backups ==="
az storage container create `
  --name $containerName `
  --account-name $storageAccountName `
  --auth-mode login `
  --public-access off

# Verificar
az storage container list `
  --account-name $storageAccountName `
  --auth-mode login `
  --output table

Write-Host "✓ Contenedor creado: $containerName"
```

### Paso 3: Configurar RBAC con Managed Identity (⭐ Recomendado)

```powershell
Write-Host "=== Configurar RBAC con Managed Identity ===" -ForegroundColor Cyan

# Obtener Principal ID de la System Assigned MI del App Service
$principalId = $(az webapp identity show `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --query principalId -o tsv)

Write-Host "System Assigned MI Principal ID: $principalId"

# Obtener Resource ID del Storage Account
$storageResourceId = $(az storage account show `
  --resource-group $resourceGroup `
  --name $storageAccountName `
  --query id -o tsv)

Write-Host "Storage Account Resource ID: $storageResourceId"

# Asignar rol "Storage Blob Data Contributor" a la Managed Identity
az role assignment create `
  --assignee $principalId `
  --role "Storage Blob Data Contributor" `
  --scope $storageResourceId

Write-Host "✓ Rol 'Storage Blob Data Contributor' asignado" -ForegroundColor Green

# Verificar asignación de roles
az role assignment list `
  --scope $storageResourceId `
  --query "[?principalId=='$principalId'].{Role:roleDefinitionName, Principal:principalId}" `
  --output table

# Configurar App Settings con URI de Storage Account (no requiere secreto)
az webapp config appsettings set `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --settings Azure__StorageBlobServiceUri="https://$storageAccountName.blob.core.windows.net/"

Write-Host "✓ App Settings configurado con URI (Managed Identity)" -ForegroundColor Green
```

### Paso 4: Reiniciar App Service

```powershell
Write-Host "=== Reiniciar App Service ==="
az webapp restart `
  --resource-group $resourceGroup `
  --name $appServiceName

Write-Host "✓ App Service reinciad. Esperando 45 segundos..."
Start-Sleep -Seconds 45
Write-Host "✓ App Service listo"
```

### Paso 5: Validar /api/diagnostico (backupReady=true)

```powershell
Write-Host "=== Validar /api/diagnostico ==="

# Nota: Requiere autenticación como admin
# Primero, obtén un token JWT válido y asígnalo a $TOKEN

$diagnosticoUrl = "https://$appServiceName.azurewebsites.net/api/diagnostico"

# Reemplaza $TOKEN con tu token JWT
$response = curl -s -H "Authorization: Bearer $TOKEN" $diagnosticoUrl | ConvertFrom-Json

Write-Host "=== Respuesta de /api/diagnostico ==="
Write-Host ($response.azure | ConvertTo-Json)

# Validar campos clave
if ($response.azure.backupReady -eq $true) {
    Write-Host "✅ backupReady = TRUE - Backups habilitados con Managed Identity" -ForegroundColor Green
} else {
    Write-Host "❌ backupReady = FALSE - Revisar logs en Application Insights" -ForegroundColor Red
}

if ($response.azure.storageConfigured -eq $true) {
    Write-Host "✅ storageConfigured = TRUE - Storage URI configurado" -ForegroundColor Green
} else {
    Write-Host "❌ storageConfigured = FALSE - Verificar RBAC y Azure__StorageBlobServiceUri" -ForegroundColor Red
}

# Verificar método de autenticación
if ($response.azure.blobStorageAuthMethod -eq "ManagedIdentity") {
    Write-Host "✅ Autenticación: Managed Identity (sin connection string)" -ForegroundColor Green
} else {
    Write-Host "⚠️  Autenticación: $($response.azure.blobStorageAuthMethod)" -ForegroundColor Yellow
}
}
```

### Paso 6: Validar /health/ready

```powershell
Write-Host "=== Validar /health/ready ==="

$healthUrl = "https://$appServiceName.azurewebsites.net/health/ready"
$health = curl -s $healthUrl | ConvertFrom-Json

Write-Host ($health | ConvertTo-Json -Depth 3)

if ($health.status -eq "Healthy") {
    Write-Host "✅ Database Health Check = Healthy"
} else {
    Write-Host "⚠️ Database Health Check no está Healthy"
}
```

### Paso 7: Verificar Blobs (Backups Existentes)

```powershell
Write-Host "=== Listar Blobs en Contenedor sql-backups ==="

az storage blob list `
  --container-name $containerName `
  --account-name $storageAccountName `
  --auth-mode login `
  --output table

Write-Host "Blobs listados exitosamente"

# Para más detalles (tamaño, fecha)
Write-Host "`n=== Detalles de Blobs ==="
az storage blob list `
  --container-name $containerName `
  --account-name $storageAccountName `
  --auth-mode login `
  --query "[].{Name:name, LastModified:properties.lastModified, Size:properties.contentLength}" `
  --output table

Write-Host "✓ Blobs verificados"
```

### Paso 8: Validación Completa (One-Liner)

```powershell
Write-Host "========== VALIDACIÓN COMPLETA BACKUPS (MANAGED IDENTITY) ==========="

# 1. Storage Account existe
$sa = az storage account show --resource-group $resourceGroup --name $storageAccountName --query "{Name:name, Status:provisioningState}"
Write-Host "✓ Storage Account: $($sa | ConvertFrom-Json | Select-Object -ExpandProperty Name)"

# 2. Contenedor existe
$cont = az storage container exists --account-name $storageAccountName --name $containerName --auth-mode login --query exists
Write-Host "✓ Contenedor sql-backups existe: $cont"

# 3. RBAC - Verificar rol asignado a Managed Identity
$principalId = $(az webapp identity show --resource-group $resourceGroup --name $appServiceName --query principalId -o tsv)
$storageResourceId = $(az storage account show --resource-group $resourceGroup --name $storageAccountName --query id -o tsv)
$roleAssignment = az role assignment list --scope $storageResourceId --query "[?principalId=='$principalId'].roleDefinitionName" -o tsv
if ($roleAssignment -contains "Storage Blob Data Contributor") {
    Write-Host "✓ RBAC: Managed Identity tiene rol 'Storage Blob Data Contributor'" -ForegroundColor Green
} else {
    Write-Host "❌ RBAC: Rol no asignado - Ejecutar Paso 3" -ForegroundColor Red
}

# 4. App Service está ejecutando
$appStatus = az webapp show --resource-group $resourceGroup --name $appServiceName --query state -o tsv
Write-Host "✓ App Service estado: $appStatus"

# 5. Blobs en contenedor (backups existentes)
$blobCount = az storage blob list --container-name $containerName --account-name $storageAccountName --auth-mode login --query "length(@)" 2>$null
Write-Host "✓ Backups en contenedor: $blobCount"

Write-Host "========== ✓ VALIDACIÓN COMPLETADA ==========="
```

---

## Tabla de Configuración Requerida

| Configuración | Valor Ejemplo | Método |
|---------------|---------------|--------|
| `Azure__StorageBlobServiceUri` | `https://lamaprodstorage2025.blob.core.windows.net/` | App Setting |
| `Azure__BackupContainerName` | `sql-backups` | appsettings.json |
| `Azure__UseAzureBlobBackup` | `true` | appsettings.json |
| **RBAC Role** | `Storage Blob Data Contributor` | Portal o CLI |
| **Managed Identity** | System Assigned | App Service Identity |

---

## Troubleshooting

### ❌ Error: "Access Denied" al intentar crear backup

**Causa:** Managed Identity no tiene permisos RBAC.

**Solución:**
```powershell
# Verificar roles asignados
$principalId = $(az webapp identity show --resource-group $resourceGroup --name $appServiceName --query principalId -o tsv)
$storageResourceId = $(az storage account show --resource-group $resourceGroup --name $storageAccountName --query id -o tsv)

az role assignment list \
  --scope $storageResourceId \
  --query "[?principalId=='$principalId']" \
  --output table

# Si no tiene rol, asignar:
az role assignment create \
  --assignee $principalId \
  --role "Storage Blob Data Contributor" \
  --scope $storageResourceId
```

### ❌ Error: "Azure--StorageConnectionString NOT FOUND in Key Vault" (Legacy)

> **Nota:** Este error solo aplica si usas connection string (método deprecado).  
> Con Managed Identity, NO necesitas este secreto.

**Solución (solo si NO usas Managed Identity):**
```powershell
# Verificar que se guardó
az keyvault secret list --vault-name $keyVaultName --query "[?name=='Azure--StorageConnectionString']"

# Si no existe, ejecutar Paso 3 nuevamente
```

### ❌ backupReady = FALSE en /api/diagnostico

**Causas posibles:**
1. Secreto no guardado correctamente en Key Vault
2. App Service no reiniciado
3. Backup no está habilitado en appsettings

**Solución:**
```powershell
# 1. Re-ejecutar Paso 3 (guardar secreto)
# 2. Re-ejecutar Paso 4 (reiniciar App Service)
# 3. Verificar logs en Application Insights:
#    Ir a: App Service → Monitoring → App Service logs
```

### ❌ Error: "Connection string is empty or null"

**Solución:**
```powershell
# Verificar que el secreto se puede leer
az keyvault secret show `
  --vault-name $keyVaultName `
  --name "Azure--StorageConnectionString" `
  --query value -o tsv | Select-Object -First 60

# Si retorna nada, re-guardar en Key Vault
```

### ❌ Contenedor NO existe

**Solución:**
```powershell
# Re-ejecutar Paso 2
az storage container create `
  --name $containerName `
  --account-name $storageAccountName `
  --auth-mode login `
  --public-access off
```

---

## Verificación de Primer Backup

**El primer backup se genera automáticamente a las 2 AM UTC** (según `CronSchedule: "0 2 * * *"` en appsettings).

**Para esperar y verificar:**

```powershell
Write-Host "Esperando próxima ejecución de backup (máximo hasta las 2 AM)..."

# Monitorear blobs cada 5 minutos
$stopTime = (Get-Date).AddHours(2)
while ((Get-Date) -lt $stopTime) {
    $blobCount = az storage blob list `
      --container-name $containerName `
      --account-name $storageAccountName `
      --auth-mode login `
      --query "length(@)" 2>$null

    Write-Host "$(Get-Date): Blobs en contenedor = $blobCount"
    
    if ($blobCount -gt 0) {
        Write-Host "✅ BACKUP DETECTADO!"
        az storage blob list `
          --container-name $containerName `
          --account-name $storageAccountName `
          --auth-mode login `
          --output table
        break
    }
    
    Start-Sleep -Seconds 300  # Esperar 5 minutos
}
```

---

## Checklist Final

- [ ] Storage Account `lamaprodstorage2025` creado
- [ ] Contenedor `sql-backups` creado
- [ ] Connection string guardada en Key Vault como `Azure--StorageConnectionString`
- [ ] App Service reiniciado
- [ ] `/api/diagnostico` retorna `backupReady=true`
- [ ] `/health/ready` retorna `Healthy`
- [ ] Blobs se ven en `az storage blob list ...`

---

**Referencia completa:** Ver [docs/AZURE_PRODUCTION_SETUP.md - Sección 7: Validación Post-Configuración](docs/AZURE_PRODUCTION_SETUP.md#7-validación-post-configuración-storage-account-y-backups)
