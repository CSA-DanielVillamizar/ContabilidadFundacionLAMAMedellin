# Guía de Modernización UI/UX (Blazor Server)

Objetivo: Unificar la experiencia visual bajo un estándar L.A.M.A. estilo SaaS financiera premium, sin modificar lógica de negocio.

## Estándar de Página
- LamaPageHeader: título grande + subtítulo + icono + acciones.
- LamaFilterCard: contenedor de filtros con `ChildContent` y `Actions`.
- LamaTableWrapper + MudTable/MudDataGrid: tabla profesional con columnas numéricas (`.lama-numeric`).
- LamaBadge: estados y etiquetas semánticas.
- MudDialog: crear/editar/anular (componentes en `Components/Tesoreria`).
- LamaEmptyState: mensaje con icono y acción opcional.
- LamaToastService: notificaciones Success/Error/Warning.

## Reglas Obligatorias
- No tocar servicios, consultas, modelos ni validaciones.
- Prohibido Bootstrap en Tesorería (alert/table/card/row/col/btn).
- Cero estilos inline en páginas refactorizadas; usar `wwwroot/css/lama-theme.css`.
- Mensajes `successMessage/errorMessage` → `LamaToastService`.
- Compila en Release.

## Diseño Global
- Archivo: `Pages/_Host.cshtml` ya incluye `css/lama-theme.css`.
- Fuentes: Inter (principal) y Roboto Mono (numérica) via `lama-theme.css`.
- Tokens en `lama-theme.css`: colores, sombras, radius 16px, tipografía y utilidades.

## Componentes Compartidos
- `Components/Shared/LamaPageHeader.razor`
- `Components/Shared/LamaFilterCard.razor`
- `Components/Shared/LamaTableWrapper.razor` (estilos definidos en `lama-theme.css`).
- `Components/Shared/LamaBadge.razor` (sin estilos inline).
- `Components/Shared/LamaEmptyState.razor` (`Message` o `Description`).
- `Components/Tesoreria/MovimientoTesoreriaFormDialog.razor`
- `Components/Tesoreria/MovimientoTesoreriaAnularDialog.razor`

## Patrón de Diálogos (MudBlazor)
```csharp
var parameters = new DialogParameters {
  ["Modo"] = "create",
  ["Movimiento"] = movimiento,
  ["Cuentas"] = cuentas,
  ["Fuentes"] = fuentes,
  ["Categorias"] = categorias
};
var dialogRef = DialogService.Show<MovimientoTesoreriaFormDialog>("Nuevo", parameters);
var result = await dialogRef.Result;
if (!result.Canceled) { /* persistir y Toast */ }
```

## Migraciones Comunes
- Reemplazar `<table class="table">` por `MudTable` con `LamaTableWrapper`.
- Sustituir `alert/card/row/col/btn` por componentes Mud.
- Mover estilos embebidos a `lama-theme.css`.

---

## Estado de Modernización - Módulo Tesorería

### ✅ Completadas (Patrón Premium Aplicado)

#### 1. **CertificadosDonacion.razor** – Listado de Certificados de Donación
- **Patrón Aplicado:**
  - `LamaPageHeader` con icono 📜, botón "Nuevo Certificado"
  - `LamaFilterCard` con búsqueda (donante), fecha (desde/hasta), estado (Borrador/Emitido/Anulado)
  - `MudTable` con columnas: Numero, Donante, Valor (`.lama-numeric`), Fecha, Estado (`LamaBadge`)
  - Acciones (Editar/Ver PDF/Anular) con diálogos MudDialog
  - `LamaEmptyState` cuando no hay resultados
  - `LamaToastService` para notificaciones (Certificado creado, anulado, etc.)
- **Líneas:** 267
- **Build:** ✅ Success | **Commit:** `feat(ui): modernize donaciones (premium)`

#### 2. **Reportes.razor** – Dashboard de Reportes Mensuales
- **Patrón Aplicado:**
  - `LamaPageHeader` con icono 📊, botones Refrescar, Descargar PDF, Descargar Excel
  - `LamaFilterCard` con filtros: Año (2020-2099), Mes (1-12) vía `MudNumericField`
  - **4 KPI Cards** (`LamaStatCard`):
    - **Saldo Inicial** (Primary): `$XX,XXX.XX`
    - **Ingresos** (Success): `$XX,XXX.XX`
    - **Egresos** (Danger): `$(XX,XXX.XX)`
    - **Saldo Final** (condicional: Success si ≥0, Danger si <0)
  - Loading: Grid de `MudSkeleton SkeletonType.Text` mientras se genera
  - `LamaEmptyState` si no hay datos para el período
  - Sección de detalles con tabla simple
- **Líneas:** 167 | **Build:** ✅ Success | **Commit:** `[main 7c03679] feat(ui): modernize reportes (premium)`

#### 3. **ReportesDonacionesCertificados.razor** – Auditoria Donaciones vs Certificados
- **Patrón Aplicado:**
  - `LamaPageHeader` con icono 🎁, botón Refrescar
  - `LamaFilterCard` con rango de fechas (desde/hasta) vía `MudDatePicker`
  - **Dos columnas de resumen** (`MudPaper`):
    - **Donaciones sin Certificado:** recuento + `LamaBadge` rojo
    - **Certificados sin Donación:** recuento + `LamaBadge` rojo
  - `MudTable` por cada sección (max-height 400px, scroll):
    - Columnas: Numero, Donante, Valor (`.lama-numeric`), Fecha
  - **4 KPI Summary Cards** al pie:
    - Total Donaciones, Total Certificados, Discrepancias, Valor en Diferencia
  - Loading con `MudSkeleton` para ambas columnas
  - `LamaEmptyState` por sección si vacía
  - `LamaToastService` integrado
- **Líneas:** 250+ | **Build:** ✅ Success | **Commit:** `[main 31a294b] feat(ui): modernize certificados donaciones reportes (premium)` (232 +, 147 -)

#### 4. **Verificacion.razor** – Auditoría y Reparación de Datos
- **Líneas:** 180 | **Build:** ✅ Success | **Commit:** `[main 9e9e19d] feat(ui): modernize verificacion (premium)`

#### 5. **Cierre.razor** – Cierre Contable Mensual
- **Líneas:** ~220 | **Build:** ✅ Success | **Commit:** `[main 0dcb7b0] feat(ui): modernize cierre (premium)` (194 -, 235 +)
- **Componentes Creados:** `Components/Tesoreria/CierreConfirmDialog.razor`

#### 6. **Respaldo.razor** – Exportación de Datos y Guía de Respaldos BD
- **Líneas:** 242 | **Build:** ✅ Success | **Commit:** `[de513de] feat(ui): modernize respaldo (premium)` (176 -, 159 +)
- **Patrón Aplicado:**
  - `LamaPageHeader` con icono backup, botones (acciones en main header)
  - Dos secciones `MudPaper` (Recibos Success / Egresos Error) con `MudDatePicker` desde/hasta + botones descarga
  - Sección respaldo BD: `MudAlert Info` + 2 `MudPaper` (SSMS method con `MudList<string>`, T-SQL method con code block + botón copiar)
  - `LamaToastService` integrado

#### 7. **Presupuestos.razor** – Gestión de Presupuestos y Ejecución
- **Líneas:** 812 | **Build:** ✅ Success | **Commit:** `[c3974ce] feat(ui): modernize presupuestos (premium - pasadas A/B/C)` 
- **Patrón Aplicado (Pasadas A/B/C):**
  - `LamaPageHeader` con icono calculate, botones (Copiar Presupuestos Outlined, Nuevo Presupuesto Filled)
  - `LamaFilterCard` con `MudSelect` (año/mes/concepto) + handlers OnAnoChanged/OnMesChanged/OnConceptoChanged
  - **4 KPI Cards** (`LamaStatCard`):
    - Total Presupuestado (Primary), Total Ejecutado (Success), Diferencia (Warning), % Ejecución Promedio (Info)
  - Tabla Tailwind con `.lama-numeric` en columnas monetarias (Presupuestado/Ejecutado/Diferencia)
  - Helpers: `FormatCurrency()`, `GetPorcentajeEjecucionTexto()`, `GetEstadoEjecucion()`
  - **Lógica funcional intacta**: Paginación Tailwind, modal eliminar inline, acciones Ver/Editar/Eliminar con SVG icons
- **Notas:** Pasada D (diálogos) omitida por estrategia de evitar refactors peligrosos. Tabla mantiene estructura Tailwind funcional. Sin Bootstrap. Sin gradients inline en código actualizado.

#### 8. **ConciliacionesBancarias.razor** – Conciliación Bancaria por Período
- **Líneas:** 332 (antes) / ~300 (después) | **Build:** ✅ Success | **Commit:** `[8f297e4] feat(ui): modernize conciliaciones bancarias (premium)`
- **Patrón Aplicado:**
  - `LamaPageHeader` con icono account_balance, botones (Limpiar Outlined, Nueva Conciliación Primary)
  - `LamaFilterCard` con `MudSelect` (año/mes/estado: Pendiente/EnProceso/Conciliada/ConDiferencias)
  - **3 KPI Cards** (`LamaStatCard`): Conciliaciones (Primary), Conciliadas (Success), Pendientes (Warning)
  - Tabla Tailwind con `.lama-numeric` en columnas monetarias (Saldo Libros/Saldo Banco/Diferencia)
  - Estados con badges Tailwind (ClaseEstado switch helper)
  - Paginación Tailwind (Anterior/Siguiente)
  - **Modal Eliminar inline mantenido** (MudDialog no aplicado para evitar cambios en lógica)
- **Notas:** Sin Bootstrap. Lógica funcional intacta (setters automáticos en filtros).

### ⏳ Siguientes en Fila (Orden de Prioridad)

*(Actualizado: Todos los módulos Tesorería core completados)*

**COMPLETADO 8/8 MÓDULOS TESORERÍA:**
1. ✅ CertificadosDonacion
2. ✅ Reportes
3. ✅ ReportesDonacionesCertificados
4. ✅ Verificacion
5. ✅ Cierre
6. ✅ Respaldo
7. ✅ Presupuestos
8. ✅ ConciliacionesBancarias

---

## Checklist de Validación Post-Modernización
- [ ] Cero Bootstrap (alert, table, card, row, col, btn, etc.)
- [ ] Cero estilos inline `<style>` en página
- [ ] `LamaPageHeader` con icono y acciones
- [ ] `LamaFilterCard` envolviendo controles MudBlazor
- [ ] Tablas con `MudTable` + `.lama-numeric` para valores
- [ ] Estados con `LamaBadge` (success/warning/danger/primary)
- [ ] `LamaEmptyState` para casos sin datos
- [ ] `LamaToastService` inyectado y usado en acciones
- [ ] `dotnet build ... -c Release` → 0 errores, solo warnings pre-existentes
- [ ] Commit: `feat(ui): modernize <modulo> (premium)`

---

## QA
- `dotnet build -c Release` debe finalizar sin errores.
- Responsive: filtros apilan en móviles; tabla con `FixedHeader` y `Height`.

## Notas
- Evitar sobre-escribir MudBlazor; complementarlo con clases `.lama-*`.
- Mantener documentación en español técnico dentro de los componentes.
