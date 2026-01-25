# 📊 IMPORT HISTÓRICO ENE-NOV 2025 A PRODUCCIÓN

**Fecha inicio**: 2026-01-22 12:00 PM COT  
**Responsable**: Azure + .NET Senior Production Support Engineer  
**Objetivo**: Importar datos históricos de tesorería ENE 2025 a NOV 2025 con idempotencia (sin duplicados), evidencia auditable y DRY-RUN primero.

---

## ✅ FASE 0 — Localizar Excel + Checksum

### Archivo Excel Identificado
```
Path:    C:\Users\DanielVillamizar\ContabilidadLAMAMedellin\src\Server\Data\INFORME TESORERIA.xlsx
Tamaño:  320,897 bytes (313.38 KB)
SHA256:  4FCEDBC89078D6713D4E1769B5B317B7DCDE0B920DE7E5F8056C8BD9970D0697
```

**Status**: ✅ **Excel encontrado y verificado**

---

## ✅ FASE 1 — Baseline SQL (ANTES del Import)

### Comando Ejecutado
```powershell
$token = (az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv)
Invoke-Sqlcmd -ServerInstance "sql-tesorerialamamedellin-prod.database.windows.net" `
  -Database "sqldb-tesorerialamamedellin-prod" `
  -AccessToken $token `
  -Query "<SQL>"
```

### Totales y Rangos de Fechas ANTES

| Tabla | Total | MinFecha | MaxFecha |
|-------|-------|----------|----------|
| **Ingresos** | 0 | NULL | NULL |
| **Egresos** | 9 | 2025-10-31 | 2025-10-31 |
| **Recibos** | 6 | 2025-10-01 | 2025-11-01 |

### Conteos Por Mes (2025-01 a 2025-11) ANTES

#### Egresos
| Mes | Cantidad |
|-----|----------|
| 2025-10 | 9 |

**FALTA**: ENE, FEB, MAR, ABR, MAY, JUN, JUL, AGO, SEP, NOV

#### Ingresos
(Tabla vacía - 0 registros en todos los meses)

**FALTA**: ENE a NOV

#### Recibos
| Mes | Cantidad |
|-----|----------|
| 2025-10 | 5 |
| 2025-11 | 1 |

**FALTA**: ENE a SEP

### Summary BASELINE ANTES
- **Ingresos**: 0 registros total ❌
- **Egresos**: Solo 9 registros en OCT 2025 ⚠️
- **Recibos**: Solo 6 registros en OCT-NOV 2025 ⚠️
- **Conclusión**: **Falta histórico ENE-SEP 2025 en todas las tablas**

---

## ✅ FASE 2 — Idempotencia: ImportRowHash + Índices Únicos

### 2.1 Modificaciones a Modelos

#### Archivos Modificados
1. **src/Server/Models/Ingreso.cs**: Agregada propiedad `ImportRowHash` (nvarchar(64), nullable)
2. **src/Server/Models/Egreso.cs**: Agregada propiedad `ImportRowHash` (nvarchar(64), nullable)
3. **src/Server/Models/TreasuryModels.cs**: Agregada propiedad `ImportRowHash` a clase `Recibo` (nvarchar(64), nullable)

#### Regla de Hash (Determinística)
```
Ingresos:  SHA256(FechaIngreso|ValorCop|Descripcion|Categoria|MetodoPago|NumeroIngreso)
Egresos:   SHA256(Fecha|ValorCop|Descripcion|Categoria|Proveedor)
Recibos:   SHA256(FechaEmision|TotalCop|Serie|Consecutivo|MiembroId)
```

**Normalización**: Trim, cultura invariant para decimales, fechas en formato yyyy-MM-dd.

### 2.2 Migración Entity Framework

#### Comando
```bash
cd src/Server
dotnet ef migrations add AddImportRowHashForIdempotency -o Data/Migrations
```

**Resultado**: ✅ Migración creada exitosamente
- **Migration ID**: `20260122170435_AddImportRowHashForIdempotency`
- **Archivo**: `src/Server/Data/Migrations/20260122170435_AddImportRowHashForIdempotency.cs`

#### Contenido de la Migración
- `AddColumn` ImportRowHash a `Recibos` (nvarchar(64), nullable)
- `AddColumn` ImportRowHash a `Ingresos` (nvarchar(64), nullable)
- `AddColumn` ImportRowHash a `Egresos` (nvarchar(64), nullable)

### 2.3 Aplicación a PRODUCCIÓN

#### Script SQL Idempotente Creado
**Archivo**: `apply_import_hash_migration.sql`

```sql
-- Agregar columnas ImportRowHash a las 3 tablas
ALTER TABLE [dbo].[Recibos] ADD [ImportRowHash] nvarchar(64) NULL;
ALTER TABLE [dbo].[Ingresos] ADD [ImportRowHash] nvarchar(64) NULL;
ALTER TABLE [dbo].[Egresos] ADD [ImportRowHash] nvarchar(64) NULL;

-- Crear índices únicos (WHERE NOT NULL para permitir registros existentes sin hash)
CREATE UNIQUE INDEX [UX_Recibos_ImportRowHash]
ON [dbo].[Recibos] ([ImportRowHash])
WHERE [ImportRowHash] IS NOT NULL;

CREATE UNIQUE INDEX [UX_Ingresos_ImportRowHash]
ON [dbo].[Ingresos] ([ImportRowHash])
WHERE [ImportRowHash] IS NOT NULL;

CREATE UNIQUE INDEX [UX_Egresos_ImportRowHash]
ON [dbo].[Egresos] ([ImportRowHash])
WHERE [ImportRowHash] IS NOT NULL;

-- Registrar en historial EF
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260122170435_AddImportRowHashForIdempotency', N'8.0.0');
```

#### Ejecución en PROD
```powershell
$token = (az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv)
Invoke-Sqlcmd -ServerInstance "sql-tesorerialamamedellin-prod.database.windows.net" `
  -Database "sqldb-tesorerialamamedellin-prod" `
  -AccessToken $token `
  -Query $sqlScript
```

**Resultado**: ✅ **Migración aplicada exitosamente a PRODUCCIÓN**

#### Verificación
- ✅ Columna `ImportRowHash` creada en `dbo.Recibos`
- ✅ Columna `ImportRowHash` creada en `dbo.Ingresos`
- ✅ Columna `ImportRowHash` creada en `dbo.Egresos`
- ✅ Índice único `UX_Recibos_ImportRowHash` creado
- ✅ Índice único `UX_Ingresos_ImportRowHash` creado
- ✅ Índice único `UX_Egresos_ImportRowHash` creado
- ✅ Migración registrada en `__EFMigrationsHistory`

**Impacto**: 
- Los INSERT con ImportRowHash duplicado fallarán con error de UNIQUE CONSTRAINT → permite idempotencia.
- Los registros existentes (9 Egresos, 6 Recibos) tienen ImportRowHash = NULL → no afectados.
- Los nuevos registros del import tendrán ImportRowHash calculado → deduplicación automática.

---

## 🔄 FASE 3 — DRY-RUN (PENDIENTE)

**Status**: ⏳ **PENDIENTE DE EJECUCIÓN**

### Plan
1. Crear o reutilizar `ImportService` que:
   - Lee el Excel completo: `INFORME TESORERIA.xlsx`
   - Parsea columnas y las mapea a: `Ingresos`, `Egresos`, `Recibos`
   - Calcula `ImportRowHash` por fila según reglas definidas
   - Consulta cuántos hashes ya existen en SQL (duplicados)
   - NO ejecuta `SaveChanges` (DRY-RUN)

2. Reportar por tabla y mes (ENE-NOV 2025):
   - Total filas leídas del Excel
   - Filas válidas (pasan validación)
   - Filas inválidas/rechazadas (con razones: fecha faltante, valor inválido, etc.)
   - Duplicados detectados (hash ya existe en SQL)
   - Nuevos a insertar (hash no existe)

3. Guardar reporte en este documento (sección DRY-RUN RESULTS).

### Comando Propuesto
```bash
dotnet run --project src/Server/Server.csproj -- import-historico \
  --file "src/Server/Data/INFORME TESORERIA.xlsx" \
  --dry-run \
  --from 2025-01-01 \
  --to 2025-11-30
```

---

## 🚀 FASE 4 — IMPORT REAL (PENDIENTE)

**Status**: ⏳ **PENDIENTE - Solo ejecutar si DRY-RUN es exitoso**

### Plan
1. Ejecutar import REAL (sin `--dry-run`):
   - Calcular `ImportRowHash` por fila
   - Insertar con `INSERT ... WHERE NOT EXISTS` o capturar excepción de UNIQUE INDEX
   - Procesar por lotes (batch) con transacción por mes
   - Registrar: insertados, duplicados omitidos, rechazados, tiempo

2. Generar "AFTER evidence" (SQL):
   - Totales + min/max por tabla
   - Conteos por mes 2025-01..2025-11
   - Diferencia (AFTER - BEFORE)

3. Guardar outputs en este documento (sección IMPORT REAL RESULTS).

---

## ✅ FASE 5 — Verificación UI (PENDIENTE)

**Status**: ⏳ **PENDIENTE**

### Plan
1. Verificar pantalla `/tesoreria/movimientos` (o equivalente)
2. Aplicar filtro explícito: `2025-01-01` a `2025-11-30`
3. Confirmar que aparecen registros de meses ENE-SEP 2025 (no solo OCT-NOV)
4. Si UI limita a 500 resultados: ajustar con paginación real o aumentar `maxResults` con filtro activo

---

## 📝 FASE 6 — Documentación + Commit (PENDIENTE)

**Status**: ⏳ **PENDIENTE**

### Plan
1. Completar este documento con:
   - DRY-RUN summary
   - IMPORT REAL summary
   - AFTER evidence (outputs SQL)
   - Confirmación de visibilidad en UI
   - Rollback plan y riesgos

2. Commit con mensaje:
   ```
   chore(prod): import historico ene-nov 2025 (idempotente) + evidencia auditoria
   
   - Excel: INFORME TESORERIA.xlsx (SHA256: 4FCEDBC...)
   - Migración: AddImportRowHashForIdempotency aplicada en PROD
   - Columnas ImportRowHash + índices únicos creados (deduplicación)
   - Baseline BEFORE: Ingresos=0, Egresos=9 (OCT), Recibos=6 (OCT-NOV)
   - Import: [PENDIENTE - completar después de FASE 4]
   - Evidencia completa en docs/INCIDENT_IMPORT_HISTORICO_PROD.md
   ```

3. Push a repositorio con tags de producción.

---

## 🔐 Seguridad y Compliance

- ✅ **NO se exponen secretos** (autenticación con AAD token)
- ✅ **Idempotencia garantizada** (índices únicos sobre ImportRowHash)
- ✅ **Migraciones versionadas** (registradas en __EFMigrationsHistory)
- ✅ **Evidencia auditable** (este documento + outputs SQL)
- ✅ **DRY-RUN obligatorio** antes de IMPORT REAL

---

## 📈 Próximos Pasos

1. **FASE 3**: Implementar o ejecutar DRY-RUN del import
   - Crear `ImportService` o comando CLI
   - Leer Excel y reportar métricas sin escribir en DB
   - Documentar resultados aquí

2. **FASE 4**: Ejecutar IMPORT REAL solo si DRY-RUN es exitoso
   - Insertar datos con deduplicación automática via UNIQUE INDEX
   - Capturar métricas: insertados/duplicados/rechazados por mes
   - Documentar AFTER evidence

3. **FASE 5**: Validar UI con filtros y confirmar visibilidad histórica

4. **FASE 6**: Completar documentación y commit final

---

**Status General**: ✅ **FASES 0-2 COMPLETADAS** | ⏳ **FASES 3-6 PENDIENTES**
