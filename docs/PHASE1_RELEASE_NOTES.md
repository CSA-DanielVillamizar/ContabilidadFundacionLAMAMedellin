# Phase 1.0 - Módulo de Tesorería Core
**Fecha de implementación:** 21 de enero de 2026  
**Versión:** 1.0.0  
**Autor:** Sistema de Tesorería LAMA Medellín

---

## 🎯 Objetivo
Implementar el módulo core de tesorería para gestionar cuentas financieras, movimientos de ingresos/egresos, y aportes mensuales de miembros, con flujos de aprobación, cierre de períodos y trazabilidad completa.

---

## ✨ Nuevas Funcionalidades

### 1. **Cuentas Financieras**
- Gestión de cuentas bancarias y cajas
- Campos: Código único, Nombre, Banco, Número enmascarado, Tipo (Bancaria/Caja), Saldos (inicial/actual)
- Fecha de apertura y estado activo/inactivo
- **Seed inicial:** Bancolombia - Cuenta Principal Tesorería (BANCO-BCOL-001)

### 2. **Movimientos de Tesorería**
- Registro unificado de ingresos y egresos
- Campos clave:
  - Número de movimiento único
  - Fecha, Tipo (Ingreso/Egreso), Estado (Borrador/Aprobado/Anulado)
  - Medio de pago (Transferencia, Consignación, Efectivo, Nequi, Daviplata, Tarjeta, Cheque)
  - Clasificación por Fuente de Ingreso o Categoría de Egreso
  - Referencia de transacción y comprobantes adjuntos
- **Regla crítica:** Solo movimientos aprobados afectan los saldos calculados
- **Protección de períodos cerrados:** No se permiten cambios en meses con cierre contable

### 3. **Aportes Mensuales**
- Seguimiento de contribuciones mensuales de miembros ($20,000 COP)
- Estados: Pendiente, Pagado, Exonerado
- Vinculación con MovimientoTesoreria al registrar pago
- Índice único: un aporte por miembro/mes/año (no duplicados)

### 4. **Catálogos de Clasificación**

#### **Fuentes de Ingreso (seeds incluidos):**
- APORTE-MEN: Aporte Mensual Miembro
- VENTA-MERCH: Venta Merchandising
- VENTA-CLUB-ART: Venta Club Arte
- VENTA-CLUB-CAFE: Venta Club Café
- VENTA-CLUB-CERV: Venta Club Cerveza
- VENTA-CLUB-COMI: Venta Club Comida
- DONACION: Donaciones
- EVENTO: Eventos y actividades
- RENOVACION-MEM: Renovación membresía
- OTROS: Otros ingresos

#### **Categorías de Egreso (seeds incluidos):**
- AYUDA-SOCIAL: Ayudas sociales
- EVENTO-LOG: Logística de eventos
- COMPRA-MERCH: Compra merchandising
- COMPRA-CLUB-CAFE: Compra insumos café
- COMPRA-CLUB-CERV: Compra insumos cerveza
- COMPRA-CLUB-COMI: Compra insumos comida
- COMPRA-CLUB-OTROS: Compra otros insumos
- ADMIN-PAPEL: Papelería y útiles
- ADMIN-TRANSP: Transporte y desplazamientos
- ADMIN-SERVICIOS: Servicios públicos/administrativos
- MANTENIMIENTO: Mantenimiento y reparaciones
- OTROS-GASTOS: Otros gastos

---

## 🗂️ Estructura de Base de Datos

### **Nuevas Tablas**
1. `CuentasFinancieras` - Cuentas bancarias y cajas
2. `MovimientosTesoreria` - Ingresos y egresos
3. `FuentesIngreso` - Catálogo de clasificación de ingresos
4. `CategoriasEgreso` - Catálogo de clasificación de egresos
5. `AportesMensuales` - Aportes mensuales de miembros

### **Índices y Restricciones**
- Códigos únicos en CuentasFinancieras, FuentesIngreso, CategoriasEgreso
- Número de movimiento único en MovimientosTesoreria
- Índice único (MiembroId, Ano, Mes) en AportesMensuales
- Relaciones con DeleteBehavior.Restrict para evitar eliminaciones en cascada

### **Migración EF Core**
- **Nombre:** `Phase1_TreasuryCore`
- **Fecha:** 21 de enero de 2026
- **Comando:** `dotnet ef migrations add Phase1_TreasuryCore`

---

## 📱 Páginas de Usuario

### 1. `/tesoreria/cuentas-financieras`
- **Roles:** Admin, Tesorero
- **Funcionalidad:**
  - Listar todas las cuentas financieras
  - Crear nueva cuenta (Código, Nombre, Banco, Tipo)
  - Ver saldo actual por cuenta

### 2. `/tesoreria/movimientos`
- **Roles:** Admin, Tesorero
- **Funcionalidad:**
  - Listar movimientos con filtros: fecha inicio/fin, cuenta, tipo, estado
  - Crear nuevo movimiento (Borrador por defecto)
  - Validación automática: no permite crear movimientos en períodos cerrados
  - Evita duplicados por número de movimiento

### 3. `/tesoreria/aportes`
- **Roles:** Admin, Tesorero
- **Funcionalidad:**
  - Listar aportes mensuales por año/mes
  - Filtros: estado (Pendiente/Pagado/Exonerado), miembro específico
  - Vista de estado de pagos por miembro

---

## 🔐 Seguridad y Autorización

### **Políticas de Acceso**
- **Admin y Tesorero:** Acceso completo a todas las funcionalidades
- **Junta y Consulta:** Solo lectura (próxima fase)
- Todas las páginas requieren autenticación
- Atributo `[Authorize(Roles = "Admin,Tesorero")]` en páginas nuevas

### **Validaciones de Negocio**
1. **Período Cerrado:** Bloqueo automático de cambios en meses con cierre contable
2. **Duplicados:** Validación de unicidad en números de movimiento y aportes mensuales
3. **Saldos:** Solo movimientos aprobados afectan el saldo calculado

---

## 🧪 Tests Implementados

### **Phase1TreasuryRulesTests.cs**
1. `NoDuplicaAportePorMiembroMesAno` - Valida índice único en AportesMensuales
2. `BloqueaMovimientoSiPeriodoCerrado` - Verifica bloqueo en meses cerrados
3. `SoloAprobadosAfectanSaldoCalculado` - Confirma que solo movimientos aprobados suman al saldo

**Estado:** ✅ Todos los tests pasando (41 tests totales en el proyecto)

---

## 🛠️ Configuración Técnica

### **Entorno de Desarrollo**
- .NET 8.0
- Entity Framework Core 8.0
- Blazor Server
- SQL Server (local: Trusted_Connection; prod: Managed Identity)

### **Conexión Base de Datos**
```bash
# Desarrollo (local)
Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;

# Producción (Azure SQL con Managed Identity)
Server=<azure-sql-server>;Database=LamaMedellin;Authentication=Active Directory Default;
```

### **Aplicar Migración**
```bash
cd src/Server
dotnet ef database update
```

---

## 📋 Próximos Pasos (Roadmap)

### **Fase 1.0.1 - Integración UI (completada en este release)**
- ✅ Menú de navegación con enlaces a nuevas páginas
- ✅ Validación de roles en navegación

### **Fase 1.2 - Importación de Histórico (siguiente)**
- Importador Excel desde `INFORME TESORERIA.xlsx`
- Carga de histórico mayo 2024 → noviembre 2025
- Validación de saldos y conciliación
- Idempotencia (no duplicar si se ejecuta 2 veces)

### **Fase 1.3 - Dashboard y Reportes**
- Dashboard de tesorería con indicadores clave
- Reportes de ingresos/egresos por período
- Gráficos de tendencias

### **Fase 1.4 - Flujo de Aprobación**
- Workflow para aprobar movimientos borradores
- Notificaciones por email
- Auditoría de aprobaciones

---

## 🚨 Notas Importantes

1. **No Modificar Flujos Existentes:** Este módulo es aditivo; no altera funcionalidades de Recibos, Egresos o Deudores existentes.
2. **Seed Automático:** Al aplicar la migración, se crean automáticamente:
   - Cuenta Bancolombia (BANCO-BCOL-001)
   - 10 Fuentes de Ingreso
   - 11 Categorías de Egreso
3. **Producción:** Antes de desplegar en Azure:
   - Configurar Managed Identity en App Service
   - Configurar autenticación Entra ID en Azure SQL
   - Permisos mínimos: db_datareader + db_datawriter (NO db_owner)
4. **Región Azure:** Central US para todos los recursos

---

## 📞 Soporte y Contacto
Para preguntas o soporte técnico, contactar al equipo de desarrollo de LAMA Medellín.

---

**Fin del documento Phase 1.0 Release Notes**
