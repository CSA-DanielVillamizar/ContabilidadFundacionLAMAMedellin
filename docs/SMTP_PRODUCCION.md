# Configuración SMTP para Producción

## Estado Actual

⚠️ **SMTP configurado parcialmente** - Falta contraseña de aplicación

## Archivo de configuración

`src/Server/appsettings.json` (y `appsettings.Production.json` para producción):

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "tesoreria@fundacionlamamedellin.org",
    "Password": "",  // ⚠️ PENDIENTE: Agregar contraseña de aplicación
    "From": "tesoreria@fundacionlamamedellin.org",
    "EnableSsl": true,
    "SendOnCertificateEmission": true
  }
}
```

## Pasos para configurar Gmail SMTP

### Opción 1: Contraseña de aplicación de Gmail (Recomendado)

1. **Habilitar 2FA en la cuenta de Gmail** (requisito obligatorio):
   - Ir a https://myaccount.google.com/security
   - Sección "Cómo accedes a Google" → **Verificación en dos pasos**
   - Activar y configurar

2. **Generar contraseña de aplicación**:
   - Ir a https://myaccount.google.com/apppasswords
   - Seleccionar aplicación: **Correo**
   - Seleccionar dispositivo: **Otro (nombre personalizado)**
   - Nombre: `LAMA Medellín - Servidor Producción`
   - Copiar la contraseña de 16 caracteres generada

3. **Actualizar `appsettings.Production.json`**:
   ```json
   {
     "Smtp": {
       "Password": "xxxx xxxx xxxx xxxx"  // Contraseña de aplicación (16 caracteres)
     }
   }
   ```

### Opción 2: Google Workspace (si la organización tiene)

Si `@fundacionlamamedellin.org` es un dominio de Google Workspace:

1. Usar las credenciales de administrador del workspace
2. Configurar autenticación OAuth 2.0 (más complejo pero más seguro)
3. Alternativa: Usar contraseña de aplicación como en Opción 1

### Opción 3: Proveedor SMTP alternativo

Si Gmail presenta problemas, considerar:

- **SendGrid** (https://sendgrid.com/) - 100 emails/día gratis
- **Mailgun** (https://www.mailgun.com/) - 5,000 emails/mes gratis
- **Amazon SES** (https://aws.amazon.com/ses/) - 62,000 emails/mes gratis (si se usa desde EC2)

## Funcionalidades que dependen de SMTP

### ✅ Ya implementadas (esperando SMTP operacional):

1. **Confirmación de email al registrarse**:
   - `RequireConfirmedEmail = false` actualmente
   - Cambiar a `true` en `Program.cs` línea 57 cuando SMTP esté listo

2. **Importación de miembros con envío de emails**:
   - Página `/config/importar-miembros`
   - Permite enviar emails individuales desde tabla de miembros
   - Botón "Enviar email de prueba" para validar configuración

3. **Certificados de donación por email**:
   - `SendOnCertificateEmission = true` en configuración
   - Envía automáticamente certificado PDF al donante cuando se emite
   - Servicio: `CertificadosDonacionService.EmitirCertificadoAsync()`

### ⏳ Pendientes de implementar:

4. **Notificaciones de cambios 2FA**:
   - Enviar email cuando usuario habilita/deshabilita 2FA
   - Template con IP, fecha/hora, dispositivo
   - Alerta de seguridad

5. **Recuperación de contraseña**:
   - Link de restablecimiento por email
   - ASP.NET Identity ya tiene soporte, solo falta habilitar

6. **Notificaciones de actividad sospechosa**:
   - Múltiples intentos de login fallidos
   - Cambios de contraseña
   - Acceso desde nueva IP

## Validar configuración SMTP

### Desde la aplicación:

1. Iniciar sesión como Admin
2. Navegar a `/config/importar-miembros`
3. Sección "Diagnóstico SMTP"
4. Ingresar email de prueba
5. Click "Enviar email de prueba"
6. Verificar recepción del email

### Desde código (opcional):

```csharp
@inject IEmailService EmailService

await EmailService.SendEmailAsync(
    to: "prueba@example.com",
    subject: "Prueba SMTP - LAMA Medellín",
    body: "Este es un email de prueba de configuración SMTP."
);
```

## Seguridad

### ⚠️ **NUNCA** incluir contraseñas en `appsettings.json` del repositorio

Opciones seguras:

1. **Variables de entorno** (Recomendado para producción):
   ```bash
   # En Windows (PowerShell)
   $env:Smtp__Password = "xxxx xxxx xxxx xxxx"
   
   # En Linux/Docker
   export Smtp__Password="xxxx xxxx xxxx xxxx"
   ```

2. **Azure Key Vault** (si se hospeda en Azure):
   ```csharp
   builder.Configuration.AddAzureKeyVault(/* config */);
   ```

3. **User Secrets** (solo para desarrollo):
   ```bash
   dotnet user-secrets set "Smtp:Password" "xxxx xxxx xxxx xxxx"
   ```

4. **Archivo `appsettings.Production.json` excluido de Git**:
   ```gitignore
   # .gitignore
   appsettings.Production.json
   ```

## Troubleshooting

### Error: "The SMTP server requires a secure connection"

✅ **Solución**: Verificar que `EnableSsl = true` en configuración

### Error: "Authentication failed"

Posibles causas:
- Contraseña incorrecta
- 2FA no habilitado en Gmail
- "Acceso de aplicaciones menos seguras" deshabilitado (deprecado en Mayo 2022)

✅ **Solución**: Usar contraseña de aplicación (ver Opción 1 arriba)

### Error: "Mailbox unavailable"

✅ **Solución**: Verificar que el email `From` coincide con el `User` autenticado

### Emails llegan a spam

✅ **Soluciones**:
1. Configurar SPF/DKIM/DMARC en DNS del dominio
2. Usar dominio verificado en Google Workspace
3. Evitar palabras spam en asunto/cuerpo
4. Implementar rate limiting (no enviar muchos emails seguidos)

## Checklist de configuración

- [ ] Habilitar 2FA en cuenta Gmail
- [ ] Generar contraseña de aplicación
- [ ] Actualizar `appsettings.Production.json` con contraseña
- [ ] **NO** commitear contraseña al repositorio
- [ ] Configurar variable de entorno en servidor de producción
- [ ] Probar envío desde `/config/importar-miembros`
- [ ] Verificar recepción de email de prueba
- [ ] Habilitar `RequireConfirmedEmail = true` en `Program.cs`
- [ ] Documentar credenciales SMTP en gestor de contraseñas seguro
- [ ] Configurar alertas de cuota si se usa proveedor con límites

## Referencias

- [Contraseñas de aplicación de Gmail](https://support.google.com/accounts/answer/185833)
- [SMTP de Google Workspace](https://support.google.com/a/answer/176600)
- [ASP.NET Core Configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration)

---

**Fecha de creación**: 27 de octubre de 2025  
**Estado**: ⚠️ Configuración pendiente  
**Prioridad**: 🔴 Alta - Desbloquea confirmación de email y notificaciones
