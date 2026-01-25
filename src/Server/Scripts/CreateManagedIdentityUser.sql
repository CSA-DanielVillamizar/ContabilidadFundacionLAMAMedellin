-- ============================================================================
-- Script: Crear Usuario de Managed Identity en Azure SQL Database
-- Propósito: Permitir que el App Service acceda a la DB con Managed Identity
-- Seguridad: SOLO roles de lectura/escritura de datos, NO db_owner
-- Idempotente: Verifica si el usuario existe antes de crearlo
-- ============================================================================

-- Ejecutar como administrador Entra ID o db_owner
-- Conexión: Azure Data Studio / SSMS con autenticación Entra ID

USE [sqldb-tesorerialamamedellin-prod];
GO

-- Verificar si ya existe el usuario
IF NOT EXISTS (
    SELECT * FROM sys.database_principals 
    WHERE name = N'app-tesorerialamamedellin-prod'
)
BEGIN
    -- Crear usuario para la Managed Identity del App Service
    -- El nombre debe coincidir exactamente con el nombre del recurso del App Service
    CREATE USER [app-tesorerialamamedellin-prod] FROM EXTERNAL PROVIDER;
    PRINT '✓ Usuario [app-tesorerialamamedellin-prod] creado exitosamente.';
END
ELSE
BEGIN
    PRINT '⚠️ Usuario [app-tesorerialamamedellin-prod] ya existe. Saltando creación.';
END
GO

-- Asignar rol de lectura de datos
IF NOT EXISTS (
    SELECT * FROM sys.database_role_members drm
    INNER JOIN sys.database_principals p ON drm.member_principal_id = p.principal_id
    INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    WHERE p.name = N'app-tesorerialamamedellin-prod' 
      AND r.name = N'db_datareader'
)
BEGIN
    ALTER ROLE [db_datareader] ADD MEMBER [app-tesorerialamamedellin-prod];
    PRINT '✓ Rol [db_datareader] asignado a [app-tesorerialamamedellin-prod].';
END
ELSE
BEGIN
    PRINT '⚠️ Rol [db_datareader] ya asignado. Saltando.';
END
GO

-- Asignar rol de escritura de datos
IF NOT EXISTS (
    SELECT * FROM sys.database_role_members drm
    INNER JOIN sys.database_principals p ON drm.member_principal_id = p.principal_id
    INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    WHERE p.name = N'app-tesorerialamamedellin-prod' 
      AND r.name = N'db_datawriter'
)
BEGIN
    ALTER ROLE [db_datawriter] ADD MEMBER [app-tesorerialamamedellin-prod];
    PRINT '✓ Rol [db_datawriter] asignado a [app-tesorerialamamedellin-prod].';
END
ELSE
BEGIN
    PRINT '⚠️ Rol [db_datawriter] ya asignado. Saltando.';
END
GO

-- ============================================================================
-- Verificación: Listar usuario y roles asignados
-- ============================================================================

PRINT '';
PRINT '=== VERIFICACIÓN DE USUARIO Y ROLES ===';
PRINT '';

SELECT 
    p.name AS [Usuario],
    p.type_desc AS [Tipo],
    p.principal_id AS [Principal ID]
FROM sys.database_principals p
WHERE p.name = N'app-tesorerialamamedellin-prod';

PRINT '';
PRINT 'Roles asignados al usuario:';
PRINT '';

SELECT 
    p.name AS [Usuario],
    r.name AS [Rol]
FROM sys.database_role_members drm
INNER JOIN sys.database_principals p ON drm.member_principal_id = p.principal_id
INNER JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE p.name = N'app-tesorerialamamedellin-prod'
ORDER BY r.name;

PRINT '';
PRINT '✅ Script completado.';
