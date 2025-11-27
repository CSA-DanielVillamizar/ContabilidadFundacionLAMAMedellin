# 🚀 Guía de Deployment a Producción - Sistema Contabilidad LAMA Medellín

## 📋 Tabla de Contenidos
1. [Pre-requisitos](#pre-requisitos)
2. [Configuración de Ambiente de Producción](#configuración-de-ambiente-de-producción)
3. [Variables de Entorno](#variables-de-entorno)
4. [Base de Datos](#base-de-datos)
5. [Publicación de la Aplicación](#publicación-de-la-aplicación)
6. [Deployment en IIS](#deployment-en-iis)
7. [Deployment en Azure App Service](#deployment-en-azure-app-service)
8. [Deployment con Docker](#deployment-con-docker)
9. [Health Checks y Monitoreo](#health-checks-y-monitoreo)
10. [Troubleshooting](#troubleshooting)

---

## 🔧 Pre-requisitos

### Servidor de Producción
- **Sistema Operativo**: Windows Server 2019+ o Linux (Ubuntu 20.04+)
- **.NET Runtime**: .NET 8.0 Runtime (ASP.NET Core)
- **Base de Datos**: SQL Server 2019+ o Azure SQL Database
- **Memoria RAM**: Mínimo 4 GB, recomendado 8 GB
- **Espacio en Disco**: Mínimo 10 GB libres

### Herramientas de Desarrollo
- **.NET SDK**: .NET 8.0 SDK (para publicar)
- **Visual Studio 2022** o **VS Code** con extensión C#
- **SQL Server Management Studio** (SSMS) o Azure Data Studio

---

## ⚙️ Configuración de Ambiente de Producción

### 1. Archivo `appsettings.Production.json`

Ya está creado en `src/Server/appsettings.Production.json` con tokens para reemplazo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Smtp": {
    "Host": "#{SmtpHost}#",
    "User": "#{SmtpUser}#",
    "Password": "#{SmtpPassword}#"
  }
}
```

### 2. Reemplazar Tokens con Valores Reales

**Opción A: Transformación de archivos (recomendado para CI/CD)**

Usar herramientas como:
- Azure DevOps: **Replace Tokens** task
- GitHub Actions: `sed` o `envsubst`
- Manual: PowerShell script

**Opción B: Variables de Entorno (más seguro)**

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=LamaMedellin;..."
export Smtp__Password="super-secret-password"

# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=LamaMedellin;..."
$env:Smtp__Password="super-secret-password"
```

**Opción C: Azure Key Vault (producción empresarial)**

Integrar con Azure Key Vault para secretos:

```csharp
// En Program.cs (ya existente en algunas configuraciones Azure)
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["KeyVault:Uri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}
```

---

## 🌍 Variables de Entorno

### Variables Críticas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server | `Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;` |
| `Smtp__Host` | Servidor SMTP | `smtp.office365.com` |
| `Smtp__Port` | Puerto SMTP | `587` |
| `Smtp__User` | Usuario SMTP | `tesoreria@fundacionlamamedellin.org` |
| `Smtp__Password` | Contraseña SMTP | `***` |
| `Smtp__From` | Email remitente | `gerencia@fundacionlamamedellin.org` |
| `Backup__BackupPath` | Ruta para backups | `D:\Backups\LamaMedellin` |
| `TwoFactorEnforcement__EnforceAfterGracePeriod` | Forzar 2FA después del periodo de gracia | `true` |

### Configurar Variables en IIS

1. Abrir **IIS Manager**
2. Seleccionar el sitio web
3. **Configuration Editor** → `system.webServer/aspNetCore`
4. Editar `environmentVariables`
5. Agregar cada variable con su valor

### Configurar Variables en Azure App Service

```bash
# Azure CLI
az webapp config appsettings set --resource-group miGrupo --name miApp \
  --settings ASPNETCORE_ENVIRONMENT=Production \
             ConnectionStrings__DefaultConnection="Server=..."

# O desde Azure Portal: Configuration → Application settings
```

---

## 🗄️ Base de Datos

### 1. Crear Base de Datos en Producción

**SQL Server (on-premises o Azure SQL)**

```sql
CREATE DATABASE LamaMedellin;
GO

-- Crear usuario de aplicación (NO usar SA)
CREATE LOGIN lama_app_user WITH PASSWORD = 'SuperSecurePassword123!';
GO

USE LamaMedellin;
GO

CREATE USER lama_app_user FOR LOGIN lama_app_user;
GO

-- Permisos mínimos necesarios
ALTER ROLE db_datareader ADD MEMBER lama_app_user;
ALTER ROLE db_datawriter ADD MEMBER lama_app_user;
GO
```

### 2. Ejecutar Migraciones

**Opción A: Desde el código (automático al iniciar)**

El `Program.cs` ya contiene lógica para aplicar migraciones automáticamente:

```csharp
// Program.cs (líneas 220-230 aprox)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // ← Aplica migraciones pendientes
}
```

**Opción B: Manualmente con dotnet ef**

```bash
cd src/Server
dotnet ef database update --connection "Server=prod-server;Database=LamaMedellin;..."
```

**Opción C: Script SQL (para DBAs)**

```bash
# Generar script SQL de migraciones
dotnet ef migrations script --idempotent --output migrate.sql
# Enviar migrate.sql al DBA para ejecución manual
```

### 3. Seed de Datos Iniciales

El sistema verifica automáticamente si ya existe seed de producción:

```csharp
// SeedData.cs
if (!await context.Miembros.AnyAsync())
{
    await SeedRolesYUsuarios(context, roleManager, userManager);
}
```

**Verificar seed exitoso** en logs:

```
✓ Roles de usuarios creados
✓ Usuario administrador creado
✓ Políticas de autorización configuradas
✓ Seed completado exitosamente
```

---

## 📦 Publicación de la Aplicación

### 1. Compilar en Modo Release

```bash
cd src/Server
dotnet publish -c Release -o ../../publish
```

**Output esperado:**

```
publish/
├── Server.dll
├── Server.pdb
├── web.config
├── appsettings.json
├── appsettings.Production.json
├── wwwroot/
├── Backups/ (se creará en runtime)
└── ...
```

### 2. Verificar Archivos Publicados

- ✅ `Server.dll` existe
- ✅ `web.config` existe (para IIS)
- ✅ `appsettings.Production.json` existe
- ✅ `wwwroot/` contiene archivos estáticos
- ✅ `Backups/` folder (se crea automáticamente)

---

## 🌐 Deployment en IIS

### 1. Instalar ASP.NET Core Hosting Bundle

Descargar e instalar desde:
- [.NET 8.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2. Crear Sitio Web en IIS

1. Abrir **IIS Manager**
2. Click derecho en **Sites** → **Add Website**
   - **Site name**: `LamaMedellin`
   - **Physical path**: `C:\inetpub\wwwroot\LamaMedellin` (copiar archivos de `publish/` aquí)
   - **Binding**: HTTP port 80 o HTTPS port 443
3. **Application Pool**:
   - **Name**: `LamaMedellin_AppPool`
   - **.NET CLR version**: **No Managed Code**
   - **Managed pipeline mode**: Integrated
   - **Identity**: `ApplicationPoolIdentity` (o cuenta personalizada con permisos SQL)

### 3. Configurar Permisos

```powershell
# PowerShell (ejecutar como Administrador)
$path = "C:\inetpub\wwwroot\LamaMedellin"
icacls $path /grant "IIS AppPool\LamaMedellin_AppPool:(OI)(CI)F" /T
```

### 4. Configurar Variables de Entorno en IIS

Ver sección [Variables de Entorno](#variables-de-entorno) arriba.

### 5. Iniciar el Sitio

1. En IIS Manager, seleccionar el sitio `LamaMedellin`
2. Click en **Start** (panel derecho)
3. Abrir navegador: `http://localhost` o `https://tu-dominio.com`

### 6. Verificar Logs

**Logs de IIS/ASP.NET Core:**

```
C:\inetpub\wwwroot\LamaMedellin\Logs\stdout_*.log
```

**Logs de aplicación** (si se configura Serilog o similar):

```
C:\inetpub\wwwroot\LamaMedellin\Logs\app-*.log
```

---

## ☁️ Deployment en Azure App Service

### 1. Crear App Service

**Azure Portal:**

1. **Create a resource** → **Web App**
2. **Basics:**
   - **Name**: `lama-medellin-contabilidad`
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Windows o Linux
   - **Region**: East US 2 (o más cercana)
3. **Pricing plan**: B1 (Basic) o superior

**Azure CLI:**

```bash
az group create --name lama-rg --location eastus2

az appservice plan create --name lama-plan --resource-group lama-rg --sku B1

az webapp create --name lama-medellin-contabilidad \
  --resource-group lama-rg \
  --plan lama-plan \
  --runtime "DOTNET:8"
```

### 2. Configurar Connection String en Azure

**Portal:**
1. App Service → **Configuration** → **Connection strings**
2. **New connection string:**
   - **Name**: `DefaultConnection`
   - **Value**: `Server=tcp:lama-sql.database.windows.net,1433;Database=LamaMedellin;User ID=lama_app_user@lama-sql;Password=...;Encrypt=True;TrustServerCertificate=False;`
   - **Type**: SQLAzure

**CLI:**

```bash
az webapp config connection-string set \
  --name lama-medellin-contabilidad \
  --resource-group lama-rg \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:..."
```

### 3. Configurar Application Settings

```bash
az webapp config appsettings set \
  --name lama-medellin-contabilidad \
  --resource-group lama-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Smtp__Host=smtp.office365.com \
    Smtp__User=tesoreria@fundacionlamamedellin.org \
    Smtp__Password="***" \
    TwoFactorEnforcement__EnforceAfterGracePeriod=true
```

### 4. Deploy desde Visual Studio

1. Click derecho en proyecto `Server` → **Publish**
2. **Target**: Azure → Azure App Service (Windows/Linux)
3. Seleccionar el App Service creado
4. Click **Publish**

### 5. Deploy desde CLI

```bash
cd src/Server
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
az webapp deployment source config-zip \
  --name lama-medellin-contabilidad \
  --resource-group lama-rg \
  --src ../deploy.zip
```

### 6. Verificar Deployment

1. Abrir: `https://lama-medellin-contabilidad.azurewebsites.net`
2. Verificar logs en **Azure Portal** → App Service → **Log stream**

---

## 🐳 Deployment con Docker

### 1. Crear `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Server/Server.csproj", "Server/"]
RUN dotnet restore "Server/Server.csproj"
COPY src/Server/ Server/
WORKDIR "/src/Server"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]
```

### 2. Crear `docker-compose.yml` (con SQL Server)

```yaml
version: '3.8'

services:
  web:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=LamaMedellin;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
      - Smtp__Host=smtp.office365.com
      - Smtp__User=tesoreria@fundacionlamamedellin.org
      - Smtp__Password=${SMTP_PASSWORD}
    depends_on:
      - db
    volumes:
      - ./Backups:/app/Backups

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

### 3. Construir y Ejecutar

```bash
# Construir imagen
docker-compose build

# Ejecutar contenedores
docker-compose up -d

# Verificar logs
docker-compose logs -f web

# Detener
docker-compose down
```

### 4. Deploy en Azure Container Apps

```bash
# Crear Container Registry
az acr create --name lamaregistry --resource-group lama-rg --sku Basic

# Login en ACR
az acr login --name lamaregistry

# Tag y push de imagen
docker tag lama-medellin:latest lamaregistry.azurecr.io/lama-medellin:v1
docker push lamaregistry.azurecr.io/lama-medellin:v1

# Crear Container App
az containerapp create \
  --name lama-contabilidad \
  --resource-group lama-rg \
  --environment lama-env \
  --image lamaregistry.azurecr.io/lama-medellin:v1 \
  --target-port 80 \
  --ingress 'external' \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Server=..."
```

---

## 🏥 Health Checks y Monitoreo

### 1. Agregar Health Checks al `Program.cs`

```csharp
// En builder.Services
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "sql-server",
        tags: new[] { "db", "sql" });

// En app pipeline (después de app.UseRouting())
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Solo verifica que el proceso esté vivo
});
```

### 2. Monitoreo con Azure Application Insights

**Instalar NuGet:**

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

**Configurar en `Program.cs`:**

```csharp
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);
```

**Agregar a `appsettings.Production.json`:**

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
  }
}
```

### 3. Logging Estructurado con Serilog

**Instalar NuGet:**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

**Configurar en `Program.cs`:**

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

**Agregar a `appsettings.Production.json`:**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

## 🔍 Troubleshooting

### Error: "An error occurred using the connection to database"

**Causa:** Connection string incorrecta o servidor SQL no accesible.

**Solución:**
1. Verificar firewall del servidor SQL
2. Verificar connection string en variables de entorno
3. Probar conexión con SSMS o Azure Data Studio

### Error: "HTTP Error 500.31 - Failed to load ASP.NET Core runtime"

**Causa:** ASP.NET Core Hosting Bundle no instalado o versión incorrecta.

**Solución:**
1. Instalar [.NET 8.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Reiniciar IIS: `iisreset`

### Error: "Access to the path '/Backups' is denied"

**Causa:** IIS Application Pool no tiene permisos de escritura.

**Solución:**

```powershell
icacls "C:\inetpub\wwwroot\LamaMedellin\Backups" /grant "IIS AppPool\LamaMedellin_AppPool:(OI)(CI)F" /T
```

### Logs no se generan

**Causa:** Ruta de logs no existe o sin permisos.

**Solución:**
1. Verificar que `Logs/` existe en el directorio de publicación
2. Dar permisos de escritura al Application Pool

### Migraciones no se aplican automáticamente

**Causa:** `db.Database.Migrate()` falla silenciosamente.

**Solución:**
1. Revisar logs para excepciones de EF Core
2. Aplicar migraciones manualmente:

```bash
dotnet ef database update --connection "Server=..."
```

---

## 📚 Referencias

- [Deploy ASP.NET Core to IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
- [Deploy to Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore)
- [Docker for .NET](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Application Insights for ASP.NET Core](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)

---

**Versión**: 1.0  
**Última actualización**: ${new Date().toLocaleDateString('es-CO')}  
**Responsable**: Daniel Villamizar
