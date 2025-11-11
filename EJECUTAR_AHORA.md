# 🚀 GUÍA RÁPIDA: Corregir Codificación UTF-8

## ⚠️ IMPORTANTE: Los comandos del terminal de VS Code están siendo cancelados
Por favor, **abre una nueva ventana de PowerShell** y ejecuta estos comandos manualmente.

---

## 📋 COMANDOS A EJECUTAR (Copia y pega uno por uno)

### ✅ PASO 1: Corregir Collation en SQL Server

```powershell
cd C:\Users\DanielVillamizar\ContabilidadLAMAMedellin

sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql"
```

**Resultado esperado:**
```
✅ Collation actualizada correctamente para soportar caracteres especiales en español
```

**¿Qué hace este paso?**
- Limpia la tabla `Miembros` (borra datos con codificación incorrecta)
- Cambia columnas de `VARCHAR` a `NVARCHAR` (Unicode)
- Aplica collation `Modern_Spanish_CI_AS` para español

---

### ✅ PASO 2: Crear Migración de Entity Framework

```powershell
cd src\Server

dotnet ef migrations add UpdateMiembroModelWithUTF8Support
```

**Resultado esperado:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

**¿Qué hace este paso?**
- Crea una migración que actualiza el esquema de la tabla `Miembros`
- Agrega columnas: `NombreCompleto`, `Cedula`, `Celular`, `NumeroSocio`
- Configura collation en las columnas de texto

---

### ✅ PASO 3: Aplicar Migración a la Base de Datos

```powershell
dotnet ef database update
```

**Resultado esperado:**
```
Build started...
Build succeeded.
Applying migration '20251019xxxxxx_UpdateMiembroModelWithUTF8Support'.
Done.
```

**¿Qué hace este paso?**
- Ejecuta la migración en la base de datos
- Actualiza el esquema de la tabla `Miembros`
- Prepara la BD para recibir datos con caracteres especiales correctos

---

### ✅ PASO 4: Compilar la Aplicación

```powershell
cd ..\..

dotnet build src\Server\Server.csproj
```

**Resultado esperado:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**¿Qué hace este paso?**
- Compila el código con los cambios realizados
- Verifica que no haya errores

---

### ✅ PASO 5: Ejecutar la Aplicación

```powershell
dotnet run --project src\Server\Server.csproj
```

**Resultado esperado en los logs:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
✅ Se cargaron 26 miembros desde el CSV
✅ Logo copiado a: c:\...\wwwroot\images\LogoLAMAMedellin.png
```

**¿Qué hace este paso?**
- Inicia la aplicación
- Ejecuta automáticamente `MembersSeed.SeedAsync()` que importa los 26 miembros desde el CSV
- Copia el logo automáticamente
- Los miembros se importan CON la codificación UTF-8 correcta

---

### ✅ PASO 6: Verificar que los Caracteres Especiales sean Correctos

**Abre OTRA ventana de PowerShell** (deja la aplicación corriendo) y ejecuta:

```powershell
cd C:\Users\DanielVillamizar\ContabilidadLAMAMedellin

sqlcmd -S localhost -d LamaMedellin -E -i "src\Server\Scripts\VerificarMiembros.sql"
```

**Resultado esperado:**
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

**✅ SIN caracteres como:** `Ã±`, `Ã¡`, `Ã³`, `Ã©`, `Ãº`

---

## 🔍 VERIFICACIÓN VISUAL EN EL NAVEGADOR

Una vez que la aplicación esté corriendo:

1. Abre el navegador en: **http://localhost:5000**
2. Haz clic en **"Miembros"** en el menú
3. Verifica que los nombres se vean así:
   - ✅ **Héctor** Mario González Henao
   - ✅ **Ramón** Antonio González Castaño
   - ✅ **César** Leonel Rodríguez Galán
   - ✅ **José** Edinson Ospina Cruz

**NO así:**
   - ❌ H**Ã©**ctor
   - ❌ Ram**Ã³**n
   - ❌ C**Ã©**sar
   - ❌ Jos**Ã©**

---

## ⚠️ SI ALGO FALLA

### Error en PASO 1 (sqlcmd no reconocido):

```powershell
# Intenta con la ruta completa:
"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe" -S localhost -d LamaMedellin -E -i "src\Server\Scripts\FixCollation.sql"
```

### Error en PASO 2 (dotnet ef no reconocido):

```powershell
# Instala la herramienta dotnet ef globalmente:
dotnet tool install --global dotnet-ef

# Luego vuelve a intentar el PASO 2
```

### Error en PASO 3 (migración falla):

```powershell
# Verifica que el PASO 1 se ejecutó correctamente
# Verifica que la aplicación NO esté corriendo (cierra si está abierta)
# Vuelve a intentar
```

### Los nombres TODAVÍA se ven mal (Ã±, Ã¡, etc.):

Esto significa que el PASO 1 NO se ejecutó correctamente. Verifica:

```powershell
# Ejecuta esto para ver las columnas de la tabla:
sqlcmd -S localhost -d LamaMedellin -E -Q "SELECT COLUMN_NAME, DATA_TYPE, COLLATION_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Miembros' AND COLUMN_NAME IN ('NombreCompleto', 'Nombres', 'Apellidos')"
```

**Debes ver:**
```
COLUMN_NAME     DATA_TYPE    COLLATION_NAME
NombreCompleto  nvarchar     Modern_Spanish_CI_AS
Nombres         nvarchar     Modern_Spanish_CI_AS
Apellidos       nvarchar     Modern_Spanish_CI_AS
```

Si ves `varchar` o `SQL_Latin1_General_CP1_CI_AS`, vuelve a ejecutar el PASO 1.

---

## 📊 RESUMEN DE LO QUE CAMBIA

| Antes | Después |
|-------|---------|
| VARCHAR | NVARCHAR ✅ |
| SQL_Latin1_General_CP1_CI_AS | Modern_Spanish_CI_AS ✅ |
| José → JosÃ© ❌ | José → José ✅ |
| González → GonzÃ¡lez ❌ | González → González ✅ |
| Patiño → PatiÃ±o ❌ | Patiño → Patiño ✅ |

---

## ✅ LISTA DE VERIFICACIÓN FINAL

Marca cada paso a medida que lo completes:

- [ ] PASO 1: Ejecuté `FixCollation.sql` ✅
- [ ] PASO 2: Creé migración `UpdateMiembroModelWithUTF8Support` ✅
- [ ] PASO 3: Apliqué migración con `database update` ✅
- [ ] PASO 4: Compilé sin errores ✅
- [ ] PASO 5: Ejecuté la aplicación y vi "✅ Se cargaron 26 miembros" ✅
- [ ] PASO 6: Verifiqué con script SQL y NO vi `Ã±`, `Ã¡`, `Ã³` ✅
- [ ] Abrí http://localhost:5000/miembros y vi nombres correctos ✅
- [ ] Vi el logo en la barra lateral ✅

---

## 🎉 ¡LISTO!

Cuando completes todos los pasos, tu aplicación tendrá:
- ✅ 26 miembros importados con **tildes, ñ y acentos correctos**
- ✅ Logo de L.A.M.A. Medellín visible en el menú
- ✅ Todas las rutas de navegación funcionando
- ✅ Base de datos con collation correcta para español

---

**💡 TIP:** Guarda este archivo para referencia futura. Si necesitas reimportar los datos en el futuro, solo ejecuta los PASOS 1, 3 y 5.
