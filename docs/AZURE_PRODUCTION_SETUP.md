# Guía de Configuración de Producción en Azure

## Información General

**Aplicación**: Contabilidad LAMA Medellín  
**Ambiente**: Producción (Azure)  
**Última actualización**: Enero 2026  
**Responsable**: Equipo DevOps

### Recursos Azure Requeridos

| Recurso | Nombre | Tipo |
|---------|--------|------|
| Grupo de Recursos | `RG-TesoreriaLAMAMedellin-Prod` | Resource Group |
| App Service | `app-tesorerialamamedellin-prod` | Web App |
| Plan App Service | `ASP-TesoreriaLAMAMedellin-Prod` | App Service Plan |
| SQL Server | `sql-tesorerialamamedellin-prod` | SQL Server |
| Base de Datos | `sqldb-tesorerialamamedellin-prod` | SQL Database |
| Key Vault | `kvtesorerialamamdln` | Key Vault |
| Application Insights | `appi-tesorerialamamedellin-prod` | Application Insights |
| Storage Account | `<STORAGE_ACCOUNT_NAME>` | Storage Account (Backups) |

---

## 1. Configuración de Managed Identity

### 1.1 Habilitar Managed Identity en App Service

**Pasos en Azure Portal:**

1. Navega a `app-tesorerialamamedellin-prod` → Settings → Identity
2. En "System assigned":
   - Cambia el estado a **ON**
   - Presiona **Save**
3. Copia el **Object ID** que aparece (ej: `00000000-0000-0000-0000-000000000000`)

**Resultado esperado:**
- Status: Enabled
- Object ID: Visible y copiable

### 1.2 Asignar Permisos a Key Vault

**Pasos en Azure Portal:**

1. Navega a `kvtesorerialamamdln` → Access policies
2. Presiona **+ Create** (o **Add Access Policy**)
3. En "Secret permissions":
   - Selecciona: `Get`, `List`
4. En "Select principal":
   - Busca el nombre del App Service: `app-tesorerialamamedellin-prod`
   - Selecciona su Managed Identity
5. Presiona **Add** y luego **Save**

**Resultado esperado:**
- Policy creada con permisos `Get`, `List` en secretos
- Principal: App Service Managed Identity

### 1.3 Asignar Permisos a Storage Account

**Pasos en Azure Portal:**

1. Navega a `<STORAGE_ACCOUNT_NAME>` → Access Control (IAM)
2. Presiona **+ Add** → **Add role assignment**
3. En "Role":
   - Busca y selecciona **Storage Blob Data Contributor**
4. En "Members":
   - Selecciona **Managed Identity**
   - Busca y selecciona `app-tesorerialamamedellin-prod`
5. Presiona **Review + assign**

**Resultado esperado:**
- Rol asignado: Storage Blob Data Contributor
- Member: App Service Managed Identity

---

## 2. Configuración de Azure SQL Database con Entra ID

### 2.1 Configurar Administrador de Entra ID en SQL Server

**Pasos en Azure Portal:**

1. Navega a `sql-tesorerialamamedellin-prod` → Access Control → Entra ID admin
2. Presiona **Set admin**
3. Busca y selecciona al usuario/grupo administrador:
   - Ej: Tu cuenta de usuario o grupo `TesoreriaAdmins`
4. Presiona **Select**

**Resultado esperado:**
- Entra ID admin configurado
- Usuario/grupo administrador visible en la lista

### 2.2 Crear Usuario de Managed Identity en la Base de Datos

**Conexión a SQL Database:**

Usa **Azure Data Studio** o **SQL Server Management Studio** con autenticación de Entra ID:

```sql
-- Conectarse como Entra ID admin

-- 1. Crear usuario para la Managed Identity del App Service
-- Nombre: El nombre exacto del App Service
CREATE USER [app-tesorerialamamedellin-prod] FROM EXTERNAL PROVIDER;

-- 2. Asignar roles mínimos requeridos (NOT db_owner)
-- La aplicación SOLO necesita leer y escribir datos, no modificar esquema
ALTER ROLE db_datareader ADD MEMBER [app-tesorerialamamedellin-prod];
ALTER ROLE db_datawriter ADD MEMBER [app-tesorerialamamedellin-prod];

-- 3. Verificar que el usuario fue creado
SELECT * FROM sys.database_principals WHERE name = 'app-tesorerialamamedellin-prod';

-- 4. Verificar roles asignados
SELECT 
    p.name AS [Usuario],
    r.name AS [Rol]
FROM sys.database_role_members drm
INNER JOIN sys.database_principals p ON drm.member_principal_id = p.principal_id
INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE p.name = 'app-tesorerialamamedellin-prod';
```

**Resultado esperado:**
- Usuario creado en la base de datos
- Roles `db_datareader` y `db_datawriter` asignados (NOT db_owner)
- Query retorna 2 filas: una para db_datareader y otra para db_datawriter

### 2.3 Ejecutar Migraciones en Producción

Las migraciones de Entity Framework Core se ejecutan **FUERA de la aplicación** (en el pipeline CI/CD o manualmente por admin).
La aplicación en Producción **NO tiene permisos para modificar esquema**.

#### Opción A: Ejecutar Migraciones en el Pipeline CI/CD (Recomendado)

En el pipeline de deployment (GitHub Actions, Azure DevOps, etc.):

```bash
# Antes de deployar la aplicación
dotnet ef database update --project src/Server/Server.csproj \
  --context AppDbContext \
  --connection "Server=tcp:sql-tesorerialamamedellin-prod.database.windows.net,1433;Initial Catalog=sqldb-tesorerialamamedellin-prod;Authentication=Active Directory Default;"
```

Requiere que el usuario/identidad que ejecuta el pipeline tenga Entra ID admin en SQL.

#### Opción B: Ejecutar Migraciones Manualmente (One-off)

Si no usas CI/CD, ejecuta manualmente con credenciales de admin:

```bash
# Desde la máquina del administrador (con SQL Server Management Studio o Azure Data Studio)
dotnet ef database update --project src/Server/Server.csproj \
  --context AppDbContext
```

Conectarse con Entra ID admin credentials.

#### Opción C: Crear Usuario Temporal con Permisos de Migración (Advanced)

Para mayor seguridad, crear un usuario temporal con solo permisos de DDL:

```sql
-- SOLO para ejecutar migraciones (temporal)
CREATE USER [app-tesorerialamamedellin-prod-migrations] FROM EXTERNAL PROVIDER;
ALTER ROLE db_ddladmin ADD MEMBER [app-tesorerialamamedellin-prod-migrations];

-- Después de ejecutar migraciones, eliminar este usuario
DROP USER IF EXISTS [app-tesorerialamamedellin-prod-migrations];
```

Usar este usuario en el pipeline temporalmente y eliminarlo después.

**Resumen de Permisos:**

| Usuario | Rol | Ambiente | Propósito |
|---------|-----|----------|----------|
| `app-tesorerialamamedellin-prod` | `db_datareader`, `db_datawriter` | Producción | Operación normal (lectura/escritura de datos) |
| `app-tesorerialamamedellin-prod-migrations` | `db_ddladmin` | Producción | Migraciones EF Core (SOLO durante deployment) |
| Entra ID Admin | `db_owner` | Producción | Administración base de datos |

---

## 3. Configuración de Secretos en Key Vault

### 3.1 Secretos Requeridos

Navega a `kvtesorerialamamdln` → Secrets y crea los siguientes secretos:

| Nombre | Valor | Descripción |
|--------|-------|-------------|
| `DefaultConnection` | Connection string SQL | `Server=tcp:sql-tesorerialamamedellin-prod.database.windows.net,1433;...` |
| `Smtp--Password` | Contraseña SMTP Office 365 | Contraseña de `tesoreria@fundacionlamamedellin.org` |
| `ApplicationInsights--ConnectionString` | App Insights conn string | Obtenida de Application Insights → Settings |
| `Azure--StorageConnectionString` | Storage Account conn string | Obtenida de Storage Account `lamaprodstorage2025` → Access keys |
| `Azure--ApplicationInsightsInstrumentationKey` | Instrumentation key | Obtenida de Application Insights |

### 3.2 Crear Secretos en Azure Portal

**Para cada secreto:**

1. En Key Vault, presiona **Secrets** → **+ Generate/Import**
2. Ingresa el nombre (ej: `DefaultConnection`)
3. Ingresa el valor
4. Presiona **Create**

**Ejemplo: Connection String SQL**

```
Server=tcp:sql-tesorerialamamedellin-prod.database.windows.net,1433;Initial Catalog=sqldb-tesorerialamamedellin-prod;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;
```

---

## 4. Configuración de Application Insights

### 4.1 Obtener Connection String

1. Navega a `appi-tesorerialamamedellin-prod`
2. Presiona **Settings** → **Connection strings**
3. Copia la connection string (InstrumentationKey o connection string completo)
4. Guárdalo en Key Vault como `ApplicationInsights--ConnectionString`

### 4.2 Verificar Telemetría

Una vez el App Service esté ejecutando:

1. Navega a Application Insights → **Live Metrics Stream**
2. Deberías ver requests en tiempo real
3. Verifica **Logs** → **traces** para ver mensajes de Serilog

---

## 5. Configuración de Application Settings en App Service

### 5.1 Variables de Ambiente

En `app-tesorerialamamedellin-prod` → Settings → Configuration → Application settings:

| Nombre | Valor | Notas |
|--------|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Obligatorio |
| `Azure__KeyVaultEndpoint` | `https://kvtesorerialamamdln.vault.azure.net/` | URL del Key Vault |
| `Azure__EnableKeyVault` | `true` | Habilita carga de Key Vault |
| `Azure__UseAzureBlobBackup` | `true` | Usa Blob Storage para backups |
| `ApplicationInsights__ConnectionString` | Obtenido de Key Vault | Se carga automáticamente |

### 5.2 Connection Strings

En la misma sección, presiona **Connection strings** y agrega:

| Nombre | Valor | Tipo |
|--------|-------|------|
| `DefaultConnection` | Obtenido de Key Vault | SQLAzure |

**Nota:** Azure cargará estos valores desde Key Vault automáticamente via `DefaultAzureCredential` cuando se configura correctamente.

---

## 6. Configuración de Backups en Azure Blob Storage

> **Importante:** En Producción el `BackupService` requiere Azure Blob Storage configurado. Si el Storage Account o el contenedor no existen, el servicio fallará en el arranque (fail-fast).

### 6.1 Crear Storage Account (Portal y CLI)

**Ejemplo de nombre real:** `lamaprodstorage2025`

**En Azure Portal:**
1. Entra a **Resource groups** → `RG-TesoreriaLAMAMedellin-Prod`
2. Presiona **Create** → **Storage account**
3. Valores requeridos:
   - **Storage account name**: `lamaprodstorage2025` (solo minúsculas, sin guiones)
   - **Region**: `eastus` (o la misma que el App Service)
   - **Performance**: Standard
   - **Redundancy**: LRS (Locally-redundant storage)
   - **Access tier**: Hot
4. Pestaña **Advanced**:
   - ✅ Require secure transfer (HTTPS only)
   - ✅ Enable blob public access
5. Presiona **Review + create** → **Create**

**Con Azure CLI (Recomendado):**

```bash
# Variables
$resourceGroup = "RG-TesoreriaLAMAMedellin-Prod"
$storageAccountName = "lamaprodstorage2025"
$region = "eastus"

# Crear Storage Account
az storage account create \
  --resource-group $resourceGroup \
  --name $storageAccountName \
  --location $region \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot \
  --https-only true \
  --min-tls-version TLS1_2 \
  --default-action Deny

# Output: Guarda el nombre para referencia futura
Write-Host "Storage Account creado: $storageAccountName"
```

### 6.2 Crear el contenedor `sql-backups`

**En Azure Portal:**
1. Navega al Storage Account `lamaprodstorage2025`
2. Presiona **Containers** (en el menú izquierdo bajo **Data storage**)
3. Presiona **+ Container**
4. Nombre: `sql-backups`
5. Public access level: `Private`
6. Presiona **Create**

**Con Azure CLI:**
```bash
# Variables
$storageAccountName = "lamaprodstorage2025"
$containerName = "sql-backups"

# Crear contenedor
az storage container create \
  --name $containerName \
  --account-name $storageAccountName \
  --auth-mode login \
  --public-access off

# Verificar que se creó
az storage container list \
  --account-name $storageAccountName \
  --auth-mode login \
  --output table
```

### 6.3 Obtener Connection String y Guardar en Key Vault

**En Azure Portal:**
1. En Storage Account `lamaprodstorage2025` → **Security + networking** → **Access keys**
2. Copia la **Connection string** bajo "key1"
3. Navega a Key Vault `kvtesorerialamamdln` → **Secrets** → **+ Generate/Import**
4. Nombre: `Azure--StorageConnectionString`
5. Valor: Pega la connection string copiada
6. Presiona **Create**

**Con Azure CLI:**
```bash
# Variables
$storageAccountName = "lamaprodstorage2025"
$resourceGroup = "RG-TesoreriaLAMAMedellin-Prod"
$keyVaultName = "kvtesorerialamamdln"

# Obtener connection string
$connectionString = $(az storage account show-connection-string \
  --resource-group $resourceGroup \
  --name $storageAccountName \
  --query connectionString -o tsv)

Write-Host "Connection String obtenido (primeros 50 chars):"
Write-Host $connectionString.Substring(0, 50) "..."

# Guardar en Key Vault
az keyvault secret set \
  --vault-name $keyVaultName \
  --name "Azure--StorageConnectionString" \
  --value $connectionString

Write-Host "✓ Secreto 'Azure--StorageConnectionString' guardado en Key Vault"
```

### 6.4 Verificar Configuración en la Aplicación

**Endpoint de diagnóstico (después de redeployar):**

```bash
# 1. Autentícate como Admin en la aplicación
# 2. Realiza una petición GET a:
curl -H "Authorization: Bearer <YOUR_AUTH_TOKEN>" \
  https://app-tesorerialamamedellin-prod.azurewebsites.net/api/diagnostico

# Respuesta esperada (sección "azure"):
# {
#   "azure": {
#     "keyVaultEnabled": true,
#     "keyVaultConfigured": true,
#     "blobStorageEnabled": true,
#     "blobStorageConfigured": true,      <-- DEBE SER: true
#     "backupContainerName": "sql-backups",
#     "storageConfigured": true,           <-- NUEVO CAMPO: true
#     "backupReady": true,                 <-- NUEVO CAMPO: true (si Backup también está habilitado)
#     "appInsightsConfigured": true
#   }
# }
```

**Si `backupReady = true`:**
- ✅ Storage Account está creado
- ✅ Contenedor `sql-backups` existe
- ✅ Connection string está en Key Vault y se cargó correctamente
- ✅ BackupHostedService está habilitado
- ✅ Backups automáticos estarán activos

**Si `backupReady = false`:**
- ❌ Falta alguno de los pasos anteriores
- ❌ Verifica los logs del App Service para ver el error específico
- ❌ El BackupHostedService no iniciará

### 6.5 Verificar Backups Automáticos

Los backups se generarán automáticamente cada día a las 2 AM (según `CronSchedule` en appsettings):

```json
{
  "Backup": {
    "Enabled": true,
    "CronSchedule": "0 2 * * *",
    "BackupPath": "Backups",
    "RetentionDays": 30,
    "Server": "sql-tesorerialamamedellin-prod.database.windows.net",
    "Database": "sqldb-tesorerialamamedellin-prod"
  }
}
```

**Para verificar manualmente que existen backups en el contenedor:**

```bash
# Variables
$storageAccountName = "lamaprodstorage2025"
$containerName = "sql-backups"

# Listar blobs en el contenedor
az storage blob list \
  --container-name $containerName \
  --account-name $storageAccountName \
  --auth-mode login \
  --output table

# Output esperado después de la primera ejecución (2 AM):
# Name                           Blob Type     Length
# Backup_20250110_020000.bak     BlockBlob     2147483648
# Backup_20250109_020000.bak     BlockBlob     2147483648
```

---

## 7. Checklist de Validación de Producción

### 7.1 Health Checks

Ejecuta después del deployment:

```bash
# General health
curl https://app-tesorerialamamedellin-prod.azurewebsites.net/health

# Readiness (incluye DB)
curl https://app-tesorerialamamedellin-prod.azurewebsites.net/health/ready

# Liveness
curl https://app-tesorerialamamedellin-prod.azurewebsites.net/health/live
```

**Resultado esperado:**
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "database", "status": "Healthy" }
  ]
}
```

### 7.2 Security Headers

Verifica que los headers de seguridad estén presentes:

```bash
curl -I https://app-tesorerialamamedellin-prod.azurewebsites.net/

# Busca estos headers:
# Strict-Transport-Security: max-age=31536000
# X-Content-Type-Options: nosniff
# X-Frame-Options: DENY
# Referrer-Policy: strict-origin-when-cross-origin
```

### 7.3 Rate Limiting

Valida rate limiting realizando múltiples requests:

```bash
# Script para probar rate limiting global (100 requests/minute)
for i in {1..105}; do
  curl -s https://app-tesorerialamamedellin-prod.azurewebsites.net/health -w "%{http_code}\n" | tail -1
done

# El request 101+ deberá retornar 429 (Too Many Requests)
```

### 7.4 Endpoint de Diagnóstico (Admin Only)

```bash
# Autentícate como admin y accede a:
curl -H "Authorization: Bearer $TOKEN" \
  https://app-tesorerialamamedellin-prod.azurewebsites.net/api/diagnostico

# Resultado esperado:
{
  "environment": "Production",
  "version": "1.0.0.0",
  "azure": {
    "keyVaultEnabled": true,
    "keyVaultConfigured": true,
    "blobStorageEnabled": true,
    "blobStorageConfigured": true,
    "appInsightsConfigured": true
  },
  "database": {
    "authenticationType": "ManagedIdentity",
    "connectionStringSet": true
  }
}
```

### 7.5 Logs en Application Insights

1. Navega a Application Insights → **Logs**
2. Ejecuta esta query:

```kusto
traces
| where timestamp > ago(1h)
| project timestamp, severityLevel, message
| take 100
```

**Resultado esperado:** Logs de inicialización y operaciones registradas.

### 7.6 Migración de Base de Datos

Verifica que las migraciones se ejecutaron:

```sql
-- Conéctate a la base de datos en producción
SELECT name, version FROM dbo.__EFMigrationsHistory ORDER BY version DESC
```

---

## 8. Monitoreo y Mantenimiento

### 8.1 Alertas en Application Insights

Configura alertas para:

- **Fallos HTTP (5xx)**: > 10 en 5 minutos
- **Excepción no controlada**: Cualquier instancia
- **Request tardío**: Duración promedio > 5 segundos

### 8.2 Logs Diarios

Los logs se guardan en:

- **Application Insights**: Telemetría y excepciones
- **Azure Storage**: Archivos de log locales (vía Serilog)
- **App Service Logs**: Stdout/stderr

### 8.3 Backups de Base de Datos

**Verificación manual:**

```powershell
# Listar backups más recientes
$backups = Get-AzStorageBlob -Container "sql-backups" -Context $ctx | Sort-Object TimeCreated -Descending
$backups | Select-Object -First 5 | Format-Table Name, LastModified, Length
```

**Restauración de backup (si es necesario):**

```sql
-- En SQL Server Management Studio o Azure Data Studio
-- Requiere que el archivo .bak esté disponible
RESTORE DATABASE [sqldb-tesorerialamamedellin-prod]
FROM DISK = N'ruta-a-backup.bak'
WITH FILE = 1, NOUNLOAD, REPLACE, NORECOVERY
```

---

## 9. Troubleshooting

### 9.1 Error: "Key Vault not found" o "Access Denied"

**Causas:**
- Managed Identity no habilitada
- Políticas de acceso no configuradas
- URL de Key Vault incorrecta

**Solución:**
1. Verifica que Managed Identity esté habilitada: `app-tesorerialamamedellin-prod` → Identity → System assigned
2. Verifica políticas de acceso en Key Vault
3. Verifica que `Azure__KeyVaultEndpoint` esté correcto

### 9.2 Error: "Login failed for user"

**Causas:**
- Usuario Managed Identity no creado en la BD
- Permisos insuficientes

**Solución:**
```sql
-- Verifica que el usuario exista
SELECT * FROM sys.database_principals WHERE name = 'app-tesorerialamamedellin-prod';

-- Si no existe, crea nuevamente (como Entra ID admin)
CREATE USER [app-tesorerialamamedellin-prod] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [app-tesorerialamamedellin-prod];
```

### 9.3 Error: "Connection timeout"

**Causas:**
- Firewall de SQL Server bloqueando conexiones
- VNet configuration incorrecta

**Solución:**
1. Verifica reglas de firewall: SQL Server → Settings → Firewalls and virtual networks
2. Asegúrate que "Allow Azure services" esté **ON**
3. Verifica que el App Service esté en la misma región (recomendado)

### 9.4 Backups no se están creando

**Causas:**
- BackupHostedService no está ejecutando
- Permisos de Storage Account insuficientes

**Solución:**
1. Verifica logs en Application Insights → Traces
2. Busca mensajes de BackupService
3. Verifica que la Managed Identity tenga rol `Storage Blob Data Contributor`

---

## 10. Deployments y Actualizaciones

### 10.1 Procedimiento de Deployment

1. **Preparar la aplicación:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Crear deployment package:**
   ```bash
   cd publish
   Compress-Archive -Path * -DestinationPath ../deploy.zip
   ```

3. **Deployment via Azure CLI:**
   ```bash
   az webapp deployment source config-zip \
     --resource-group RG-TesoreriaLAMAMedellin-Prod \
     --name app-tesorerialamamedellin-prod \
     --src deploy.zip
   ```

4. **Verificar deployment:**
   - Accede a https://app-tesorerialamamedellin-prod.azurewebsites.net
   - Verifica logs en Application Insights

### 10.2 Rollback si es Necesario

1. En Azure Portal → App Service → Deployment → Deployment slots
2. Selecciona una versión anterior
3. Presiona **Swap** si tienes staging slot
4. O redeploy la versión anterior

---

## 11. Contactos y Escalación

| Rol | Contacto | Responsabilidad |
|-----|----------|-----------------|
| DevOps/Cloud Admin | [contacto] | Infraestructura Azure, Key Vault, permisos |
| DBA | [contacto] | Base de datos, backups, performance |
| Desarrollador | [contacto] | Código, migraciones EF Core, troubleshooting |

---

**Última verificación de esta guía:** Enero 2026  
**Próxima revisión recomendada:** Cada trimestre o ante cambios de infraestructura
