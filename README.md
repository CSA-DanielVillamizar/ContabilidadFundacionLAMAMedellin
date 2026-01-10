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
- Backups automáticos programables
- Configuración de entidad RTE
- Actualización masiva de deudores
- Corrección de datos históricos
- Panel de administración completo

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

## 🔐 Roles y permisos

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
  "Backup": {
    "Enabled": true,
    "Schedule": "0 2 * * *",
    "Path": "C:\\Backups\\LamaMedellin",
    "RetentionDays": 30
  }
}
```

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
