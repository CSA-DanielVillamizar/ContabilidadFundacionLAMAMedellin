# Historias de Usuario - Contabilidad LAMA Medellín

## 1. Gestión de Miembros

### HU1. Como administrador, quiero registrar nuevos miembros para que puedan ser parte de la asociación.
- Criterios de aceptación:
  - Se puede ingresar información personal, fecha de ingreso y rango (miembro/asociado).
  - El sistema valida que no existan duplicados por cédula o correo.

### HU2. Como administrador, quiero actualizar los datos de un miembro para mantener la información al día.
- Criterios de aceptación:
  - Se pueden editar campos personales, de contacto y rango.
  - Se registra la fecha y usuario de la última actualización.

### HU3. Como administrador, quiero eliminar o desactivar miembros para mantener la base de datos limpia.
- Criterios de aceptación:
  - El sistema permite desactivar miembros sin borrar su historial.

## 2. Gestión de Recibos y Pagos

### HU4. Como tesorero, quiero generar recibos de mensualidad para los miembros activos.
- Criterios de aceptación:
  - El sistema genera recibos automáticamente para todos los miembros activos, excepto asociados.
  - Se puede consultar el historial de recibos por miembro y por mes.

### HU5. Como tesorero, quiero registrar pagos de mensualidad para llevar el control de las finanzas.
- Criterios de aceptación:
  - Se puede marcar un recibo como pagado y registrar el método de pago.
  - El sistema actualiza el estado del recibo y refleja el pago en los reportes.

## 3. Gestión de Deudores

### HU6. Como tesorero, quiero ver la lista de deudores de mensualidad para gestionar cobros.
- Criterios de aceptación:
  - El sistema muestra solo miembros con mensualidades pendientes, excluyendo asociados.
  - Se puede filtrar por mes y exportar la lista a Excel.

## 4. Gestión de Egresos

### HU7. Como tesorero, quiero registrar egresos con soporte para mantener la trazabilidad de gastos.
- Criterios de aceptación:
  - Se pueden adjuntar archivos de soporte (PDF, imagen).
  - El sistema valida que el mes no esté cerrado antes de registrar el egreso.

### HU8. Como tesorero, quiero actualizar o eliminar egresos para corregir errores o depurar información.
- Criterios de aceptación:
  - Se puede reemplazar el archivo de soporte y modificar los datos del egreso.
  - Al eliminar, se borra el archivo físico y el registro de la base de datos.

## 5. Cierre Contable

### HU9. Como tesorero, quiero cerrar meses contables para evitar modificaciones posteriores.
- Criterios de aceptación:
  - El sistema impide registrar egresos o ingresos en meses cerrados.
  - Se registra el usuario y la fecha del cierre.

## 6. Reportes y Exportaciones

### HU10. Como usuario autorizado, quiero generar reportes de ingresos, egresos y deudores para análisis financiero.
- Criterios de aceptación:
  - Los reportes pueden exportarse a Excel o PDF.
  - Se pueden filtrar por rango de fechas y conceptos.

## 7. Seguridad y Auditoría

### HU11. Como administrador, quiero que todas las acciones relevantes queden auditadas para cumplir con normativas y trazabilidad.
- Criterios de aceptación:
  - Se registra quién, cuándo y qué acción realizó sobre miembros, recibos, egresos y cierres.
  - Se puede consultar el historial de auditoría por entidad.

## 8. Gestión de Usuarios y Roles

### HU12. Como administrador, quiero gestionar usuarios y roles para controlar el acceso a las funcionalidades.
- Criterios de aceptación:
  - Se pueden asignar roles de administrador, tesorero y consulta.
  - El acceso a páginas y acciones está restringido según el rol.

---

Estas historias cubren la funcionalidad principal de la aplicación de contabilidad para LAMA Medellín, siguiendo buenas prácticas de Clean Architecture y asegurando trazabilidad, seguridad y facilidad de uso.