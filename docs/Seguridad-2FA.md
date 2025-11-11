# Autenticación de Dos Factores (2FA)

## Resumen

El sistema implementa autenticación de dos factores (2FA) basada en TOTP (Time-based One-Time Password) usando ASP.NET Identity. Los usuarios pueden habilitar 2FA escaneando un código QR con aplicaciones como Microsoft Authenticator, Google Authenticator o Authy.

## Activación de 2FA para usuarios

1. Iniciar sesión en la aplicación
2. Navegar a **Cuenta** → **Seguridad (2FA)** en el menú lateral
3. Click en **Habilitar autenticador**
4. Escanear el código QR con la aplicación de autenticación
5. Ingresar el código de verificación de 6 dígitos
6. **Guardar los códigos de recuperación** (10 códigos únicos para emergencias)
7. Confirmar activación

## Políticas de autorización disponibles

El sistema define dos políticas de autorización para proteger páginas sensibles:

### 1. `Require2FA`
- **Requisito**: Usuario autenticado con `TwoFactorEnabled = true`
- **Uso**: Para cualquier operación que requiera 2FA independientemente del rol

```csharp
@attribute [Authorize(Policy = "Require2FA")]
```

### 2. `AdminOrTesoreroWith2FA`
- **Requisitos**: 
  - Usuario con rol `Admin` O `Tesorero`
  - Y además `TwoFactorEnabled = true`
- **Uso**: Para operaciones administrativas o financieras críticas

```csharp
@attribute [Authorize(Policy = "AdminOrTesoreroWith2FA")]
```

## Páginas actualmente protegidas

Las siguientes páginas requieren 2FA habilitado:

| Página | Ruta | Política | Descripción |
|--------|------|----------|-------------|
| Gestión de Usuarios | `/config/usuarios` | `AdminOrTesoreroWith2FA` | Crear usuarios, asignar roles |
| Importar Miembros | `/config/importar-miembros` | `AdminOrTesoreroWith2FA` | Importación masiva CSV, pruebas SMTP |
| Auditoría | `/admin/auditoria` | `AdminOrTesoreroWith2FA` | Consulta de registros de auditoría |
| **Cierre Contable** | `/tesoreria/cierre` | `AdminOrTesoreroWith2FA` | Cerrar períodos mensuales, bloquear ediciones |
| **Conceptos** | `/config/conceptos` | `AdminOrTesoreroWith2FA` | Gestión de conceptos de ingresos/egresos |
| **Respaldo** | `/tesoreria/respaldo` | `AdminOrTesoreroWith2FA` | Exportaciones y operaciones de backup |

## Cómo proteger nuevas páginas

### Páginas Razor (Blazor Server)

En el archivo `.razor`, agregar el atributo de autorización:

```csharp
@page "/ruta/pagina"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Policy = "AdminOrTesoreroWith2FA")]

<PageTitle>Título</PageTitle>

<h3>Contenido sensible</h3>
```

### Controladores API (si se agregan en el futuro)

```csharp
[Authorize(Policy = "AdminOrTesoreroWith2FA")]
[ApiController]
[Route("api/[controller]")]
public class OperacionesCriticasController : ControllerBase
{
    // endpoints protegidos
}
```

### Métodos individuales

```csharp
[Authorize(Policy = "Require2FA")]
public async Task<IActionResult> EliminarDatos()
{
    // lógica sensible
}
```

## Comportamiento cuando 2FA no está habilitado

Si un usuario con rol `Admin` o `Tesorero` intenta acceder a una página protegida **sin tener 2FA habilitado**:

1. El handler `TwoFactorEnabledHandler` verifica `ApplicationUser.TwoFactorEnabled`
2. Si es `false`, la autorización falla
3. ASP.NET Core redirige automáticamente a `/Identity/Account/AccessDenied`
4. El usuario ve un mensaje de acceso denegado

**Recomendación**: Agregar un banner o notificación en el Dashboard para incentivar a los usuarios Admin/Tesorero a habilitar 2FA.

## Códigos de recuperación

- Se generan **10 códigos únicos** al activar 2FA
- Permiten acceso en caso de pérdida del dispositivo
- **Cada código solo puede usarse una vez**
- El usuario debe guardarlos en un lugar seguro
- Se pueden regenerar desde la página de gestión de 2FA

## Configuración técnica

### Registro de políticas (`Program.cs`)

```csharp
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Require2FA", policy =>
        policy.Requirements.Add(new TwoFactorEnabledRequirement()));
        
    options.AddPolicy("AdminOrTesoreroWith2FA", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Tesorero");
        policy.Requirements.Add(new TwoFactorEnabledRequirement());
    });
});

// Registrar handler (solo fuera de Testing)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<IAuthorizationHandler, TwoFactorEnabledHandler>();
}
```

### Handler personalizado (`TwoFactorEnabledHandler.cs`)

El handler consulta `UserManager<ApplicationUser>` para verificar la propiedad `TwoFactorEnabled`:

```csharp
protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    TwoFactorEnabledRequirement requirement)
{
    if (context.User?.Identity?.IsAuthenticated != true)
        return;

    var userId = _userManager.GetUserId(context.User);
    if (userId == null) return;

    var user = await _userManager.FindByIdAsync(userId);
    if (user?.TwoFactorEnabled == true)
    {
        context.Succeed(requirement);
    }
}
```

## Consideraciones de seguridad

1. **No desactivar 2FA para usuarios críticos**: Los usuarios Admin y Tesorero deben mantener 2FA habilitado siempre
2. **Códigos de recuperación**: Comunicar la importancia de guardarlos de forma segura
3. **Dispositivo recordado**: Los usuarios pueden marcar "Recordar este dispositivo" para evitar ingresar código en cada sesión (duración: 14 días por defecto)
4. **Auditoría**: Los intentos de acceso a páginas protegidas se registran en logs de ASP.NET Core

## Testing

El entorno de pruebas (`Testing`) usa `TestingAuthenticationHandler` y **no registra** el `TwoFactorEnabledHandler` ni servicios de Identity. Esto permite que las pruebas unitarias funcionen sin dependencias de `UserManager`.

Las 37 pruebas actuales pasan correctamente con esta configuración.

## Banner de advertencia

El sistema incluye un **banner de advertencia** que se muestra automáticamente a usuarios con rol Admin o Tesorero que **no tienen 2FA habilitado**.

### Características del banner:
- **Ubicación**: Parte superior del contenido, visible en todas las páginas
- **Audiencia**: Solo visible para usuarios Admin/Tesorero sin `TwoFactorEnabled = true`
- **Diseño**: Banner amarillo/ámbar con icono de advertencia, acorde al sistema de diseño Tailwind
- **Acciones disponibles**:
  - **"Activar 2FA ahora"**: Redirige directamente a `/Identity/Account/Manage/TwoFactorAuthentication`
  - **"Recordar más tarde"**: Oculta el banner temporalmente (solo en la sesión actual)
  - **Botón de cierre (X)**: Alternativa para descartar el banner

### Comportamiento:
- El banner se muestra **en cada carga de página** hasta que el usuario active 2FA
- Se oculta automáticamente después de que el usuario habilita 2FA
- No bloquea el acceso; es un recordatorio no invasivo

### Componente:
- **Archivo**: `src/Server/Components/Security/TwoFactorWarningBanner.razor`
- **Integrado en**: `src/Server/Pages/Shared/MainLayout.razor`
- **Lógica**: Consulta `UserManager<ApplicationUser>` para verificar `TwoFactorEnabled`

## Auditoría de eventos 2FA

El sistema registra automáticamente todos los eventos relacionados con 2FA en la tabla `AuditLogs`:

### Eventos auditados:

| Evento | Action Code | Descripción |
|--------|-------------|-------------|
| **Habilitación 2FA** | `2FA_ENABLED` | Usuario activa autenticación de dos factores |
| **Deshabilitación 2FA** | `2FA_DISABLED` | Usuario desactiva 2FA (requiere confirmación) |
| **Códigos recuperación** | `2FA_RECOVERY_CODES_GENERATED` | Generación de nuevos códigos de recuperación |
| **Reset autenticador** | `2FA_AUTHENTICATOR_RESET` | Usuario restablece su app autenticadora |

### Información registrada:
- **Usuario** (email)
- **IP del cliente** (extraída de HttpContext)
- **Timestamp** (UTC)
- **Valores anteriores/nuevos** (JSON con detalles del cambio)
- **Información adicional** (descripción legible del evento)

### Servicio implementado:
- **Interface**: `ITwoFactorAuditService`
- **Implementación**: `TwoFactorAuditService`
- **Archivo**: `src/Server/Services/Auth/TwoFactorAuditService.cs`
- **Integrado en**:
  - `EnableAuthenticator.cshtml.cs`
  - `Disable2fa.cshtml.cs`
  - `ResetAuthenticator.cshtml.cs`

### Consultar eventos desde UI:
1. Navegar a `/admin/auditoria`
2. Filtrar por **Tipo de Entidad**: `UserAccount`
3. Filtrar por **Acción**: `2FA_ENABLED`, `2FA_DISABLED`, etc.
4. Opcionalmente filtrar por usuario específico

## Recomendaciones futuras

- [x] ~~Implementar banner de advertencia en Dashboard para usuarios sin 2FA~~ **✅ COMPLETADO**
- [x] ~~Agregar registro de auditoría explícito para eventos de activación/desactivación de 2FA en tabla `AuditLogs`~~ **✅ COMPLETADO**
- [x] ~~Aplicar política `AdminOrTesoreroWith2FA` a operaciones financieras críticas: cierres contables, conceptos, respaldo~~ **✅ COMPLETADO**
- [ ] Persistir el estado de "Recordar más tarde" usando JSInterop (sessionStorage o localStorage) para evitar que el banner reaparezca inmediatamente
- [ ] Considerar hacer 2FA **obligatorio** (no opcional) para roles Admin y Tesorero después de un período de gracia configurado
- [ ] Habilitar confirmación de email (`RequireConfirmedEmail = true`) cuando SMTP esté completamente configurado y probado en producción
- [ ] Implementar notificación por email cuando un usuario activa/desactiva 2FA (cuando SMTP esté operativo)
