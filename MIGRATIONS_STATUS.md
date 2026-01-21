# Estado de Migraciones EF Core - Diagnóstico y Resolución

**Fecha**: 21 de Enero 2026  
**Status**: 🔴 CRÍTICO - Migración desalineada detectada

---

## 1. DIAGNÓSTICO INICIAL

### A. Configuración Git (.gitignore)

**Línea 35-36**:
```gitignore
# Entity Framework
*.db
*.db-shm
*.db-wal
Migrations/
```

❌ **PROBLEMA CRÍTICO**: `Migrations/` está ignorado en .gitignore

**Impacto**:
- Las migraciones NO se versionan en Git
- Cada desarrollador puede generar migraciones diferentes
- Imposible reproducir estado de BD en producción
- Viola principio de "Infrastructure as Code"

### B. Migraciones en Sistema de Archivos

**Comando**:
```powershell
Get-ChildItem 'src/Server/Migrations' -Filter '*.cs'
```

**Resultado**: ✅ 23 archivos de migración encontrados (46 archivos incluyendo Designer)

**Migraciones Presentes**:
```
20251017210847_InitialCreate.cs
20251019144916_UpdateMiembroModelWithUTF8Support.cs
20251022063256_AddCierreMensual.cs
20251023151037_AddCertificadosDonacion.cs
20251024004901_AddAuditLogs.cs
20251024053700_AddTwoFactorRequiredSince.cs
20251107030919_AgregarModuloGerenciaNegocios.cs
20251107092353_AgregarNuevosModulosCompletos.cs
20251108064736_AddDescuentoSubtotalToCotizaciones.cs
20251112212910_PerformanceIndexes.cs
20260121225943_AddAnulacionFieldsToMovimientoTesoreria.cs ⚠️ PENDING
AppDbContextModelSnapshot.cs
```

### C. Migraciones en Base de Datos

**Comando**:
```sql
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId
```

**Resultado**:
```
20251017210847_InitialCreate
20251019144916_UpdateMiembroModelWithUTF8Support
20251022063256_AddCierreMensual
20251023151037_AddCertificadosDonacion
20251024004901_AddAuditLogs
20251024053700_AddTwoFactorRequiredSince
20251107030919_AgregarModuloGerenciaNegocios
20251107092353_AgregarNuevosModulosCompletos
20251108064736_AddDescuentoSubtotalToCotizaciones
20251112212910_PerformanceIndexes
20251210233613_AddStockConstraintAndAuditToPresupuestos ⚠️
20251216051157_AddConciliacionFlagsToIngresosEgresos ⚠️
20251216053917_AddPresupuestoAnualYItemsPresupuesto ⚠️
20251217220228_FilterCertificadoConsecutivoIndex ⚠️
20251226005657_AgregarDocumentosMiembro ⚠️
```

❌ **PROBLEMA CRÍTICO**: La BD tiene 5 migraciones (diciembre 2025) que NO existen en el código

### D. Estado de EF Core Migrations

**Comando**:
```bash
dotnet ef migrations list --project src/Server/Server.csproj
```

**Resultado**:
```
20251017210847_InitialCreate
...
20251112212910_PerformanceIndexes
20260121225943_AddAnulacionFieldsToMovimientoTesoreria (Pending)
```

⚠️ EF Core NO conoce las 5 migraciones de diciembre 2025

### E. Intento de Actualizar Base de Datos

**Comando**:
```bash
dotnet ef database update --project src/Server/Server.csproj
```

**Error**:
```
Microsoft.Data.SqlClient.SqlException (0x80131904): 
Column names in each table must be unique. 
Column name 'CreatedAt' in table 'TasasCambio' is specified more than once.

Failed executing DbCommand:
ALTER TABLE [TasasCambio] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
```

❌ **PROBLEMA**: La migración 20260121225943 intenta:
- Agregar columna `CreatedAt` a `TasasCambio` (ya existe)
- Crear tablas `CategoriasEgreso`, `CuentasFinancieras`, `FuentesIngreso` desde cero
- Crear tabla `MovimientosTesoreria` completa

Pero estas tablas **NO existen en la BD real** porque se crearon en migraciones de diciembre que NO están en el código.

---

## 2. ANÁLISIS DE CAUSA RAÍZ

### Escenario Reconstruido:

1. **Octubre-Noviembre 2025**: Desarrollo inicial con migraciones hasta 20251112212910
2. **Diciembre 2025**: Alguien generó 5 migraciones adicionales:
   - AddStockConstraintAndAuditToPresupuestos
   - AddConciliacionFlagsToIngresosEgresos
   - AddPresupuestoAnualYItemsPresupuesto
   - FilterCertificadoConsecutivoIndex
   - AgregarDocumentosMiembro
3. **Problema**: Estas 5 migraciones **NO se subieron a Git** debido a `Migrations/` en .gitignore
4. **21 Enero 2026**: Al generar nueva migración (AddAnulacionFieldsToMovimientoTesoreria):
   - EF Core comparó el modelo actual con el snapshot de noviembre 2025
   - Detectó diferencias masivas (tablas MovimientosTesoreria, CuentasFinancieras, etc.)
   - Generó una migración "mega" que intenta crear todo desde cero

### Tablas Faltantes en Código pero Presentes en BD:

**Comando**:
```sql
SELECT name FROM sys.tables WHERE name NOT IN (
  'AspNetRoleClaims', 'AspNetRoles', 'AspNetUserClaims', 'AspNetUserLogins',
  'AspNetUserRoles', 'AspNetUsers', 'AspNetUserTokens', '__EFMigrationsHistory',
  'AuditLogs', 'CertificadosDonacion', 'CierresMensuales', 'Conceptos', 'Miembros',
  'Recibos', 'ReciboItems', 'Pagos', 'TasasCambio'
) ORDER BY name
```

**Resultado**: 19 tablas creadas por migraciones de diciembre que NO están en código:
- Clientes, Proveedores
- ComprasProductos, DetallesComprasProductos
- VentasProductos, DetallesVentasProductos
- Cotizaciones, DetallesCotizaciones
- Productos, MovimientosInventario, HistorialesPrecios
- ConciliacionesBancarias, ItemsConciliacion
- Presupuestos, PresupuestosAnuales, ItemsPresupuesto
- Egresos, Ingresos (campos adicionales)
- DocumentosMiembro
- Notificaciones

❌ **PERO FALTAN**: MovimientosTesoreria, CuentasFinancieras, CategoriasEgreso, FuentesIngreso

**Conclusión**: La migración 20260121225943 está tratando de crear estas 4 tablas core, pero EF Core está confundido porque el snapshot no refleja el estado real de la BD.

---

## 3. ESTRATEGIA DE RESOLUCIÓN (PRODUCTION-SAFE)

### Opción A: Bridge Migration (RECOMENDADA PARA PROD)

**Pasos**:
1. **NO BORRAR** ninguna migración existente en BD
2. Eliminar migración 20260121225943 (mal generada)
3. Sincronizar snapshot con BD real:
   - Opción 3a: Obtener las 5 migraciones de diciembre del Git history o de otro dev
   - Opción 3b: Regenerar snapshot desde BD existente
4. Generar nueva migración limpia que SOLO agregue:
   - `UsuarioAnulacion` (nvarchar(256)) a MovimientosTesoreria
   - `FechaAnulacion` (datetime2) a MovimientosTesoreria
5. Aplicar migración

**Ventajas**:
- ✅ No pierde historia de migraciones en BD producción
- ✅ Auditable y reversible
- ✅ Reproducible en todos los entornos

**Desventajas**:
- ⚠️ Requiere obtener migraciones de diciembre (buscar en Git history o colaboradores)

### Opción B: Snapshot Reset (SOLO LOCAL DEV)

**Pasos** (⚠️ SOLO EN ENTORNO LOCAL, NUNCA EN PROD):
1. Backup de BD actual: `BACKUP DATABASE LamaMedellin TO DISK='C:\backup\LamaMedellin_20260121.bak'`
2. Drop y recrear BD desde cero
3. Eliminar todas las migraciones excepto InitialCreate
4. Regenerar migraciones desde modelo actual
5. Aplicar todas las migraciones

**Ventajas**:
- ✅ Snapshot 100% limpio y alineado

**Desventajas**:
- ❌ NO VÁLIDO PARA PRODUCCIÓN (perdería datos)
- ❌ Borra historia de migraciones
- ❌ No reproducible en prod

### Opción C: Manual Bridge Migration (PRAGMÁTICA)

**Pasos**:
1. Eliminar migración 20260121225943 mal generada
2. Crear manualmente archivo de migración "bridge":
   ```csharp
   public class SyncDatabaseState : Migration
   {
       protected override void Up(MigrationBuilder migrationBuilder)
       {
           // Registrar que las 5 migraciones de diciembre ya se aplicaron
           // (insertar en __EFMigrationsHistory sin ejecutar comandos)
       }
   }
   ```
3. Actualizar AppDbContextModelSnapshot.cs manualmente para reflejar BD real
4. Generar migración limpia para UsuarioAnulacion/FechaAnulacion

**Ventajas**:
- ✅ No toca BD existente
- ✅ Sincroniza código con realidad

**Desventajas**:
- ⚠️ Requiere conocimiento técnico profundo de EF Core
- ⚠️ Error-prone si snapshot no coincide exactamente

---

## 4. DECISIÓN Y PLAN DE ACCIÓN

### Decisión: **Opción A - Bridge Migration** (Production-Safe)

**Justificación**:
- Sistema está en producción (Junta Directiva / Revisoría Fiscal)
- No podemos perder historia de migraciones
- Necesitamos reproducibilidad en todos los entornos

### Plan de Implementación:

#### FASE 1: Arreglar .gitignore
```bash
# Editar .gitignore: Remover línea "Migrations/"
git add .gitignore
git commit -m "fix: track EF Core migrations in version control"
```

#### FASE 2: Recuperar Migraciones de Diciembre

**Opción 2a**: Buscar en Git history
```bash
git log --all --full-history --diff-filter=D -- "**/Migrations/*.cs"
git show <commit-hash>:src/Server/Migrations/<file>.cs > restored_file.cs
```

**Opción 2b**: Si no están en Git, consultar:
- Otro desarrollador con copia local
- Backup de servidor
- Generar desde BD actual usando herramientas reverse (EF Power Tools)

**Opción 2c** (SI NO HAY OPCIÓN 2a/2b): Recrear manualmente desde BD
```bash
# Instalar EF Power Tools (VS extension)
# Reverse engineer desde BD → Generar migraciones "sintéticas"
```

#### FASE 3: Eliminar Migración Incorrecta
```bash
cd src/Server
dotnet ef migrations remove --force
# Esto eliminará 20260121225943_AddAnulacionFieldsToMovimientoTesoreria
```

#### FASE 4: Generar Migración Limpia
```bash
# Asegurar que AppDbContextModelSnapshot refleja BD actual
dotnet ef migrations add AddAnulacionFieldsToMovimientoTesoreria_Clean
```

**Validar que SOLO contenga**:
```csharp
migrationBuilder.AddColumn<string>(
    name: "UsuarioAnulacion",
    table: "MovimientosTesoreria",
    type: "nvarchar(256)",
    maxLength: 256,
    nullable: true);

migrationBuilder.AddColumn<DateTime>(
    name: "FechaAnulacion",
    table: "MovimientosTesoreria",
    type: "datetime2",
    nullable: true);
```

#### FASE 5: Aplicar Migración
```bash
dotnet ef database update
```

#### FASE 6: Validar
```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'MovimientosTesoreria'
  AND COLUMN_NAME IN ('UsuarioAnulacion', 'FechaAnulacion')
```

**Resultado Esperado**:
```
UsuarioAnulacion | nvarchar | 256 | YES
FechaAnulacion   | datetime2| NULL| YES
```

#### FASE 7: Commit Final
```bash
git add src/Server/Migrations/
git commit -m "fix: clean AddAnulacionFieldsToMovimientoTesoreria migration

- Removed malformed migration that tried to recreate existing tables
- Generated clean migration that only adds audit fields to MovimientosTesoreria
- Synced AppDbContextModelSnapshot with production database state
- All 5 December 2025 migrations now tracked in Git"
```

---

## 5. PRÓXIMOS PASOS INMEDIATOS

### Acción Requerida AHORA:

1. ✅ **Confirmar estrategia** con equipo/usuario
2. 🔧 **Ejecutar FASE 1**: Arreglar .gitignore
3. 🔍 **Ejecutar FASE 2**: Recuperar migraciones de diciembre
   - Buscar en Git history
   - Si no existen, consultar a otro dev o backup
4. 🗑️ **Ejecutar FASE 3**: Eliminar migración incorrecta
5. ✨ **Ejecutar FASE 4**: Generar migración limpia
6. ✅ **Ejecutar FASE 5**: Aplicar y validar

### Preguntas Críticas para Usuario:

**Q1**: ¿Tienes acceso a las migraciones de diciembre 2025?
- Git history de otro branch?
- Backup de servidor?
- Otro desarrollador?

**Q2**: Si NO tienes migraciones de diciembre, ¿prefieres:
- Opción A: Generar migraciones "sintéticas" que registren el estado actual (pragmático)
- Opción B: Reset completo de entorno local DEV (pierde historia local, pero OK si PROD tiene las migraciones correctas)

**Q3**: ¿Este es entorno de desarrollo local o producción?
- Si DEV local: Opción B es viable
- Si PROD o shared: Solo Opción A

---

## 6. VERIFICACIÓN FINAL (CHECKLIST)

Después de aplicar la solución:

- [ ] .gitignore NO ignora Migrations/
- [ ] `git status` muestra src/Server/Migrations/ tracked
- [ ] `dotnet ef migrations list` muestra todas las migraciones
- [ ] `dotnet ef database update` ejecuta sin errores
- [ ] Tabla MovimientosTesoreria tiene columnas UsuarioAnulacion y FechaAnulacion
- [ ] `SELECT * FROM __EFMigrationsHistory` incluye todas las migraciones
- [ ] AppDbContextModelSnapshot.cs refleja el modelo actual
- [ ] `dotnet build` y `dotnet test` pasan sin errores

---

**Status**: ⏳ ESPERANDO DECISIÓN DEL USUARIO  
**Next Action**: Confirmar estrategia y proceder con FASE 1-7
