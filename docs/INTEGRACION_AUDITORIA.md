# 🔍 Integración de Auditoría - Sistema LAMA Medellín

## ✅ Implementación Completada

### **Servicios Actualizados con Auditoría:**

---

## 📋 1. CertificadosDonacionService

### **Operaciones Auditadas:**

#### ✅ Emisión de Certificados (`EmitirAsync`)
```csharp
await _audit.LogAsync(
    entityType: "CertificadoDonacion",
    entityId: certificado.Id.ToString(),
    action: "Emitted",
    userName: currentUser,  // Usuario real: tesoreria@fundacionlamamedellin.org
    newValues: new 
    { 
        Consecutivo = certificado.Consecutivo,
        Ano = certificado.Ano,
        Estado = certificado.Estado,
        FechaEmision = certificado.FechaEmision,
        NombreDonante = certificado.NombreDonante,
        ValorDonacionCOP = certificado.ValorDonacionCOP
    },
    additionalInfo: $"Certificado CD-{certificado.Ano}-{certificado.Consecutivo:D5} emitido para {certificado.NombreDonante}"
);
```

**Qué registra:**
- ✓ Consecutivo asignado
- ✓ Año del certificado
- ✓ Estado (Borrador → Emitido)
- ✓ Fecha de emisión
- ✓ Nombre del donante
- ✓ Valor de la donación
- ✓ Usuario que emitió

#### ✅ Anulación de Certificados (`AnularAsync`)
```csharp
await _audit.LogAsync(
    entityType: "CertificadoDonacion",
    entityId: certificado.Id.ToString(),
    action: "Annulled",
    userName: currentUser,
    oldValues: new { Estado = EstadoCertificado.Emitido },
    newValues: new 
    { 
        Estado = EstadoCertificado.Anulado,
        RazonAnulacion = dto.RazonAnulacion,
        FechaAnulacion = certificado.FechaAnulacion
    },
    additionalInfo: $"Certificado CD-{certificado.Ano}-{certificado.Consecutivo:D5} anulado. Razón: {dto.RazonAnulacion}"
);
```

**Qué registra:**
- ✓ Estado anterior (Emitido)
- ✓ Estado nuevo (Anulado)
- ✓ Razón de la anulación
- ✓ Fecha de anulación
- ✓ Usuario que anuló

---

## 🧾 2. RecibosService

### **Operaciones Auditadas:**

#### ✅ Emisión de Recibos (`EmitirReciboAsync`)
```csharp
await _audit.LogAsync(
    entityType: "Recibo",
    entityId: recibo.Id.ToString(),
    action: "Emitted",
    userName: "system", // TODO: Mejorar pasando currentUser
    newValues: new 
    { 
        Consecutivo = recibo.Consecutivo,
        Serie = recibo.Serie,
        Ano = recibo.Ano,
        Estado = recibo.Estado,
        FechaEmision = recibo.FechaEmision,
        TotalCop = recibo.TotalCop
    },
    additionalInfo: $"Recibo {recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo:D5} emitido. Total: ${recibo.TotalCop:N0}"
);
```

**Qué registra:**
- ✓ Consecutivo asignado
- ✓ Serie del recibo (SI, CD, etc.)
- ✓ Año del recibo
- ✓ Estado (Borrador → Emitido)
- ✓ Fecha de emisión
- ✓ Total en COP

**⚠️ NOTA:** Actualmente usa "system" como userName. Se recomienda actualizar el método para recibir `currentUser` como parámetro.

#### ✅ Anulación de Recibos (`AnularAsync`)
```csharp
await _audit.LogAsync(
    entityType: "Recibo",
    entityId: recibo.Id.ToString(),
    action: "Annulled",
    userName: currentUser,
    oldValues: new { Estado = EstadoRecibo.Emitido },
    newValues: new 
    { 
        Estado = EstadoRecibo.Anulado,
        Observaciones = recibo.Observaciones
    },
    additionalInfo: $"Recibo {recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo:D5} anulado. Razón: {razon}"
);
```

**Qué registra:**
- ✓ Estado anterior (Emitido)
- ✓ Estado nuevo (Anulado)
- ✓ Observaciones actualizadas
- ✓ Razón de anulación
- ✓ Usuario que anuló

---

## 👤 3. Integración de CurrentUserService

### **Archivo Actualizado: `ListaMiembros.razor`**

**ANTES (❌):**
```csharp
await MiembrosService.UpdateAsync(updateDto, "current-user");
await MiembrosService.CreateAsync(formData, "current-user");
```

**DESPUÉS (✅):**
```csharp
@inject Server.Services.Auth.ICurrentUserService CurrentUserService

// En el código:
await MiembrosService.UpdateAsync(updateDto, CurrentUserService.GetUserName());
await MiembrosService.CreateAsync(formData, CurrentUserService.GetUserName());
```

**Beneficios:**
- ✅ Usuario real en auditoría: `tesoreria@fundacionlamamedellin.org`
- ✅ Trazabilidad completa de cambios
- ✅ Cumplimiento normativo DIAN

---

## 📊 Consultas de Auditoría Disponibles

### **Ver historial de una entidad específica:**
```csharp
// Ver todos los cambios de un certificado
var logs = await _audit.GetEntityLogsAsync(
    "CertificadoDonacion", 
    certificadoId.ToString()
);

// Ver todos los cambios de un recibo
var logs = await _audit.GetEntityLogsAsync(
    "Recibo", 
    reciboId.ToString()
);
```

### **Ver actividad reciente del sistema:**
```csharp
// Últimos 100 eventos de auditoría
var recentLogs = await _audit.GetRecentLogsAsync(100);
```

---

## 🗂️ Estructura de la Tabla AuditLogs

```sql
CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier PRIMARY KEY,
    [EntityType] nvarchar(100) NOT NULL,       -- "CertificadoDonacion", "Recibo", etc.
    [EntityId] nvarchar(100) NOT NULL,         -- GUID de la entidad
    [Action] nvarchar(100) NOT NULL,           -- "Emitted", "Annulled", "Created", etc.
    [UserName] nvarchar(256) NOT NULL,         -- tesoreria@fundacionlamamedellin.org
    [Timestamp] datetime2 NOT NULL,            -- Fecha/hora del evento
    [OldValues] nvarchar(max) NULL,            -- JSON con valores anteriores
    [NewValues] nvarchar(max) NULL,            -- JSON con valores nuevos
    [IpAddress] nvarchar(50) NULL,             -- IP del usuario (opcional)
    [AdditionalInfo] nvarchar(max) NULL        -- Información adicional
);

-- Índices para consultas rápidas
CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
```

---

## 📝 Ejemplo de Registro en la Base de Datos

```json
{
  "Id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "EntityType": "CertificadoDonacion",
  "EntityId": "12345678-90ab-cdef-1234-567890abcdef",
  "Action": "Emitted",
  "UserName": "tesoreria@fundacionlamamedellin.org",
  "Timestamp": "2025-10-23T15:30:45",
  "OldValues": null,
  "NewValues": "{\"Consecutivo\":42,\"Ano\":2025,\"Estado\":\"Emitido\",\"FechaEmision\":\"2025-10-23T15:30:45\",\"NombreDonante\":\"Juan Pérez\",\"ValorDonacionCOP\":500000}",
  "IpAddress": null,
  "AdditionalInfo": "Certificado CD-2025-00042 emitido para Juan Pérez"
}
```

---

## 🎯 Casos de Uso de Auditoría

### 1. **Rastreo de Certificados Emitidos**
```sql
SELECT 
    UserName,
    CONVERT(DATE, Timestamp) AS Fecha,
    COUNT(*) AS CertificadosEmitidos
FROM AuditLogs
WHERE EntityType = 'CertificadoDonacion' 
  AND Action = 'Emitted'
  AND Timestamp >= '2025-01-01'
GROUP BY UserName, CONVERT(DATE, Timestamp)
ORDER BY Fecha DESC;
```

### 2. **Ver Certificados Anulados (últimos 30 días)**
```sql
SELECT 
    EntityId,
    UserName,
    Timestamp,
    AdditionalInfo
FROM AuditLogs
WHERE EntityType = 'CertificadoDonacion' 
  AND Action = 'Annulled'
  AND Timestamp >= DATEADD(day, -30, GETDATE())
ORDER BY Timestamp DESC;
```

### 3. **Actividad de un Usuario Específico**
```sql
SELECT 
    EntityType,
    Action,
    Timestamp,
    AdditionalInfo
FROM AuditLogs
WHERE UserName = 'tesoreria@fundacionlamamedellin.org'
  AND Timestamp >= DATEADD(day, -7, GETDATE())
ORDER BY Timestamp DESC;
```

---

## 🔧 Próximas Mejoras Recomendadas

### ALTA Prioridad:
1. **Actualizar `RecibosService.EmitirReciboAsync`** para pasar `currentUser` en lugar de "system"
2. **Agregar IP del usuario** en todos los logs (capturar desde `IHttpContextAccessor`)
3. **Crear página UI de auditoría** para consultar logs sin SQL

### MEDIA Prioridad:
4. Auditar operaciones de **Miembros** (Create/Update/Delete)
5. Auditar operaciones de **Egresos**
6. Auditar **Cierre Mensual** de contabilidad

### BAJA Prioridad:
7. Exportar logs de auditoría a CSV
8. Dashboard de actividad del sistema
9. Alertas de actividad sospechosa

---

## ✅ Estado Final

### **Completado:**
- ✅ Migración `AddAuditLogs` aplicada
- ✅ Tabla `AuditLogs` creada con índices
- ✅ `CurrentUserService` implementado
- ✅ TODOs de "current-user" eliminados en `ListaMiembros.razor`
- ✅ Auditoría en `CertificadosDonacionService` (Emitir/Anular)
- ✅ Auditoría en `RecibosService` (Emitir/Anular)
- ✅ Compilación exitosa sin errores

### **Pendiente:**
- ⏳ Crear UI para ver logs de auditoría
- ⏳ Mejorar captura de usuario en `RecibosService.EmitirReciboAsync`
- ⏳ Agregar captura de IP del usuario

---

## 🌐 Dominio Correcto de la Fundación

**Recordatorio:** Todas las cuentas de usuario deben usar el dominio:

```
@fundacionlamamedellin.org
```

**Ejemplos:**
- `tesoreria@fundacionlamamedellin.org`
- `admin@fundacionlamamedellin.org`
- `contador@fundacionlamamedellin.org`

---

**Fecha de implementación:** 23 de octubre de 2025  
**Version:** 2.1.0  
**Estado:** ✅ COMPLETADO Y FUNCIONAL
