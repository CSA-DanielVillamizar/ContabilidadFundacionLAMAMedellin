# DOMAIN_PLAN.MD — Modelo de Dominio Propuesto

**Fecha**: 2026-01-21  
**Proyecto**: Sistema de Tesorería/Contabilidad Fundación L.A.M.A. Medellín  
**Objetivo**: Definir modelo de datos y reglas de negocio para convertir sistema en producto funcional completo

---

## 1. PRINCIPIOS DE DISEÑO

### 1.1 Filosofía
- **Simplicidad**: Contabilidad simplificada, no NIIF completo
- **Trazabilidad**: Todo movimiento rastreable (quién, cuándo, por qué, soporte)
- **Auditoría**: Cambios críticos en AuditLog
- **Escalabilidad**: Preparado para RTE y Casa Club
- **No Breaking Changes**: Migrar progresivamente sin romper funcionalidad existente

### 1.2 Reglas Contables Mínimas
1. **Doble partida simplificada**: Todo movimiento tiene débito y crédito (Σ débitos = Σ créditos)
2. **Aprobación de movimientos**: Borrador → Aprobado → impacta saldo
3. **Períodos cerrados**: Bloqueo de edición en meses cerrados
4. **Conciliación bancaria**: Movimientos matcheados con extracto real
5. **Trazabilidad de gasto social**: Egresos ligados a proyectos (RTE)

---

## 2. NUEVAS ENTIDADES PROPUESTAS

### 2.1 Cuenta Financiera (CuentaFinanciera)

**Propósito**: Representa una cuenta bancaria o caja física donde se mueve dinero real.

**Campos**:
```csharp
public class CuentaFinanciera
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }  // ej: BANCO-001, CAJA-001
    public string Nombre { get; set; }  // ej: "Bancolombia Cuenta Corriente"
    public TipoCuenta Tipo { get; set; } // Bancaria, Caja
    public string? Banco { get; set; }   // "Bancolombia"
    public string? NumeroCuenta { get; set; } // Enmascarado: "****1234"
    public string? TitularCuenta { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoActual { get; set; } // Calculado o actualizado
    public DateTime FechaApertura { get; set; }
    public bool Activa { get; set; } = true;
    public string? Observaciones { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public enum TipoCuenta { Bancaria = 1, Caja = 2 }
```

**Relaciones**:
- 1:N → `MovimientoTesoreria` (cada movimiento afecta una cuenta)

**Validaciones**:
- Saldo actual = Saldo inicial + Σ ingresos - Σ egresos (aprobados)
- No permitir saldo negativo (configurable por cuenta)

**Seed Inicial**:
```csharp
new CuentaFinanciera
{
    Codigo = "BANCO-BCOL-001",
    Nombre = "Bancolombia Cuenta Corriente Principal",
    Tipo = TipoCuenta.Bancaria,
    Banco = "Bancolombia",
    NumeroCuenta = "****5678", // Enmascarado
    SaldoInicial = 0m, // Ajustar con saldo real a fecha de migración
    SaldoActual = 0m,  // Se recalcula al migrar movimientos históricos
    FechaApertura = new DateTime(2024, 1, 1),
    Activa = true
}
```

---

### 2.2 Movimiento de Tesorería (MovimientoTesoreria)

**Propósito**: Registro unificado de todos los movimientos de dinero (ingresos y egresos) en cuentas reales.

**Campos**:
```csharp
public class MovimientoTesoreria
{
    public Guid Id { get; set; }
    public string NumeroMovimiento { get; set; } // Consecutivo: MV-2025-00001
    public DateTime Fecha { get; set; }
    public TipoMovimiento Tipo { get; set; } // Ingreso, Egreso
    
    // Cuenta afectada
    public Guid CuentaFinancieraId { get; set; }
    public CuentaFinanciera CuentaFinanciera { get; set; }
    
    // Clasificación
    public Guid? FuenteIngresoId { get; set; } // FK a catálogo (si es ingreso)
    public FuenteIngreso? FuenteIngreso { get; set; }
    
    public Guid? CategoriaEgresoId { get; set; } // FK a catálogo (si es egreso)
    public CategoriaEgreso? CategoriaEgreso { get; set; }
    
    // Datos del movimiento
    public decimal Valor { get; set; }
    public string Descripcion { get; set; }
    public MedioPago Medio { get; set; } // Transferencia, Consignación, Efectivo, Cheque
    public string? ReferenciaTransaccion { get; set; }
    
    // Tercero (opcional)
    public Guid? TerceroId { get; set; } // FK a Miembro, Cliente, Proveedor (polimórfico o texto libre)
    public string? TerceroNombre { get; set; } // Texto libre si no está catalogado
    
    // Soporte
    public string? SoporteUrl { get; set; }
    
    // Estado
    public EstadoMovimiento Estado { get; set; } // Borrador, Aprobado, Anulado
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioAprobacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    
    // Relación con otros módulos
    public Guid? ReciboId { get; set; } // Si el ingreso generó un recibo
    public Recibo? Recibo { get; set; }
    
    public Guid? ProyectoSocialId { get; set; } // Si es egreso de proyecto social
    public ProyectoSocial? ProyectoSocial { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public enum TipoMovimiento { Ingreso = 1, Egreso = 2 }
public enum MedioPago 
{ 
    Transferencia = 1, 
    Consignacion = 2, 
    Efectivo = 3, 
    Cheque = 4, 
    Nequi = 5, 
    Daviplata = 6, 
    Tarjeta = 7 
}
public enum EstadoMovimiento { Borrador = 0, Aprobado = 1, Anulado = 2 }
```

**Reglas de Negocio**:
1. **Solo movimientos aprobados impactan saldo** de CuentaFinanciera
2. **Validación de aprobación**:
   - Tesorero puede aprobar movimientos propios (configurable)
   - Admin aprueba todos
   - Borrador puede editarse, Aprobado no
3. **Anulación**: Solo Admin, requiere motivo, no elimina registro (soft delete lógico)
4. **Recibo automático**: Si es ingreso por Aporte Miembro o Venta, generar Recibo automáticamente

**Relaciones**:
- N:1 → `CuentaFinanciera`
- N:1 → `FuenteIngreso` (catálogo)
- N:1 → `CategoriaEgreso` (catálogo)
- 1:1 → `Recibo` (opcional)
- N:1 → `ProyectoSocial` (opcional)

**Migración desde entidades actuales**:
- `Ingreso` → `MovimientoTesoreria` (tipo=Ingreso, mapear categoría a FuenteIngreso)
- `Egreso` → `MovimientoTesoreria` (tipo=Egreso, mapear categoría a CategoriaEgreso)
- `Pago` (de Recibo) → `MovimientoTesoreria` (tipo=Ingreso, ligado a ReciboId)

---

### 2.3 Fuente de Ingreso (FuenteIngreso)

**Propósito**: Catálogo de orígenes de ingresos de la fundación.

**Campos**:
```csharp
public class FuenteIngreso
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }  // ej: APORTE-MEN, VENTA-MERCH, VENTA-CLUB, DONACION
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;
    
    // Contabilidad
    public Guid? CuentaContableId { get; set; } // FK a PlanCuentas (Fase 2)
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}
```

**Catálogo Inicial** (Seed):
```csharp
new List<FuenteIngreso>
{
    new() { Codigo = "APORTE-MEN", Nombre = "Aporte Mensual Miembro", Descripcion = "$20.000 COP recurrente" },
    new() { Codigo = "VENTA-MERCH", Nombre = "Venta Mercancía", Descripcion = "Souvenirs, jerseys, parches, gorras" },
    new() { Codigo = "VENTA-CLUB-ART", Nombre = "Venta Casa Club - Artículos Moteros", Descripcion = "Artículos moteros vendidos en casa club" },
    new() { Codigo = "VENTA-CLUB-CAFE", Nombre = "Venta Casa Club - Café", Descripcion = "Café vendido en casa club" },
    new() { Codigo = "VENTA-CLUB-CERV", Nombre = "Venta Casa Club - Cerveza", Descripcion = "Cerveza vendida en casa club" },
    new() { Codigo = "VENTA-CLUB-COMI", Nombre = "Venta Casa Club - Comida", Descripcion = "Emparedados, snacks, comida ligera" },
    new() { Codigo = "DONACION", Nombre = "Donación", Descripcion = "Donaciones recibidas (RTE)" },
    new() { Codigo = "EVENTO", Nombre = "Evento", Descripcion = "Ingresos por eventos organizados" },
    new() { Codigo = "RENOVACION-MEM", Nombre = "Renovación Membresía", Descripcion = "Renovación anual de membresía" },
    new() { Codigo = "OTROS", Nombre = "Otros Ingresos", Descripcion = "Ingresos misceláneos" }
}
```

---

### 2.4 Categoría de Egreso (CategoriaEgreso)

**Propósito**: Catálogo de tipos de gastos de la fundación.

**Campos**:
```csharp
public class CategoriaEgreso
{
    public Guid Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool EsGastoSocial { get; set; } // true si es gasto social (RTE)
    public bool Activa { get; set; } = true;
    
    // Contabilidad
    public Guid? CuentaContableId { get; set; } // FK a PlanCuentas (Fase 2)
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}
```

**Catálogo Inicial** (Seed):
```csharp
new List<CategoriaEgreso>
{
    new() { Codigo = "AYUDA-SOCIAL", Nombre = "Ayuda Social", Descripcion = "Proyectos de ayuda social (RTE)", EsGastoSocial = true },
    new() { Codigo = "EVENTO-LOG", Nombre = "Logística de Eventos", Descripcion = "Gastos de organización de eventos", EsGastoSocial = false },
    new() { Codigo = "COMPRA-MERCH", Nombre = "Compra Inventario Mercancía", Descripcion = "Parches, souvenirs, jerseys", EsGastoSocial = false },
    new() { Codigo = "COMPRA-CLUB-CAFE", Nombre = "Compra Insumos Casa Club - Café", Descripcion = "Café, capuchino, etc.", EsGastoSocial = false },
    new() { Codigo = "COMPRA-CLUB-CERV", Nombre = "Compra Insumos Casa Club - Cerveza", Descripcion = "Cerveza, bebidas alcohólicas", EsGastoSocial = false },
    new() { Codigo = "COMPRA-CLUB-COMI", Nombre = "Compra Insumos Casa Club - Comida", Descripcion = "Alimentos para emparedados, snacks", EsGastoSocial = false },
    new() { Codigo = "COMPRA-CLUB-OTROS", Nombre = "Compra Insumos Casa Club - Otros", Descripcion = "Artículos moteros para venta", EsGastoSocial = false },
    new() { Codigo = "ADMIN-PAPEL", Nombre = "Gastos Administrativos - Papelería", Descripcion = "Papelería, oficina", EsGastoSocial = false },
    new() { Codigo = "ADMIN-TRANSP", Nombre = "Gastos Administrativos - Transporte", Descripcion = "Transporte, combustible", EsGastoSocial = false },
    new() { Codigo = "ADMIN-SERVICIOS", Nombre = "Gastos Administrativos - Servicios", Descripcion = "Internet, telefonía, servicios públicos", EsGastoSocial = false },
    new() { Codigo = "MANTENIMIENTO", Nombre = "Mantenimiento", Descripcion = "Mantenimiento de infraestructura", EsGastoSocial = false },
    new() { Codigo = "OTROS-GASTOS", Nombre = "Otros Gastos", Descripcion = "Gastos misceláneos", EsGastoSocial = false }
}
```

---

### 2.5 Aporte Mensual (AporteMensual)

**Propósito**: Registro de obligación mensual de cada miembro activo.

**Campos**:
```csharp
public class AporteMensual
{
    public Guid Id { get; set; }
    public Guid MiembroId { get; set; }
    public Miembro Miembro { get; set; }
    
    public int Ano { get; set; } // 2025
    public int Mes { get; set; }  // 1-12
    
    public decimal ValorEsperado { get; set; } // Default 20000 COP
    public EstadoAporte Estado { get; set; } // Pendiente, Pagado, Exonerado
    
    public DateTime? FechaPago { get; set; }
    public Guid? MovimientoTesoreriaId { get; set; } // FK al movimiento que pagó
    public MovimientoTesoreria? MovimientoTesoreria { get; set; }
    
    public string? Observaciones { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public enum EstadoAporte { Pendiente = 0, Pagado = 1, Exonerado = 2 }
```

**Reglas de Negocio**:
1. **Generación automática**: Job mensual crea registros para todos los miembros activos (Estado = Activo, Rango <> "Asociado").
2. **Lógica de generación**:
   - Si FechaIngreso >= mes actual → no generar aporte (primer mes gratis o según lógica de negocio)
   - Si FechaIngreso < mes actual → generar aporte
3. **Marcado de pagado**:
   - Cuando se registre un MovimientoTesoreria con FuenteIngreso = "APORTE-MEN" y TerceroId = MiembroId → buscar AporteMensual pendiente más antiguo y marcarlo como Pagado.
4. **Exoneración**: Admin puede marcar como Exonerado (ej: junta directiva, casos especiales).
5. **Reporte de deudores**: Calcular aportes pendientes por miembro para dashboard.

**Migración**:
- Analizar `Recibos` históricos con Concepto "MENSUALIDAD" para reconstruir estado de aportes 2025.

---

### 2.6 Proyecto Social (ProyectoSocial)

**Propósito**: Rastrear proyectos de ayuda social con presupuesto y ejecución (RTE).

**Campos**:
```csharp
public class ProyectoSocial
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } // ej: PROY-2025-001
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Objetivo { get; set; }
    
    public decimal PresupuestoAsignado { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    
    public EstadoProyecto Estado { get; set; } // Planeado, EnEjecucion, Finalizado, Cancelado
    
    public string? ResponsableId { get; set; } // FK a ApplicationUser
    public ApplicationUser? Responsable { get; set; }
    
    public string? Observaciones { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<MovimientoTesoreria> Movimientos { get; set; } = new List<MovimientoTesoreria>();
}

public enum EstadoProyecto { Planeado = 0, EnEjecucion = 1, Finalizado = 2, Cancelado = 3 }
```

**Reportes**:
- Ejecución presupuestal por proyecto: PresupuestoAsignado vs Σ egresos ligados
- Soporte de gasto social para DIAN (RTE)

---

## 3. PLAN DE CUENTAS CONTABLE (SIMPLIFICADO)

**Propósito**: Estructura mínima para doble partida y reportes contables (Estado de Resultados, Balance).

**No implementar contabilidad completa NIIF**: Solo lo necesario para:
- Transparencia
- Auditoría
- Preparación RTE
- Reportes básicos

### 3.1 Estructura de Cuenta Contable

```csharp
public class CuentaContable
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } // ej: 1105, 1435, 4101, 5101
    public string Nombre { get; set; }
    public TipoCuentaContable Tipo { get; set; } // Activo, Pasivo, Patrimonio, Ingreso, Gasto
    public NaturalezaCuenta Naturaleza { get; set; } // Debito, Credito
    public bool EsCuentaMayor { get; set; } // true si es cuenta de agrupación (no recibe movimientos)
    public Guid? CuentaPadreId { get; set; } // Para jerarquía (opcional)
    public CuentaContable? CuentaPadre { get; set; }
    public bool Activa { get; set; } = true;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}

public enum TipoCuentaContable { Activo = 1, Pasivo = 2, Patrimonio = 3, Ingreso = 4, Gasto = 5 }
public enum NaturalezaCuenta { Debito = 1, Credito = 2 }
```

### 3.2 Catálogo Inicial de Cuentas (Seed)

**ACTIVOS (1)**
```
1105 - Caja (Naturaleza: Débito)
1110 - Bancos (Naturaleza: Débito)
  1110.01 - Bancolombia Cuenta Corriente
  1110.02 - Cuenta de Ahorros (si aplica)
1435 - Inventario Mercancía (Naturaleza: Débito)
1436 - Inventario Insumos Casa Club (Naturaleza: Débito)
```

**PASIVOS (2)**
```
2205 - Proveedores Nacionales (Naturaleza: Crédito)
```

**PATRIMONIO (3)**
```
3605 - Aportes Sociales (Naturaleza: Crédito)
3705 - Resultados del Ejercicio (Naturaleza: Crédito)
3710 - Resultados de Ejercicios Anteriores (Naturaleza: Crédito)
```

**INGRESOS (4)**
```
4101 - Ingresos por Aportes de Miembros (Naturaleza: Crédito)
4102 - Ingresos por Ventas de Mercancía (Naturaleza: Crédito)
4103 - Ingresos por Ventas Casa Club (Naturaleza: Crédito)
  4103.01 - Ventas Café
  4103.02 - Ventas Cerveza
  4103.03 - Ventas Comida
  4103.04 - Ventas Artículos Moteros
4201 - Donaciones Recibidas (Naturaleza: Crédito)
4301 - Otros Ingresos (Naturaleza: Crédito)
```

**GASTOS (5)**
```
5101 - Gasto Social - Proyectos de Ayuda (Naturaleza: Débito)
5201 - Gastos Operativos de Eventos (Naturaleza: Débito)
5301 - Costo de Ventas - Mercancía (Naturaleza: Débito)
5302 - Costo de Ventas - Casa Club (Naturaleza: Débito)
5401 - Gastos Administrativos (Naturaleza: Débito)
  5401.01 - Papelería y Oficina
  5401.02 - Transporte
  5401.03 - Servicios (Internet, Telefonía)
5501 - Mantenimiento (Naturaleza: Débito)
5901 - Otros Gastos (Naturaleza: Débito)
```

### 3.3 Movimiento Contable (ComprobanteContable)

**Propósito**: Registro de doble partida (débito = crédito).

```csharp
public class ComprobanteContable
{
    public Guid Id { get; set; }
    public string NumeroComprobante { get; set; } // Consecutivo: COM-2025-00001
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; }
    
    public EstadoComprobante Estado { get; set; } // Borrador, Aprobado, Anulado
    
    // Relación con MovimientoTesoreria (opcional)
    public Guid? MovimientoTesoreriaId { get; set; }
    public MovimientoTesoreria? MovimientoTesoreria { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<MovimientoContable> Movimientos { get; set; } = new List<MovimientoContable>();
}

public class MovimientoContable
{
    public Guid Id { get; set; }
    public Guid ComprobanteContableId { get; set; }
    public ComprobanteContable ComprobanteContable { get; set; }
    
    public Guid CuentaContableId { get; set; }
    public CuentaContable CuentaContable { get; set; }
    
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
    
    public string? Descripcion { get; set; }
}

public enum EstadoComprobante { Borrador = 0, Aprobado = 1, Anulado = 2 }
```

**Regla de Validación**:
```csharp
// Antes de aprobar ComprobanteContable:
if (comprobante.Movimientos.Sum(m => m.Debito) != comprobante.Movimientos.Sum(m => m.Credito))
{
    throw new InvalidOperationException("El comprobante no cuadra: débitos != créditos");
}
```

### 3.4 Generación Automática de Comprobantes

**Mapeo de MovimientoTesoreria → ComprobanteContable**:

| Tipo Movimiento | Fuente/Categoría | Débito | Crédito |
|-----------------|------------------|--------|---------|
| **Ingreso** - Aporte Miembro | APORTE-MEN | 1110 (Bancos) | 4101 (Ingresos Aportes) |
| **Ingreso** - Venta Merch | VENTA-MERCH | 1110 (Bancos) | 4102 (Ingresos Ventas Merch) |
| **Ingreso** - Venta Casa Club Café | VENTA-CLUB-CAFE | 1110 (Bancos) | 4103.01 (Ventas Café) |
| **Ingreso** - Donación | DONACION | 1110 (Bancos) | 4201 (Donaciones) |
| **Egreso** - Compra Merch | COMPRA-MERCH | 1435 (Inventario Merch) | 1110 (Bancos) |
| **Egreso** - Compra Insumos Club Café | COMPRA-CLUB-CAFE | 1436 (Inv. Insumos Club) | 1110 (Bancos) |
| **Egreso** - Ayuda Social | AYUDA-SOCIAL | 5101 (Gasto Social) | 1110 (Bancos) |
| **Egreso** - Gastos Admin | ADMIN-SERVICIOS | 5401.03 (Servicios) | 1110 (Bancos) |

**Implementación**:
- Servicio `ComprobanteContableService.GenerarDesdeMovimiento(MovimientoTesoreria mov)`:
  - Detectar FuenteIngreso o CategoriaEgreso
  - Buscar mapeo en configuración
  - Crear ComprobanteContable con 2 MovimientoContable (débito y crédito)
  - Validar cuadre
  - Aprobar automáticamente si movimiento ya está aprobado

---

## 4. ACTUALIZACIÓN DE ENTIDADES EXISTENTES

### 4.1 Miembro (Sin Cambios Estructurales)
- Mantener como está
- Agregar relación 1:N → `AporteMensual`

### 4.2 Recibo (Adaptación)
- Agregar FK opcional `MovimientoTesoreriaId`
- Relación: Si un ingreso genera recibo, ambos están ligados

### 4.3 Producto (Separar Tipos)
- Mantener entidad actual para mercancía
- Considerar crear `ProductoCasaClub` o usar mismo `Producto` con flag `EsProductoCasaClub`
- Productos Casa Club tienen `CostoPorUnidad` para calcular margen

### 4.4 Egreso e Ingreso (Deprecar en Fase 1)
- Migrar datos a `MovimientoTesoreria`
- Mantener entidades antiguas en BD (no eliminar) para historial
- Desactivar páginas de creación (redirigir a nueva UI de MovimientoTesoreria)

---

## 5. FLUJOS DE NEGOCIO PROPUESTOS

### 5.1 Flujo: Generar Aportes Mensuales (Automatizado)

**Trigger**: Primer día de cada mes (job programado)

**Lógica**:
```csharp
var miembrosActivos = await _context.Miembros
    .Where(m => m.Estado == EstadoMiembro.Activo && m.Rango != "Asociado")
    .ToListAsync();

var mesPeriodo = DateTime.UtcNow.Month;
var anoPeriodo = DateTime.UtcNow.Year;

foreach (var miembro in miembrosActivos)
{
    // Validar si ya tiene aporte generado para este mes
    var existeAporte = await _context.AportesMensuales
        .AnyAsync(a => a.MiembroId == miembro.Id && a.Mes == mesPeriodo && a.Ano == anoPeriodo);
    
    if (existeAporte) continue;
    
    // Validar fecha de ingreso (no cobrar primer mes si recién ingresó)
    if (miembro.FechaIngreso.HasValue && 
        miembro.FechaIngreso.Value.Year == anoPeriodo && 
        miembro.FechaIngreso.Value.Month == mesPeriodo)
    {
        continue; // Primer mes gratis
    }
    
    // Crear aporte pendiente
    var aporte = new AporteMensual
    {
        MiembroId = miembro.Id,
        Ano = anoPeriodo,
        Mes = mesPeriodo,
        ValorEsperado = 20000m,
        Estado = EstadoAporte.Pendiente,
        CreatedBy = "system"
    };
    
    _context.AportesMensuales.Add(aporte);
}

await _context.SaveChangesAsync();
```

---

### 5.2 Flujo: Registrar Ingreso Bancario (Aporte Miembro)

**UI**: Página "Registrar Ingreso" o "Movimiento de Tesorería"

**Pasos**:
1. Usuario selecciona:
   - Cuenta: Bancolombia
   - Tipo: Ingreso
   - Fuente: Aporte Mensual Miembro
   - Miembro: [Buscar en dropdown]
   - Valor: 20000 COP
   - Medio: Transferencia
   - Referencia: "TRF-12345"
   - Fecha: 2025-01-15
2. Sistema crea `MovimientoTesoreria` en estado Borrador
3. Usuario aprueba movimiento
4. Sistema:
   - Actualiza `CuentaFinanciera.SaldoActual += 20000`
   - Busca `AporteMensual` pendiente más antiguo del miembro
   - Marca `AporteMensual.Estado = Pagado`, `FechaPago = now`, `MovimientoTesoreriaId = mov.Id`
   - Genera `ComprobanteContable` automático:
     - Dr 1110 (Bancos) 20000
     - Cr 4101 (Ingresos Aportes) 20000
   - Genera `Recibo` automático (si configurado)
5. Auditoría: `AuditLog` registra aprobación

---

### 5.3 Flujo: Registrar Egreso (Compra Insumos Casa Club)

**UI**: Página "Registrar Egreso"

**Pasos**:
1. Usuario selecciona:
   - Cuenta: Bancolombia
   - Tipo: Egreso
   - Categoría: Compra Insumos Casa Club - Café
   - Proveedor: [Texto libre o dropdown]
   - Valor: 150000 COP
   - Medio: Transferencia
   - Referencia: "TRF-98765"
   - Fecha: 2025-01-16
   - Soporte: [Adjuntar PDF factura]
2. Sistema crea `MovimientoTesoreria` en estado Borrador
3. Tesorero aprueba
4. Sistema:
   - Actualiza `CuentaFinanciera.SaldoActual -= 150000`
   - Genera `ComprobanteContable`:
     - Dr 1436 (Inventario Insumos Club) 150000
     - Cr 1110 (Bancos) 150000
5. Auditoría: `AuditLog`

---

### 5.4 Flujo: Cierre Mensual (Mantenido y Mejorado)

**UI**: Página "Cierre Mensual"

**Validaciones Adicionales**:
- Todos los `MovimientoTesoreria` del mes deben estar Aprobados (no Borrador)
- Todos los `ComprobanteContable` del mes deben cuadrar

**Pasos**:
1. Calcular:
   - Saldo inicial (del mes anterior)
   - Σ ingresos del mes
   - Σ egresos del mes
   - Saldo final
2. Crear `CierreMensual` con datos calculados
3. Bloquear ediciones de:
   - `MovimientoTesoreria` con Fecha en mes cerrado
   - `Recibo` con FechaEmision en mes cerrado
   - `ComprobanteContable` con Fecha en mes cerrado

---

## 6. REPORTES PROPUESTOS

### 6.1 Dashboard (Actualizado)
- **Saldo por Cuenta**: Mostrar Bancolombia y Caja por separado
- **Ingresos del Mes por Fuente**: Gráfica con:
  - Aportes Miembros
  - Ventas Merch
  - Ventas Casa Club (desglosado: café, cerveza, comida)
  - Donaciones
- **Egresos del Mes por Categoría**: Gráfica con:
  - Ayuda Social
  - Compras Inventario
  - Compras Insumos Casa Club
  - Gastos Administrativos
- **Aportes Pendientes**: Contador de miembros con aportes pendientes + tabla top 10 deudores
- **Proyectos Sociales**: Estado de ejecución presupuestal

### 6.2 Libro Diario (Tesorería)
- Lista cronológica de `MovimientoTesoreria` aprobados
- Filtros: Fecha inicio/fin, Cuenta, Tipo, Fuente/Categoría
- Exportación Excel

### 6.3 Mayor por Cuenta Contable
- Movimientos agrupados por `CuentaContable`
- Saldo inicial + débitos - créditos = saldo final
- Filtros: Cuenta, período

### 6.4 Estado de Resultados (Simplificado)
```
INGRESOS
  Aportes de Miembros:       $X
  Ventas Mercancía:           $Y
  Ventas Casa Club:           $Z
  Donaciones:                 $W
  Otros Ingresos:             $V
  TOTAL INGRESOS:             $A

GASTOS
  Gasto Social:               $P
  Gastos Operativos:          $Q
  Costo de Ventas:            $R
  Gastos Administrativos:     $S
  Otros Gastos:               $T
  TOTAL GASTOS:               $B

RESULTADO DEL EJERCICIO:      $A - $B
```

### 6.5 Balance General (Simplificado)
```
ACTIVOS
  Caja:                       $X
  Bancos:                     $Y
  Inventario Mercancía:       $Z
  Inventario Insumos:         $W
  TOTAL ACTIVOS:              $A

PASIVOS
  Proveedores:                $P
  TOTAL PASIVOS:              $B

PATRIMONIO
  Aportes Sociales:           $C
  Resultados del Ejercicio:   $D
  TOTAL PATRIMONIO:           $E

TOTAL PASIVO + PATRIMONIO:    $B + $E (debe ser = $A)
```

### 6.6 Reporte de Deudores (Mejorado)
- Basado en `AporteMensual` con Estado = Pendiente
- Agrupar por miembro: mostrar meses adeudados, total adeudado
- Exportación Excel con emails para recordatorios

### 6.7 Reporte de Ejecución de Proyectos Sociales
- Por cada `ProyectoSocial`:
  - Presupuesto asignado
  - Σ egresos ejecutados (MovimientoTesoreria ligados)
  - % ejecución
  - Soportes adjuntos
- Para auditoría RTE

---

## 7. ESTRATEGIA DE MIGRACIÓN

### 7.1 Fase 1.0: Entidades Base (Sin Tocar Funcionalidad Existente)
**Objetivo**: Crear nuevas entidades sin romper nada.

**Migraciones**:
1. Crear tablas:
   - `CuentasFinancieras`
   - `FuentesIngreso`
   - `CategoriasEgreso`
   - `MovimientosTesoreria`
   - `AportesMensuales`
2. Seed:
   - CuentaFinanciera: Bancolombia (SaldoInicial = 0, calcular después)
   - FuentesIngreso: catálogo completo
   - CategoriasEgreso: catálogo completo
3. NO migrar datos aún (tablas vacías)
4. Desplegar a Development
5. Verificar build OK

### 7.2 Fase 1.1: Páginas UI Nuevas
**Objetivo**: Crear UI para nuevas entidades (coexistiendo con antiguas).

**Páginas nuevas**:
- `/Tesoreria/CuentasFinancieras` (CRUD)
- `/Tesoreria/MovimientosTesoreria` (CRUD con filtros avanzados)
- `/Tesoreria/AportesMensuales` (Vista de gestión)

**Mantener páginas antiguas**:
- `/Tesoreria/Recibos` (sin cambios)
- `/Tesoreria/Egresos` (sin cambios)

**Desplegar a Development**

### 7.3 Fase 1.2: Migración de Datos Históricos
**Objetivo**: Poblar nuevas entidades con datos existentes.

**Script de migración**:
```csharp
// Migrar Egresos → MovimientosTesoreria
var egresos = await _context.Egresos.ToListAsync();
foreach (var egreso in egresos)
{
    var categoriaEgreso = await MapearCategoriaEgreso(egreso.Categoria);
    var movimiento = new MovimientoTesoreria
    {
        NumeroMovimiento = $"MV-MIGRADO-{egreso.Id}",
        Fecha = egreso.Fecha,
        Tipo = TipoMovimiento.Egreso,
        CuentaFinancieraId = cuentaBancolombiaId,
        CategoriaEgresoId = categoriaEgreso.Id,
        Valor = egreso.ValorCop,
        Descripcion = egreso.Descripcion,
        Medio = MedioPago.Transferencia, // Default
        TerceroNombre = egreso.Proveedor,
        SoporteUrl = egreso.SoporteUrl,
        Estado = EstadoMovimiento.Aprobado, // Ya están aprobados
        CreatedBy = "migration"
    };
    _context.MovimientosTesoreria.Add(movimiento);
}

// Migrar Ingresos → MovimientosTesoreria
// Similar...

// Migrar Pagos (de Recibos) → MovimientosTesoreria
// Similar...

await _context.SaveChangesAsync();
```

**Recalcular Saldo de CuentaBancolombia**:
```csharp
var totalIngresos = await _context.MovimientosTesoreria
    .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Estado == EstadoMovimiento.Aprobado)
    .SumAsync(m => m.Valor);

var totalEgresos = await _context.MovimientosTesoreria
    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Estado == EstadoMovimiento.Aprobado)
    .SumAsync(m => m.Valor);

cuentaBancolombia.SaldoActual = cuentaBancolombia.SaldoInicial + totalIngresos - totalEgresos;
await _context.SaveChangesAsync();
```

**Reconstruir AportesMensuales desde Recibos**:
```csharp
var recibosMensualidad = await _context.Recibos
    .Include(r => r.Items)
    .Where(r => r.Items.Any(i => i.Concepto.Codigo == "MENSUALIDAD"))
    .ToListAsync();

foreach (var recibo in recibosMensualidad)
{
    var miembroId = recibo.MiembroId;
    var ano = recibo.FechaEmision.Year;
    var mes = recibo.FechaEmision.Month;
    
    var aporte = new AporteMensual
    {
        MiembroId = miembroId.Value,
        Ano = ano,
        Mes = mes,
        ValorEsperado = 20000m,
        Estado = EstadoAporte.Pagado,
        FechaPago = recibo.FechaEmision,
        CreatedBy = "migration"
    };
    _context.AportesMensuales.Add(aporte);
}
await _context.SaveChangesAsync();
```

**Desplegar script de migración en Development**

### 7.4 Fase 1.3: Deprecar Páginas Antiguas
**Objetivo**: Ocultar/deshabilitar creación en páginas antiguas.

**Cambios**:
- `/Tesoreria/Egresos`: Mostrar banner "Esta funcionalidad está deprecada. Usa MovimientosTesoreria"
- `/Tesoreria/Recibos`: Mantener funcional (no deprecar aún, es core)
- Desactivar botones "Nuevo Egreso", "Nuevo Ingreso"

**Desplegar a Development**

### 7.5 Fase 2.0: Contabilidad de Doble Partida
- Crear tablas: `CuentasContables`, `ComprobantesContables`, `MovimientosContables`
- Seed: Plan de cuentas inicial
- Implementar generación automática de comprobantes desde MovimientosTesoreria
- UI: `/Contabilidad/PlanCuentas`, `/Contabilidad/Comprobantes`
- Reportes: Mayor, Balance, Estado de Resultados

### 7.6 Fase 3.0: Proyectos Sociales y RTE
- Crear tabla: `ProyectosSociales`
- Ligar MovimientosTesoreria a ProyectosSociales
- UI: `/Admin/ProyectosSociales`
- Reportes de ejecución presupuestal

---

## 8. VALIDACIONES Y REGLAS CRÍTICAS

### 8.1 Validaciones de Negocio (Tests Unitarios)
1. **Saldo no negativo** (si configurado por cuenta):
   ```csharp
   if (cuenta.SaldoActual - egreso.Valor < 0 && !cuenta.PermiteSaldoNegativo)
       throw new InvalidOperationException("Saldo insuficiente");
   ```
2. **Aportes mensuales no duplicados**:
   ```csharp
   if (await _context.AportesMensuales.AnyAsync(a => a.MiembroId == id && a.Ano == ano && a.Mes == mes))
       throw new InvalidOperationException("Aporte ya existe para este mes");
   ```
3. **Comprobante contable cuadrado**:
   ```csharp
   if (comprobante.Movimientos.Sum(m => m.Debito) != comprobante.Movimientos.Sum(m => m.Credito))
       throw new InvalidOperationException("Comprobante no cuadra");
   ```
4. **Período cerrado no editable**:
   ```csharp
   if (await _context.CierresMensuales.AnyAsync(c => c.Ano == mov.Fecha.Year && c.Mes == mov.Fecha.Month))
       throw new InvalidOperationException("Período cerrado");
   ```

### 8.2 Políticas de Autorización
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AprobarMovimientos", policy =>
        policy.RequireRole("Admin", "Tesorero"));
    
    options.AddPolicy("CerrarMes", policy =>
        policy.RequireRole("Admin", "Tesorero"));
    
    options.AddPolicy("AnularMovimientos", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("GestionarProyectosSociales", policy =>
        policy.RequireRole("Admin", "Junta"));
});
```

---

## 9. DOCUMENTACIÓN PENDIENTE (POST-IMPLEMENTACIÓN)

### 9.1 Manual de Usuario Tesorero (`docs/USER_GUIDE.md`)
**Contenido**:
1. Cómo registrar ingreso (aporte, venta, donación)
2. Cómo registrar egreso (compra, gasto)
3. Cómo aprobar movimientos
4. Cómo generar aportes del mes (botón automatizado)
5. Cómo cerrar mes contable
6. Cómo consultar reportes
7. FAQ

### 9.2 Guía de Configuración Inicial (`docs/SETUP_GUIDE.md`)
**Contenido**:
1. Configurar NIT, razón social, RTE
2. Crear primera cuenta bancaria
3. Configurar catálogo de fuentes/categorías
4. Importar miembros iniciales
5. Configurar SMTP para notificaciones
6. Configurar backup automático

### 9.3 Diagrama ER Conceptual (`docs/ER_DIAGRAM.md`)
- Mermaid o imagen con relaciones entre entidades
- Explicación de cada relación

---

## 10. CRONOGRAMA ESTIMADO (FASES)

| Fase | Descripción | Esfuerzo Estimado | Entregables |
|------|-------------|-------------------|-------------|
| **0** | Inventario + Plan (ESTE DOC) | ✅ Completado | INVENTORY.md, DOMAIN_PLAN.md |
| **1.0** | Crear entidades base (sin migrar datos) | 3-5 días | Migraciones + Seeds vacíos |
| **1.1** | UI para nuevas entidades | 5-7 días | Páginas CRUD funcionales |
| **1.2** | Migración de datos históricos | 2-3 días | Scripts de migración + validación |
| **1.3** | Deprecar páginas antiguas | 1-2 días | Banners + desactivación botones |
| **1.4** | Tests unitarios críticos | 2-3 días | Cobertura de validaciones |
| **1.5** | Documentación usuario | 2-3 días | USER_GUIDE.md + SETUP_GUIDE.md |
| **Total Fase 1** | MVP Funcional Tesorería | **15-23 días** | Sistema listo para producción básica |
| **2.0** | Doble partida + reportes contables | 7-10 días | Plan de cuentas + comprobantes |
| **2.1** | Integración Casa Club | 5-7 días | Productos Casa Club + ventas |
| **2.2** | Reportes avanzados | 3-5 días | Balance, Mayor, Estado Resultados |
| **Total Fase 2** | Contabilidad Simplificada | **15-22 días** | Reportes auditables |
| **3.0** | Proyectos Sociales | 3-5 días | CRUD + asignación presupuesto |
| **3.1** | Conciliación bancaria automatizada | 5-7 días | Importar extracto + matcheo |
| **3.2** | Cierre anual + RTE | 3-5 días | Reportes para DIAN |
| **Total Fase 3** | Listo para RTE/Auditoría | **11-17 días** | Compliance RTE completo |

**Total Estimado**: 41-62 días de desarrollo (dependiendo de complejidad y recursos).

---

## 11. CONCLUSIÓN Y PRÓXIMOS PASOS

### 11.1 Resumen del Plan
Este documento define el modelo de dominio completo para transformar el sistema actual en un producto funcional de tesorería/contabilidad para Fundación L.A.M.A. Medellín.

**Cambios estructurales clave**:
1. ✅ Introducir `CuentaFinanciera` para trazabilidad por cuenta bancaria
2. ✅ Introducir `MovimientoTesoreria` como registro unificado de ingresos/egresos
3. ✅ Introducir `AporteMensual` para gestión recurrente automatizada
4. ✅ Introducir catálogos `FuenteIngreso` y `CategoriaEgreso` (estandarización)
5. ✅ Introducir `ComprobanteContable` + `MovimientoContable` para doble partida
6. ✅ Introducir `ProyectoSocial` para trazabilidad de gasto social (RTE)

**Modelo alineado al negocio real**:
- ✅ Aportes de $20k COP recurrentes
- ✅ Ventas de mercancía (souvenirs, jerseys, parches)
- ✅ Ventas Casa Club (café, cerveza, comida, artículos moteros)
- ✅ Donaciones con trazabilidad RTE
- ✅ Proyectos sociales con presupuesto y ejecución

### 11.2 Próximo Paso Inmediato
**Comenzar Fase 1.0**:
1. Crear migración EF Core con entidades:
   - `CuentaFinanciera`
   - `FuenteIngreso`
   - `CategoriaEgreso`
   - `MovimientoTesoreria`
   - `AporteMensual`
2. Crear seeds con catálogos iniciales
3. Verificar build sin errores
4. Commit: "feat: add Phase 1.0 entities - treasury core model"

**Comando para crear migración**:
```bash
cd src/Server
dotnet ef migrations add Phase1_TreasuryCore --context AppDbContext
```

---

**Aprobación requerida del usuario antes de continuar con Fase 1.0**.
