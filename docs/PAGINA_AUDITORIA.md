# 📊 Página de Auditoría - Sistema LAMA Medellín

## ✅ Implementación Completada

### **Ubicación:**
`src/Server/Pages/Admin/Auditoria.razor`

### **Ruta:**
`/admin/auditoria`

### **Permisos:**
Solo accesible para usuarios con roles: **Admin** o **Tesorero**

---

## 🎨 Características de la UI

### **1. Filtros Avanzados**

La página incluye 7 filtros para búsquedas precisas:

| Filtro | Tipo | Valores | Descripción |
|--------|------|---------|-------------|
| **Tipo de Entidad** | Select | Todas, Certificados, Recibos, Miembros, Egresos, Cierres | Filtra por tipo de registro |
| **Acción** | Select | Todas, Emitido, Anulado, Creado, Actualizado, Eliminado | Filtra por operación realizada |
| **Usuario** | Text | email@fundacionlamamedellin.org | Busca por usuario específico |
| **Fecha Desde** | Date | dd/mm/yyyy | Rango inicial |
| **Fecha Hasta** | Date | dd/mm/yyyy | Rango final |
| **ID de Entidad** | Text | GUID | Busca logs de una entidad específica |
| **Registros** | Select | 50, 100, 200, 500 | Cantidad de resultados |

**Valores por Defecto:**
- Fecha Desde: Último mes
- Fecha Hasta: Hoy
- Registros: 100

---

## 📋 Tabla de Resultados

### **Columnas:**

1. **Fecha/Hora**
   - Fecha: formato dd/MM/yyyy
   - Hora: formato HH:mm:ss

2. **Usuario**
   - Nombre corto (antes del @)
   - Email completo debajo

3. **Entidad**
   - Badge con color según tipo
   - GUID truncado (8 caracteres)

4. **Acción**
   - Badge con color según acción:
     - Verde: Emitido, Creado
     - Rojo: Anulado, Eliminado
     - Azul: Actualizado

5. **Detalles**
   - Información adicional del log
   - Truncado con tooltip

6. **Acciones**
   - Botón "Ver" para detalles completos

---

## 🔍 Modal de Detalles

Al hacer clic en el botón "Ver", se muestra un modal con:

### **Información General:**
- Fecha y hora completa
- Usuario que realizó la acción
- Tipo de entidad
- ID completo de la entidad
- Acción realizada
- Dirección IP (si está disponible)

### **Información Adicional:**
- Descripción textual del evento
- Contexto adicional

### **Valores Anteriores (OldValues):**
- JSON formateado con sintaxis color
- Fondo rojo para indicar "antes"
- Solo si hay cambios

### **Valores Nuevos (NewValues):**
- JSON formateado con sintaxis color
- Fondo verde para indicar "después"
- Siempre presente en creaciones/actualizaciones

---

## 🎨 Código de Colores

### **Por Tipo de Entidad:**

```csharp
"CertificadoDonacion" → Verde (success)
"Recibo"             → Azul (primary)
"Miembro"            → Cyan (info)
"Egreso"             → Amarillo (warning)
"CierreMensual"      → Rojo (danger)
```

### **Por Acción:**

```csharp
"Emitted", "Created" → Verde (success)
"Annulled", "Deleted" → Rojo (danger)
"Updated"            → Azul (info)
```

---

## 🔧 Funcionalidades

### **1. Búsqueda por Entidad Específica**
Si se proporciona un `ID de Entidad`, la búsqueda se centra en ese registro específico:
```csharp
logs = await AuditService.GetEntityLogsAsync(filtroEntityType, filtroEntityId);
```

### **2. Búsqueda General**
Sin ID de entidad, obtiene los logs más recientes:
```csharp
logs = await AuditService.GetRecentLogsAsync(cantidadRegistros);
```

### **3. Filtros en Memoria**
Después de obtener los datos, aplica filtros adicionales:
- Por tipo de entidad
- Por acción
- Por usuario (búsqueda parcial)
- Por rango de fechas

### **4. Exportar CSV** (Marcador TODO)
Botón disponible pero pendiente de implementación:
```csharp
private async Task ExportarCsv()
{
    // TODO: Implementar exportación CSV
}
```

---

## 📱 Responsive Design

La página usa Tailwind CSS con diseño adaptable:

- **Desktop:** Grid de 4 columnas para filtros
- **Mobile:** Grid de 1 columna automática
- **Tabla:** Scroll horizontal en pantallas pequeñas
- **Modal:** Máximo ancho de 4xl (56rem)

---

## 🔐 Seguridad y Permisos

### **Autorización:**
```razor
<AuthorizeView Roles="Admin,Tesorero">
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
    <Authorized>
        <!-- Contenido -->
    </Authorized>
</AuthorizeView>
```

Solo usuarios con rol **Admin** o **Tesorero** pueden acceder.

---

## 📊 Casos de Uso

### **Caso 1: Verificar quién emitió un certificado**
1. Ir a `/admin/auditoria`
2. Filtrar: Tipo = "Certificados", Acción = "Emitido"
3. Buscar el certificado por fecha o usuario
4. Ver detalles para información completa

### **Caso 2: Auditar actividad de un usuario**
1. Filtro: Usuario = "tesoreria@fundacionlamamedellin.org"
2. Fecha Desde = hace 7 días
3. Ver todas las acciones realizadas

### **Caso 3: Investigar cambios en un recibo específico**
1. Obtener GUID del recibo
2. Filtro: ID de Entidad = GUID
3. Ver historial completo de cambios

### **Caso 4: Revisar anulaciones del mes**
1. Filtro: Acción = "Anulado"
2. Fecha Desde = primer día del mes
3. Exportar lista (cuando se implemente)

---

## 🚀 Mejoras Futuras

### **Alta Prioridad:**
1. ✅ Implementar exportación a CSV
2. ✅ Agregar paginación para grandes volúmenes
3. ✅ Capturar IP del usuario en logs

### **Media Prioridad:**
4. Agregar gráficos de actividad
5. Alertas de actividad inusual
6. Búsqueda por texto en JSON

### **Baja Prioridad:**
7. Comparación visual de cambios (diff)
8. Timeline de eventos
9. Filtros guardados

---

## 📝 Ejemplo de Uso en Código

### **Ver historial de un certificado:**
```csharp
// En CertificadoDetalle.razor
<a href="/admin/auditoria?entityType=CertificadoDonacion&entityId=@certificado.Id" 
   class="text-sm text-blue-600 hover:underline">
    Ver historial de auditoría
</a>
```

### **Ver actividad del día:**
```csharp
// En Dashboard
var today = DateTime.Today;
var logs = await AuditService.GetRecentLogsAsync(100);
var todayLogs = logs.Where(l => l.Timestamp.Date == today).ToList();
```

---

## 🎯 Integración con Menú

La página está integrada en el menú de navegación:

**Sección:** Administración  
**Icono:** Documento con check (púrpura)  
**Texto:** "Auditoría del Sistema"

---

## ✅ Estado Final

- ✅ Página creada y compilando
- ✅ Integrada en menú de navegación
- ✅ Filtros funcionando
- ✅ Modal de detalles completo
- ✅ Diseño responsive
- ✅ Autorización implementada
- ⏳ Exportación CSV pendiente

---

## 📸 Vista Previa de la UI

### **Tabla Principal:**
```
┌─────────────┬──────────────┬──────────┬─────────┬───────────────┬──────────┐
│ Fecha/Hora  │ Usuario      │ Entidad  │ Acción  │ Detalles      │ Acciones │
├─────────────┼──────────────┼──────────┼─────────┼───────────────┼──────────┤
│ 23/10/2025  │ tesoreria    │ [Certif] │ Emitido │ Certificado   │  [Ver]   │
│ 15:30:45    │ @fundacion.. │ 12345... │         │ CD-2025-00042 │          │
└─────────────┴──────────────┴──────────┴─────────┴───────────────┴──────────┘
```

### **Modal de Detalles:**
```
┌──────────────────────────────────────────────────┐
│ Detalles del Registro de Auditoría              │
├──────────────────────────────────────────────────┤
│ Fecha: 23/10/2025 15:30:45                      │
│ Usuario: tesoreria@fundacionlamamedellin.org    │
│ Entidad: Certificado                             │
│ ID: 12345678-90ab-cdef-1234-567890abcdef        │
│                                                  │
│ [Valores Nuevos - Fondo Verde]                  │
│ {                                                │
│   "Consecutivo": 42,                            │
│   "Ano": 2025,                                  │
│   "Estado": "Emitido"                           │
│ }                                                │
│                                                  │
│        [Cerrar]                                  │
└──────────────────────────────────────────────────┘
```

---

**Fecha de implementación:** 23 de octubre de 2025  
**Version:** 2.2.0  
**Estado:** ✅ FUNCIONAL Y LISTO PARA USAR
