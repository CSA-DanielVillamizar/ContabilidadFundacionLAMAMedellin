# 📊 Actualización de Deudores - Octubre 2025

## Resumen

Se ha creado una página administrativa para actualizar el estado de pagos de mensualidades de los miembros según los datos proporcionados para octubre 2025.

## 🎯 Acceso a la Página

**URL:** `/admin/actualizar-deudores-octubre`

**Ruta completa:** `http://localhost:5002/admin/actualizar-deudores-octubre`

**Requisitos:**
- Usuario con rol `Admin` o `Tesorero`
- Autenticación 2FA habilitada (según políticas actuales)

## 📋 Funcionalidad

La página realiza las siguientes acciones:

### 1. Actualiza Fecha de Ingreso - Nuevos Miembros (Octubre 2025)
Establece la fecha de ingreso al 1 de octubre de 2025 para:
- LAURA VIVIAN ASALAZAR MORENO
- JOSE JULIAN VILLAMIZAR ARAQUE
- GUSTAVO ADOLFO GÓMEZ ZULUAGA
- Nelson Augusto Montoya Mataute

**Efecto:** Estos miembros NO tendrán deuda en octubre 2025

### 2. Crea Recibos de Pago Retroactivos

#### Miembros al Día
- **RAMÓN ANTONIO GONZALEZ CASTAÑO**: 10 meses (ene-oct 2025) → 0 meses de deuda
- **CARLOS ALBERTO ARAQUE BETANCUR**: 12 meses (ene-dic 2025) → 0 meses (adelantado)

#### Deuda Moderada (1-5 meses)
- **MILTON DARIO GOMEZ RIVERA**: 6 meses pagados → Debe 4 meses (jul-oct)
- **DANIEL ANDREY VILLAMIZAR ARAQUE**: 6 meses pagados → Debe 4 meses (jul-oct)
- **ANGELA MARIA RODRIGUEZ**: 9 meses pagados → Debe 1 mes (octubre)
- **CESAR LEONEL RODRIGUEZ GALAN**: 9 meses pagados → Debe 1 mes (octubre)

#### Deuda Alta
- **GIRLESA MARÍA BUITRAGO**: 1 mes pagado → Debe 9 meses (feb-oct)

#### Sin Pagos Registrados (Deuda Total: 10 meses)
Los siguientes 16 miembros NO tendrán recibos creados, por lo que aparecerán con 10 meses de deuda (enero-octubre):
- HECTOR MARIO GONZALEZ HENAO
- JHON JARVEY GÓMEZ PATIÑO
- CARLOS MARIO CEBALLOS
- CARLOS ANDRES PEREZ AREIZA
- JUAN ESTEBAN SUAREZ CORREA
- JOSÉ EDINSON OSPINA CRUZ
- JEFFERSON MONTOYA MUÑOZ
- ROBINSON ALEHANDRO GALVIS PARRA
- JHON ENMANUEL ARZUZA PÁEZ
- JUAN ESTEBAN OSORIO
- YEFERSON BAIRÓN USUGA AGUDELO
- JHON DAVID SANCHEZ
- CARLOS JULIO RENDÓN DÍAZ
- JENNIFER ANDREA CARDONA BENITEZ
- WILLIAM HUMBERTO JIMENEZ PEREZ
- CARLOS MARIO DIAZ DIAZ

## 🚀 Instrucciones de Uso

### Paso 1: Crear Backup
Antes de ejecutar, asegúrate de tener un backup de la base de datos:

```powershell
# Opción 1: Usar la página de Backups
# Ve a: http://localhost:5002/admin/backups
# Click en "Crear Backup Manual"

# Opción 2: Ejecutar desde terminal
dotnet run --project .\src\Server\Server.csproj -- backup create
```

### Paso 2: Acceder a la Página
1. Abre tu navegador
2. Ve a: `http://localhost:5002/admin/actualizar-deudores-octubre`
3. Inicia sesión si no lo has hecho
4. Revisa el resumen de actualizaciones

### Paso 3: Ejecutar Actualización
1. Lee cuidadosamente el resumen de cambios
2. Click en el botón **"✅ Ejecutar Actualización"**
3. Espera a que el proceso termine (verás un indicador de "⏳ Procesando...")
4. Revisa el log de ejecución

### Paso 4: Verificar Resultados
1. Click en **"Ver Listado de Deudores"**
2. Verifica que los deudores aparezcan con las deudas correctas
3. Opcionalmente, revisa miembros individuales en sus páginas de detalle

## 🔍 Verificación Manual

### Revisar Deudores
```
URL: /tesoreria/deudores
```

### Revisar Recibos Creados
```sql
SELECT 
    r.Serie,
    r.Ano,
    r.Consecutivo,
    r.FechaEmision,
    m.NombreCompleto,
    r.TotalCop,
    r.Observaciones,
    r.CreatedBy
FROM Recibos r
INNER JOIN Miembros m ON r.MiembroId = m.Id
WHERE r.CreatedBy = 'admin_actualizacion_octubre_2025'
ORDER BY m.NombreCompleto;
```

### Revisar Fechas de Ingreso Actualizadas
```sql
SELECT 
    NombreCompleto,
    FechaIngreso,
    UpdatedAt,
    UpdatedBy
FROM Miembros
WHERE UpdatedBy = 'script_actualizacion_octubre_2025'
ORDER BY NombreCompleto;
```

## ⚠️ Consideraciones Importantes

1. **Idempotencia**: El script verifica si ya existen recibos antes de crear nuevos. Puedes ejecutarlo múltiples veces sin crear duplicados.

2. **Nombres Exactos**: El script busca miembros por nombre completo (ignorando mayúsculas/minúsculas). Si un nombre no coincide exactamente, aparecerá "NO ENCONTRADO" en el log.

3. **Conceptos**: El script usa el concepto `MENSUALIDAD` de la base de datos. Si este concepto no existe, el script fallará.

4. **Series de Recibos**: Los recibos creados usarán la serie predeterminada "LM" (LAMA Medellín).

5. **Auditoría**: Todos los cambios quedan registrados con:
   - `CreatedBy`: "admin_actualizacion_octubre_2025"
   - `UpdatedBy`: "script_actualizacion_octubre_2025"

## 🗑️ Rollback (Si es Necesario)

Si necesitas revertir los cambios:

```sql
-- 1. Eliminar recibos creados por el script
DELETE FROM ReciboItems 
WHERE ReciboId IN (
    SELECT Id FROM Recibos 
    WHERE CreatedBy = 'admin_actualizacion_octubre_2025'
);

DELETE FROM Recibos 
WHERE CreatedBy = 'admin_actualizacion_octubre_2025';

-- 2. Revertir fechas de ingreso (si es necesario)
-- PRECAUCIÓN: Solo si tienes un backup con las fechas originales
-- Restaura desde backup en lugar de ejecutar esto manualmente
```

## 📝 Logs y Auditoría

El script genera un log detallado que incluye:
- ✓ Operaciones exitosas
- ℹ️ Información (registros ya existentes)
- ⚠️ Advertencias (miembros no encontrados)
- ❌ Errores (si ocurren)

Ejemplo de log:
```
=== Iniciando actualización de deudores - Octubre 2025 ===

1️⃣ Actualizando fecha de ingreso nuevos miembros...
  ✓ LAURA VIVIAN ASALAZAR MORENO
  ✓ JOSE JULIAN VILLAMIZAR ARAQUE
  ⚠️ GUSTAVO ADOLFO GÓMEZ ZULUAGA: NO ENCONTRADO

2️⃣ Creando recibos de pago...
  ✓ RAMÓN ANTONIO GONZALEZ CASTAÑO: 10 meses
  ℹ️ CARLOS ALBERTO ARAQUE BETANCUR: Ya registrado

💾 Guardando cambios...

✅ Actualización completada exitosamente!
```

## 🔗 Páginas Relacionadas

- **Deudores**: `/tesoreria/deudores`
- **Detalle de Deudor**: `/tesoreria/deudor/{id}`
- **Recibos**: `/recibos`
- **Backups**: `/admin/backups`
- **Auditoría**: `/admin/auditoria`

## 📞 Soporte

Si encuentras algún problema:
1. Revisa el log de ejecución en la página
2. Consulta la página de Auditoría (`/admin/auditoria`) para ver eventos del sistema
3. Verifica que todos los miembros existan en la base de datos con nombres exactos
4. Asegúrate de que el concepto "MENSUALIDAD" esté configurado

---

**Última actualización:** 27 de octubre de 2025
**Versión:** 1.0
