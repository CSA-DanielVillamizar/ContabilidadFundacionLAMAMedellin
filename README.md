# L.A.M.A. Medellín - Sistema de Tesorería# L.A.M.A. Medellín - Tesorería (esqueleto)



Sistema de gestión de tesorería desarrollado en .NET 8 con Blazor Server, aplicando Clean Architecture y principios SOLID.Proyecto esqueleto para la aplicación de tesorería en .NET 8 - Blazor Server.



## 🚀 Características principalesPasos rápidos:



- **Gestión de miembros**: Importación desde CSV/Excel, estados (Activo/Inactivo)1. Restaurar paquetes y agregar NuGet necesarios (ejecutar en la carpeta /src/Server):

- **Recibos de caja**: Generación automática con PDF, QR y numeración consecutiva

- **Control de egresos**: CRUD completo con archivos adjuntos y control de roles   dotnet add package Microsoft.EntityFrameworkCore.SqlServer

- **Deudores de mensualidad**: Cálculo automático con filtros por rango, TRM histórica por mes, exportaciones Excel/PDF   dotnet add package Microsoft.EntityFrameworkCore.Tools

- **Reportes de tesorería**: Exportación mensual a PDF y Excel   dotnet add package CsvHelper

- **TRM (Tasa de Cambio)**: Sincronización automática USD→COP desde fuente externa   dotnet add package ClosedXML

   dotnet add package QuestPDF

## 📋 Requisitos previos   dotnet add package QRCoder



- .NET 8 SDK2. Configura `appsettings.json` usando `appsettings.sample.json` (connection string a LocalDB o SQL Server Express).

- SQL Server (LocalDB o Express)

- Visual Studio 2022+ o VS Code con extensiones de C#3. Generar migraciones y actualizar la base de datos (desde /src/Server):



## ⚙️ Instalación   dotnet ef migrations add Init_Treasury

   dotnet ef database update

### 1. Restaurar paquetes

4. Ejecutar la aplicación:

Ejecutar desde la carpeta `/src/Server`:

   dotnet run --project src/Server

```bash

dotnet restoreNotas:

```- El proyecto generado es un esqueleto con modelos, DbContext, servicios de importación, TRM básico y recibos (PDF + QR).

- Revisa Program.cs para ajustes de Identity, políticas y otros servicios.

### 2. Configurar conexión a base de datos- Coloca el logo en `src/Server/wwwroot/img/logo-lama-medellin.png`.


Copiar `appsettings.sample.json` a `appsettings.json` y configurar el `ConnectionString`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LamaMedellin;Trusted_Connection=True;"
  }
}
```

### 3. Crear base de datos

Desde `/src/Server`:

```bash
dotnet ef migrations add Init_Treasury
dotnet ef database update
```

### 4. Ejecutar aplicación

```bash
dotnet run --project src/Server
```

La aplicación estará disponible en: `https://localhost:5001`

## 🏗️ Arquitectura

### Capas

```
┌─────────────────────────────────────┐
│   Presentación (Blazor Server)      │
│   - Pages/Tesoreria/*.razor          │
│   - Controllers (API REST)           │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   Capa de Negocio (Services)        │
│   - IDeudoresService                 │
│   - IDeudoresExportService           │
│   - IRecibosService                  │
│   - IEgresosService                  │
│   - IReportesService                 │
│   - IExchangeRateService             │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   Capa de Datos (EF Core)            │
│   - AppDbContext                     │
│   - Miembro, Recibo, Egreso, etc.    │
└─────────────────────────────────────┘
```

### Principios aplicados

- ✅ Clean Architecture (separación en capas)
- ✅ Dependency Injection
- ✅ SOLID principles
- ✅ Repository pattern (vía EF Core)
- ✅ Unit of Work (DbContext)

## 📊 Funcionalidades principales

### 1. Gestión de Deudores

**Ubicación UI**: `/tesoreria/deudores`

#### Características

- **Cálculo automático** de meses adeudados por miembro activo
- **Filtros por rango de fechas** (`desde` / `hasta` en formato yyyy-MM)
- **TRM histórica por mes**: totales precisos cuando la mensualidad está en USD
- **Exportaciones**:
  - Excel (`.xlsx`)
  - PDF con QuestPDF
- **Generación de recibos** para múltiples meses (rol Tesorero/Junta)

#### API Endpoints

| Método | Endpoint | Descripción | Roles |
|--------|----------|-------------|-------|
| GET | `/api/deudores` | Lista deudores con totales | Tesorero, Junta, Consulta |
| GET | `/api/deudores/excel` | Exporta a Excel | Tesorero, Junta, Consulta |
| GET | `/api/deudores/pdf` | Exporta a PDF | Tesorero, Junta, Consulta |
| POST | `/api/deudores/generar-recibo` | Genera recibo por miembro | Tesorero, Junta |

#### Ejemplo de uso (API)

**Consultar deudores del primer semestre 2024:**

```bash
GET /api/deudores?desde=2024-01&hasta=2024-06
```

**Respuesta (JSON):**

```json
[
  {
    "miembroId": "guid...",
    "nombre": "Juan Pérez",
    "ingreso": "2023-01-01",
    "mesesPendientes": ["2024-01", "2024-02", "2024-03"],
    "precioMensualCop": 25000,
    "totalEstimadoCop": 75000
  }
]
```

**Exportar a Excel:**

```bash
GET /api/deudores/excel?desde=2024-01&hasta=2024-12
```

Retorna archivo `deudores.xlsx` con:
- Content-Type: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename="deudores.xlsx"`

**Generar recibo para miembro deudor:**

```bash
POST /api/deudores/generar-recibo
Content-Type: application/json

{
  "miembroId": "guid...",
  "cantidadMeses": 3
}
```

#### Lógica de cálculo

1. **Rango de meses**: desde `FechaIngreso` del miembro (o primer mes del rango) hasta `hasta` (o mes actual).
2. **Exclusión de meses pagados**: se filtran meses con recibos emitidos y pagados.
3. **TRM por mes**: si la mensualidad está en USD, se consulta la TRM del mes correspondiente para cada mes adeudado.
4. **Total estimado**: suma de `PrecioBase × TRM(mes)` para cada mes pendiente.

### 2. Gestión de Egresos

**Ubicación UI**: `/tesoreria/egresos`

#### Características

- CRUD completo (Create, Read, Update, Delete)
- Adjuntar archivos (PDF, imágenes, documentos)
- Filtros por fecha y concepto
- Control de roles (Tesorero/Junta puede crear/editar/eliminar)
- Archivos almacenados en `wwwroot/data/egresos/`

#### API Endpoints

| Método | Endpoint | Descripción | Roles |
|--------|----------|-------------|-------|
| GET | `/api/egresos` | Lista egresos | Tesorero, Junta, Consulta |
| POST | `/api/egresos` | Crear egreso | Tesorero, Junta |
| PUT | `/api/egresos/{id}` | Actualizar egreso | Tesorero, Junta |
| DELETE | `/api/egresos/{id}` | Eliminar egreso | Tesorero, Junta |

### 3. Recibos de Caja

**Ubicación UI**: `/tesoreria/recibos`

#### Características

- Numeración consecutiva automática por año
- Generación de PDF con QR
- Soporte para conceptos recurrentes (mensualidad) y únicos
- Estados: Borrador → Emitido → Anulado
- TRM aplicada automáticamente para conceptos en USD

#### API Endpoints

| Método | Endpoint | Descripción | Roles |
|--------|----------|-------------|-------|
| GET | `/api/recibos` | Lista recibos | Tesorero, Junta, Consulta |
| POST | `/api/recibos` | Crear y emitir recibo | Tesorero, Junta |
| GET | `/api/recibos/{id}` | Obtener recibo | Tesorero, Junta, Consulta |
| GET | `/api/recibos/{id}/pdf` | Descargar PDF | Tesorero, Junta, Consulta |

### 4. Reportes de Tesorería

**Ubicación UI**: `/tesoreria/reportes`

#### Características

- Reporte mensual consolidado (ingresos vs egresos)
- Exportación a Excel y PDF
- Logos personalizables
- Formato de moneda colombiana (COP)

#### API Endpoints

| Método | Endpoint | Descripción | Roles |
|--------|----------|-------------|-------|
| GET | `/api/reportes/tesoreria` | Datos del reporte | Tesorero, Junta, Consulta |
| GET | `/api/reportes/tesoreria/excel` | Exportar Excel | Tesorero, Junta, Consulta |
| GET | `/api/reportes/tesoreria/pdf` | Exportar PDF | Tesorero, Junta, Consulta |

## 🧪 Tests

### Ejecutar todos los tests

```bash
dotnet test
```

### Cobertura actual

- **29 tests** (Unit + E2E)
- ✅ Unit tests: `DeudoresService`, `EgresosService`
- ✅ E2E tests: Deudores (GET/exports/generar-recibo), Egresos (CRUD), Reportes (exports)

### Estructura de tests

```
tests/
└── UnitTests/
    ├── DeudoresServiceTests.cs      (8 tests)
    ├── DeudoresE2ETests.cs          (6 tests)
    ├── EgresosServiceTests.cs       (3 tests)
    ├── EgresosE2ETests.cs           (6 tests)
    └── ReportesE2ETests.cs          (6 tests)
```

## 🔐 Roles y permisos

| Rol | Permisos |
|-----|----------|
| **Tesorero** | Acceso completo (crear/editar/eliminar egresos, recibos, generar reportes) |
| **Junta** | Similar a Tesorero (supervisión y aprobaciones) |
| **Consulta** | Solo lectura (ver reportes, deudores, recibos) |

## 📦 Dependencias principales

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="CsvHelper" Version="30.0.1" />
<PackageReference Include="ClosedXML" Version="0.104.1" />
<PackageReference Include="QuestPDF" Version="2024.3.0" />
<PackageReference Include="QRCoder" Version="1.6.0" />
```

## 🎨 Personalización

### Logo

Coloca el logo en: `src/Server/wwwroot/img/logo-lama-medellin.png`

### TRM (Tasa de Cambio)

El servicio `ExchangeRateHostedService` sincroniza automáticamente la TRM cada 6 horas desde una fuente externa.

Para configurar manualmente:

```sql
INSERT INTO TasasCambio (Fecha, UsdCop, Fuente, ObtenidaAutomaticamente)
VALUES ('2024-01-15', 3950.00, 'Manual', 0);
```

## 📝 Convenciones de código

- **Idioma**: Comentarios y documentación en español técnico
- **Naming**: PascalCase para clases/métodos, camelCase para variables locales
- **Async**: sufijo `Async` para métodos asíncronos
- **DTOs**: record types para DTOs de API

## 🛠️ Comandos útiles

### Crear nueva migración

```bash
dotnet ef migrations add NombreMigracion --project src/Server
```

### Revertir última migración

```bash
dotnet ef migrations remove --project src/Server
```

### Actualizar base de datos

```bash
dotnet ef database update --project src/Server
```

### Generar script SQL

```bash
dotnet ef migrations script --project src/Server --output migration.sql
```

## 🐛 Troubleshooting

### Error: "Sequence contains no elements" en Deudores

**Causa**: No existe el concepto "MENSUALIDAD" en la base de datos.

**Solución**:

```sql
INSERT INTO Conceptos (Codigo, Nombre, PrecioBase, Moneda, EsIngreso, EsRecurrente, Periodicidad)
VALUES ('MENSUALIDAD', 'Mensualidad', 20000, 1, 1, 1, 1);
```

### Error: Testing authentication no funciona

**Causa**: Falta el header `X-Test-Role` en las pruebas E2E.

**Solución**: Asegurar que el cliente de test incluya:

```csharp
client.DefaultRequestHeaders.Add("X-Test-Role", "Tesorero");
```

## 📚 Recursos adicionales

- [Documentación de .NET 8](https://learn.microsoft.com/dotnet/)
- [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor/)
- [QuestPDF](https://www.questpdf.com/)
- [ClosedXML](https://github.com/ClosedXML/ClosedXML)

## 📄 Licencia

Este proyecto es de uso interno para L.A.M.A. Medellín.

## 👥 Contribución

Para contribuir al proyecto:

1. Crear una rama feature: `git checkout -b feature/nueva-funcionalidad`
2. Realizar cambios siguiendo las convenciones de código
3. Agregar tests unitarios y E2E
4. Ejecutar `dotnet test` para verificar
5. Crear Pull Request con descripción detallada

---

**Versión**: 1.0.0  
**Última actualización**: Octubre 2025
