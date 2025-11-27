# ✅ CHECKLIST DE DEPLOYMENT A PRODUCCIÓN
**Fundación LAMA Medellín - Sistema de Contabilidad**

---

## **1. CONFIGURACIÓN DE SEGURIDAD**

### **HTTPS & TLS**
- [ ] Certificado SSL válido instalado (no expirado)
- [ ] HTTPS Redirection habilitada (`app.UseHttpsRedirection()`)
- [ ] HSTS habilitado (`app.UseHsts()`)
- [ ] TLS 1.2+ configurado en servidor

### **COOKIES & SESIONES**
- [ ] Cookies con `Secure=true` (automático en HTTPS)
- [ ] `SameSite=Lax` o `Strict` configurado
- [ ] `HttpOnly=true` para prevenir XSS
- [ ] Timeout de sesión configurado (default: 14 días de cookie persistente)

### **AUTENTICACIÓN & AUTORIZACIÓN**
- [ ] 2FA obligatorio para Admin/Tesorero (`EnforceAfterGracePeriod=true`)
- [ ] Políticas de autorización correctas en las 13 páginas auditadas
- [ ] Account lockout habilitado (5 intentos, 10 min)
- [ ] Passwords: mínimo 8 caracteres, 1 dígito, 1 minúscula, 4 caracteres únicos

---

## **2. BASE DE DATOS**

### **CONNECTION STRING**
- [ ] Connection string no hardcoded en código
- [ ] Variable de entorno `#{ConnectionString}#` reemplazada en pipeline
- [ ] **RECOMENDADO:** Usar Azure SQL Managed Identity en lugar de SQL Auth

### **MIGRACIONES**
```powershell
# Validar migraciones pendientes antes de deploy
cd src/Server
dotnet ef migrations list
```
- [ ] Todas las migraciones aplicadas en staging
- [ ] Plan de rollback documentado si falla una migración
- [ ] Backup de base de datos **antes** de aplicar migraciones

### **PERMISOS**
- [ ] Usuario de aplicación con permisos mínimos (NO `db_owner`)
- [ ] Permisos específicos: `db_datareader`, `db_datawriter`, `db_ddladmin` (solo si se aplican migraciones automáticas)

---

## **3. SECRETOS & CONFIGURACIÓN**

### **AZURE KEY VAULT (RECOMENDADO)**
- [ ] Crear Azure Key Vault para secretos
- [ ] Migrar secretos desde appsettings:
  - `ConnectionString` → Key Vault secret
  - `SmtpPassword` → Key Vault secret
  - Cualquier API key externa
- [ ] Configurar Managed Identity del App Service para acceder a Key Vault
- [ ] Actualizar `Program.cs` para leer desde Key Vault:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### **VARIABLES DE ENTORNO (ALTERNATIVA)**
Si no se usa Key Vault, configurar en Azure App Service:
- [ ] `ConnectionStrings__DefaultConnection`
- [ ] `Smtp__Host`
- [ ] `Smtp__User`
- [ ] `Smtp__Password`
- [ ] `Smtp__From`
- [ ] `Backup__BackupPath`

---

## **4. SMTP & NOTIFICACIONES**

### **CONFIGURACIÓN**
- [ ] SMTP host configurado (ej: `smtp.office365.com`)
- [ ] Puerto 587 con TLS habilitado (`EnableSsl=true`)
- [ ] Credenciales SMTP válidas y probadas
- [ ] Email "From" autorizado por el servidor SMTP
- [ ] Certificado de donación: envío automático habilitado (`SendOnCertificateEmission=true`)

### **PRUEBAS**
```csharp
// Endpoint de diagnóstico en /diagnosticos/email (solo Admin)
```
- [ ] Enviar email de prueba desde la interfaz de diagnóstico
- [ ] Verificar recepción en buzón real
- [ ] Probar envío de certificado de donación

---

## **5. BACKUP AUTOMÁTICO**

### **CONFIGURACIÓN**
```json
"Backup": {
  "Enabled": true,
  "CronSchedule": "0 2 * * *",  // 2 AM diario
  "BackupPath": "/backups",      // Ruta en servidor
  "RetentionDays": 30            // 30 días de retención
}
```

### **VALIDACIONES**
- [ ] Carpeta de backups accesible por el servicio de aplicación
- [ ] Permisos de escritura en `BackupPath`
- [ ] Espacio suficiente en disco (mínimo 10 GB recomendado)
- [ ] **CRÍTICO:** Probar restauración de backup **antes** de producción

### **AZURE BLOB STORAGE (RECOMENDADO)**
En lugar de backups locales, usar Azure Blob Storage:
```csharp
// Modificar BackupService.cs para usar Azure.Storage.Blobs
var blobClient = new BlobServiceClient(connectionString);
```
- [ ] Crear Storage Account en Azure
- [ ] Configurar lifecycle management para retención automática
- [ ] Habilitar soft delete para recuperación de backups borrados

---

## **6. LOGGING & MONITORING**

### **AZURE APPLICATION INSIGHTS (RECOMENDADO)**
```csharp
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);
```
- [ ] Crear recurso de Application Insights en Azure
- [ ] Configurar Connection String en variables de entorno
- [ ] Habilitar métricas de performance (response time, exceptions)
- [ ] Configurar alertas para:
  - Excepciones no controladas
  - Response time > 3s
  - CPU > 80%
  - Memory > 90%

### **LOG LEVELS**
Verificar `appsettings.Production.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",              // ✅ No Information en prod
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore.Database.Command": "Warning"  // ✅ No logear SQL queries
  }
}
```

---

## **7. PERFORMANCE & ESCALABILIDAD**

### **OUTPUT CACHING**
- [ ] Output caching habilitado para endpoints de lectura (`/dev/api/conceptos`, etc.)
- [ ] Políticas de caché configuradas (default: 5 min)

### **RESPONSE COMPRESSION**
- [ ] Compresión habilitada para HTTPS (`EnableForHttps=true`)
- [ ] Verificar headers `Content-Encoding: gzip` en producción

### **DATABASE INDEXING**
```sql
-- Validar índices críticos antes de deploy
SELECT * FROM sys.dm_db_missing_index_details
```
- [ ] Índices en columnas frecuentemente filtradas (`Recibos.FechaEmision`, `Conceptos.Nombre`, etc.)
- [ ] Ejecutar ANALYZE TABLE después de carga masiva de datos

---

## **8. SEEDS & DATOS INICIALES**

### **PRODUCCIÓN**
Verificar `Program.cs`:
```csharp
// ❌ DESHABILITADO - datos de prueba
// await Recibos2025Seed.SeedAsync(db);

// ✅ HABILITADO - saldo inicial
await HistoricoTesoreria2025Seed.SeedAsync(db);
```

### **USUARIOS INICIALES**
- [ ] Seed de roles ejecutado (`Admin`, `Tesorero`, `Contador`, `Junta`, `Consulta`, `gerentenegocios`)
- [ ] **CRÍTICO:** Cambiar passwords de usuarios seed:
  - `admin@fundacionlamamedellin.org` → contraseña **NO** debe ser `Admin123!`
  - `tesorero@fundacionlamamedellin.org` → contraseña **NO** debe ser `Tesorero123!`
- [ ] Forzar cambio de contraseña en primer login

---

## **9. DEPLOYMENT A AZURE APP SERVICE**

### **PRE-DEPLOYMENT**
- [ ] Backup de base de datos de producción
- [ ] Plan de rollback documentado
- [ ] Ventana de mantenimiento comunicada a usuarios

### **CONFIGURACIÓN DEL APP SERVICE**
```powershell
# Crear App Service Plan (Linux)
az appservice plan create --name lama-prod-plan --resource-group lama-prod-rg --is-linux --sku P1V2

# Crear Web App
az webapp create --name lama-contabilidad-prod --resource-group lama-prod-rg --plan lama-prod-plan --runtime "DOTNETCORE:8.0"
```

- [ ] App Service Plan: **P1V2** o superior (producción)
- [ ] Runtime: **.NET 8**
- [ ] Platform: **Linux** (más económico que Windows)
- [ ] Always On: **Habilitado** (evita cold starts)
- [ ] HTTPS Only: **Habilitado**

### **DEPLOYMENT SLOTS (RECOMENDADO)**
```powershell
# Crear slot de staging
az webapp deployment slot create --name lama-contabilidad-prod --resource-group lama-prod-rg --slot staging
```
- [ ] Slot `staging` para validación pre-producción
- [ ] Swap a producción después de validar en staging

### **PIPELINE DE CI/CD (AZURE DEVOPS)**
```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.x'
  
  - task: DotNetCoreCLI@2
    displayName: 'Restore'
    inputs:
      command: restore
      projects: '**/*.csproj'
  
  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: build
      projects: '**/*.csproj'
      arguments: '--configuration Release'
  
  - task: DotNetCoreCLI@2
    displayName: 'Run Integration Tests'
    inputs:
      command: test
      projects: '**/tests/Integration/*.csproj'
  
  - task: DotNetCoreCLI@2
    displayName: 'Publish'
    inputs:
      command: publish
      projects: 'src/Server/Server.csproj'
      arguments: '--configuration Release --output $(Build.ArtifactStagingDirectory)'
  
  - task: AzureRmWebAppDeployment@4
    displayName: 'Deploy to Staging Slot'
    inputs:
      azureSubscription: 'lama-prod-subscription'
      appType: 'webAppLinux'
      WebAppName: 'lama-contabilidad-prod'
      deployToSlotOrASE: true
      ResourceGroupName: 'lama-prod-rg'
      SlotName: 'staging'
      packageForLinux: '$(Build.ArtifactStagingDirectory)/**/*.zip'
```

---

## **10. VALIDACIÓN POST-DEPLOYMENT**

### **SMOKE TESTS**
- [ ] Login con usuario Admin
- [ ] Login con usuario Tesorero
- [ ] Verificar 2FA funciona correctamente
- [ ] Crear recibo de prueba
- [ ] Exportar certificado de donación
- [ ] Verificar envío de email
- [ ] Validar generación de reporte PDF

### **PERFORMANCE**
```powershell
# Endpoint de health check
Invoke-RestMethod https://lama-contabilidad-prod.azurewebsites.net/api/health/perf
```
- [ ] Response time < 500ms para `/api/health/perf`
- [ ] Verificar métricas en Application Insights

### **SEGURIDAD**
- [ ] Escaneo de vulnerabilidades con OWASP ZAP
- [ ] Verificar headers de seguridad:
  - `Strict-Transport-Security: max-age=31536000`
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`

---

## **11. DOCUMENTACIÓN**

- [ ] README actualizado con instrucciones de deployment
- [ ] Variables de entorno documentadas
- [ ] Plan de recuperación ante desastres (DR)
- [ ] Contactos de soporte técnico
- [ ] Procedimientos de rollback

---

## **12. COMPLIANCE & LEGAL**

### **RETENCIÓN DE DATOS TRIBUTARIOS**
Según normativa colombiana (DIAN):
- [ ] Recibos: **5 años** de retención mínima
- [ ] Certificados de donación: **5 años**
- [ ] Libros contables: **10 años**

### **PROTECCIÓN DE DATOS PERSONALES (LEY 1581 DE 2012)**
- [ ] Política de privacidad publicada
- [ ] Autorización de tratamiento de datos de miembros
- [ ] Proceso de ejercicio de derechos ARCO (Acceso, Rectificación, Cancelación, Oposición)

---

## **✅ APROBACIONES FINALES**

- [ ] **Representante Legal:** Aprueba go-live
- [ ] **Contador Público:** Valida estructura contable
- [ ] **IT/DevOps:** Confirma infraestructura lista
- [ ] **Usuario final (Tesorero):** Valida funcionalidad

---

**Fecha de revisión:** _________________________
**Aprobado por:** _________________________
**Fecha de deployment:** _________________________
