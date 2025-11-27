# Migración de Autorización basada en Roles a Políticas (Policies)

## Estado Actual
El sistema utiliza `[Authorize(Roles="...")]` directamente en controladores y páginas, mezclando lógica de seguridad con implementación.

## Objetivo
Centralizar lógica de autorización en políticas declarativas en `Program.cs` y migrar todos los `[Authorize(Roles=...)]` a `[Authorize(Policy="...")]`.

---

## Políticas Existentes (Program.cs)

| Política | Roles | Descripción |
|----------|-------|-------------|
| `TesoreroJunta` | Tesorero, Junta | Acceso para Tesorería y Junta Directiva |
| `GerenciaNegocios` | Admin, Gerente, gerentenegocios, Tesorero | Gerencia de Negocios (incluye Tesorero para consulta) |
| `Require2FA` | — | Usuario autenticado con 2FA habilitado |
| `AdminOrTesoreroWith2FA` | Admin, Tesorero + 2FA | Admin/Tesorero con 2FA obligatorio |

---

## Políticas Nuevas a Crear

Basadas en las combinaciones de roles encontradas:

```csharp
// Tesorero, Junta, Consulta (Lectura ampliada para reportes)
options.AddPolicy("TesoreroJuntaConsulta", policy =>
    policy.RequireRole("Tesorero", "Junta", "Consulta"));

// Admin, Tesorero (Operaciones administrativas de tesorería)
options.AddPolicy("AdminTesorero", policy =>
    policy.RequireRole("Admin", "Tesorero"));

// Admin, Gerente, Tesorero (Presupuestos y Conciliación Bancaria)
options.AddPolicy("AdminGerenteTesorero", policy =>
    policy.RequireRole("Admin", "Gerente", "Tesorero"));

// Admin, Gerente (Operaciones de Gerencia sin Tesorero)
options.AddPolicy("AdminGerente", policy =>
    policy.RequireRole("Admin", "Gerente"));
```

---

## Plan de Migración por Módulo

### 🏛️ Tesorería (Recibos, Egresos, Deudores)

**Archivos afectados:**
- `Controllers/RecibosController.cs` (6 ocurrencias)
- `Pages/Tesoreria/Deudores.razor`
- `Pages/Tesoreria/Egresos.razor`
- `Pages/Tesoreria/DeudorDetalle.razor`

**Mapeo:**
- `[Authorize(Roles = "Tesorero,Junta,Consulta")]` → `[Authorize(Policy = "TesoreroJuntaConsulta")]`
- `[Authorize(Roles = "Tesorero,Junta")]` → `[Authorize(Policy = "TesoreroJunta")]`

---

### 📜 Certificados de Donación

**Archivos afectados:**
- `Controllers/CertificadosDonacionController.cs` (2 ocurrencias)
- `Pages/Tesoreria/CertificadosDonacion.razor`
- `Pages/Tesoreria/CertificadosDonacionForm.razor`

**Mapeo:**
- `[Authorize(Roles = "Tesorero,Junta,Consulta")]` → `[Authorize(Policy = "TesoreroJuntaConsulta")]`
- `[Authorize(Roles = "Tesorero,Junta")]` → `[Authorize(Policy = "TesoreroJunta")]`

---

### 👥 Miembros

**Archivos afectados:**
- `Pages/Miembros/Importar.razor`

**Mapeo:**
- `[Authorize(Roles = "Tesorero,Junta")]` → `[Authorize(Policy = "TesoreroJunta")]`

---

### 📊 Reportes

**Archivos afectados:**
- `Controllers/ReportsController.cs` (3 ocurrencias)

**Mapeo:**
- `[Authorize(Roles = "Tesorero,Junta,Consulta")]` → `[Authorize(Policy = "TesoreroJuntaConsulta")]`

---

### 💰 Presupuestos

**Archivos afectados:**
- `Controllers/PresupuestosController.cs` (2 ocurrencias)
- `Pages/Tesoreria/Presupuestos.razor`
- `Pages/Tesoreria/PresupuestoForm.razor`
- `Pages/Tesoreria/PresupuestoDetalle.razor`

**Mapeo:**
- `[Authorize(Roles = "Admin,Gerente,Tesorero")]` → `[Authorize(Policy = "AdminGerenteTesorero")]`
- `[Authorize(Roles = "Admin,Gerente")]` → `[Authorize(Policy = "AdminGerente")]`

---

### 🏦 Conciliación Bancaria

**Archivos afectados:**
- `Controllers/ConciliacionBancariaController.cs` (5 ocurrencias)
- `Pages/Tesoreria/ConciliacionesBancarias.razor`
- `Pages/Tesoreria/ConciliacionForm.razor`
- `Pages/Tesoreria/ConciliacionDetalle.razor`

**Mapeo:**
- `[Authorize(Roles = "Admin,Gerente,Tesorero")]` → `[Authorize(Policy = "AdminGerenteTesorero")]`
- `[Authorize(Roles = "Admin,Gerente")]` → `[Authorize(Policy = "AdminGerente")]`

---

### ⚙️ Administración

**Archivos afectados:**
- `Pages/Admin/ActualizarDeudoresOctubre.razor`
- `Pages/Admin/CorreccionFechasIngresoOct2025.razor`

**Mapeo:**
- `[Authorize(Roles = "Admin,Tesorero")]` → `[Authorize(Policy = "AdminTesorero")]`

---

### 📦 Gerencia de Negocios (Comentados)

**Archivos afectados (solo documentación, no cambios):**
- `Controllers/MiembrosController.cs` (comentado)
- `Controllers/ProductosController.cs` (comentado)
- `Controllers/CotizacionesController.cs` (comentado)

**Nota:** Estos controladores ya están comentados temporalmente. Una vez que se decida habilitarlos, usar la política `GerenciaNegocios` existente.

---

## Ejecución de Migración

### Paso 1: Agregar políticas nuevas a `Program.cs`

Antes del bloque de autorización existente, agregar:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Políticas existentes...
    options.AddPolicy("TesoreroJunta", policy => policy.RequireRole("Tesorero", "Junta"));
    options.AddPolicy("GerenciaNegocios", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Gerente", "gerentenegocios", "Tesorero");
    });
    options.AddPolicy("Require2FA", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new TwoFactorEnabledRequirement());
    });
    options.AddPolicy("AdminOrTesoreroWith2FA", policy =>
    {
        policy.RequireRole("Admin", "Tesorero");
        policy.Requirements.Add(new TwoFactorEnabledRequirement());
    });
    
    // 🆕 NUEVAS POLÍTICAS
    options.AddPolicy("TesoreroJuntaConsulta", policy =>
        policy.RequireRole("Tesorero", "Junta", "Consulta"));
    
    options.AddPolicy("AdminTesorero", policy =>
        policy.RequireRole("Admin", "Tesorero"));
    
    options.AddPolicy("AdminGerenteTesorero", policy =>
        policy.RequireRole("Admin", "Gerente", "Tesorero"));
    
    options.AddPolicy("AdminGerente", policy =>
        policy.RequireRole("Admin", "Gerente"));
});
```

### Paso 2: Reemplazar atributos en archivos

Utilizar búsqueda/reemplazo global (regex) en VS Code:

| Buscar (regex) | Reemplazar |
|----------------|------------|
| `\[Authorize\(Roles\s*=\s*"Tesorero,Junta,Consulta"\)\]` | `[Authorize(Policy = "TesoreroJuntaConsulta")]` |
| `\[Authorize\(Roles\s*=\s*"Tesorero,Junta"\)\]` | `[Authorize(Policy = "TesoreroJunta")]` |
| `\[Authorize\(Roles\s*=\s*"Admin,Tesorero"\)\]` | `[Authorize(Policy = "AdminTesorero")]` |
| `\[Authorize\(Roles\s*=\s*"Admin,Gerente,Tesorero"\)\]` | `[Authorize(Policy = "AdminGerenteTesorero")]` |
| `\[Authorize\(Roles\s*=\s*"Admin,Gerente"\)\]` | `[Authorize(Policy = "AdminGerente")]` |

### Paso 3: Verificar compilación

```powershell
dotnet build
```

### Paso 4: Pruebas de acceso por rol

Crear usuarios de prueba con cada rol y verificar acceso a:
- Recibos (TesoreroJuntaConsulta)
- Presupuestos (AdminGerenteTesorero)
- Conciliación Bancaria (AdminGerenteTesorero)
- Admin (AdminTesorero)

---

## Beneficios

✅ **Centralización:** Lógica de seguridad en un solo lugar  
✅ **Mantenibilidad:** Cambios de permisos sin tocar código de negocio  
✅ **Escalabilidad:** Fácil agregar nuevas políticas (ej. basadas en claims, recursos, etc.)  
✅ **Testabilidad:** Políticas pueden probarse aisladamente  
✅ **Clean Architecture:** Separación de concerns (seguridad vs. lógica de aplicación)

---

## Notas Técnicas

- Las políticas de ASP.NET Core Authorization evalúan en el pipeline **antes** de ejecutar el método del controller.
- Para lógica de autorización basada en recursos (ej. "solo el creador puede editar"), se pueden usar `IAuthorizationHandler` personalizados con `IAuthorizationService.AuthorizeAsync()` dentro del método.
- La política `Require2FA` ya implementa un `IAuthorizationRequirement` personalizado (`TwoFactorEnabledRequirement`) con su handler (`TwoFactorEnabledHandler`).

---

## Referencia

- [ASP.NET Core Authorization Policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)
- [Role-based vs Policy-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)
