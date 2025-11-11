# Configuración SMTP (Producción)

Para habilitar el envío de correos (recibos/certificados), establezca la contraseña de la cuenta institucional y valide un correo de prueba.

## 1) appsettings.json (solo local/desarrollo)

En `src/Server/appsettings.json`:

```
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "User": "tesoreria@fundacionlamamedellin.org",
  "Password": "<APP_PASSWORD>",
  "From": "tesoreria@fundacionlamamedellin.org",
  "EnableSsl": true,
  "SendOnCertificateEmission": true
}
```

Recomendado: usar password de "App" (Gmail/App Password) y no la contraseña personal.

## 2) Variables de entorno (recomendado para producción)

La aplicación lee la configuración estándar de .NET, por lo que puede definir variables de entorno en el host con prefijo `Smtp__` (doble guion bajo):

- `Smtp__Host`
- `Smtp__Port`
- `Smtp__User`
- `Smtp__Password`
- `Smtp__From`
- `Smtp__EnableSsl`

Ejemplo (Windows PowerShell, sesión actual):

```powershell
$env:Smtp__User = "tesoreria@fundacionlamamedellin.org"
$env:Smtp__Password = "<APP_PASSWORD>"
```

## 3) Prueba rápida

- Emitir un certificado de donación o un recibo y verificar el log de auditoría y la bandeja del destinatario.
- Si ocurre error, revise el log de la consola y confirme Host/Port/SSL y credenciales.

---

## Exchange Online (Microsoft 365)

Si su correo está en Microsoft 365, use:

- Host: `smtp.office365.com`
- Puerto: `587`
- TLS/STARTTLS: habilitado (`EnableSsl = true`)
- Autenticación: usuario/contraseña de la cuenta de Microsoft 365

Importante:

- El remitente (`From`) debe ser el mismo buzón que autentica (`User`) o la cuenta debe tener permiso de "Enviar como" sobre el buzón configurado en `From`.
- Evite guardar la contraseña en `appsettings.json`. Use variables de entorno:

```powershell
$env:Smtp__Host = "smtp.office365.com"
$env:Smtp__Port = "587"
$env:Smtp__User = "gerencia@fundacionlamamedellin.org"
$env:Smtp__Password = "<CONTRASEÑA>"
$env:Smtp__From = "gerencia@fundacionlamamedellin.org"
$env:Smtp__EnableSsl = "true"
```

Errores comunes:

- `5.7.60 Client does not have permissions to send as this sender` → alinear `From` con `User` o otorgar "Enviar como" en Microsoft 365.
- Problemas de TLS → confirmar puerto 587 y `EnableSsl = true`.

Seguridad:

- No suba contraseñas al repositorio. Si una contraseña fue publicada, cámbiela inmediatamente y rote las credenciales.
