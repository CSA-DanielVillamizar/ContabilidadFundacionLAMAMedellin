# 🔧 Guía para Corregir Problema de Codificación UTF-8

## ❌ Problema Identificado

Los datos en la base de datos muestran caracteres incorrectos como:
- `SuÃ¡rez` en lugar de `Suárez`
- `GÃ³mez` en lugar de `Gómez`  
- `PatiÃ±o` en lugar de `Patiño`
- `PÃ©rez` en lugar de `Pérez`
- `JosÃ©` en lugar de `José`

**Causa:** Problema de **doble codificación**. SQL Server está interpretando UTF-8 como Latin1.

## ✅ Solución Implementada

### Cambios Realizados:

1. **Modelo `Miembro.cs` actualizado** con propiedades para el CSV:
   - `NombreCompleto`
   - `Cedula`
   - `Celular`
   - `NumeroSocio`
   - Collation `Modern_Spanish_CI_AS` en campos de texto

2. **`AppDbContext.cs` configurado** con:
   - `.UseCollation("Modern_Spanish_CI_AS")` en columnas de texto
   - Índices en `Cedula` y `Documento`
   - MaxLength apropiado para cada campo

3. **`MembersSeed.cs` mejorado**:
   - Lectura explícita con `UTF8` encoding
   - Parser CSV robusto para manejar comas dentro de comillas
   - Mapeo correcto a las nuevas propiedades del modelo

4. **Script SQL `FixCollation.sql`** creado para:
   - Limpiar datos incorrectos existentes (`TRUNCATE TABLE Miembros`)
   - Cambiar tipo de columnas a `NVARCHAR` con collation `Modern_Spanish_CI_AS`

---

## 📝 Pasos para Aplicar la Corrección

### Paso 1: Ejecutar Script de Collation

```powershell
# En el directorio raíz del proyecto
sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql"
```

**Resultado esperado:**
```
✅ Collation actualizada correctamente para soportar caracteres especiales en español
```

Este comando:
- Limpia la tabla `Miembros` (elimina datos con codificación incorrecta)
- Cambia las columnas de `VARCHAR` a `NVARCHAR` 
- Aplica collation `Modern_Spanish_CI_AS` (compatible con español)

---

### Paso 2: Crear y Aplicar Migración

```powershell
# Ir al directorio del proyecto Server
cd src\Server

# Crear nueva migración
dotnet ef migrations add UpdateMiembroModelWithUTF8Support

# Aplicar migración a la base de datos
dotnet ef database update
```

**Resultado esperado:**
```
Build succeeded.
Done. To undo this action, use 'ef migrations remove'

Build succeeded.
Applying migration '20251019_UpdateMiembroModelWithUTF8Support'.
Done.
```

---

### Paso 3: Ejecutar la Aplicación

```powershell
# Desde el directorio raíz del proyecto
cd ..\..

# Limpiar compilación previa
dotnet clean src\Server\Server.csproj

# Compilar
dotnet build src\Server\Server.csproj

# Ejecutar
dotnet run --project src\Server\Server.csproj
```

**En los logs verás:**
```
✅ Se cargaron 26 miembros desde el CSV
✅ Logo copiado a: wwwroot\images\LogoLAMAMedellin.png
```

---

### Paso 4: Verificar que los Caracteres Especiales sean Correctos

```powershell
# Ejecutar script de verificación
sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\VerificarMiembros.sql"
```

**Deberías ver:**
```
NombreCompleto
------------------------------------------------
Héctor Mario González Henao
Ramón Antonio González Castaño
César Leonel Rodríguez Galán
José Edinson Ospina Cruz
Carlos Andrés Pérez Areiza
Ángela Maria Rodríguez Ochoa
Milton Darío Gómez Rivera
Juan Esteban Suárez Correa
Jhon Harvey Gómez Patiño
```

**✅ Sin `Ã±`, `Ã¡`, `Ã³`, etc.**

---

## 🔍 Verificación Rápida en SQL

```sql
USE LamaMedellin;

-- Debe retornar 26
SELECT COUNT(*) FROM Miembros;

-- Debe mostrar nombres CON tildes correctas
SELECT TOP 5 NombreCompleto, Apellidos, Cargo 
FROM Miembros 
ORDER BY NumeroSocio;
```

**Resultado esperado:**
```
NombreCompleto                          Apellidos              Cargo
--------------------------------------- -------------------- ----------
Héctor Mario González Henao             González Henao        SOCIO
Ramón Antonio González Castaño          González Castaño      SOCIO
César Leonel Rodríguez Galán            Rodríguez Galán       SOCIO
```

---

## 🎯 ¿Por Qué Funciona Ahora?

### Antes:
1. CSV en UTF-8 → C# lee con encoding por defecto (puede ser Latin1)
2. C# envía a SQL como VARCHAR → SQL interpreta como Latin1
3. **Resultado:** `José` se guarda como `JosÃ©`

### Ahora:
1. CSV en UTF-8 → C# lee **explícitamente** con `UTF8`
2. C# envía a SQL como **NVARCHAR** con collation `Modern_Spanish_CI_AS`
3. SQL guarda nativamente en Unicode
4. **Resultado:** `José` se guarda correctamente como `José` ✅

---

## 📚 Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Models/Miembro.cs` | Agregadas propiedades: `NombreCompleto`, `Cedula`, `Celular`, `NumeroSocio` |
| `Data/AppDbContext.cs` | Configurado con `.UseCollation("Modern_Spanish_CI_AS")` |
| `Data/Seed/MembersSeed.cs` | Lectura con `UTF8` encoding, parser CSV robusto |
| `Scripts/FixCollation.sql` | Script para cambiar columnas a NVARCHAR con collation correcta |
| `Scripts/VerificarMiembros.sql` | Script de verificación de caracteres especiales |

---

## ⚠️ Importante

- **TRUNCATE TABLE Miembros:** El script `FixCollation.sql` borra los datos existentes porque tienen codificación incorrecta
- **Reimportación automática:** Al ejecutar `dotnet run`, los 26 miembros se cargarán correctamente desde el CSV
- **Una sola vez:** La importación solo ocurre si la tabla está vacía

---

## 🚀 Resumen de Comandos

```powershell
# 1. Corregir collation
sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql"

# 2. Aplicar migración
cd src\Server
dotnet ef migrations add UpdateMiembroModelWithUTF8Support
dotnet ef database update

# 3. Ejecutar aplicación
cd ..\..
dotnet run --project src\Server\Server.csproj

# 4. Verificar
sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\VerificarMiembros.sql"
```

---

## ✅ Lista de Verificación

- [ ] Ejecuté `FixCollation.sql` (TRUNCATE + ALTER COLUMN)
- [ ] Creé migración `dotnet ef migrations add UpdateMiembroModelWithUTF8Support`
- [ ] Apliqué migración `dotnet ef database update`
- [ ] Ejecuté la aplicación `dotnet run --project src\Server\Server.csproj`
- [ ] Vi el mensaje "✅ Se cargaron 26 miembros desde el CSV"
- [ ] Verifiqué con `VerificarMiembros.sql` que NO hay `Ã±`, `Ã¡`, `Ã³`
- [ ] Verifiqué en el navegador http://localhost:5000/miembros

---

¡Todo listo! Los caracteres especiales ahora se verán correctamente. 🎉
