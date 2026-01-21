# 📋 Control de Cierre Contable Mensual
## Documento Funcional para Junta Directiva y Revisoría Fiscal

**Versión:** 1.0  
**Fecha:** Enero 2026  
**Aprobado por:** Sistema de Contabilidad LAMA Medellín  
**Clasificación:** Documento de Control Financiero Interno

---

## 1. ¿QUÉ ES EL CIERRE CONTABLE MENSUAL?

El **Cierre Contable Mensual** es un proceso de control que **congela un período específico** (un mes completo) para:

✅ **Garantizar integridad:** Impide cambios accidentales o malintencionados en transacciones pasadas  
✅ **Facilitar auditoría:** Genera un punto de referencia auditado y trazable  
✅ **Cumplir normativa:** Apoya la preparación de informes para DIAN y supervisores  
✅ **Controlar acceso:** Solo administradores pueden reabrir un mes cerrado

**Analogía:** Es como sellar una carpeta de documentos en una caja de archivo. Una vez sellada, no se puede sacar ni modificar documentos sin dejar huella de quién lo hizo y por qué.

---

## 2. ¿QUIÉN PUEDE CERRAR UN MES?

| Rol | Permiso | Acción |
|-----|---------|--------|
| **Tesorero** | ✅ Sí | Puede iniciar el cierre de un mes |
| **Junta Directiva** | ✅ Sí | Puede aprobar o rechazar un cierre |
| **Revisor Fiscal** | ✅ Ver | Puede ver cierres y su historial, pero NO cerrar |
| **Admin** | ✅ Sí | Puede cerrar, reabrir y anular cierres |
| **Otros usuarios** | ❌ No | No tienen acceso a cierres |

---

## 3. ¿POR QUÉ NO SE PUEDE MODIFICAR UN MES CERRADO?

Una vez que un mes está **CERRADO**, el sistema impide cualquier operación que altere transacciones de ese período:

### ❌ BLOQUEADO (No permitido):
- Crear nuevos recibos o egresos
- Modificar recibos o egresos existentes
- Anular (marcar como "Anulado") transacciones
- Importar histórico de Excel para ese mes
- Cambiar saldos o montos

### ✅ PERMITIDO (Sigue funcionando):
- Consultar/ver transacciones
- Generar reportes
- Auditar transacciones
- Ver historial de cambios

**Razón:** Garantiza que los números reportados a la DIAN y supervisores no cambien. Si fuera posible modificar un mes cerrado, cualquier auditor externo tendría dudas sobre la integridad de los datos.

---

## 4. EL PROCESO MENSUAL RECOMENDADO

### **Semana 1-3 del mes:**
1. Tesorero impone nuevos recibos y egresos
2. Sistema calcula automáticamente saldos (reconciliación automática)
3. Importar Excel de tesorería (si hay histórico)

### **Últimos días del mes (2-3 del mes siguiente):**
1. **Tesorero** hace un "Dry Run" (simulación) de importación en página `/admin/import-tesoreria`
   - Verifica que los números coincidan
   - Revisa advertencias y diferencias
   
2. **Tesorero** ejecuta importación real (si Dry Run fue exitoso)

3. **Revisor Fiscal** valida los números:
   - Recibe informe de saldos del mes
   - Verifica movimientos contra documentos originales
   - Anota discrepancias (si las hay)

4. **Junta Directiva** aprueba cierre:
   - Revisa informe de Tesorero y Revisor
   - Toma decisión de cerrar o rechazar

5. **Admin** ejecuta cierre en página `/tesoreria/cierre`:
   - Selecciona Año y Mes
   - Agrega observaciones (si hay)
   - Confirma cierre

6. **Sistema genera:**
   - Registro de cierre inmutable
   - Auditoría con usuario y hora
   - Bloqueo de ediciones para el mes

---

## 5. ¿QUÉ INFORMACIÓN SE GUARDA EN UN CIERRE?

Cuando se cierra un mes, el sistema **automáticamente calcula y almacena:**

```
Período: Mayo 2025
├─ Saldo Inicial: $450,000 COP (saldo final de abril)
├─ Total Ingresos: $1,200,000 COP (todos los recibos de mayo)
├─ Total Egresos: $800,000 COP (todos los egresos de mayo)
├─ Saldo Final: $850,000 COP (calculado: 450k + 1.2M - 800k)
├─ Fecha Cierre: 2025-06-02 10:35:42
├─ Usuario: junta@lama.org.co
├─ Observaciones: "Revisión OK, todos los documentos cuadran"
└─ Hash de Integridad: a3f2d8e... (para detectar cambios)
```

**Nada de esto se puede modificar una vez guardado.** Si hay un error, la única opción es:
1. Admin reabre el mes (dejando auditoría)
2. Se corrigen los datos
3. Se vuelve a cerrar

---

## 6. ¿QUÉ PASA SI IMPORTAMOS DESPUÉS DE CERRAR?

El sistema **BLOQUEA automáticamente**. Ejemplo:

```
❌ Error: "No se puede importar. Los siguientes meses ya están CERRADOS: 
           Mayo 2025, Junio 2025. Para re-importar, contacte al Admin."
```

**¿Por qué?** Porque si ya dijimos que mayo está listo y auditable, no podemos de repente agregar transacciones nuevas de mayo dos semanas después. Eso violaría la integridad del reporte.

---

## 7. AUDITORÍA Y TRAZABILIDAD

**Todo queda registrado.** Si alguien se pregunta "¿Quién cerró enero? ¿Cuándo? ¿Con qué saldos?", la respuesta está en la **base de datos de auditoría:**

### Ver Historial de Cierres:
1. Ir a **Administración** → **Auditoría**
2. Filtrar por Entidad: `CierreMensual`
3. Ver:
   - Usuario que cerró
   - Fecha exacta
   - Acción: `CIERRE_MENSUAL_EJECUTADO`
   - Saldos grabados

### Ver Todos los Cambios (si se reabrió):
1. Filtrar por: `CIERRE_MENSUAL_REABIERTO` o similar
2. Ver motivo de reapertura
3. Ver quién lo hizo y cuándo

---

## 8. CORRECCIONES DESPUÉS DE CERRAR

### ❌ NO USAR: Reabrir mes, cambiar datos, cerrar de nuevo
→ *Deja rastro de "cambio post-cierre"* (visible en auditoría)

### ✅ USAR: Lanzar movimiento de ajuste
→ Crear un nuevo recibo o egreso de ajuste en el mes siguiente
→ Ejemplo: "Ajuste retroactivo por diferencia en depósito de mayo"
→ Queda registrado como transacción del mes siguiente

**Ventaja:** La auditoría ve claramente qué se ajustó y por qué, sin modificar datos ya cerrados.

---

## 9. RESTRICCIONES TÉCNICAS (GARANTÍAS DEL SISTEMA)

El sistema implementa **validaciones obligatorias** en el código:

✅ **Validación en base de datos:** Si intenta forzar un INSERT/UPDATE de junio en una transacción cerrada, la BD lo rechaza.

✅ **Validación en aplicación:** Antes de guardar cualquier cambio, el sistema verifica si el mes está cerrado.

✅ **Validación en importación:** Si intenta importar Excel de un mes cerrado, se bloquea inmediatamente.

✅ **Auditoría automática:** Toda acción de cierre/reapertura genera registro inmutable.

---

## 10. PREGUNTAS FRECUENTES

### **P: ¿Puedo cerrar un mes a mitad del mes?**
**R:** Sí, pero NO es recomendado. Mejor hacerlo cuando estés seguro de que no habrá más cambios.

### **P: ¿Qué pasa si cierro un mes pero luego llega un recibo que debería estar en ese mes?**
**R:** Tienes dos opciones:
1. **Mejor:** Crear un ajuste en el mes siguiente (queda auditable)
2. **Temporal:** Admin reabre el mes, agregas el recibo, cierras de nuevo (queda en auditoría que fue reabierto)

### **P: ¿Puede el Tesorero reabrir un mes?**
**R:** No. Solo el Admin. Esto previene que cualquiera cierre un mes "por accidente" y luego lo abra nuevamente para cambiar datos.

### **P: ¿Qué diferencia hay entre un mes cerrado y auditable?**
**R:** 
- **Cerrado:** El sistema no permite ediciones (bloqueo técnico)
- **Auditable:** Está registrado en auditoría cómo y cuándo se cerró

Un mes puede estar auditable sin estar cerrado (auditoría registra todo lo que pasa).

### **P: ¿El Revisor Fiscal puede deshacer un cierre?**
**R:** No. Solo lectura. El Revisor puede objetal, pero el Admin es quien reabre.

---

## 11. PROCEDIMIENTO TÉCNICO (Para Admins)

### Cerrar un mes (Admin)
```
1. Ir a /tesoreria/cierre
2. Seleccionar Año y Mes
3. (Opcional) Agregar Observaciones
4. Clic en "Cerrar Mes"
5. Confirmar en modal de advertencia
6. Sistema registra automáticamente
```

### Reabrir un mes (Admin)
```
1. Ir a /tesoreria/cierre
2. Buscar el mes en historial
3. Clic en "Reabrir" (si está disponible)
4. Escribir MOTIVO de reapertura
5. Confirmar
6. Auditoría automáticamente registra: usuario, hora, motivo
```

### Verificar cierre en auditoría (Admin/Junta/Revisor)
```
1. Ir a /admin/auditoria
2. Filtrar Entidad: CierreMensual
3. Ver: usuario, fecha, saldos, estado
4. Clic en fila para ver detalles completos
```

---

## 12. GARANTÍAS PARA DIAN Y SUPERVISORES

Cuando presentamos reportes a DIAN o supervisores, podemos garantizar:

✅ **Integridad:** Los números están bloqueados tras cierre  
✅ **Trazabilidad:** Sabemos quién cierra, cuándo y con qué valores  
✅ **Auditoría completa:** Cualquier reapertura queda registrada  
✅ **No hay secretos:** Todo cambio post-cierre es explícito y documentado  

**Esto nos posiciona como una organización seria y transparente.**

---

## 13. AUTORIDADES Y CUMPLIMIENTO

Este control cierre contribuye a:

- **Ley 1314/2009** (Normas de Contabilidad): Integridad de registros
- **DIAN RTE 2000**: Documentación y auditoría interna
- **Régimen Tributario Especial**: Transparencia contable
- **Mejores prácticas:** Control interno según COSO

---

## 14. REAPERTURA DE PERÍODOS CERRADOS (EXCEPCIONES)

En casos excepcionales donde se detectan errores en un período ya cerrado, existe un procedimiento **controlado** de reapertura:

### ¿Cuándo se reabre un período?

✅ **Motivos válidos:**
- Error detectado en importación de datos históricos
- Recibos o egresos NO grabados por falla técnica
- Corrección de fecha/monto en documentos originales (con evidencia)
- Auditoría externa solicitando ajustes

❌ **Motivos inválidos:**
- "Cambié de opinión sobre un monto"
- Querer modificar transacciones sin justificación
- Omisión de transacciones que debieron estar en el mes

### Procedimiento de reapertura:

1. **Revisión:** Revisor Fiscal documenta el error con evidencia
2. **Solicitud:** Admin recibe solicitud con:
   - Período exacto (año/mes)
   - Motivo detallado del error
   - Documento que respalda (comprobante, email, etc.)
3. **Reapertura:** Admin ejecuta reapertura en sistema (acción auditada)
4. **Corrección:** Tesorero hace ajustes (cada ajuste es auditado)
5. **Cierre nuevo:** Se cierra nuevamente con nota de corrección

### Auditoría obligatoria:

Cada reapertura genera un registro que incluye:
- **Quién:** Usuario admin que reabrió
- **Cuándo:** Fecha y hora exacta
- **Por qué:** Motivo registrado en sistema
- **Impacto:** Qué transacciones fueron ajustadas

**Nota:** Las reaperturas son muy infrecuentes. Si ocurren regularmente, indica problemas de control.

---

## RESUMEN EJECUTIVO

| Aspecto | Detalle |
|--------|---------|
| **Propósito** | Bloquear y auditar períodos para garantizar integridad |
| **Quién puede** | Tesorero (iniciar), Junta/Admin (aprobar/ejecutar) |
| **Cuándo** | Fin de cada mes, después de validación |
| **Qué bloquea** | Ediciones de transacciones, nuevos movimientos, importaciones |
| **Qué permite** | Consulta, auditoría, generación de reportes |
| **Cómo se audita** | Registro automático de usuario, hora, saldos |
| **Cómo se corrige** | Reapertura controlada + movimientos de ajuste (excepcional) |
| **Impacto normativo** | Cumple DIAN, supervisores, estándares de control |

---

**Documento preparado por:** Sistema de Contabilidad LAMA Medellín  
**Para:** Junta Directiva, Revisoría Fiscal, Administración  
**Efectividad:** Enero 2026 en adelante

