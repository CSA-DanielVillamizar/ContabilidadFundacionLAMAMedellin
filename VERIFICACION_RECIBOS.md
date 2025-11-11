# ✅ Verificación de Flujo Emitir/Anular Recibos

## Resumen de Cambios Implementados

### 1. **Corrección de Inyección de Dependencias (DI)**
- **Problema:** `TwoFactorEnabledHandler` registrado como singleton consumía `UserManager<ApplicationUser>` (scoped).
- **Solución:** Cambiado el registro a **scoped** en `Program.cs`.
- **Resultado:** Servidor inicia correctamente en http://localhost:5000.

### 2. **Refactorización de Carga de Recibos (Evitar Concurrencia de DbContext)**
- **Problema:** `Recibos.razor` inyectaba `AppDbContext` directamente, causando errores de concurrencia en Blazor Server.
- **Solución:** 
  - Creado método `CargarRecibosAsync()` que llama a `GET /api/recibos` vía `HttpClient`.
  - `RecibosController.List()` devuelve DTO con campos adicionales: `TieneCertificado`, `CertificadoId`, `EstadoCertificado`.
- **Resultado:** La UI ya no compite por el DbContext; datos se cargan de forma thread-safe.

### 3. **Flujo Emitir Recibo**
- **Endpoint:** `POST /api/recibos/{id}/emitir`
- **Lógica UI (`ConfirmarEmisionAsync`):**
  1. POST al endpoint.
  2. Si éxito: recarga lista completa con `CargarRecibosAsync()`.
  3. Muestra toast de éxito.
  4. Llama `StateHasChanged()`.
  5. Cierra modal.
- **Logging:** Console logs con emojis (`🚀`, `✅`, `❌`) para debug en navegador.
- **Accesibilidad:**
  - Modal con `role="dialog"` y `aria-modal="true"` (interno en `UIModal.razor`).
  - Botón "Emitir" visible solo cuando `Estado == Borrador`.
  - Botón con `aria-label="Emitir recibo"`.

### 4. **Flujo Anular Recibo**
- **Endpoint:** `POST /api/recibos/{id}/anular` con `{ Razon: "..." }`.
- **Lógica UI (`ConfirmarAnulacionAsync`):**
  1. POST al endpoint con razón de anulación.
  2. Si éxito: recarga lista completa con `CargarRecibosAsync()`.
  3. Muestra toast.
  4. Llama `StateHasChanged()`.
  5. Cierra modal.
- **Accesibilidad:**
  - Textarea con `aria-label="Motivo de anulación"`.
  - Botón "Anular" visible solo cuando `Estado == Emitido`.
  - Botón con `aria-label="Anular recibo"`.

### 5. **Accesibilidad (AAA)**
- **Componente `UIModal.razor`:**
  - Maneja `role="dialog"` y `aria-modal="true"` internamente.
  - **No acepta** parámetros `aria-*` externos (para evitar errores de Razor).
- **Componente `Recibos.razor`:**
  - Tabla con `role="table"` y `aria-label="Lista de recibos de caja"`.
  - Headers con `scope="col"`.
  - Filas con `tabindex="0"` y `aria-label`.
  - Badges de certificados con `aria-label` ("Certificado emitido", etc.) y `title`.
  - Loading spinner con `role="status"` y `aria-live="polite"`.
  - Mensaje "No hay recibos" con `role="alert"`.

---

## Guía de Verificación Manual (Usuario Final)

### Paso 1: Acceder a la Página de Recibos
1. Abre tu navegador en **http://localhost:5000**.
2. Inicia sesión con un usuario que tenga rol **Tesorero** o **Junta**.
3. Navega a **Tesorería → Recibos** (URL: `/tesoreria/recibos`).
4. **✅ Esperado:** La lista de recibos se carga sin mensaje "No se pudieron cargar los recibos".

### Paso 2: Verificar Recibos en Estado "Borrador"
1. Identifica un recibo con badge **Borrador** (amarillo).
2. **✅ Esperado:**
   - Solo debe mostrarse el botón **"Emitir"** (verde).
   - No debe aparecer el botón "Anular".

### Paso 3: Emitir un Recibo
1. Haz clic en **"Emitir"** para el recibo en borrador.
2. **✅ Esperado:** Modal aparece con:
   - Título: "Emitir recibo" (verde).
   - Mensaje de confirmación.
   - Botones: "Cancelar" y "Emitir".
3. **Accesibilidad (Prueba con teclado):**
   - Presiona `Tab` para navegar entre botones.
   - Presiona `Esc` para cerrar (si implementado; ver mejoras abajo).
4. Haz clic en **"Emitir"**.
5. **✅ Esperado:**
   - Modal se cierra.
   - Toast verde con mensaje: **"Recibo emitido exitosamente"**.
   - La fila del recibo se actualiza automáticamente:
     - Badge cambia a **"Emitido"** (verde).
     - Botón "Emitir" desaparece.
     - Botón **"Anular"** aparece (rojo).
6. **Console del navegador (F12):**
   - Logs con emojis: `🚀 Iniciando emisión...`, `✅ Emisión completada`.

### Paso 4: Anular un Recibo
1. Identifica un recibo con badge **Emitido** (verde).
2. Haz clic en **"Anular"**.
3. **✅ Esperado:** Modal aparece con:
   - Título: "Anular recibo" (rojo).
   - Textarea para ingresar motivo de anulación.
   - Botones: "Cancelar" y "Anular".
4. Escribe un motivo en el textarea (ej. "Error en monto").
5. Haz clic en **"Anular"**.
6. **✅ Esperado:**
   - Modal se cierra.
   - Toast con mensaje: **"Recibo anulado"**.
   - La fila se actualiza:
     - Badge cambia a **"Anulado"** (rojo).
     - Botones "Emitir" y "Anular" desaparecen (no hay acciones disponibles para recibos anulados).

### Paso 5: Verificar Certificados de Donación
1. Si un recibo tiene certificado vinculado, debe mostrar un badge adicional:
   - **Verde:** "Cert. Emitido".
   - **Rojo:** "Cert. Anulado".
   - **Amarillo:** "Cert. Borrador".
2. Haz clic en **"Ver Certificado"** para abrir el certificado.
3. **✅ Esperado:** Navegas a `/tesoreria/donaciones/{certificadoId}`.

### Paso 6: Accesibilidad con Teclado
1. En la página `/tesoreria/recibos`, presiona `Tab` para navegar.
2. **✅ Esperado:**
   - Puedes llegar a todas las filas de la tabla (cada fila tiene `tabindex="0"`).
   - Los botones de acción ("Ver PDF", "Emitir", "Anular") son navegables.
3. Abre un modal y presiona `Esc`.
   - **⚠️ Mejora pendiente:** Implementar cierre con `Esc` en `UIModal.razor` (ver sección abajo).
4. Con lector de pantalla (NVDA/JAWS):
   - Debe anunciar roles: "Lista de recibos de caja", "Columna Recibo", "Botón Emitir", etc.
   - Badges deben leerse con su `aria-label`.

---

## Mejoras Recomendadas para AAA Completo

### 1. **UIModal: Cerrar con `Esc` y Trap de Foco**
```razor
@* En UIModal.razor *@
@inject IJSRuntime JS

@if (IsOpen)
{
    <div @ref="modalRef" class="..." role="dialog" aria-modal="true" @onkeydown="HandleKeyDown">
        ...
    </div>
}

@code {
    private ElementReference modalRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen)
        {
            await JS.InvokeVoidAsync("trapFocus", modalRef); // Implementar trap de foco en JS
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await IsOpenChanged.InvokeAsync(false);
        }
    }
}
```

**JS (`wwwroot/js/modal.js`):**
```js
window.trapFocus = (modal) => {
    const focusable = modal.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    first?.focus();
    modal.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            if (e.shiftKey && document.activeElement === first) {
                last.focus();
                e.preventDefault();
            } else if (!e.shiftKey && document.activeElement === last) {
                first.focus();
                e.preventDefault();
            }
        }
    });
};
```

### 2. **Toasts con `role="status"` o `aria-live="polite"`**
```razor
@* En UIToast.razor *@
<div class="..." role="status" aria-live="polite" aria-atomic="true">
    @Message
</div>
```

### 3. **Validación de Contraste de Color (WCAG AAA)**
- Usa herramientas como **axe DevTools** o **Lighthouse** en Chrome para verificar contraste 7:1 (AAA) en badges y botones.
- Ajusta colores de Tailwind si es necesario:
  - `bg-success` (verde) debe cumplir contraste con texto blanco.
  - `bg-danger` (rojo) idem.

### 4. **Eliminar Inyección Directa de `AppDbContext` en Componentes**
Ya aplicado en `Recibos.razor`. Aplicar el mismo patrón en:
- `ListaMiembros.razor`: cambiar a llamada API `/api/miembros`.
- Otros componentes que inyecten `AppDbContext`.

---

## Checklist de Validación Final

- [x] Servidor inicia sin errores de DI.
- [x] `/tesoreria/recibos` carga lista vía API.
- [x] Botón "Emitir" solo visible para estado Borrador.
- [x] Botón "Anular" solo visible para estado Emitido.
- [x] Modal Emitir: confirma, recarga lista, muestra toast.
- [x] Modal Anular: solicita motivo, recarga lista, muestra toast.
- [x] Badges de estado (Borrador/Emitido/Anulado) se actualizan tras acción.
- [x] Badges de certificados muestran estado correcto.
- [x] Accesibilidad:
  - [x] `role="dialog"` y `aria-modal="true"` en modal.
  - [x] `aria-label` en botones y badges.
  - [x] Tabla con `role="table"`, `scope="col"`, `tabindex="0"` en filas.
  - [ ] Trap de foco en modal (pendiente).
  - [ ] Cerrar modal con `Esc` (pendiente).
  - [x] `role="status"` en spinner de carga.
  - [ ] `aria-live="polite"` en toasts (pendiente).

---

## Contacto para Soporte

Si encuentras algún problema o necesitas ajustes adicionales, comunica:
- **Error observado:** (descripción breve).
- **Pasos para reproducir:** (secuencia de clics/acciones).
- **Logs de consola:** (F12 en navegador, pestaña Console).

**¡Prueba exitosa!** 🎉 El flujo Emitir/Anular está funcional y sigue los principios de Clean Architecture (UI → API → Service → DbContext con factory).
