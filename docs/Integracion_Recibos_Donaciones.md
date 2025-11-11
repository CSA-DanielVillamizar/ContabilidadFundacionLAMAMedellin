# Integración Recibos ↔ Certificados de Donación

## ✅ Implementación Completada

Se ha implementado la integración automática entre el sistema de **Recibos de Caja** y los **Certificados de Donación (RTE)**.

---

## 🎯 Funcionalidad

### Flujo Automático

```
Usuario crea Recibo → Guarda Recibo → Sistema detecta concepto DONACION 
    ↓
Modal aparece: "¿Generar Certificado de Donación?"
    ↓
    ├─→ Usuario dice "SÍ" → Redirige a formulario con datos pre-llenados
    │                        ↓
    │                    Usuario completa/revisa → Guarda → Emite PDF
    │
    └─→ Usuario dice "NO" → Redirige a lista de recibos
```

---

## 📝 Detalles de Implementación

### 1. Detección Automática

**Ubicación**: `RecibosForm.razor` - Método `GuardarReciboAsync()`

**Lógica**:
```csharp
// Después de guardar el recibo
var tieneDonacion = formData.Items.Any(item => 
{
    var concepto = conceptos.FirstOrDefault(c => c.Id == item.ConceptoId);
    return concepto != null && concepto.Codigo.Contains("DONACION", StringComparison.OrdinalIgnoreCase);
});

if (tieneDonacion) {
    // Mostrar modal para crear certificado
}
```

**Criterio**: Busca conceptos cuyo código contenga "DONACION" (insensible a mayúsculas/minúsculas).

### 2. Modal de Confirmación

**Componente**: Modal Bootstrap en `RecibosForm.razor`

**Elementos**:
- ✅ Título con ícono de certificado
- ✅ Mensaje de confirmación
- ✅ Muestra nombre del donante detectado
- ✅ Tip sobre auto-llenado de datos
- ✅ Botones:
  - **"No, ir a lista de recibos"** → Navegación normal
  - **"Sí, Crear Certificado"** → Pre-llenado automático

### 3. Pre-llenado Automático de Datos

**Ubicación**: `CertificadosDonacionForm.razor` - Método `CargarDatosDesdeRecibo()`

**Datos Auto-completados desde el Recibo**:

| Campo Certificado | Origen | Notas |
|-------------------|--------|-------|
| **ReciboId** | `recibo.Id` | Vincula certificado con recibo |
| **FechaDonacion** | `recibo.FechaEmision` | Fecha del recibo |
| **ValorDonacionCOP** | `recibo.TotalCop` | Total del recibo |
| **FormaDonacion** | "Transferencia Bancaria" | Valor por defecto (editable) |
| **DescripcionDonacion** | `recibo.Items` → conceptos | "Concepto1 (x2), Concepto2 (x1)" |

**Datos Auto-completados desde el Miembro** (si existe):

| Campo Certificado | Origen | Notas |
|-------------------|--------|-------|
| **TipoIdentificacionDonante** | "CC" | Por defecto (editable) |
| **IdentificacionDonante** | `miembro.Cedula` | |
| **NombreDonante** | `miembro.NombreCompleto` | |
| **DireccionDonante** | `miembro.Direccion` | |
| **TelefonoDonante** | `miembro.Celular` | |
| **EmailDonante** | `miembro.Email` | |

**Si es Tercero Libre** (sin miembro):
- Solo se llena el `NombreDonante` con `recibo.TerceroLibre`

### 4. Navegación con Query String

**URL Generada**:
```
/tesoreria/donaciones/nuevo?reciboId={GUID}
```

**Procesamiento**:
```csharp
var uri = new Uri(Navigation.Uri);
var queryParams = QueryHelpers.ParseQuery(uri.Query);

if (queryParams.TryGetValue("reciboId", out var reciboIdStr) 
    && Guid.TryParse(reciboIdStr, out var reciboId))
{
    await CargarDatosDesdeRecibo(reciboId);
}
```

---

## 🎬 Casos de Uso

### Caso 1: Recibo con Miembro + Concepto DONACION

**Escenario**:
1. Tesorero crea recibo para **Juan Pérez** (miembro activo)
2. Agrega item: Concepto "DONACION" - $500,000 COP
3. Guarda recibo

**Resultado**:
- Modal aparece con nombre "Juan Pérez"
- Si acepta, formulario certificado pre-llena:
  - ✅ Cédula de Juan
  - ✅ Nombre completo
  - ✅ Dirección, teléfono, email
  - ✅ Valor: $500,000
  - ✅ Descripción: "DONACION (x1)"
  - ✅ Fecha: fecha del recibo
  - ✅ ReciboId vinculado

**Acción Usuario**: Solo necesita revisar, ajustar si es necesario, y emitir.

### Caso 2: Recibo con Tercero Libre + DONACION

**Escenario**:
1. Tesorero crea recibo para **"Empresa ABC S.A.S."** (tercero libre, sin ficha de miembro)
2. Agrega item: Concepto "DONACION ESPECIE" - $2,000,000 COP
3. Guarda recibo

**Resultado**:
- Modal aparece con nombre "Empresa ABC S.A.S."
- Si acepta, formulario certificado pre-llena:
  - ✅ Nombre: "Empresa ABC S.A.S."
  - ✅ Valor: $2,000,000
  - ✅ Descripción: "DONACION ESPECIE (x1)"
  - ❌ Identificación, dirección: VACÍOS (usuario debe llenar)

**Acción Usuario**: Completar tipo y número de NIT, dirección, teléfono, email, luego emitir.

### Caso 3: Recibo SIN Concepto Donación

**Escenario**:
1. Tesorero crea recibo con concepto "MENSUALIDAD" - $100,000
2. Guarda recibo

**Resultado**:
- ✅ NO aparece modal
- ✅ Navegación directa a lista de recibos
- ✅ Toast: "Recibo guardado exitosamente"

---

## 🛠️ Archivos Modificados

### 1. `RecibosForm.razor`

**Cambios**:
- ✅ Agregado `@inject ICertificadosDonacionService`
- ✅ Variables de estado: `mostrarModalCertificado`, `reciboIdParaCertificado`, `nombreTerceroParaCertificado`
- ✅ Lógica en `GuardarReciboAsync()` para detectar donaciones
- ✅ Modal de confirmación (HTML)
- ✅ Métodos:
  - `IrAListaRecibos()` - Cierra modal y navega
  - `CrearCertificadoDonacion()` - Navega con query string

### 2. `CertificadosDonacionForm.razor`

**Cambios**:
- ✅ Agregado `@inject IRecibosService`
- ✅ Agregado `@inject IMiembrosService`
- ✅ Agregado `@using Microsoft.AspNetCore.WebUtilities`
- ✅ Lógica en `OnInitializedAsync()` para detectar query string
- ✅ Nuevo método: `CargarDatosDesdeRecibo(Guid reciboId)`
  - Obtiene recibo completo
  - Extrae valor, fecha, descripción
  - Si hay miembro: obtiene datos completos
  - Si es tercero: solo nombre
  - Pre-llena modelo del formulario

---

## ✅ Ventajas de la Integración

### Para el Usuario (Tesorero)

1. **Ahorro de Tiempo**: No re-escribe datos que ya ingresó en el recibo
2. **Menos Errores**: Los valores y fechas se copian exactamente
3. **Flujo Natural**: Recibo → Certificado en un solo flujo
4. **Opcional**: Puede decir "NO" si no quiere certificado ahora
5. **Trazabilidad**: Certificado vinculado al recibo (`ReciboId`)

### Para la Organización

1. **Consistencia**: Certificados siempre vinculados a recibos
2. **Auditoría**: Relación bidireccional Recibo ↔ Certificado
3. **Cumplimiento**: Facilita generar certificados RTE sin omisiones
4. **Reportes**: Puede cruzar donaciones con recibos de caja

---

## 🔍 Validaciones y Casos Especiales

### ¿Qué pasa si el recibo tiene múltiples conceptos?

```
Recibo con:
- Item 1: MENSUALIDAD - $100,000
- Item 2: DONACION - $500,000
```

**Respuesta**: Modal aparece porque AL MENOS UN concepto es donación. La descripción incluirá ambos conceptos, pero el usuario puede editarla.

### ¿Qué pasa si el donante no es miembro?

**Respuesta**: Solo se pre-llena el nombre desde `TerceroLibre`. El usuario debe completar manualmente: tipo ID, número ID, dirección, teléfono, email.

### ¿El usuario puede NO crear el certificado ahora?

**Sí**. Puede:
1. Clic en "No, ir a lista de recibos"
2. Más tarde, ir a `/tesoreria/donaciones/nuevo`
3. Crear certificado manualmente
4. En el formulario, puede vincular con el recibo si recuerda el ID

---

## 📊 Flujo Visual Completo

```
┌─────────────────────────────────────────────────────────────┐
│ 1. CREAR RECIBO                                             │
│    - Miembro: Juan Pérez                                    │
│    - Concepto: DONACION - $500,000                          │
│    - Guardar                                                │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. SISTEMA DETECTA                                          │
│    ✓ Recibo guardado con ID: abc-123-def                   │
│    ✓ Concepto contiene "DONACION"                          │
│    ✓ Donante: Juan Pérez (miembro activo)                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. MODAL DE CONFIRMACIÓN                                    │
│    ┌───────────────────────────────────────────────────┐   │
│    │ ¿Generar Certificado de Donación (RTE)?          │   │
│    │                                                   │   │
│    │ Donante: Juan Pérez                              │   │
│    │                                                   │   │
│    │ [No, ir a lista] [Sí, Crear Certificado]        │   │
│    └───────────────────────────────────────────────────┘   │
└─────────────────┬───────────────────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
┌──────────────────┐  ┌──────────────────────────────────────┐
│ OPCIÓN A: NO     │  │ OPCIÓN B: SÍ                         │
│                  │  │                                      │
│ → Lista Recibos  │  │ → Formulario Certificado             │
│                  │  │   Pre-llenado:                       │
│ Toast: OK        │  │   ✓ Cédula: 12.345.678              │
│                  │  │   ✓ Nombre: Juan Pérez              │
│                  │  │   ✓ Email: juan@email.com           │
│                  │  │   ✓ Valor: $500,000                 │
│                  │  │   ✓ Fecha: hoy                      │
│                  │  │   ✓ Descripción: DONACION (x1)      │
│                  │  │   ✓ ReciboId: abc-123-def           │
└──────────────────┘  └──────────────────┬───────────────────┘
                                         │
                                         ▼
                      ┌──────────────────────────────────────┐
                      │ 4. USUARIO REVISA/EDITA              │
                      │    - Ajusta descripción si quiere    │
                      │    - Cambia forma donación           │
                      │    - Guarda borrador                 │
                      └──────────────────┬───────────────────┘
                                         │
                                         ▼
                      ┌──────────────────────────────────────┐
                      │ 5. EMITIR CERTIFICADO                │
                      │    - Asigna consecutivo CD-2025-0001│
                      │    - Genera PDF oficial              │
                      │    - Firmas: Representante + Contador│
                      └──────────────────┬───────────────────┘
                                         │
                                         ▼
                      ┌──────────────────────────────────────┐
                      │ 6. DESCARGAR PDF                     │
                      │    ✓ Certificado listo para donante │
                      │    ✓ Vinculado a recibo              │
                      └──────────────────────────────────────┘
```

---

## 🎓 Conclusión

La integración entre recibos y certificados de donación está **100% funcional** y lista para uso.

**Beneficios clave**:
- ✅ Flujo intuitivo y rápido
- ✅ Pre-llenado inteligente de datos
- ✅ Reduce errores de transcripción
- ✅ Vinculación automática Recibo ↔ Certificado
- ✅ Cumplimiento RTE facilitado
- ✅ Experiencia de usuario mejorada

**Próximos pasos opcionales**:
- Agregar botón "Ver Certificado" en la vista de recibos individuales
- Mostrar badge "Tiene Certificado" en lista de recibos
- Reportes cruzados: donaciones con/sin certificado

---

*Implementado: Enero 2025*  
*Versión: 1.0*
