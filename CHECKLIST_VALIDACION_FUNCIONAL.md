# ✅ Checklist de Validación Funcional - Sistema Contabilidad LAMA Medellín

## 🎯 Objetivo
Validar que todas las funcionalidades CRUD principales funcionan correctamente antes del despliegue a producción.

---

## 🌐 Acceso Inicial
- [ ] Servidor corriendo: **http://localhost:5000**
- [ ] Página de login carga correctamente
- [ ] Base de datos conectada (seed octubre 2025 cargado)

---

## 🔐 1. Autenticación y Autorización

### Login y Roles
- [ ] **Login exitoso** con usuario administrador
  - Usuario: `admin@fundacionlamamedellin.org`
  - Verificar: Avatar, nombre de usuario, menú lateral
- [ ] **Login exitoso** con usuario tesorero
  - Verificar: Menú limitado según policies
- [ ] **Login exitoso** con usuario contador
  - Verificar: Acceso a reportes contables
- [ ] **Logout funcional**
- [ ] **Redirección a login** cuando se intenta acceder sin autenticar

### Políticas (Policies)
- [ ] Usuario sin permisos **NO** puede acceder a módulos restringidos
- [ ] Botones de acción (Crear, Editar, Eliminar) se ocultan según policies
- [ ] Mensajes de error claros cuando se intenta acceder sin permisos

---

## 👥 2. Módulo: Clientes

### Listar Clientes
- [ ] Tabla carga correctamente con paginación
- [ ] **Búsqueda** por nombre/NIT funciona
- [ ] **Filtro por estado** (Activo/Inactivo) funciona
- [ ] **Ordenamiento** por columnas funciona

### Crear Cliente
- [ ] Formulario se abre con FormSection
- [ ] **Validación de campos obligatorios** (Nombre, TipoIdentificacion, NumeroIdentificacion)
- [ ] **Validación de NIT único** (no permite duplicados)
- [ ] **Toast de éxito** al guardar
- [ ] Cliente aparece en la tabla después de crear

### Editar Cliente
- [ ] Modal de edición se abre con datos precargados
- [ ] **Modificar nombre** y guardar funciona
- [ ] **Toast de éxito** al actualizar
- [ ] Cambios se reflejan en la tabla

### Eliminar Cliente
- [ ] **Confirmación** antes de eliminar
- [ ] **Toast de éxito** al eliminar
- [ ] Cliente desaparece de la tabla
- [ ] **Validación**: No se puede eliminar si tiene movimientos asociados

---

## 📦 3. Módulo: Productos

### Listar Productos
- [ ] Tabla carga con inventario actual
- [ ] **Búsqueda** por código/nombre funciona
- [ ] **Filtro por categoría** funciona
- [ ] **Alerta de stock bajo** visible (si aplica)

### Crear Producto
- [ ] Formulario con FormSection funcional
- [ ] **Validación**: Código único, Precio > 0, Stock >= 0
- [ ] **Categorías** se cargan en dropdown
- [ ] **Toast de éxito** al guardar

### Editar Producto
- [ ] Modal se abre con datos correctos
- [ ] **Actualizar precio** y stock funciona
- [ ] **Toast de éxito** al actualizar

### Eliminar Producto
- [ ] Confirmación funciona
- [ ] **Toast de éxito** al eliminar
- [ ] **Validación**: No se puede eliminar si tiene movimientos

---

## 💰 4. Módulo: Ventas

### Listar Ventas
- [ ] Tabla carga con ventas registradas
- [ ] **Filtro por fecha** funciona
- [ ] **Filtro por estado** (Pendiente/Pagado/Anulado) funciona
- [ ] **Ver detalle** de venta abre modal correcto

### Crear Venta
- [ ] **Selección de cliente** (autocomplete) funciona
- [ ] **Agregar productos** a la venta funciona
- [ ] **Cálculo automático** de subtotal e IVA funciona
- [ ] **Validación**: No se puede vender sin productos
- [ ] **Validación**: Stock insuficiente muestra error
- [ ] **Toast de éxito** al guardar
- [ ] **Stock se actualiza** después de la venta

### Registrar Pago de Venta
- [ ] Modal de pago se abre
- [ ] **Métodos de pago** (Efectivo, Transferencia, etc.) funcionan
- [ ] **Validación**: Monto > 0
- [ ] **Toast de éxito** al registrar pago
- [ ] **Estado de venta** cambia a "Pagado"

### Anular Venta
- [ ] Confirmación funciona
- [ ] **Stock se restaura** al anular
- [ ] **Toast de éxito** al anular

---

## 🛒 5. Módulo: Compras

### Listar Compras
- [ ] Tabla carga con compras registradas
- [ ] **Filtro por proveedor** funciona
- [ ] **Filtro por fecha** funciona
- [ ] **Ver detalle** abre modal correcto

### Crear Compra
- [ ] **Selección de proveedor** funciona
- [ ] **Agregar productos** a la compra funciona
- [ ] **Número de factura** es obligatorio
- [ ] **Cálculo automático** de totales funciona
- [ ] **Toast de éxito** al guardar

### Registrar Pago de Compra
- [ ] Modal de pago funcional
- [ ] **Métodos de pago** funcionan
- [ ] **Toast de éxito** al registrar pago
- [ ] **Estado** cambia correctamente

### Recepción de Compra
- [ ] Modal de recepción se abre
- [ ] **Stock se actualiza** al recibir productos
- [ ] **Toast de éxito** al recepcionar

---

## 🧾 6. Módulo: Recibos de Caja

### Listar Recibos
- [ ] Tabla carga con recibos emitidos
- [ ] **Filtro por fecha** funciona
- [ ] **Filtro por concepto** funciona
- [ ] **Ver PDF** del recibo funciona

### Crear Recibo
- [ ] **Selección de miembro/tercero** funciona
- [ ] **Selección de concepto** funciona
- [ ] **Monto** es obligatorio y > 0
- [ ] **Método de pago** es obligatorio
- [ ] **Toast de éxito** al guardar
- [ ] **PDF se genera correctamente** (QuestPDF)

### Anular Recibo
- [ ] Confirmación funciona
- [ ] **Toast de éxito** al anular
- [ ] **Estado** cambia a "Anulado"

---

## 🎁 7. Módulo: Certificados de Donación

### Listar Certificados
- [ ] Tabla carga con certificados emitidos
- [ ] **Filtro por año fiscal** funciona
- [ ] **Ver PDF** del certificado funciona

### Emitir Certificado
- [ ] **Selección de donante** (miembro o tercero libre) funciona
- [ ] **Total donado** se calcula automáticamente desde recibos
- [ ] **Validación**: Año fiscal válido
- [ ] **PDF se genera** con información RTE correcta
- [ ] **Toast de éxito** al emitir
- [ ] **Email se envía** si está configurado (opcional)

### Anular Certificado
- [ ] Confirmación funciona
- [ ] **Toast de éxito** al anular

---

## 💳 8. Módulo: Conceptos de Cobro

### Listar Conceptos
- [ ] Tabla carga con conceptos configurados
- [ ] **Búsqueda** por nombre funciona

### Crear Concepto
- [ ] FormSection funcional
- [ ] **Validación**: Nombre es obligatorio
- [ ] **Clasificación contable** es obligatoria
- [ ] **Toast de éxito** al guardar

### Editar Concepto
- [ ] Modal se abre con datos correctos
- [ ] **Actualizar valor por defecto** funciona
- [ ] **Toast de éxito** al actualizar

### Eliminar Concepto
- [ ] Confirmación funciona
- [ ] **Validación**: No se puede eliminar si tiene movimientos asociados
- [ ] **Toast de éxito** al eliminar

---

## 📊 9. Reportes y Consultas

### Reportes de Tesorería
- [ ] **Libro de Tesorería** se genera correctamente
- [ ] **Filtro por rango de fechas** funciona
- [ ] **Exportar a Excel** funciona (opcional)

### Reportes de Cartera
- [ ] **Deudores** se listan correctamente
- [ ] **Detalle de deudor** muestra movimientos
- [ ] **Antigüedad de cartera** se calcula bien

### Reportes Contables
- [ ] **Balance General** se genera (si aplica)
- [ ] **Estado de Resultados** se genera (si aplica)

---

## 🚨 10. Validación de UI y UX

### Toasts (ToastService)
- [ ] **Toasts de éxito** se muestran correctamente (verde, con ícono ✓)
- [ ] **Toasts de error** se muestran correctamente (rojo, con ícono ✗)
- [ ] **Toasts de advertencia** se muestran correctamente (amarillo)
- [ ] **Toasts se auto-ocultan** después de 3-5 segundos

### Modales (ModalService)
- [ ] **Modales se centran** correctamente
- [ ] **Cierre con X** funciona
- [ ] **Cierre con botón Cancelar** funciona
- [ ] **Fondo oscuro (backdrop)** funciona

### Responsive Design
- [ ] **Menú lateral** se colapsa en pantallas pequeñas
- [ ] **Tablas** tienen scroll horizontal en móvil
- [ ] **Formularios** se ajustan a pantalla pequeña

---

## 🔧 11. Validación de Servicios de Fondo

### Servicio de Backup
- [ ] **Backup automático** se ejecuta (verificar en logs)
- [ ] **Archivo de backup** se crea en `Backups/`
- [ ] **Retención** elimina backups antiguos > 30 días

### Servicio de Cálculo de Deudores
- [ ] **Cálculo automático** se ejecuta diariamente
- [ ] **Saldos** se actualizan correctamente en la tabla

---

## 📝 12. Validación de Logs y Diagnóstico

### Logs de Aplicación
- [ ] **Archivo de log** se crea en `Logs/`
- [ ] **Nivel de log** es apropiado (Information en dev, Warning en prod)
- [ ] **Excepciones** se registran con stack trace

### Manejo de Errores
- [ ] **Página de error** (`/Error`) se muestra correctamente
- [ ] **Errores 404** se manejan bien
- [ ] **Errores 500** muestran mensaje amigable (sin stack trace en producción)

---

## ✅ Resumen de Validación

| Módulo | Estado | Notas |
|--------|--------|-------|
| Autenticación | ⬜ | |
| Clientes | ⬜ | |
| Productos | ⬜ | |
| Ventas | ⬜ | |
| Compras | ⬜ | |
| Recibos | ⬜ | |
| Certificados | ⬜ | |
| Conceptos | ⬜ | |
| Reportes | ⬜ | |
| UI/UX | ⬜ | |
| Servicios de Fondo | ⬜ | |
| Logs y Errores | ⬜ | |

---

## 🎯 Próximos Pasos Después de Validación

1. **Resolver issues** encontrados durante validación
2. **Configurar ambiente de producción** (appsettings.Production.json, variables de entorno)
3. **Optimizar performance** (queries, paginación, caching)
4. **Configurar observabilidad** (logging estructurado, health checks)
5. **Crear artefactos de deployment** (publish profile, Dockerfile, CI/CD)

---

**Fecha de creación**: ${new Date().toLocaleDateString('es-CO')}  
**Responsable**: Daniel Villamizar  
**Versión**: 1.0
