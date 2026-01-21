# INVENTORY.md — Inventario del Sistema Actual

**Fecha**: 2026-01-21  
**Proyecto**: Sistema de Tesorería/Contabilidad Fundación L.A.M.A. Medellín  
**Objetivo**: Documentar estado actual del sistema antes de implementar nuevas fases funcionales

---

## 1. ARQUITECTURA Y ESTRUCTURA DEL PROYECTO

### 1.1 Tecnología Base
- **Framework**: ASP.NET Core 8.0 (Blazor Server)
- **UI**: MudBlazor components
- **Base de datos**: Azure SQL Database / SQL Server LocalDB (desarrollo)
- **ORM**: Entity Framework Core 8.0
- **Autenticación**: ASP.NET Core Identity
- **Logging**: Serilog + Azure Application Insights
- **Infraestructura**: Azure (App Service, Key Vault, Blob Storage, Application Insights)

### 1.2 Capas de la Aplicación

```
ContabilidadLAMAMedellin/
├── src/Server/
│   ├── Program.cs                          # Punto de entrada, DI, middleware
│   ├── Data/
│   │   ├── AppDbContext.cs                 # Contexto EF Core (IdentityDbContext)
│   │   ├── Seed/                           # Seeds (Identity, Miembros, Recibos2025, etc.)
│   ├── Models/                             # Entidades de dominio
│   │   ├── ApplicationUser.cs
│   │   ├── Miembro.cs
│   │   ├── TreasuryModels.cs               # Recibo, ReciboItem, Pago, Concepto, TasaCambio
│   │   ├── Ingreso.cs
│   │   ├── Egreso.cs
│   │   ├── CierreMensual.cs
│   │   ├── Producto.cs                     # Inventario de mercancía
│   │   ├── CompraProducto.cs / VentaProducto.cs
│   │   ├── DonacionModels.cs               # CertificadoDonacion
│   │   ├── Cliente.cs / Proveedor.cs
│   │   ├── Cotizacion.cs
│   │   ├── ConciliacionBancaria.cs
│   │   ├── AuditLog.cs
│   ├── Services/                           # Lógica de negocio
│   │   ├── Recibos/
│   │   ├── Miembros/
│   │   ├── Egresos/
│   │   ├── Donaciones/
│   │   ├── Inventario/
│   │   ├── Ventas/
│   │   ├── Compras/
│   │   ├── CierreContable/
│   │   ├── ConciliacionBancaria/
│   │   ├── Export/
│   │   ├── Reportes/
│   │   ├── DashboardService.cs
│   │   ├── Backup/                         # Backups automáticos a Azure Blob
│   │   ├── Email/                          # EmailService para notificaciones
│   │   ├── Auth/                           # TwoFactorAuditService
│   │   ├── Audit/                          # AuditService
│   ├── Pages/                              # Blazor Pages (Razor components)
│   │   ├── Index.razor                     # Dashboard
│   │   ├── Miembros/
│   │   ├── Tesoreria/                      # Recibos, Egresos, CierreMensual
│   │   ├── GerenciaNegocios/               # Inventario, Ventas, Compras, Cotizaciones
│   │   ├── Admin/                          # Auditoría, Configuración
│   │   ├── Config/                         # Usuarios, ImportarMiembros
│   ├── Controllers/                        # API Controllers
│   │   ├── DiagnosticoController.cs        # Health check completo (Admin only)
│   ├── Configuration/                      # AzureOptions, BackupOptions, SmtpOptions
│   ├── Security/                           # Políticas de autorización
│   ├── Migrations/                         # Migraciones EF Core
│   ├── Properties/appsettings.*.json       # Configuración por ambiente
├── tests/                                  # Tests unitarios, integración, E2E
├── docs/                                   # Documentación
│   ├── AZURE_PRODUCTION_SETUP.md
│   ├── GUIA_RAPIDA_BACKUPS.md
│   ├── CertificadosDonacion_COMPLETO.md
│   └── ... (otros docs de implementaciones)
├── scripts/                                # Scripts PowerShell de validación/migración
```

---

## 2. ENTIDADES ACTUALES (MODELO DE DATOS)

### 2.1 Entidades Core de Tesorería

#### **Miembro**
- **Propósito**: Registro de miembros del capítulo (socios, tesoreros, junta, prospecto, etc.)
- **Campos principales**:
  - `Id` (Guid, PK)
  - `NombreCompleto`, `Nombres`, `Apellidos`
  - `Cedula`, `Documento` (alias)
  - `Email`, `Celular`, `Direccion`
  - `NumeroSocio`, `Cargo`, `Rango`
  - `Estado` (Activo/Inactivo)
  - `FechaIngreso` (DateOnly)
  - Auditoría: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- **Relaciones**: FK opcional en Recibo, Cotizacion, VentaProducto
- **Estado**: ✅ Funcional, con datos seed (ImportarMiembros.sql)

#### **Concepto**
- **Propósito**: Catálogo de conceptos de cobro (mensualidad, renovación, parches, etc.)
- **Campos**:
  - `Id` (int, PK)
  - `Codigo` (ej: MENSUALIDAD)
  - `Nombre`, `Descripcion`
  - `Moneda` (COP/USD)
  - `PrecioBase`
  - `EsRecurrente`, `Periodicidad`, `EsIngreso`
- **Estado**: ✅ Funcional, seed con conceptos iniciales

#### **TasaCambio**
- **Propósito**: Tasa de cambio USD/COP por fecha para cálculos multi-moneda
- **Campos**: `Fecha` (DateOnly), `UsdCop`, `Fuente`, `ObtenidaAutomaticamente`, `EsOficial`
- **Estado**: ✅ Funcional, ExchangeRateHostedService obtiene TRM automáticamente cada 6 horas

#### **Recibo**
- **Propósito**: Recibo de caja (comprobante de ingreso) con numeración consecutiva
- **Campos**:
  - `Id` (Guid, PK)
  - `Serie` (ej: "RC"), `Ano`, `Consecutivo`
  - `FechaEmision`, `Estado` (Borrador/Emitido/Anulado)
  - `MiembroId` (FK opcional), `TerceroLibre` (texto libre si no es miembro)
  - `TotalCop`
  - `Observaciones`
  - Auditoría: `CreatedAt`, `CreatedBy`
- **Relaciones**:
  - 1:N → `ReciboItem` (items del recibo)
  - 1:1 → `Pago` (datos del pago asociado)
  - FK opcional en `CertificadoDonacion`
- **Estado**: ✅ Funcional con generación de PDF, numeración automática
- **Seed**: Recibos2025Seed.cs con datos históricos enero-octubre 2025

#### **ReciboItem**
- **Propósito**: Línea de detalle de un recibo (concepto + cantidad + precio)
- **Campos**:
  - `Id` (int, PK)
  - `ReciboId` (FK), `ConceptoId` (FK)
  - `Cantidad`, `PrecioUnitarioMonedaOrigen`, `MonedaOrigen`, `TrmAplicada`, `SubtotalCop`
  - `Notas`
- **Estado**: ✅ Funcional

#### **Pago**
- **Propósito**: Datos del pago asociado a un recibo
- **Campos**:
  - `Id` (int, PK)
  - `ReciboId` (FK)
  - `Metodo` (Efectivo/Transferencia/Nequi/Daviplata/Tarjeta)
  - `Referencia`, `FechaPago`, `ValorPagadoCop`
  - `UsuarioRegistro`
- **Estado**: ✅ Funcional

#### **Ingreso**
- **Propósito**: Registro complementario de ingresos (no siempre ligado a recibo)
- **Campos**:
  - `NumeroIngreso`, `FechaIngreso`
  - `Categoria`, `Descripcion`, `ValorCop`
  - `MetodoPago`, `ReferenciaTransaccion`, `Observaciones`
  - Auditoría
- **Estado**: ✅ Definido, uso limitado (complementario a Recibo)

#### **Egreso**
- **Propósito**: Registro de gastos de tesorería
- **Campos**:
  - `Id` (Guid, PK)
  - `Fecha`, `Categoria`, `Proveedor`, `Descripcion`, `ValorCop`
  - `SoporteUrl` (adjunto)
  - `UsuarioRegistro`
  - Auditoría
- **Estado**: ✅ Funcional, página de registro implementada

#### **CierreMensual**
- **Propósito**: Cierre contable mensual con bloqueo de ediciones
- **Campos**:
  - `Ano`, `Mes`, `FechaCierre`, `UsuarioCierre`
  - `SaldoInicialCalculado`, `TotalIngresos`, `TotalEgresos`, `SaldoFinal`
  - `Observaciones`
- **Estado**: ✅ Funcional con validación de períodos cerrados

---

### 2.2 Entidades de Inventario y Ventas (Gerencia de Negocios)

#### **Producto**
- **Propósito**: Catálogo de productos para venta (souvenirs, parches, camisetas, jerseys)
- **Campos**:
  - `Codigo`, `Nombre`, `Tipo` (enum: Parche/Souvenir/Camiseta/Jersey/Gorra/Sticker/Llavero/Otros)
  - `PrecioVentaCOP`, `PrecioVentaUSD`
  - `StockActual`, `StockMinimo`
  - `Talla`, `Descripcion`, `EsParcheOficial`, `ImagenUrl`, `Activo`
- **Estado**: ✅ Funcional con gestión de inventario

#### **CompraProducto** / **DetalleCompraProducto**
- **Propósito**: Registro de compras de inventario (ej: comprar parches a LAMA International)
- **Estado**: ✅ Funcional

#### **VentaProducto** / **DetalleVentaProducto**
- **Propósito**: Registro de ventas de mercancía (a miembros o clientes)
- **Campos**:
  - FK a `MiembroId`, `ClienteId`, `ReciboId` (opcionales)
  - `TotalCOP`, `TotalUSD`, estado, observaciones
- **Estado**: ✅ Funcional con integración a recibos

#### **MovimientoInventario**
- **Propósito**: Trazabilidad de entradas/salidas de inventario
- **Estado**: ✅ Funcional

#### **Cliente** / **Proveedor**
- **Propósito**: Terceros (clientes y proveedores)
- **Estado**: ✅ Funcional

#### **Cotizacion** / **DetalleCotizacion**
- **Propósito**: Cotizaciones de productos antes de venta
- **Estado**: ✅ Funcional

---

### 2.3 Entidades de Donaciones (RTE)

#### **CertificadoDonacion**
- **Propósito**: Certificado oficial para donantes (cumple Art. 125-2 y 158-1 del ET colombiano)
- **Campos completos**: Datos del donante, valor donación, destinación, resolución RTE, firmas representantes
- **Estado**: ✅ Funcional con generación de PDF + QR + logo fundación
- **Documentación**: `docs/CertificadosDonacion_COMPLETO.md`

---

### 2.4 Entidades de Conciliación Bancaria

#### **ConciliacionBancaria** / **ItemConciliacion**
- **Propósito**: Conciliar movimientos bancarios vs contables
- **Estado**: ✅ Funcional

---

### 2.5 Auditoría y Seguridad

#### **AuditLog**
- **Propósito**: Registro de auditoría de todas las acciones críticas
- **Campos**: `EntityType`, `EntityId`, `Action`, `UserName`, `IpAddress`, `Timestamp`, `Changes` (JSON)
- **Estado**: ✅ Funcional con middleware automático

#### **ApplicationUser** (ASP.NET Core Identity)
- **Extensión de IdentityUser** con campos personalizados
- **2FA**: Autenticación de dos factores implementada
- **Estado**: ✅ Funcional con roles (Admin, Tesorero, Junta, Consulta, GerenteNegocios)

---

## 3. PÁGINAS Y FLUJOS ACTUALES

### 3.1 Dashboard (`/`)
- **Propósito**: Resumen ejecutivo de tesorería
- **Widgets**:
  - Saldo de caja actual
  - Ingresos del mes
  - Egresos del mes
  - Gráfica de ingresos/egresos (últimos 6 meses)
  - Pendiente por aprobar (recibos borradores)
  - Aportes pendientes (miembros deudores)
  - Productos con stock bajo
- **Estado**: ✅ Funcional con DashboardService
- **Roles**: Todos (filtros según rol)

### 3.2 Módulo Miembros (`/Miembros`)
- **Páginas**:
  - Lista de miembros (búsqueda, filtros, exportación Excel)
  - Detalle/edición de miembro
  - Importación masiva desde CSV
- **Estado**: ✅ Funcional con seed de ~70 miembros
- **Roles**: Admin, Tesorero (edición); Junta, Consulta (lectura)

### 3.3 Módulo Tesorería (`/Tesoreria`)
- **Páginas**:
  - **Recibos**: Lista, crear, editar, anular, PDF, búsqueda avanzada
  - **Egresos**: Registro de gastos con categorías y soportes
  - **Cierre Mensual**: Ejecutar cierre contable mensual (bloqueo de ediciones)
  - **TasasCambio**: Gestión manual de TRM
- **Estado**: ✅ Funcional
- **Roles**: Admin, Tesorero (full); Junta (lectura + reportes); Consulta (lectura dashboard)

### 3.4 Módulo Gerencia de Negocios (`/GerenciaNegocios`)
- **Páginas**:
  - **Productos**: CRUD de inventario de mercancía
  - **Ventas**: Registro de ventas con generación de recibo
  - **Compras**: Registro de compras de inventario
  - **Cotizaciones**: Generar cotizaciones de productos
  - **Inventario**: Movimientos y kardex
- **Estado**: ✅ Funcional
- **Roles**: Admin, GerenteNegocios (full); Tesorero (lectura)

### 3.5 Módulo Admin (`/Admin`)
- **Páginas**:
  - **Auditoría**: Consulta del AuditLog con filtros
  - **Diagnóstico**: Endpoint `/api/diagnostico` (health check detallado)
  - **Usuarios**: Gestión de usuarios y roles (Identity)
  - **Configuración**: Parámetros del sistema
- **Estado**: ✅ Funcional
- **Roles**: Admin (exclusivo)

### 3.6 Certificados de Donación (`/Donaciones`)
- **Funcionalidad**: Emisión de certificados RTE con PDF + QR
- **Estado**: ✅ Funcional completo
- **Roles**: Admin, Tesorero

### 3.7 Reportes
- **Disponibles**:
  - Reporte de deudores (mensualidades pendientes)
  - Libro diario (movimientos cronológicos)
  - Estado de resultados simplificado (ingresos - egresos)
  - Reporte de ventas (por periodo, producto, miembro)
  - Exportación Excel de múltiples entidades
- **Estado**: ✅ Funcional
- **Roles**: Admin, Tesorero, Junta (lectura)

---

## 4. SERVICIOS IMPLEMENTADOS

### 4.1 Servicios de Negocio
- **DashboardService**: Métricas y widgets del dashboard
- **RecibosService**: Lógica de recibos (numeración, validación, PDF)
- **MiembrosService**: CRUD de miembros, importación CSV
- **EgresosService**: Gestión de egresos
- **DonacionesService**: Emisión de certificados de donación
- **InventarioService**: Control de stock, movimientos
- **VentasService**: Ventas con integración a recibos
- **ComprasService**: Compras de inventario
- **CierreContableService**: Cierre mensual con validaciones
- **ConciliacionBancariaService**: Conciliación bancaria
- **ExportService**: Exportación a Excel
- **ReportesService**: Generación de reportes en PDF

### 4.2 Servicios de Infraestructura
- **BackupService**: Backups automáticos diarios (2 AM UTC) a Azure Blob Storage con Managed Identity
- **EmailService**: Envío de correos SMTP (notificaciones, certificados)
- **AuditService**: Auditoría automática de cambios
- **TwoFactorAuditService**: Auditoría de 2FA
- **ExchangeRateHostedService**: Sincronización automática de TRM cada 6 horas

### 4.3 Seguridad
- **Autenticación**: ASP.NET Core Identity con 2FA
- **Autorización**: Políticas basadas en roles (Admin, Tesorero, Junta, Consulta, GerenteNegocios)
- **Rate Limiting**: 100 req/min global, 5 intentos login/15min
- **Security Headers**: CSP, X-Frame-Options, HSTS, etc.
- **HTTPS**: Obligatorio en producción
- **Key Vault**: Secretos en Azure Key Vault con Managed Identity
- **Managed Identity**: Acceso a SQL, Blob Storage, Key Vault sin credenciales

### 4.4 Observabilidad
- **Serilog**: Logging estructurado
- **Application Insights**: Telemetría y monitoreo en Azure
- **Health Checks**: `/health`, `/health/ready`, `/health/live`
- **Diagnostico Endpoint**: `/api/diagnostico` (Admin only) con estado completo del sistema

---

## 5. QUÉ FALTA SEGÚN NEGOCIO REAL

### 5.1 Ausencias Críticas para Operación Real

#### **5.1.1 NO HAY CONCEPTO DE "CUENTA BANCARIA" COMO ENTIDAD**
- **Problema**: Actualmente los ingresos/egresos no se asocian a una cuenta bancaria específica.
- **Impacto**: No hay trazabilidad de movimientos por cuenta (Bancolombia cuenta corriente vs cuenta ahorros vs caja).
- **Necesidad real**: Fundación tiene al menos:
  - Cuenta Bancolombia (principal)
  - Potencialmente caja menor (efectivo)
  - En el futuro: múltiples cuentas (ahorros, CDT, etc.)

#### **5.1.2 NO HAY GESTIÓN DE APORTES MENSUALES RECURRENTES**
- **Problema**: Actualmente los aportes se registran manualmente mes a mes con recibos.
- **Impacto**: No hay:
  - Generación automática de aportes pendientes por mes
  - Vista consolidada de "quién debe qué mes"
  - Alerta de morosos
  - Marcado automático de "pagado" al registrar ingreso bancario
- **Necesidad real**: Core del negocio es cobro mensual de $20.000 COP a ~70 miembros activos.

#### **5.1.3 NO HAY CATÁLOGO DE FUENTES DE INGRESO / CATEGORÍAS DE EGRESO**
- **Problema**: Campos `Categoria` en `Ingreso` y `Egreso` son texto libre.
- **Impacto**: Reportes inconsistentes, difícil consolidación.
- **Necesidad real**:
  - **Fuentes de ingreso**:
    - Aporte Mensual Miembro
    - Venta Merch (souvenir/jersey/parche)
    - Venta Casa Club (artículos/café/cerveza/comida)
    - Donación
    - Eventos
    - Otros ingresos
  - **Categorías de egreso**:
    - Ayuda social (proyectos)
    - Logística eventos
    - Compras inventario merch
    - Compras insumos casa club (café, cerveza, alimentos)
    - Gastos administrativos (papelería, transporte, servicios)
    - Mantenimiento
    - Otros gastos

#### **5.1.4 NO HAY FLUJO DE APROBACIÓN FORMAL**
- **Problema**: Los movimientos se registran pero no hay workflow de "Borrador → Revisado → Aprobado".
- **Impacto**: Falta control interno.
- **Necesidad real**: Tesorero registra, Junta/Admin aprueba antes de impactar saldo.

#### **5.1.5 NO HAY PLAN DE CUENTAS CONTABLE (SIMPLIFICADO)**
- **Problema**: No existe modelo de doble partida ni cuentas contables.
- **Impacto**: Reportes de "Estado de Resultados" y "Balance" se calculan directo de Ingresos/Egresos sin estructura contable.
- **Necesidad real**:
  - Mínimo para RTE y auditoría: saber qué cuentas afecta cada movimiento
  - Doble partida simplificada (débitos = créditos)
  - Ejemplo:
    - Aporte miembro: Dr Bancos / Cr Ingresos Aportes
    - Venta merch: Dr Bancos / Cr Ingresos Ventas Merch
    - Compra inventario: Dr Inventario / Cr Bancos
    - Ayuda social: Dr Gasto Social / Cr Bancos

#### **5.1.6 NO HAY GESTIÓN DE "CASA CLUB" (FUTURO CERCANO)**
- **Problema**: Casa Club tendrá:
  - Ventas de café, cerveza, emparedados (productos perecederos)
  - Inventario de insumos (diferente a mercancía)
  - Costos de venta (COGS)
- **Impacto**: No existe estructura para:
  - Registrar compra de insumos (café, cerveza, alimentos)
  - Vender productos de consumo (no son souvenirs)
  - Calcular margen (precio venta - costo)
- **Necesidad real**: Modelo debe soportar operación de punto de venta simple.

#### **5.1.7 NO HAY PROYECTOS SOCIALES RASTREADOS**
- **Problema**: Egresos de "ayuda social" no se asocian a proyectos específicos.
- **Impacto**: Para RTE se necesita trazabilidad:
  - Proyecto X: presupuesto Y, ejecutado Z
  - Soportes por proyecto
  - Reporte de ejecución por proyecto
- **Necesidad real**: DIAN exige justificación del gasto social en RTE.

#### **5.1.8 NO HAY SALDO CALCULADO EN TIEMPO REAL**
- **Problema**: Dashboard muestra saldo pero no hay entidad `CuentaFinanciera` con saldo persistido.
- **Impacto**: Saldo se recalcula cada vez (costoso, sin validación de consistencia).
- **Necesidad real**: Saldo inicial + movimientos = saldo actual (validable contra banco).

#### **5.1.9 NO HAY CONCILIACIÓN BANCARIA AUTOMATIZADA**
- **Problema**: Existe entidad pero no flujo real de importar extracto bancario y matchear.
- **Impacto**: Conciliación manual, propenso a errores.
- **Necesidad real**: Importar Excel de Bancolombia → comparar con movimientos → marcar conciliado.

---

### 5.2 Mejoras de UX/Flujos

#### **5.2.1 Generación Masiva de Recibos de Aportes**
- **Actual**: Recibos se crean uno a uno.
- **Necesidad**: Botón "Generar aportes del mes" → crea automáticamente recibos para todos los miembros activos.

#### **5.2.2 Dashboard con Métricas de Casa Club**
- **Actual**: Dashboard muestra ingresos/egresos generales.
- **Necesidad**: Widget específico para ventas Casa Club (día/semana/mes).

#### **5.2.3 Alerta de Stock Bajo para Insumos Casa Club**
- **Actual**: Alerta solo para productos de inventario (mercancía).
- **Necesidad**: Separar mercancía de insumos operativos.

---

### 5.3 Documentación Faltante

#### **5.3.1 Manual de Usuario para Tesorero**
- **Necesidad**: Guía paso a paso para operación diaria (registrar ingreso, egreso, cerrar mes).
- **Estado**: ❌ No existe

#### **5.3.2 Guía de Configuración Inicial**
- **Necesidad**: Cómo configurar fundación nueva (NIT, cuentas, conceptos, miembros iniciales).
- **Estado**: Parcial (AZURE_PRODUCTION_SETUP.md cubre infraestructura, no operación)

#### **5.3.3 Modelo de Datos Conceptual**
- **Necesidad**: Diagrama ER documentado con reglas de negocio.
- **Estado**: ❌ No existe (solo código)

---

## 6. SUMMARY: INVENTARIO DE GAPS

| Gap | Criticidad | Impacto | Fase Sugerida |
|-----|------------|---------|---------------|
| **Cuenta Bancaria como entidad** | 🔴 Alta | Sin trazabilidad por cuenta | Fase 1 |
| **Gestión de Aportes Mensuales** | 🔴 Alta | Core del negocio no automatizado | Fase 1 |
| **Catálogo de Fuentes/Categorías** | 🔴 Alta | Reportes inconsistentes | Fase 1 |
| **Flujo de Aprobación** | 🟡 Media | Control interno débil | Fase 1 |
| **Plan de Cuentas Contable** | 🟡 Media | Necesario para RTE/auditoría | Fase 2 |
| **Gestión Casa Club** | 🟡 Media | Operación futura cercana | Fase 2 |
| **Proyectos Sociales** | 🟢 Baja | Necesario para RTE (mediano plazo) | Fase 3 |
| **Saldo Calculado Persistido** | 🟡 Media | Performance + validación | Fase 2 |
| **Conciliación Automatizada** | 🟢 Baja | Eficiencia operativa | Fase 3 |

---

## 7. CONCLUSIÓN

El sistema actual es **funcional para operación básica** de tesorería (recibos, egresos, inventario, donaciones), con infraestructura robusta (Azure, seguridad, auditoría, backups).

**Principales fortalezas**:
- ✅ Infraestructura cloud lista para producción
- ✅ Seguridad implementada (Identity, 2FA, RBAC, MI, Key Vault)
- ✅ Auditoría completa con AuditLog
- ✅ Inventario de mercancía funcional
- ✅ Certificados de donación listos para RTE
- ✅ Backups automáticos a Blob Storage
- ✅ Health checks y observabilidad (Application Insights)

**Principales debilidades**:
- ❌ No hay gestión de cuentas bancarias como entidades
- ❌ Aportes mensuales no automatizados
- ❌ No hay catálogos (fuentes/categorías) → texto libre
- ❌ No hay contabilidad de doble partida (necesaria para RTE)
- ❌ No hay modelo para Casa Club (operación futura)
- ❌ Proyectos sociales no rastreados

**Recomendación**: Implementar **Fase 1** (cuentas bancarias, aportes mensuales, catálogos) antes de salida a producción oficial. Fase 2 y 3 pueden ser iterativas post-lanzamiento.

---

**Próximo paso**: Crear `DOMAIN_PLAN.md` con modelo propuesto para subsanar gaps.
