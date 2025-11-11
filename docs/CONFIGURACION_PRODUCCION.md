# Configuración para Producción - LAMA Medellín

Este documento lista los elementos de configuración que deben actualizarse antes de desplegar a producción.

## ✅ Tareas Completadas

### 1. Certificado RTE (Datos Actuales en `appsettings.json`)

Los siguientes datos están configurados y deben **verificarse antes de producción**:

```json
"EntidadRTE": {
    "NIT": "900.123.456-7",  // ⚠️ VERIFICAR: NIT real de la fundación
    "NombreCompleto": "Fundación Latin American Motorcycle Association Medellín",
    "NombreCorto": "Fundación LAMA Medellín",
    "Ciudad": "Medellín",
    "Departamento": "Antioquia",
    "Direccion": "Calle 8 SUR # 43 B - 112",
    "Telefono": "+57 (4) 444-5555",  // ⚠️ VERIFICAR: Teléfono de contacto real
    "Email": "contacto@fundacionlamamedellin.org",
    "WebSite": "https://www.fundacionlamamedellin.org",
    
    "EsRTE": true,
    "NumeroResolucionRTE": "RES-2024-001234",  // ⚠️ VERIFICAR: Número de resolución DIAN real
    "FechaResolucionRTE": "2024-01-15",  // ⚠️ VERIFICAR: Fecha de la resolución DIAN
    
    "RepresentanteLegal": {
        "NombreCompleto": "DANIEL ANDREY VILLAMIZAR ARAQUE",
        "TipoIdentificacion": "CC",
        "NumeroIdentificacion": "8.106.002",
        "Cargo": "Representante Legal"
    },
    
    "ContadorPublico": {
        "NombreCompleto": "JUAN SEBASTIAN BARRETO GRANADA",
        "TarjetaProfesional": "167104-T",  // ⚠️ VERIFICAR: Vigencia de la tarjeta profesional
        "Telefono": "+57 300 123 4567",
        "Email": "contador@fundacionlamamedellin.org"
    }
}
```

### **Checklist de Verificación RTE:**

- [ ] **NIT**: Verificar que el NIT `900.123.456-7` es correcto y coincide con el certificado de existencia
- [ ] **Resolución DIAN**: Confirmar número y fecha de resolución RTE con la DIAN
- [ ] **Representante Legal**: Verificar que los datos coinciden con el certificado de existencia vigente
- [ ] **Contador Público**: Confirmar vigencia de la tarjeta profesional `TP-167104-T` en la JCC (Junta Central de Contadores)
- [ ] **Datos de Contacto**: Actualizar teléfonos y correos con información real

---

## 🔧 Configuraciones Adicionales para Producción

### 2. SMTP (Correo Electrónico)

Ver documentación completa en: **`docs/SMTP_PRODUCCION.md`**

```json
"Smtp": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "User": "tesoreria@fundacionlamamedellin.org",
    "Password": "",  // ⚠️ CRÍTICO: Configurar en variables de entorno o Azure Key Vault
    "From": "gerencia@fundacionlamamedellin.org",
    "EnableSsl": true,
    "SendOnCertificateEmission": true
}
```

**Acción Requerida:**
- Configurar `Smtp:Password` usando variables de entorno o Azure Key Vault (NUNCA en appsettings.json)
- Verificar que las cuentas de correo existan y estén activas

### 3. Connection String (Base de Datos)

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LamaMedellin;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Acción Requerida:**
- Cambiar `Server=localhost` por el servidor de producción
- Configurar autenticación (SQL Server Authentication o Managed Identity en Azure)
- Usar Azure Key Vault para almacenar la connection string

### 4. Backup

```json
"Backup": {
    "Enabled": true,
    "CronSchedule": "0 2 * * *",  // Diariamente a las 2 AM
    "BackupPath": "Backups",  // ⚠️ Configurar ruta absoluta en producción
    "RetentionDays": 30,
    "Server": "localhost",  // ⚠️ Cambiar a servidor de producción
    "Database": "LamaMedellin"
}
```

**Acción Requerida:**
- Cambiar `BackupPath` a una ruta absoluta con suficiente espacio (e.g., `D:\Backups\LamaMedellin`)
- Actualizar `Server` con el nombre del servidor de producción
- Considerar configurar backups adicionales en Azure Storage o S3

### 5. Autenticación de Dos Factores

```json
"TwoFactorEnforcement": {
    "GracePeriodDays": 30,
    "EnforceAfterGracePeriod": false  // ⚠️ Cambiar a true después del período de gracia
}
```

**Acción Requerida:**
- Monitorear adopción de 2FA durante los primeros 30 días
- Cambiar `EnforceAfterGracePeriod` a `true` después del período de gracia

---

## 📋 Resumen de Acciones Pre-Producción

| Elemento | Archivo | Acción | Prioridad |
|----------|---------|--------|-----------|
| NIT y Resolución DIAN | `appsettings.json` | Verificar datos con certificados oficiales | 🔴 Alta |
| Contador Público TP | `appsettings.json` | Confirmar vigencia en JCC | 🔴 Alta |
| SMTP Password | Variables de entorno | Configurar en Key Vault | 🔴 Alta |
| Connection String | Variables de entorno | Mover a Key Vault | 🔴 Alta |
| Backup Path | `appsettings.json` | Configurar ruta absoluta | 🟡 Media |
| 2FA Enforcement | `appsettings.json` | Activar después de gracia | 🟢 Baja |

---

## 🔒 Seguridad

**CRÍTICO:** NUNCA incluir secretos en `appsettings.json` o archivos versionados.

**Usar en su lugar:**
- Variables de entorno para desarrollo local
- Azure Key Vault para producción
- User Secrets para desarrollo (`dotnet user-secrets`)

**Ejemplo con User Secrets:**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=prod-server;Database=LamaMedellin;..."
dotnet user-secrets set "Smtp:Password" "tu-password-aqui"
```

**Ejemplo con Azure Key Vault:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## 📚 Documentación Relacionada

- [SMTP_PRODUCCION.md](./SMTP_PRODUCCION.md) - Configuración detallada de correo electrónico
- [Seguridad-2FA.md](./Seguridad-2FA.md) - Autenticación de dos factores
- [Azure Key Vault Docs](https://learn.microsoft.com/azure/key-vault/)
