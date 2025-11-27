# 🔐 Credenciales de Prueba y Checklist de Validación

## 📋 Credenciales de Usuarios Seed

### Usuario Tesorero
```
Email:    tesorero@fundacionlamamedellin.org
Password: T3s0r3r0!2025
Rol:      Tesorero
```

### Usuario Admin
```
Email:    admin@fundacionlamamedellin.org
Password: Adm1nLAMAMedellin*2025
Rol:      Admin
```

### Usuario Gerencia de Negocios
```
Email:    gerentenegocios@fundacionlamamedellin.org
Password: Gerenc1aNeg0c10s!2025
Rol:      gerentenegocios
```

---

## ✅ Checklist de Validación - Acceso Tesorero a GerenciaNegocios

### Preparación
- [x] Servidor corriendo en `http://localhost:5000`
- [x] Policy "GerenciaNegocios" incluye rol Tesorero en Program.cs
- [x] 10 páginas Razor actualizadas a usar Policy
- [x] Compilación exitosa sin errores

### 1. Login con Usuario Tesorero
- [ ] Navegar a `http://localhost:5000`
- [ ] Click en "Login" o "Iniciar Sesión"
- [ ] Ingresar:
  - Email: `tesorero@fundacionlamamedellin.org`
  - Password: `T3s0r3r0!2025`
- [ ] Click en "Iniciar Sesión"
- [ ] **Resultado esperado:** Login exitoso, redirección al dashboard

### 2. Validar Acceso a Módulo Clientes
- [ ] Navegar a `/gerencia-negocios/clientes` o click en menú "Clientes"
- [ ] **Resultado esperado:** 
  - ✅ Página carga correctamente (NO redirige a Access Denied)
  - ✅ Se muestra tabla de clientes
  - ✅ Botón "Nuevo Cliente" visible
- [ ] **Resultado NO esperado:**
  - ❌ Redirección a `/Identity/Account/AccessDenied`
  - ❌ Error 403 Forbidden
  - ❌ Página en blanco

### 3. Validar CRUD de Clientes
- [ ] Click en "Nuevo Cliente"
- [ ] **Resultado esperado:** Formulario de creación se carga
- [ ] Completar datos de prueba:
  - Nombre: `Cliente Prueba Tesorero`
  - NIT: `900123456-7`
  - Email: `prueba@test.com`
  - Teléfono: `3001234567`
- [ ] Guardar cliente
- [ ] **Resultado esperado:** Cliente se crea exitosamente
- [ ] Click en "Editar" sobre el cliente recién creado
- [ ] **Resultado esperado:** Formulario de edición se carga
- [ ] Modificar teléfono a `3009876543`
- [ ] Guardar cambios
- [ ] **Resultado esperado:** Cambios se guardan exitosamente
- [ ] Click en "Ver Detalle" del cliente
- [ ] **Resultado esperado:** Página de detalle se carga con datos actualizados

### 4. Validar Acceso a Módulo Proveedores
- [ ] Navegar a `/gerencia-negocios/proveedores` o click en menú "Proveedores"
- [ ] **Resultado esperado:** 
  - ✅ Página carga correctamente
  - ✅ Se muestra tabla de proveedores
  - ✅ Botón "Nuevo Proveedor" visible
- [ ] Click en "Ver Detalle" de un proveedor existente
- [ ] **Resultado esperado:** Página de detalle se carga correctamente

### 5. Validar Acceso a Módulo Cotizaciones
- [ ] Navegar a `/gerencia-negocios/cotizaciones` o click en menú "Cotizaciones"
- [ ] **Resultado esperado:**
  - ✅ Página carga correctamente
  - ✅ Se muestra tabla de cotizaciones
  - ✅ Botón "Nueva Cotización" visible
- [ ] Click en "Nueva Cotización"
- [ ] **Resultado esperado:** Formulario de creación se carga
- [ ] Click en "Editar" sobre una cotización existente (si hay)
- [ ] **Resultado esperado:** Formulario de edición se carga
- [ ] Click en "Ver Detalle" de una cotización
- [ ] **Resultado esperado:** Página de detalle se carga

### 6. Validar Restricciones (Control Negativo)
- [ ] Logout del usuario Tesorero
- [ ] Login con usuario sin rol Tesorero/Admin/Gerente (si existe)
- [ ] Intentar navegar a `/gerencia-negocios/clientes`
- [ ] **Resultado esperado:**
  - ❌ Redirección a `/Identity/Account/AccessDenied`
  - ❌ Mensaje "No tienes permisos para acceder a esta página"

---

## 🎯 Resultados Esperados Globales

### ✅ Acceso Permitido para Tesorero
- Clientes: Listar, Crear, Editar, Ver Detalle
- Proveedores: Listar, Ver Detalle
- Cotizaciones: Listar, Crear, Editar, Ver Detalle

### ✅ Permisos Heredados del Rol Tesorero
- Recibos: Consultar, Crear, Generar PDF
- Egresos: Consultar, Crear, Editar
- Deudores: Consultar, Generar Recibo
- Certificados de Donación: Consultar, Crear
- Productos: Consultar, Crear, Editar
- Compras: Consultar, Crear
- Ventas: Consultar, Crear
- Inventario: Consultar
- Presupuestos: Consultar, Editar
- Conciliación Bancaria: Consultar, Crear

---

## 📊 Importación de CSV de Miembros

### Archivo SQL Generado
- **Ubicación:** `ImportarMiembros.sql`
- **Total registros:** 28 miembros
- **Datos temporales:** 4 campos (2 cédulas, 2 emails)

### Pasos para Importar
1. [ ] Abrir SQL Server Management Studio (SSMS) o Azure Data Studio
2. [ ] Conectar a la base de datos `ContabilidadLAMA`
3. [ ] Abrir archivo `ImportarMiembros.sql`
4. [ ] Revisar tabla staging con query:
   ```sql
   SELECT MemberNumber, FullName, Cedula, Email 
   FROM #MiembrosTemp 
   ORDER BY MemberNumber;
   ```
5. [ ] Verificar qué MemberNumbers ya existen:
   ```sql
   SELECT MemberNumber, FullName FROM Miembros ORDER BY MemberNumber;
   ```
6. [ ] Descomentar sección MERGE o INSERT según necesidad
7. [ ] Ejecutar script completo
8. [ ] Verificar datos importados:
   ```sql
   SELECT COUNT(*) FROM Miembros; -- Debe ser >= 28
   ```
9. [ ] Validar datos temporales:
   ```sql
   SELECT MemberNumber, FullName, Cedula, Email
   FROM Miembros
   WHERE Cedula LIKE '1000000%' OR Email LIKE '%.temp@%';
   ```

### Datos Temporales a Actualizar
- **MemberNumber 71:** Yeferson Bairon Úsuga Agudelo - Cédula `1000000071` → Solicitar cédula real
- **MemberNumber 72:** Jhon David Sánchez - Cédula `1000000072` → Solicitar cédula real
- **MemberNumber 87:** Gustavo Adolfo Gómez Zuluaga - Email `gustavo.gomez.temp@fundacionlamamedellin.org` → Solicitar email real
- **MemberNumber 89:** Nelson Augusto Montoya Mataute - Email `nelson.montoya.temp@fundacionlamamedellin.org` → Solicitar email real

---

## 🚨 Troubleshooting

### Problema: Usuario Tesorero redirige a Access Denied
**Solución:**
1. Verificar que el servidor se haya reiniciado después de cambios en Program.cs
2. Revisar logs del servidor para errores de autorización
3. Confirmar que el usuario tiene el rol "Tesorero" asignado:
   ```sql
   SELECT u.Email, r.Name
   FROM AspNetUsers u
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   JOIN AspNetRoles r ON ur.RoleId = r.Id
   WHERE u.Email = 'tesorero@fundacionlamamedellin.org';
   ```

### Problema: Error 500 en páginas de GerenciaNegocios
**Solución:**
1. Revisar logs del servidor en terminal
2. Verificar que los servicios inyectados estén registrados en Program.cs
3. Confirmar conexión a base de datos

### Problema: Datos CSV no se importan
**Solución:**
1. Verificar que el schema de tabla Miembros coincida con el script
2. Revisar constraints FK en tabla Recibos
3. Ejecutar sección de validaciones del script para identificar conflictos

### Problema: Cédulas o emails duplicados
**Solución:**
1. Ejecutar query de detección de duplicados:
   ```sql
   SELECT Cedula, COUNT(*) AS Total
   FROM Miembros
   GROUP BY Cedula
   HAVING COUNT(*) > 1;
   ```
2. Resolver duplicados manualmente antes de MERGE

---

## 📝 Notas Importantes

### Seguridad
- ⚠️ Las contraseñas seed son **temporales** y deben cambiarse en producción
- ⚠️ Los datos con extensión `.temp@fundacionlamamedellin.org` son **temporales**
- ⚠️ Las cédulas `1000000XXX` son **temporales** y deben actualizarse

### 2FA (Autenticación de Dos Factores)
- Los roles **Admin** y **Tesorero** requieren 2FA obligatorio después de 7 días de asignación
- Para habilitar 2FA:
  1. Login con usuario Admin/Tesorero
  2. Navegar a "Mi Cuenta" → "Autenticación de Dos Factores"
  3. Escanear código QR con Google Authenticator o Authy
  4. Ingresar código de verificación

### Auditoría
- Todos los cambios de usuarios con rol Admin/Tesorero se registran en tabla `TwoFactorAudits`
- Los cambios de autorización se logean en consola del servidor

---

## 📞 Contacto

**Para soporte técnico:**
- Revisar documentación en `TAREAS_COMPLETADAS_2025-11-11.md`
- Revisar cambios en `RESUMEN_CAMBIOS_2025-11-11.md`
- Consultar datos faltantes en `FALTANTES_VALIDAR_URGENTE.md`

---

**Generado el:** 11 de noviembre de 2025  
**Servidor:** http://localhost:5000  
**Estado:** ✅ Corriendo y listo para validación
