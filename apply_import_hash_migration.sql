-- Aplicar migración: AddImportRowHashForIdempotency
-- Agregar columna ImportRowHash a las tres tablas con índices únicos para deduplicación

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- Verificar e insertar en el historial de migraciones
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122170435_AddImportRowHashForIdempotency'
)
BEGIN
    -- Agregar columna a Recibos si no existe
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID('dbo.Recibos') 
        AND name = 'ImportRowHash'
    )
    BEGIN
        ALTER TABLE [dbo].[Recibos]
        ADD [ImportRowHash] nvarchar(64) NULL;
    END;

    -- Agregar columna a Ingresos si no existe
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID('dbo.Ingresos') 
        AND name = 'ImportRowHash'
    )
    BEGIN
        ALTER TABLE [dbo].[Ingresos]
        ADD [ImportRowHash] nvarchar(64) NULL;
    END;

    -- Agregar columna a Egresos si no existe
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID('dbo.Egresos') 
        AND name = 'ImportRowHash'
    )
    BEGIN
        ALTER TABLE [dbo].[Egresos]
        ADD [ImportRowHash] nvarchar(64) NULL;
    END;

    -- Registrar la migración en el historial
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260122170435_AddImportRowHashForIdempotency', N'8.0.0');
END;
GO

-- Crear índices únicos en un segundo lote (después de que las columnas existan)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Recibos_ImportRowHash' AND object_id = OBJECT_ID('dbo.Recibos'))
BEGIN
    CREATE UNIQUE INDEX [UX_Recibos_ImportRowHash]
    ON [dbo].[Recibos] ([ImportRowHash])
    WHERE [ImportRowHash] IS NOT NULL;
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Ingresos_ImportRowHash' AND object_id = OBJECT_ID('dbo.Ingresos'))
BEGIN
    CREATE UNIQUE INDEX [UX_Ingresos_ImportRowHash]
    ON [dbo].[Ingresos] ([ImportRowHash])
    WHERE [ImportRowHash] IS NOT NULL;
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Egresos_ImportRowHash' AND object_id = OBJECT_ID('dbo.Egresos'))
BEGIN
    CREATE UNIQUE INDEX [UX_Egresos_ImportRowHash]
    ON [dbo].[Egresos] ([ImportRowHash])
    WHERE [ImportRowHash] IS NOT NULL;
END;
GO
GO

PRINT '✅ Migración AddImportRowHashForIdempotency aplicada correctamente'
GO
