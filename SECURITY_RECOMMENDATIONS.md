# 🔒 RECOMENDACIONES DE SEGURIDAD CRÍTICAS PARA PRODUCCIÓN
**Fundación LAMA Medellín - Sistema de Contabilidad**

---

## **1. ⚠️ CAMBIAR PASSWORDS DE USUARIOS SEED**

**CRÍTICO:** Los usuarios de prueba tienen contraseñas conocidas públicamente en el código fuente:

```csharp
// src/Server/Data/Seed/IdentitySeed.cs
await EnsureTestUserAsync(userManager, "admin@fundacionlamamedellin.org", "Admin123!", new[] { "Admin" }, enable2FAForSeed);
await EnsureTestUserAsync(userManager, "tesorero@fundacionlamamedellin.org", "Tesorero123!", new[] { "Tesorero" }, enable2FAForSeed);
```

### **Acción Requerida:**
1. **Inmediatamente después del primer deployment:**
   ```powershell
   # Conectar a la aplicación de producción
   # Ir a /Identity/Account/Manage/ChangePassword
   ```
2. Cambiar contraseñas de **todos** los usuarios seed:
   - `admin@fundacionlamamedellin.org`
   - `tesorero@fundacionlamamedellin.org`
   - `contador@fundacionlamamedellin.org`
   - `junta@fundacionlamamedellin.org`
   - `consulta@fundacionlamamedellin.org`

3. Usar contraseñas **únicas, complejas, no reutilizadas**:
   - Mínimo 16 caracteres
   - Mezcla de mayúsculas, minúsculas, números y símbolos
   - **NUNCA** usar `Admin123!`, `Tesorero123!`, etc.

---

## **2. 🔐 MIGRAR A AZURE KEY VAULT**

### **Problema Actual:**
`appsettings.Production.json` tiene placeholders que serán reemplazados en pipeline:
```json
"ConnectionStrings": {
  "DefaultConnection": "#{ConnectionString}#"
},
"Smtp": {
  "Password": "#{SmtpPassword}#"
}
```

**Riesgo:** Si el pipeline de deployment expone logs, las credenciales pueden filtrarse.

### **Solución: Azure Key Vault**

#### **Paso 1: Crear Key Vault**
```powershell
# Crear Key Vault
az keyvault create --name lama-prod-kv --resource-group lama-prod-rg --location eastus

# Crear secretos
az keyvault secret set --vault-name lama-prod-kv --name "ConnectionString" --value "Server=...;Database=...;User Id=...;Password=..."
az keyvault secret set --vault-name lama-prod-kv --name "SmtpPassword" --value "tu-password-smtp-real"
```

#### **Paso 2: Configurar Managed Identity**
```powershell
# Habilitar Managed Identity en App Service
az webapp identity assign --name lama-contabilidad-prod --resource-group lama-prod-rg

# Obtener el Object ID de la Managed Identity
$principalId = az webapp identity show --name lama-contabilidad-prod --resource-group lama-prod-rg --query principalId -o tsv

# Dar permisos al App Service para leer secretos
az keyvault set-policy --name lama-prod-kv --object-id $principalId --secret-permissions get list
```

#### **Paso 3: Actualizar `Program.cs`**
```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// ✅ AGREGAR ANTES DE CUALQUIER CONFIGURACIÓN
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
{
    var keyVaultName = builder.Configuration["KeyVaultName"]; // ej: "lama-prod-kv"
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new DefaultAzureCredential());
    }
}

// Resto del código continúa igual...
```

#### **Paso 4: Configurar Variable de Entorno**
En Azure App Service → Configuration:
```
KeyVaultName = lama-prod-kv
```

#### **Paso 5: Actualizar `appsettings.Production.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""  // ← Se leerá desde Key Vault
  },
  "Smtp": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "User": "tesoreria@fundacionlamamedellin.org",
    "Password": "",  // ← Se leerá desde Key Vault
    "From": "gerencia@fundacionlamamedellin.org",
    "EnableSsl": true
  }
}
```

**Ventajas:**
- ✅ Credenciales **nunca** en archivos de configuración
- ✅ Rotación de secretos sin redesplegar aplicación
- ✅ Auditoría de acceso a secretos
- ✅ Encriptación en reposo y en tránsito

---

## **3. 🗄️ USAR AZURE SQL MANAGED IDENTITY**

**Problema:** Connection strings con SQL Authentication exponen usuario/contraseña.

**Solución:** Usar Managed Identity del App Service para conectar a Azure SQL.

### **Paso 1: Habilitar Entra ID en Azure SQL**
```powershell
# Configurar admin de Entra ID en SQL Server
az sql server ad-admin create --resource-group lama-prod-rg --server-name lama-sql-prod --display-name "Azure SQL Admin" --object-id <tu-object-id>
```

### **Paso 2: Crear usuario de base de datos para Managed Identity**
```sql
-- Conectar a la base de datos con cuenta de Entra ID admin
CREATE USER [lama-contabilidad-prod] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [lama-contabilidad-prod];
ALTER ROLE db_datawriter ADD MEMBER [lama-contabilidad-prod];
ALTER ROLE db_ddladmin ADD MEMBER [lama-contabilidad-prod];  -- Solo si migraciones automáticas
```

### **Paso 3: Actualizar Connection String**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=lama-sql-prod.database.windows.net;Database=LamaMedellin;Authentication=Active Directory Default;TrustServerCertificate=False;"
}
```

**NOTA:** `Authentication=Active Directory Default` usa automáticamente la Managed Identity del App Service.

**Ventajas:**
- ✅ **Sin contraseñas** en connection strings
- ✅ Rotación automática de credenciales
- ✅ Mayor seguridad (Managed Identity ≠ SQL Auth)

---

## **4. 📧 DESHABILITAR ENDPOINTS DE DESARROLLO EN PRODUCCIÓN**

### **Problema:**
`Program.cs` expone endpoints sin autenticación en desarrollo:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/api/conceptos", ...).AllowAnonymous();
    app.MapGet("/dev/api/productos", ...).AllowAnonymous();
}
```

### **Validación:**
```powershell
# Verificar que endpoints /dev/* NO responden en producción
Invoke-RestMethod https://lama-contabilidad-prod.azurewebsites.net/dev/api/conceptos
# Debe retornar 404 Not Found
```

**Estado:** ✅ Correctamente protegido por `if (app.Environment.IsDevelopment())`

---

## **5. 🔒 HEADERS DE SEGURIDAD HTTP**

### **Agregar Middleware de Seguridad**

Crear archivo `src/Server/Middleware/SecurityHeadersMiddleware.cs`:
```csharp
namespace Server.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // HSTS (HTTP Strict Transport Security)
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        
        // Prevenir MIME sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        
        // Clickjacking protection
        context.Response.Headers["X-Frame-Options"] = "DENY";
        
        // XSS Protection (legacy, pero aún útil)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        
        // Content Security Policy (restrictivo)
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +  // MudBlazor requiere unsafe-inline/eval
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';";
        
        // Referrer Policy
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Permissions Policy (deshabilitar APIs peligrosas)
        context.Response.Headers["Permissions-Policy"] = 
            "geolocation=(), microphone=(), camera=(), payment=()";

        await _next(context);
    }
}
```

### **Registrar Middleware en `Program.cs`**
```csharp
var app = builder.Build();

// ✅ AGREGAR ANTES DE UseStaticFiles()
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<Server.Middleware.SecurityHeadersMiddleware>();
}

app.UseStaticFiles();
// ... resto del código
```

---

## **6. 🛡️ RATE LIMITING (PREVENCIÓN DE BRUTE FORCE)**

### **Agregar Rate Limiting en `Program.cs`**
```csharp
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ✅ AGREGAR DESPUÉS DE builder.Services.AddRazorPages()
builder.Services.AddRateLimiter(options =>
{
    // Límite global: 100 requests por minuto por IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10
        });
    });

    // Límite estricto para login: 5 intentos por 15 minutos
    options.AddPolicy("login", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(15)
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// ✅ AGREGAR DESPUÉS DE app.UseRouting()
app.UseRateLimiter();
```

### **Aplicar Rate Limiting a Páginas de Identity**
Crear `src/Server/Areas/Identity/Pages/Account/Login.cshtml.cs` (override):
```csharp
[EnableRateLimiting("login")]
public class LoginModel : PageModel
{
    // ... código existente
}
```

---

## **7. 📊 HABILITAR APPLICATION INSIGHTS**

### **Paso 1: Crear Recurso**
```powershell
az monitor app-insights component create --app lama-prod-insights --location eastus --resource-group lama-prod-rg --application-type web
```

### **Paso 2: Obtener Connection String**
```powershell
az monitor app-insights component show --app lama-prod-insights --resource-group lama-prod-rg --query connectionString -o tsv
```

### **Paso 3: Configurar en App Service**
```powershell
az webapp config appsettings set --name lama-contabilidad-prod --resource-group lama-prod-rg --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;IngestionEndpoint=..."
```

### **Paso 4: Agregar NuGet Package**
```powershell
cd src/Server
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### **Paso 5: Habilitar en `Program.cs`**
```csharp
var builder = WebApplication.CreateBuilder(args);

// ✅ AGREGAR AL INICIO
builder.Services.AddApplicationInsightsTelemetry();

// Resto del código...
```

### **Paso 6: Configurar Alertas**
```powershell
# Alerta por excepciones
az monitor metrics alert create --name lama-exceptions-alert --resource-group lama-prod-rg --scopes /subscriptions/.../resourceGroups/lama-prod-rg/providers/Microsoft.Insights/components/lama-prod-insights --condition "count exceptions/count > 10" --window-size 5m --evaluation-frequency 1m

# Alerta por response time alto
az monitor metrics alert create --name lama-response-time-alert --resource-group lama-prod-rg --scopes /subscriptions/.../resourceGroups/lama-prod-rg/providers/Microsoft.Insights/components/lama-prod-insights --condition "avg requests/duration > 3000" --window-size 5m --evaluation-frequency 1m
```

---

## **8. 🔄 BACKUP AUTOMÁTICO A AZURE BLOB STORAGE**

### **Problema Actual:**
`BackupService.cs` guarda backups localmente en disco, que se pierden si el App Service se reinicia.

### **Solución: Azure Blob Storage**

#### **Paso 1: Crear Storage Account**
```powershell
az storage account create --name lamaprodbackups --resource-group lama-prod-rg --location eastus --sku Standard_LRS

az storage container create --name sql-backups --account-name lamaprodbackups
```

#### **Paso 2: Habilitar Lifecycle Management**
```powershell
# Crear policy para borrar backups > 30 días
az storage account management-policy create --account-name lamaprodbackups --policy '{
  "rules": [
    {
      "enabled": true,
      "name": "DeleteOldBackups",
      "type": "Lifecycle",
      "definition": {
        "filters": {
          "blobTypes": ["blockBlob"],
          "prefixMatch": ["sql-backups/"]
        },
        "actions": {
          "baseBlob": {
            "delete": {
              "daysAfterModificationGreaterThan": 30
            }
          }
        }
      }
    }
  ]
}'
```

#### **Paso 3: Actualizar `BackupService.cs`**
```csharp
using Azure.Storage.Blobs;

public class BackupService : IBackupService
{
    private readonly BackupOptions _options;
    private readonly ILogger<BackupService> _logger;
    private readonly BlobServiceClient _blobClient;  // ✅ NUEVO

    public BackupService(
        IOptions<BackupOptions> options,
        ILogger<BackupService> logger,
        BlobServiceClient blobClient)  // ✅ INYECTAR
    {
        _options = options.Value;
        _logger = logger;
        _blobClient = blobClient;
    }

    public async Task<string> CreateBackupAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"backup_{_options.Database}_{timestamp}.bak";
        
        // 1. Crear backup SQL en disco temporal
        var tempPath = Path.Combine(Path.GetTempPath(), backupFileName);
        var sql = $"BACKUP DATABASE [{_options.Database}] TO DISK = '{tempPath}' WITH FORMAT, COMPRESSION;";
        
        using (var conn = new SqlConnection($"Server={_options.Server};Database=master;..."))
        {
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 600;  // 10 min
            await cmd.ExecuteNonQueryAsync();
        }

        // 2. Subir a Azure Blob Storage
        var containerClient = _blobClient.GetBlobContainerClient("sql-backups");
        var blobClient = containerClient.GetBlobClient(backupFileName);
        
        using (var fileStream = File.OpenRead(tempPath))
        {
            await blobClient.UploadAsync(fileStream, overwrite: false);
        }

        // 3. Limpiar archivo temporal
        File.Delete(tempPath);

        _logger.LogInformation($"✓ Backup subido a Azure Blob Storage: {backupFileName}");
        return blobClient.Uri.ToString();
    }
}
```

#### **Paso 4: Registrar `BlobServiceClient` en `Program.cs`**
```csharp
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
    return new BlobServiceClient(connectionString);
});
```

---

## **9. 🔍 ESCANEO DE VULNERABILIDADES (OWASP ZAP)**

### **Ejecutar antes de go-live:**
```powershell
# Docker con OWASP ZAP
docker run -t owasp/zap2docker-stable zap-baseline.py -t https://lama-contabilidad-prod-staging.azurewebsites.net

# Revisar reporte en HTML
```

**Verificar:**
- ✅ Sin SQL Injection
- ✅ Sin XSS (Cross-Site Scripting)
- ✅ Sin CSRF (mitigado por ASP.NET Core Identity)
- ✅ Headers de seguridad presentes

---

## **10. 📄 CHECKLIST FINAL PRE-GO-LIVE**

| **Item** | **Estado** | **Responsable** | **Fecha** |
|----------|------------|-----------------|-----------|
| Passwords de usuarios seed cambiadas | ⬜ | Admin | ________ |
| Azure Key Vault configurado | ⬜ | DevOps | ________ |
| Managed Identity para SQL habilitada | ⬜ | DevOps | ________ |
| Headers de seguridad habilitados | ⬜ | Dev | ________ |
| Rate limiting configurado | ⬜ | Dev | ________ |
| Application Insights activo | ⬜ | DevOps | ________ |
| Backups en Azure Blob Storage | ⬜ | DevOps | ________ |
| Escaneo OWASP ZAP sin issues críticos | ⬜ | Security | ________ |
| Smoke tests pasados en staging | ⬜ | QA | ________ |
| Plan de rollback documentado | ⬜ | DevOps | ________ |

---

**Aprobado por:** _________________________  
**Fecha:** _________________________
