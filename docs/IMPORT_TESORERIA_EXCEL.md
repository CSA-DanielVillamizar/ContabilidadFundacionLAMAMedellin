# Guía de Importación de Histórico de Tesorería desde Excel

**Sistema de Contabilidad LAMA Medellín**  
**Versión:** 1.2.0  
**Fecha:** 21 de enero de 2026

---

## 📋 Tabla de Contenido
1. [Descripción General](#descripción-general)
2. [Requisitos Previos](#requisitos-previos)
3. [Estructura del Excel](#estructura-del-excel)
4. [Funcionalidades del Importador](#funcionalidades-del-importador)
5. [Uso del Importador](#uso-del-importador)
6. [Validaciones y Reglas](#validaciones-y-reglas)
7. [Troubleshooting](#troubleshooting)
8. [Configuración de Producción](#configuración-de-producción)

---

## 🎯 Descripción General

El importador de tesorería permite cargar el histórico completo de movimientos financieros desde el archivo Excel `INFORME TESORERIA.xlsx`, que contiene registros desde **mayo 2024 hasta noviembre 2025**.

### **Características Clave**
- ✅ **Idempotente:** Puede ejecutarse múltiples veces sin duplicar datos
- ✅ **Validación de Saldos:** Compara saldo calculado vs. saldo en Excel por cada fila
- ✅ **Trazabilidad Completa:** Cada movimiento importado incluye hash único, fuente, hoja y número de fila
- ✅ **Dry Run:** Modo simulación para verificar antes de importar
- ✅ **Clasificación Inteligente:** Mapea automáticamente conceptos a categorías (Ingresos/Egresos)
- ✅ **Tolerancia a Errores:** Registra mismatches sin abortar la importación

---

## 📦 Requisitos Previos

### **Base de Datos**
1. Aplicar migración `Phase1_TreasuryCore` (crea tablas de tesorería)
2. Aplicar migración `Phase1Import` (agrega campos de trazabilidad)
```bash
cd src/Server
dotnet ef database update
```

### **Archivo Excel**
- **Ubicación:** Raíz del repositorio (`INFORME TESORERIA.xlsx`)
- **Formato:** Excel `.xlsx` (no `.xls`)
- **Hojas:** Nombres tipo `CORTE MAYO - 24`, `CORTE A MAYO 2024`, `CORTE NOVIEMBRE 30-25`, etc.

### **Permisos**
- Solo usuarios con rol **Admin** pueden ejecutar la importación
- La herramienta está en `/admin/import-tesoreria`

---

## 📊 Estructura del Excel

### **Formato de Hojas**
Cada hoja mensual debe contener:
- **Nombre de Hoja:** Formato reconocible (ejemplos: `CORTE MAYO - 24`, `CORTE A MAYO 2024`)
- **Encabezados de Tabla:**
  - `FECHA`
  - `CONCEPTO`
  - `INGRESOS`
  - `EGRESOS`
  - `SALDO` (opcional pero recomendado para validación)

### **Ejemplo de Tabla**
```
FECHA       | CONCEPTO                     | INGRESOS  | EGRESOS   | SALDO
------------|------------------------------|-----------|-----------|----------
2024-05-01  | Aporte mensual miembro 1001  | 20,000    |           | 20,000
2024-05-05  | Donación evento aniversario  | 50,000    |           | 70,000
2024-05-10  | Compra insumos café          |           | 15,000    | 55,000
```

### **Filas Resumen (NO se importan)**
El importador detecta y omite filas tipo:
- `SALDO EFECTIVO MES ANTERIOR`
- `TOTAL INGRESOS`
- `TOTAL EGRESOS`
- `INGRESOS dolares`
- `SALDO EN TESORERIA A LA FECHA`

---

## ⚙️ Funcionalidades del Importador

### **1. Detección de Hojas**
- Escanea el Excel buscando hojas con nombres tipo `CORTE ...`
- Extrae mes y año del nombre
- Ordena hojas cronológicamente antes de importar

### **2. Parseo de Datos**
- **Fechas:** Acepta múltiples formatos (`dd/MM/yyyy`, `yyyy-MM-dd`, `MM/dd/yyyy`)
- **Montos:** Elimina símbolos (`$`, `,`, `.`) y parsea como decimal
- **Concepto:** Normaliza espacios en blanco

### **3. Clasificación Automática**
El sistema mapea conceptos a:
- **Fuentes de Ingreso:**
  - Palabras clave: `APORTE`, `DONACIÓN`, `VENTA MERCH`, `CLUB CAFE`, `EVENTO`, etc.
  - Fallback: `OTROS`
- **Categorías de Egreso:**
  - Palabras clave: `AYUDA SOCIAL`, `PAPELERIA`, `TRANSPORTE`, `MANTENIMIENTO`, etc.
  - Fallback: `OTROS-GASTOS`

### **4. Idempotencia (Hash Único)**
Cada movimiento genera un hash SHA256 basado en:
```
SHA256(Fecha | Concepto | Tipo | Valor | Saldo | NombreHoja)
```
- Si el hash ya existe en BD, el movimiento se omite (no se duplica)
- Índice en campo `ImportHash` garantiza unicidad

### **5. Validación de Saldos**
Por cada fila:
1. Calcula saldo acumulado: `Saldo Anterior + Ingresos - Egresos`
2. Compara con columna `SALDO` del Excel
3. Si diferencia > ±1 COP:
   - Marca movimiento con `ImportHasBalanceMismatch = true`
   - Registra saldos esperado/encontrado
   - **NO aborta** la importación (solo advierte)

### **6. Trazabilidad**
Cada movimiento importado incluye:
- `ImportHash`: Hash único
- `ImportSource`: `"INFORME TESORERIA.xlsx"`
- `ImportSheet`: Nombre de la hoja
- `ImportRowNumber`: Número de fila en Excel
- `ImportedAtUtc`: Timestamp de importación
- `ImportBalanceExpected`: Saldo esperado (del Excel)
- `ImportBalanceFound`: Saldo calculado
- `ImportHasBalanceMismatch`: Flag de discrepancia

---

## 🚀 Uso del Importador

### **Acceso a la Herramienta**
1. Iniciar sesión como **Admin**
2. Navegar a: **ADMINISTRACIÓN** → **Importar Tesorería**
3. URL: `/admin/import-tesoreria`

### **Paso 1: Dry Run (Simulación)**
1. Hacer clic en el botón **🧪 Dry Run (Simular)**
2. El sistema:
   - Lee el Excel
   - Procesa todas las hojas
   - Valida saldos
   - **NO crea registros** en la BD
3. Revisar el resumen:
   - Filas procesadas
   - Movimientos que serían importados
   - Movimientos que serían omitidos (ya existen)
   - Mismatches de saldo
   - Advertencias y errores

### **Paso 2: Importación Real**
1. Si el Dry Run es satisfactorio, hacer clic en **✅ Importar (Real)**
2. Confirmar la operación
3. El sistema:
   - Lee el Excel
   - Crea movimientos en `MovimientosTesoreria`
   - Actualiza cuenta Bancolombia con histórico
   - Registra trazabilidad completa
4. Revisar el resumen final

### **Resultado Esperado**
- **Movimientos Importados:** ~500-1000 (dependiendo del histórico)
- **Movimientos Omitidos:** 0 en primera ejecución; aumenta si se re-ejecuta
- **Mismatches:** Idealmente 0; si hay algunos, revisar advertencias

---

## ✅ Validaciones y Reglas

### **Filas Válidas**
Para que una fila se importe como movimiento válido:
1. ✅ `FECHA` debe ser parseable
2. ✅ `CONCEPTO` no debe estar vacío
3. ✅ `INGRESOS > 0` **XOR** `EGRESOS > 0` (solo uno de los dos)
4. ✅ No debe ser fila resumen (ver palabras clave arriba)

### **Filas Omitidas**
Se omiten:
- Filas con `FECHA` vacía o inválida
- Filas con `CONCEPTO` vacío
- Filas donde `INGRESOS` y `EGRESOS` están ambos vacíos o ambos llenos
- Filas con palabras clave de resumen

### **Mismatches de Saldo**
- **Tolerancia:** ±1 COP (para redondeos)
- Si diferencia > 1 COP:
  - Se registra advertencia
  - Movimiento se crea igual (con flag `ImportHasBalanceMismatch`)
  - Permite auditoría posterior

---

## 🛠️ Troubleshooting

### **Problema: "Archivo no encontrado"**
**Causa:** El Excel no está en la ruta configurada  
**Solución:**
1. Verificar que `INFORME TESORERIA.xlsx` esté en la raíz del repo
2. Revisar configuración en `appsettings.json`:
   ```json
   "Import": {
     "TreasuryExcelPath": "INFORME TESORERIA.xlsx"
   }
   ```

### **Problema: "No se encontró encabezado"**
**Causa:** La hoja no tiene columnas `FECHA`, `CONCEPTO`, `INGRESOS`, `EGRESOS`  
**Solución:**
1. Verificar que las primeras 20 filas tengan el encabezado
2. Asegurar que los nombres de columnas coincidan exactamente

### **Problema: "Muchos Mismatches de Saldo"**
**Causa:** Diferencias entre saldo calculado y saldo en Excel  
**Solución:**
1. Revisar advertencias en el resumen de importación
2. Si todos los mismatches son < 10 COP, probablemente son redondeos (aceptable)
3. Si hay diferencias grandes (> $10,000), verificar:
   - Saldo inicial de la cuenta
   - Posibles filas omitidas por errores de parseo

### **Problema: "Importación Deshabilitada"**
**Causa:** Configuración `Import:Enabled = false`  
**Solución:**
1. Editar `appsettings.json`:
   ```json
   "Import": {
     "Enabled": true
   }
   ```
2. Reiniciar aplicación

### **Problema: "Movimientos Duplicados"**
**Situación:** No debería ocurrir por el hash único, pero si pasa:  
**Solución:**
1. Verificar que el hash se esté calculando correctamente
2. Ejecutar SQL para verificar duplicados:
   ```sql
   SELECT ImportHash, COUNT(*)
   FROM MovimientosTesoreria
   WHERE ImportHash IS NOT NULL
   GROUP BY ImportHash
   HAVING COUNT(*) > 1
   ```

---

## 🔧 Configuración de Producción

### **appsettings.json (Producción)**
```json
{
  "Import": {
    "TreasuryExcelPath": "INFORME TESORERIA.xlsx",
    "Enabled": true
  }
}
```

### **Pasos para Producción**
1. **Antes de Desplegar:**
   - Ejecutar Dry Run en entorno de staging
   - Verificar que no hay errores críticos
   - Revisar mismatches y advertencias

2. **En Producción:**
   - Subir el Excel al servidor (misma carpeta que la app)
   - Ejecutar Dry Run en producción
   - Si todo OK, ejecutar Importación Real

3. **Después de Importar:**
   - **Deshabilitar importación** para evitar re-ejecuciones accidentales:
     ```json
     "Import": { "Enabled": false }
     ```
   - Reiniciar app
   - Verificar en `/tesoreria/movimientos` que los datos estén correctos

4. **Backup:**
   - Hacer backup de BD antes y después de importar
   - Guardar copia del Excel original

---

## 📞 Comandos Útiles

### **Desarrollo (Dry Run)**
```bash
# Terminal o Postman
POST https://localhost:5000/api/admin/import/tesoreria/excel?dryRun=true
Authorization: Bearer <token-admin>
```

### **Desarrollo (Importación Real)**
```bash
POST https://localhost:5000/api/admin/import/tesoreria/excel?dryRun=false
Authorization: Bearer <token-admin>
```

### **Verificar Movimientos Importados**
```sql
SELECT COUNT(*) AS TotalImportados
FROM MovimientosTesoreria
WHERE ImportSource = 'INFORME TESORERIA.xlsx';

SELECT ImportSheet, COUNT(*) AS Movimientos
FROM MovimientosTesoreria
WHERE ImportSource = 'INFORME TESORERIA.xlsx'
GROUP BY ImportSheet
ORDER BY ImportSheet;
```

### **Verificar Mismatches**
```sql
SELECT * 
FROM MovimientosTesoreria
WHERE ImportHasBalanceMismatch = 1
ORDER BY Fecha;
```

---

## ✅ Checklist de Importación

- [ ] Migraciones aplicadas (`Phase1_TreasuryCore`, `Phase1Import`)
- [ ] Excel disponible en ruta configurada
- [ ] Usuario Admin autenticado
- [ ] Dry Run ejecutado y revisado
- [ ] Backup de BD creado
- [ ] Importación Real ejecutada
- [ ] Verificado en `/tesoreria/movimientos` que datos son correctos
- [ ] Importación deshabilitada (`Enabled: false`) para producción
- [ ] Documento de auditoría generado (guardar resumen de importación)

---

**Fin de la Guía de Importación**
