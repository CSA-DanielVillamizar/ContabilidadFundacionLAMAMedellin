# 🔍 DIAGNÓSTICO SQL: Histórico ENE-NOV 2025 en PRODUCCIÓN

**Fecha**: 2026-01-22  
**Responsable**: Production Support Engineer  
**Objetivo**: Determinar si histórico ENE-NOV 2025 NO aparece por (A) falta de datos, (B) migraciones no aplicadas, (C) UI recorta resultados

---

## ✅ PASO 0 — Pre-flight Checks

### WebApp Status
```powershell
Invoke-WebRequest -Uri "https://app-tesorerialamamedellin-prod.azurewebsites.net/" -Method HEAD
```
**Resultado**: ✅ HTTP 200 OK

### Configuración SQL via Key Vault
```bash
az webapp config appsettings list --name app-tesorerialamamedellin-prod
```
**Resultado**: ✅ `ConnectionStrings__DefaultConnection` usa `@Microsoft.KeyVault(...)` correctamente

---

## 📊 PASO 1 — Evidencia SQL (BASELINE)

### 1.1 Autenticación AAD
```powershell
$token = (az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv)
```
**Resultado**: ✅ Token obtenido exitosamente

### 1.2 Tablas en la Base de Datos

**Total tablas**: 35

**Tablas transaccionales identificadas**:
- `Ingresos` (vacía)
- `Egresos` (9 registros, solo OCT 2025)
- `Recibos` (6 registros, OCT-NOV 2025)
- `ReciboItems` (items de recibos)
- `Pagos` (pagos registrados)
- `CierresMensuales` (cierres contables)
- `ConciliacionesBancarias` (conciliaciones)

**⚠️ HALLAZGO CRÍTICO**: NO existe tabla `MovimientosTesoreria`

### 1.3 Conteos y Rangos de Fechas

#### Tabla: Ingresos
```
Total:    0 registros
MinFecha: NULL
MaxFecha: NULL
```
**Status**: ❌ **VACÍA - NO HAY DATOS**

#### Tabla: Egresos
```
Total:    9 registros
MinFecha: 2025-10-31
MaxFecha: 2025-10-31
```

**Distribución por mes (2025)**:
| Mes     | Cantidad |
|---------|----------|
| 2025-10 | 9        |

**Status**: ⚠️ **SOLO OCTUBRE 2025 - FALTA ENE-SEP y NOV**

#### Tabla: Recibos
```
Total:    6 registros
MinFecha: 2025-10-01
MaxFecha: 2025-11-01
```

**Distribución por mes (2025)**:
| Mes     | Cantidad |
|---------|----------|
| 2025-10 | 5        |
| 2025-11 | 1        |

**Status**: ⚠️ **SOLO OCT-NOV 2025 - FALTA ENE-SEP**

### 1.4 Migraciones Entity Framework

**Última migración aplicada**: `20251226005657_AgregarDocumentosMiembro` (EF Core 8.0.0)

**Total migraciones**: 10 migraciones aplicadas correctamente

**Status**: ✅ **Todas las migraciones están aplicadas**

---

## 🎯 PASO 2 — CONCLUSIÓN: Causa Raíz Identificada

### Análisis de Resultados

| Criterio | Estado | Evidencia |
|----------|--------|-----------|
| **(A) Falta de datos/import** | ✅ **SÍ - CAUSA RAÍZ** | `Ingresos = 0 registros`, `Egresos = solo OCT 2025`, `Recibos = solo OCT-NOV 2025` |
| **(B) Migraciones no aplicadas** | ❌ No | 10 migraciones aplicadas correctamente |
| **(C) UI recorta resultados** | ⚠️ **TAMBIÉN APLICA** | maxResults=500 + filtros NULL (ya corregido en [deployment d62b7c4c](../docs/INCIDENT_NO_HISTORICO_PROD.md)) |

### 🔴 **CAUSA RAÍZ CONFIRMADA**: Falta Import Histórico

**Hallazgos**:
1. **NO existe tabla `MovimientosTesoreria`** (la app usa `Ingresos`, `Egresos`, `Recibos`)
2. **`Ingresos` está completamente vacía** (0 registros)
3. **`Egresos` solo tiene 9 registros de OCT 2025** (falta ENE-SEP y NOV)
4. **`Recibos` solo tiene 6 registros de OCT-NOV 2025** (falta ENE-SEP)
5. **Migraciones EF aplicadas correctamente** (no es problema de esquema)

**Implicación**:
- El problema de visibilidad del histórico ENE-NOV 2025 NO es solo por recorte de UI (aunque eso también fue un problema secundario que ya se corrigió).
- **La causa principal es que NO SE IMPORTARON los datos históricos de ENE-SEP 2025 en PRODUCCIÓN**.
- Solo hay datos de OCT-NOV 2025 (probablemente datos de prueba o registros manuales recientes).

---

## 📋 PASO 3 — Plan de Acción Recomendado

### Opción 1: Import Histórico desde Excel (RECOMENDADO)
Si existe un archivo Excel con histórico 2025-01 a 2025-11:

1. **Preparar Excel**:
   - Validar que contiene columnas compatibles con `Ingresos`, `Egresos`, `Recibos`
   - Calcular checksum SHA256 del archivo

2. **Implementar Import Idempotente**:
   - Agregar columna `UniqueKey` (SHA256 de campos clave)
   - Implementar deduplicación en lógica de import
   - Import por lotes (batch) con transacción por mes

3. **Ejecutar DRY-RUN**:
   - Validar datos sin escribir en DB
   - Reportar: total, válidos, duplicados, inválidos

4. **Import REAL**:
   - Ejecutar import con evidencia auditable
   - Registrar conteos por mes (ENE-NOV)

5. **Validación POST**:
   - Confirmar conteos en SQL
   - Verificar visibilidad en UI

### Opción 2: Generación Manual de Datos de Prueba
Si NO existe archivo histórico y solo se requieren datos de prueba:

1. Crear script SQL para insertar datos sintéticos en `Ingresos`, `Egresos`, `Recibos`
2. Generar 50-100 registros por mes (ENE-NOV 2025)
3. Distribuir valores y categorías de forma realista
4. Ejecutar vía `Invoke-Sqlcmd` con transacción

### Opción 3: Esperar Operación Normal
Si el sistema está iniciando operaciones y se acumularán datos reales:

- Confirmar con stakeholders que NO se requiere histórico 2025
- Documentar que PROD solo tiene datos desde OCT 2025
- Monitorear acumulación de datos en adelante

---

## 📈 Próximos Pasos

**Acción inmediata requerida**: Definir con el cliente:

1. ¿Existe archivo Excel con histórico ENE-NOV 2025?
   - **SÍ** → Proceder con Opción 1 (Import desde Excel)
   - **NO** → Elegir Opción 2 (datos sintéticos) u Opción 3 (sin histórico)

2. Si se elige Opción 1:
   - Ubicar archivo Excel
   - Seguir procedimiento PASO 0-6 del plan original
   - Generar documento de evidencia auditable

---

## 🔗 Referencias

- **Deployment UI fix**: [INCIDENT_NO_HISTORICO_PROD.md](./INCIDENT_NO_HISTORICO_PROD.md)
- **Deployment ID**: d62b7c4c148d4102b6e752eacf82a18d
- **Fecha fix UI**: 2026-01-22 07:06:53 UTC
- **Cambios UI**: Filtros por defecto (últimos 18 meses) + maxResults 5000

---

**Status**: ✅ **DIAGNÓSTICO COMPLETADO - CAUSA RAÍZ CONFIRMADA: FALTA IMPORT HISTÓRICO**
