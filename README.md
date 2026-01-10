# 🏛️ L.A.M.A. Medellín - Sistema Integral de Contabilidad

Sistema de gestión contable y tesorera desarrollado en .NET 8 con Blazor Server y MudBlazor, aplicando Clean Architecture y principios SOLID.

## 🎯 Descripción

Aplicación web completa para la gestión administrativa, contable y financiera de la Fundación L.A.M.A. Medellín. Incluye módulos de tesorería, contabilidad, inventario, facturación, gestión de miembros, certificados de donación y herramientas de auditoría.

## ✨ Módulos principales

### 📊 Tesorería
- **Recibos de caja**: Generación con PDF, QR, numeración consecutiva automática por año
- **Egresos**: Control completo con adjuntos, categorías y aprobaciones por rol
- **Deudores de mensualidad**: Cálculo automático, filtros por rango, TRM histórica, exportaciones
- **Conciliaciones bancarias**: Registro y seguimiento de movimientos bancarios
- **Presupuestos**: Creación, seguimiento y control de presupuestos por periodo
- **Reportes mensuales**: Consolidados de ingresos/egresos con Excel y PDF
- **Cierre contable mensual**: Proceso de cierre con validaciones y reportes
- **Verificación de tesorería**: Validación de saldos y movimientos
- **Certificados de donación**: Generación automática de certificados tributarios
- **TRM (Tasa de Cambio)**: Sincronización automática USD→COP cada 6 horas

### 👥 Gestión de Miembros
- Importación masiva desde CSV/Excel
- Estados: Activo/Inactivo/Suspendido
- Registro de fechas de ingreso y retiro
- Historial de movimientos

### 📦 Inventario y Productos
- CRUD de productos con seguimiento de stock
- Movimientos de inventario (entrada/salida)
- Alertas de stock bajo
- Historial de precios

### 🧾 Facturación
- Gestión de clientes y proveedores
- Cotizaciones
- Órdenes de compra
- Ventas con facturación
- Cuentas de cobro

### 🔐 Seguridad y Auditoría
- ASP.NET Core Identity con roles (Tesorero, Junta, Consulta, Admin)
- 2FA (Autenticación de dos factores) configurable
- Registro completo de auditoría (AuditLog)
- Página de auditoría con timeline y filtros avanzados

### 🛠️ Administración
- Backups automáticos programables en Azure Blob Storage con Managed Identity
- Health Checks (`/health`, `/health/ready`, `/health/live`)
- Configuración de entidad RTE
- Actualización masiva de deudores
- Corrección de datos históricos
- Panel de administración completo
- Endpoint de diagnóstico con información de configuración

## 📋 Requisitos previos

- **.NET 8 SDK** (última versión)
- **SQL Server 2019+** (LocalDB, Express o Enterprise)
- **Visual Studio 2022+** o **VS Code** con extensiones de C#
- **Node.js 18+** (para Tailwind CSS, opcional)

## ⚙️ Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/CSA-DanielVillamizar/ContabilidadFundacionLAMAMedellin.git
cd ContabilidadFundacionLAMAMedellin
```

### 2. Restaurar paquetes

```bash
cd src/Server
dotnet restore
```

### 3. Configurar base de datos

Copiar `appsettings.sample.json` a `appsettings.json`:

```bash
cp appsettings.sample.json appsettings.json
```

Editar `appsettings.json` y configurar el `ConnectionString`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Aplicar migraciones

```bash
dotnet ef database update
```

### 5. Ejecutar aplicación

```bash
dotnet run
```

La aplicación estará disponible en: `https://localhost:5001`

## 🏗️ Arquitectura

### Estructura de la solución

```
ContabilidadLAMAMedellin/
├── src/
│   └── Server/                    # Proyecto principal Blazor Server
│       ├── Areas/                 # Identity UI
│       ├── Components/            # Componentes Razor reutilizables
│       ├── Configuration/         # Clases de configuración
│       ├── Controllers/           # API REST Controllers (19 controllers)
│       ├── Data/                  # DbContext y configuración EF
│       ├── DTOs/                  # Data Transfer Objects
│       ├── Infrastructure/        # Handlers, middleware
│       ├── Migrations/            # Migraciones de EF Core
│       ├── Models/                # Entidades de dominio
│       ├── Pages/                 # Páginas Razor/Blazor
│       │   ├── Admin/            # Módulos administrativos (6 páginas)
│       │   ├── Configuracion/    # Configuraciones del sistema
│       │   ├── GerenciaNegocios/ # Módulo de negocios
│       │   ├── Miembros/         # Gestión de miembros
│       │   └── Tesoreria/        # Módulo de tesorería (19 páginas)
│       ├── Scripts/              # Scripts SQL y utilidades
│       ├── Security/             # Autenticación y autorización
│       ├── Services/             # Capa de lógica de negocio (30+ servicios)
│       │   ├── Audit/           # Servicios de auditoría
│       │   ├── Auth/            # Autenticación
│       │   ├── Backup/          # Respaldos
│       │   ├── CierreContable/  # Cierres mensuales
│       │   ├── Clientes/        # Gestión de clientes
│       │   ├── ConciliacionBancaria/
│       │   ├── Deudores/        # Cálculo de deudores
│       │   ├── Donaciones/      # Certificados de donación
│       │   ├── Egresos/         # Control de egresos
│       │   ├── Email/           # Envío de correos
│       │   ├── Exchange/        # TRM y tasas de cambio
│       │   ├── Export/          # Exportaciones (CSV, Excel)
│       │   ├── Import/          # Importaciones
│       │   ├── Inventario/      # Gestión de inventario
│       │   ├── Miembros/        # Servicios de miembros
│       │   ├── Recibos/         # Recibos de caja
│       │   └── Reportes/        # Generación de reportes
│       └── wwwroot/             # Assets estáticos (CSS, JS, imágenes)
├── tests/
│   ├── UnitTests/               # Tests unitarios (xUnit)
│   ├── Integration/             # Tests de integración
│   └── E2E/                     # Tests E2E (Playwright)
├── scripts/
│   ├── ActualizadorDeudores/    # Utilidad de actualización masiva
│   └── ValidateReportFlow/      # Validador de flujos de reportes
└── docs/                        # Documentación adicional
```

### Arquitectura en capas

```
┌───────────────────────────────────────────────┐
│         Presentación (Blazor Server)          │
│  - 19 páginas de tesorería                    │
│  - 6 páginas de administración                │
│  - Componentes MudBlazor reutilizables        │
│  - 19 Controllers API REST                    │
└───────────────────────────────────────────────┘
                     ▼
┌───────────────────────────────────────────────┐
│         Capa de Negocio (Services)            │
│  - 30+ servicios especializados               │
│  - DTOs para transferencia de datos           │
│  - Validaciones de negocio                    │
└───────────────────────────────────────────────┘
                     ▼
┌───────────────────────────────────────────────┐
│         Capa de Datos (EF Core)               │
│  - AppDbContext                               │
│  - 19 entidades de dominio                    │
│  - Configuraciones Fluent API                 │
└───────────────────────────────────────────────┘
```

### Principios aplicados

- ✅ **Clean Architecture** - Separación en capas bien definidas
- ✅ **Dependency Injection** - Inyección de dependencias en toda la aplicación
- ✅ **SOLID Principles** - Código mantenible y escalable
- ✅ **Repository Pattern** - Abstracción de acceso a datos vía EF Core
- ✅ **Unit of Work** - DbContext como unidad de trabajo

## 📊 API REST

La aplicación expone 19 controladores API REST:

### Tesorería
- `/api/recibos` - Gestión de recibos de caja
- `/api/egresos` - Control de egresos
- `/api/deudores` - Cálculo y consulta de deudores
- `/api/conciliacionbancaria` - Conciliaciones bancarias
- `/api/presupuestos` - Gestión de presupuestos
- `/api/certificadosdonacion` - Certificados tributarios
- `/api/reports` - Generación de reportes

### Contabilidad y Facturación
- `/api/clientes` - Gestión de clientes
- `/api/proveedores` - Gestión de proveedores
- `/api/cotizaciones` - Cotizaciones
- `/api/compras` - Órdenes de compra
- `/api/ventas` - Ventas y facturación
- `/api/cuentascobro` - Cuentas de cobro

### Inventario
- `/api/productos` - Productos
- `/api/inventario` - Movimientos de inventario

### Administración
- `/api/miembros` - Gestión de miembros
- `/api/conceptos` - Conceptos contables
- `/api/imports` - Importaciones masivas
- `/api/exportaciones` - Exportaciones a Excel/PDF

## 🧪 Tests

### Ejecutar tests

```bash
# Todos los tests
dotnet test

# Solo tests unitarios
dotnet test --filter "FullyQualifiedName~UnitTests"

# Solo tests E2E
dotnet test --filter "FullyQualifiedName~E2E"
```

### Cobertura actual

- **Unit Tests**: Servicios de deudores, egresos, recibos, exportaciones
- **Integration Tests**: Flujos completos de negocio
- **E2E Tests**: Pruebas de interfaz con Playwright

### Estructura de tests

```
tests/
├── UnitTests/
│   ├── Services/
│   │   ├── DeudoresServiceTests.cs
│   │   ├── EgresosServiceTests.cs
│   │   ├── RecibosServiceTests.cs
│   │   └── [otros servicios]
│   └── Helpers/
├── Integration/
│   └── [tests de integración]
└── E2E/
    ├── DeudoresE2ETests.cs
    ├── EgresosE2ETests.cs
    └── [otros tests E2E]
```

## 🔐 Seguridad

### Autenticación y Autorización
- **ASP.NET Core Identity** con roles (Admin, Tesorero, Junta, Consulta)
- **Managed Identity** para Azure SQL Database (sin credenciales expuestas)
- **Two-Factor Authentication (2FA)** configurable por usuario
- **Authorization policies** por rol y funcionalidad

### Protección de datos
- **Azure Key Vault** para gestión de secretos (configuración, connection strings)
- **HTTPS obligatorio** en producción (TLS 1.2+)
- **Endpoint de diagnóstico protegido** (`/api/diagnostico` - Admin only)
- **SQL Injection prevention** mediante Entity Framework Core y Parameterized Queries
- **CSRF tokens** en formularios Blazor

### Auditoría y Logging
- **Registro completo de auditoría** (AuditLog) con:
  - Usuario que realizó la acción
  - Timestamp exacto
  - Tipo de operación (Create, Update, Delete)
  - Valores anteriores y nuevos
- **Structured logging** con Serilog
- **Application Insights** para monitoreo en producción

### Rate Limiting
- **Limitación de tasa de solicitudes** (100 req/min por IP globalmente)
- **Protección en login** (máximo 5 intentos fallidos en 15 minutos)

### Security Headers
- **Content-Security-Policy (CSP)**
- **X-Frame-Options** contra clickjacking
- **X-Content-Type-Options** para prevenir MIME sniffing
- **Strict-Transport-Security** para HTTPS

---

## ☁️ Deployement en Azure

### Infraestructura recomendada

- **App Service** (B2 o superior) - Linux con .NET 8 runtime
- **Azure SQL Database** - Managed, con Entra ID authentication
- **Azure Blob Storage** - Para backups automáticos
- **Application Insights** - Monitoreo y diagnósticos
- **Azure Key Vault** - Gestión de secretos
- **App Service Plan** - Standard o Premium

### Backups automáticos en Azure Blob Storage

Los backups se almacenan automáticamente en Azure Blob Storage con **Managed Identity** (sin connection strings):

```json
{
  "Azure": {
    "StorageBlobServiceUri": "https://<storage-account>.blob.core.windows.net/",
    "BackupContainerName": "sql-backups",
    "UseAzureBlobBackup": true,
    "EnableKeyVault": true,
    "KeyVaultEndpoint": "https://<keyvault>.vault.azure.net/"
  },
  "Backup": {
    "Enabled": true,
    "CronSchedule": "0 2 * * *",
    "RetentionDays": 30
  }
}
```

**Configuración requerida:**
1. System Assigned Managed Identity habilitada en App Service
2. Rol "Storage Blob Data Contributor" asignado a la MI
3. App Setting: `Azure__StorageBlobServiceUri`
4. App Setting: `Azure__BackupContainerName=sql-backups`

Ver guía completa: [docs/AZURE_PRODUCTION_SETUP.md](docs/AZURE_PRODUCTION_SETUP.md)

### Health Checks

Endpoints de verificación disponibles:

- **`GET /health`** - Estado general de la aplicación
- **`GET /health/ready`** - Aplicación lista (incluye verificación de BD)
- **`GET /health/live`** - Aplicación viva (health check ligero)

Ejemplo de respuesta en `/health/ready`:

```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "status": "Healthy",
      "description": "Entity Framework Core database health check"
    }
  },
  "totalDuration": "00:00:00.1234567"
}
```

### Diagnóstico y monitoreo

**Endpoint protegido** (requiere rol Admin):

```bash
GET /api/diagnostico

# Respuesta:
{
  "timestamp": "2026-01-10T15:30:00Z",
  "environment": "Production",
  "version": "2.0.0",
  "azure": {
    "keyVaultEnabled": true,
    "keyVaultConfigured": true,
    "blobStorageEnabled": true,
    "blobStorageConfigured": true,
    "blobStorageAuthMethod": "ManagedIdentity",
    "storageConfigured": true,
    "backupReady": true,
    "appInsightsConfigured": true
  },
  "database": {
    "authenticationType": "ManagedIdentity",
    "connectionStringSet": true
  },
  "backup": {
    "enabled": true,
    "schedule": "0 2 * * *",
    "retentionDays": 30
  }
}
```

---

## � Health Checks

Monitorear la salud de la aplicación en producción:

```bash
# Estado general
curl https://tesorerialamamedellin.azurewebsites.net/health

# Listos para servir (readiness)
curl https://tesorerialamamedellin.azurewebsites.net/health/ready

# Vivo/responsivo (liveness)
curl https://tesorerialamamedellin.azurewebsites.net/health/live

# Diagnóstico completo (requiere Admin)
curl -H "Authorization: Bearer <token>" \
  https://tesorerialamamedellin.azurewebsites.net/api/diagnostico
```

**Ejemplo de respuesta `/health/ready`:**

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "keyVault": "Healthy",
    "blobStorage": "Healthy"
  },
  "totalDuration": "125ms"
}
```

**Configurar en Azure Monitor:**

1. En Azure Portal → App Service → Health Check
2. Ruta: `/health/ready`
3. Intervalo: 60 segundos
4. Umbral de error: 3

---

## 🔐 Seguridad

### Autenticación y Autorización

La aplicación implementa múltiples capas de seguridad:

**Identidad Administrada (Managed Identity):**
- ✅ Acceso a Azure SQL Database sin credentials
- ✅ Acceso a Azure Blob Storage (backups) sin connection strings
- ✅ Acceso a Azure Key Vault sin secrets en código
- ✅ Basado en RBAC (Role-Based Access Control)

```csharp
// En Program.cs
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(storageBlobServiceUri)
        .WithCredential(new DefaultAzureCredential());
});
```

**ASP.NET Core Identity:**
- ✅ Usuarios y contraseñas hasheadas (PBKDF2)
- ✅ Autenticación de 2 factores (2FA)
- ✅ Confirmación de email requerida
- ✅ Lockout temporal tras intentos fallidos

**Control de Acceso Basado en Roles (RBAC):**

| Rol | Permisos |
|-----|----------|
| **Admin** | Acceso completo, auditoría, diagnósticos, backups |
| **Tesorero** | Crear/editar recibos, consultas financieras |
| **Junta** | Consultas de reportes, estados financieros |
| **Consulta** | Lectura de datos públicos, reportes básicos |

### Protección de Datos

**Key Vault Integration:**
- Secretos almacenados en Azure Key Vault
- Acceso mediante Managed Identity
- Rotación automática de secretos

```bash
# Recuperar secreto (el app usa MI automáticamente)
az keyvault secret show \
  --vault-name kvtesorerialamamdln \
  --name ConnectionString-LamaMedellin
```

**HTTPS obligatorio:**
```csharp
// En Program.cs
app.UseHttpsRedirection();
```

**Protección contra CSRF:**
```html
<!-- Cada formulario genera token automáticamente -->
<form method="post" action="/api/recibos">
    @Html.AntiForgeryToken()
    <!-- ... -->
</form>
```

**Prevención de inyección SQL:**
```csharp
// ✅ SEGURO: Parámetros con Entity Framework Core
var miembros = await _context.Miembros
    .FromSqlInterpolated($"SELECT * FROM Miembros WHERE NumeroSocio = {numero}")
    .ToListAsync();

// ❌ INSEGURO (nunca hacer esto)
var sql = $"SELECT * FROM Miembros WHERE NumeroSocio = {numero}";
```

**Encabezados de Seguridad:**

```csharp
// En Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdn.jsdelivr.net; style-src 'self' 'unsafe-inline'");
    await next();
});
```

### Auditoría y Logging

**Tabla de Auditoría:**
```sql
SELECT TOP 10 
    Id,
    Usuario,
    Accion,
    Tabla,
    Valores,
    FechaHora
FROM AuditLog
ORDER BY FechaHora DESC;
```

**Logging Estructurado (Serilog):**
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "ApplicationInsights",
        "Args": { "connectionString": "InstrumentationKey=..." }
      }
    ]
  }
}
```

**Monitoreo en Application Insights:**
- Rastreo de excepciones
- Métricas de rendimiento
- Eventos personalizados de negocio

### Rate Limiting

Protección contra abuso y ataques de fuerza bruta:

```csharp
// Límite global: 100 solicitudes por minuto
// Límite de login: 5 intentos por 15 minutos
// Por IP y usuario

app.UseRateLimiter();
```

---

## ☁️ Deployment en Azure

### Arquitectura Recomendada

```
┌─────────────────────────────────────────────┐
│ Azure App Service (Blazor Server)           │
│ SKU: B2 o superior                          │
│ Runtime: .NET 8                             │
│ Managed Identity: System Assigned           │
└──────────────┬──────────────────────────────┘
               │
    ┌──────────┼──────────┬──────────────┐
    │          │          │              │
    ▼          ▼          ▼              ▼
┌────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│Azure   │ │Azure SQL │ │Azure     │ │Azure App │
│Key     │ │Database  │ │Blob      │ │Insights  │
│Vault   │ │          │ │Storage   │ │          │
└────────┘ └──────────┘ └──────────┘ └──────────┘
```

### Configuración de Backups con Managed Identity

**Archivo `appsettings.json` (Production):**

```json
{
  "Azure": {
    "StorageBlobServiceUri": "https://lamaprodstorage2025.blob.core.windows.net/",
    "StorageAccountName": "lamaprodstorage2025",
    "BackupContainerName": "sql-backups",
    "UseAzureBlobBackup": true,
    "EnableKeyVault": true,
    "KeyVaultEndpoint": "https://kvtesorerialamamdln.vault.azure.net/"
  },
  "Backup": {
    "Enabled": true,
    "CronSchedule": "0 2 * * *",
    "RetentionDays": 30,
    "Server": "sql-tesorerialamamedellin-prod.database.windows.net",
    "Database": "sqldb-tesorerialamamedellin-prod"
  }
}
```

**Configuración de RBAC (Role-Based Access Control):**

```bash
# 1. Obtener el Principal ID de la Managed Identity
PRINCIPAL_ID=$(az webapp identity show \
  --resource-group rg-tesorerialamamedellin-prod \
  --name tesorerialamamedellin \
  --query principalId -o tsv)

# 2. Asignar rol "Storage Blob Data Contributor"
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-tesorerialamamedellin-prod/providers/Microsoft.Storage/storageAccounts/lamaprodstorage2025

# 3. Verificar la asignación
az role assignment list \
  --assignee $PRINCIPAL_ID \
  --output table
```

**Método de Autenticación:** `DefaultAzureCredential()`
- Búsqueda automática de Managed Identity
- Sin almacenamiento de credenciales
- Fallback: Variables de entorno, CLI, Visual Studio

Ver guía detallada: [docs/AZURE_PRODUCTION_SETUP.md](docs/AZURE_PRODUCTION_SETUP.md)

### Endpoints de Health Check

```
GET /health                    → Estado general (público)
GET /health/ready             → Listos para servir (público)
GET /health/live              → Vivo/responsivo (público)
GET /api/diagnostico          → Diagnósticos completos (requiere Admin)
```

**Ejemplo de respuesta `/api/diagnostico` (requiere rol Admin):**

```json
{
  "applicationVersion": "1.0.0",
  "environment": "AzureProduction",
  "aspNetCoreEnvironment": "Production",
  "databaseConnected": true,
  "databaseName": "sqldb-tesorerialamamedellin-prod",
  "keyVaultEnabled": true,
  "keyVaultConnected": true,
  "storageConfigured": true,
  "backupReady": true,
  "blobStorageAuthMethod": "ManagedIdentity",
  "healthChecksPassed": 5,
  "lastBackup": "2025-01-13T02:15:30Z",
  "timestamp": "2025-01-13T10:45:22Z"
}
```

### Tabla de Configuración Requerida

| Configuración | Valor Ejemplo | Requerido | Notas |
|---------------|---------------|-----------|-------|
| `StorageBlobServiceUri` | `https://lamaprodstorage2025.blob.core.windows.net/` | Sí (Prod) | URI de Storage Account |
| `StorageAccountName` | `lamaprodstorage2025` | Alternativo | Si no se proporciona URI |
| `BackupContainerName` | `sql-backups` | Sí (Prod) | Debe ser exacto |
| `UseAzureBlobBackup` | `true` | Sí (Prod) | Falla en Prod si es false |
| `KeyVaultEndpoint` | `https://kvtesorerialamamdln.vault.azure.net/` | Sí (Prod) | Para secrets rotables |
| `EnableKeyVault` | `true` | Sí (Prod) | Requiere Key Vault |
| **RBAC Role** | `Storage Blob Data Contributor` | Sí | Asignado a System Assigned MI |
| **Managed Identity** | System Assigned | Sí | En App Service |

---

## �🔐 Roles y permisos

| Rol | Permisos |
|-----|----------|
| **Admin** | Acceso total al sistema, configuración avanzada |
| **Tesorero** | Crear/editar/eliminar recibos, egresos, conciliaciones, reportes |
| **Junta** | Similar a Tesorero + aprobaciones y supervisión |
| **Consulta** | Solo lectura (ver reportes, deudores, recibos) |

## 📦 Dependencias principales

```xml
<!-- Framework y UI -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="MudBlazor" Version="7.4.0" />

<!-- Base de datos -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />

<!-- Importación/Exportación -->
<PackageReference Include="CsvHelper" Version="30.0.1" />
<PackageReference Include="ClosedXML" Version="0.104.0" />

<!-- Generación de documentos -->
<PackageReference Include="QuestPDF" Version="2024.6.0" />
<PackageReference Include="QRCoder" Version="1.4.3" />

<!-- Compresión -->
<PackageReference Include="Microsoft.AspNetCore.ResponseCompression" Version="2.3.0" />
```

## 🎨 Personalización

### Logo de la organización

Coloca el logo en: `src/Server/wwwroot/img/logo-lama-medellin.png`

Formatos recomendados: PNG con fondo transparente, 200x200px mínimo

### Configuración de correo electrónico

Editar en `appsettings.json`:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "tu-correo@gmail.com",
    "Password": "tu-contraseña-app",
    "FromEmail": "noreply@lamemedellin.org",
    "FromName": "L.A.M.A. Medellín"
  }
}
```

### Tasas de cambio (TRM)

El servicio `ExchangeRateHostedService` sincroniza automáticamente la TRM cada 6 horas. Para configurar manualmente:

```sql
INSERT INTO TasasCambio (Fecha, UsdCop, Fuente, ObtenidaAutomaticamente)
VALUES ('2026-01-09', 4350.00, 'Manual', 0);
```

### Backups automáticos

Configurar en `appsettings.json`:

```json
{
  "Azure": {
    "StorageBlobServiceUri": "https://lamaprodstorage2025.blob.core.windows.net/",
    "BackupContainerName": "sql-backups",
    "UseAzureBlobBackup": true,
    "EnableKeyVault": true,
    "KeyVaultEndpoint": "https://kvtesorerialamamdln.vault.azure.net/"
  },
  "Backup": {
    "Enabled": true,
    "CronSchedule": "0 2 * * *",
    "Path": "Backups",
    "RetentionDays": 30,
    "Server": "sql-tesorerialamamedellin-prod.database.windows.net",
    "Database": "sqldb-tesorerialamamedellin-prod"
  }
}
```

**Características:**
- ✅ Backups automáticos diarios a las 2 AM UTC
- ✅ Autenticación con Managed Identity (sin credentials)
- ✅ Retención configurable (default 30 días)
- ✅ Compresión de archivos .bak
- ✅ Limpieza automática de backups antiguos

**Verificar backups:**

```bash
# Listar backups en Azure Blob Storage
az storage blob list \
  --container-name sql-backups \
  --account-name lamaprodstorage2025 \
  --auth-mode login \
  --output table
```

Ver guía rápida: [GUIA_RAPIDA_BACKUPS.md](GUIA_RAPIDA_BACKUPS.md)

## 📝 Convenciones de código

- **Idioma**: Comentarios y documentación en español técnico
- **Naming**: 
  - PascalCase para clases, métodos, propiedades
  - camelCase para variables locales y parámetros
  - Interfaces con prefijo `I` (ej: `IRecibosService`)
- **Async**: Sufijo `Async` para métodos asíncronos
- **DTOs**: Record types para DTOs de API
- **Servicios**: Separar interfaces de implementaciones

## 🛠️ Comandos útiles

### Entity Framework

```bash
# Nueva migración
dotnet ef migrations add NombreMigracion --project src/Server

# Revertir última migración
dotnet ef migrations remove --project src/Server

# Actualizar base de datos
dotnet ef database update --project src/Server

# Generar script SQL
dotnet ef migrations script --project src/Server --output migration.sql

# Ver migraciones pendientes
dotnet ef migrations list --project src/Server
```

### Compilación y ejecución

```bash
# Compilar en modo Release
dotnet build src/Server -c Release

# Ejecutar en modo Development
dotnet run --project src/Server --launch-profile "Development"

# Publicar aplicación
dotnet publish src/Server -c Release -o publish
```

### Tests

```bash
# Ejecutar con verbosidad
dotnet test -v detailed

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests específicos
dotnet test --filter "FullyQualifiedName~DeudoresServiceTests"
```

## 🐛 Troubleshooting

### Error: "Sequence contains no elements" en Deudores

**Causa**: No existe el concepto "MENSUALIDAD" en la base de datos.

**Solución**:

```sql
INSERT INTO Conceptos (Codigo, Nombre, PrecioBase, Moneda, EsIngreso, EsRecurrente, Periodicidad)
VALUES ('MENSUALIDAD', 'Mensualidad', 20000, 1, 1, 1, 1);
```

### Error: La aplicación no arranca en Testing

**Causa**: El entorno de Testing deshabilita ciertos servicios.

**Solución**: Verificar `appsettings.Test.json`:

```json
{
  "DisableHostedServices": true,
  "EnableIdentityInTesting": true
}
```

### Error: No se pueden generar PDFs

**Causa**: Falta la fuente Lato en `wwwroot/fonts/`.

**Solución**: Descargar fuentes Lato y colocarlas en el directorio especificado.

### Error: TRM no se actualiza

**Causa**: El servicio `ExchangeRateHostedService` está deshabilitado.

**Solución**: Verificar `DisableHostedServices=false` en `appsettings.json`.

## 📚 Recursos adicionales

- [Documentación de .NET 8](https://learn.microsoft.com/dotnet/)
- [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor/)
- [MudBlazor](https://mudblazor.com/)
- [QuestPDF](https://www.questpdf.com/)
- [ClosedXML](https://github.com/ClosedXML/ClosedXML)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

## 📄 Licencia

Este proyecto es de uso interno para L.A.M.A. Medellín.

## 👥 Contribución

Para contribuir al proyecto:

1. Crear rama feature: `git checkout -b feature/nueva-funcionalidad`
2. Realizar cambios siguiendo las convenciones de código
3. Agregar tests unitarios y E2E
4. Ejecutar `dotnet test` para verificar
5. Crear Pull Request con descripción detallada

---

**Versión**: 2.0.0  
**Última actualización**: Enero 2026  
**Desarrollado con**: .NET 8, Blazor Server, MudBlazor, Entity Framework Core
