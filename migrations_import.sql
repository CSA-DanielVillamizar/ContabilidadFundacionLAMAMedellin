IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [Conceptos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(60) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [Moneda] int NOT NULL,
        [PrecioBase] decimal(18,2) NOT NULL,
        [EsRecurrente] bit NOT NULL,
        [Periodicidad] int NOT NULL,
        [EsIngreso] bit NOT NULL,
        CONSTRAINT [PK_Conceptos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [Egresos] (
        [Id] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Categoria] nvarchar(120) NOT NULL,
        [Proveedor] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(1000) NOT NULL,
        [ValorCop] decimal(18,2) NOT NULL,
        [SoporteUrl] nvarchar(max) NULL,
        [UsuarioRegistro] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Egresos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [Miembros] (
        [Id] uniqueidentifier NOT NULL,
        [Nombres] nvarchar(120) NOT NULL,
        [Apellidos] nvarchar(120) NOT NULL,
        [Documento] nvarchar(40) NOT NULL,
        [Email] nvarchar(160) NOT NULL,
        [Telefono] nvarchar(max) NOT NULL,
        [Direccion] nvarchar(max) NOT NULL,
        [MemberNumber] int NULL,
        [Cargo] nvarchar(max) NOT NULL,
        [Rango] nvarchar(max) NOT NULL,
        [Estado] int NOT NULL,
        [FechaIngreso] date NULL,
        [DatosIncompletos] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Miembros] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [TasasCambio] (
        [Id] int NOT NULL IDENTITY,
        [Fecha] date NOT NULL,
        [UsdCop] decimal(18,2) NOT NULL,
        [Fuente] nvarchar(max) NOT NULL,
        [ObtenidaAutomaticamente] bit NOT NULL,
        CONSTRAINT [PK_TasasCambio] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [Recibos] (
        [Id] uniqueidentifier NOT NULL,
        [Serie] nvarchar(max) NOT NULL,
        [Ano] int NOT NULL,
        [Consecutivo] int NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [Estado] int NOT NULL,
        [MiembroId] uniqueidentifier NULL,
        [TerceroLibre] nvarchar(max) NULL,
        [TotalCop] decimal(18,2) NOT NULL,
        [Observaciones] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Recibos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Recibos_Miembros_MiembroId] FOREIGN KEY ([MiembroId]) REFERENCES [Miembros] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [Pagos] (
        [Id] int NOT NULL IDENTITY,
        [ReciboId] uniqueidentifier NOT NULL,
        [Metodo] int NOT NULL,
        [Referencia] nvarchar(max) NULL,
        [FechaPago] datetime2 NOT NULL,
        [ValorPagadoCop] decimal(18,2) NOT NULL,
        [UsuarioRegistro] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Pagos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pagos_Recibos_ReciboId] FOREIGN KEY ([ReciboId]) REFERENCES [Recibos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE TABLE [ReciboItems] (
        [Id] int NOT NULL IDENTITY,
        [ReciboId] uniqueidentifier NOT NULL,
        [ConceptoId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitarioMonedaOrigen] decimal(18,2) NOT NULL,
        [MonedaOrigen] int NOT NULL,
        [TrmAplicada] decimal(18,2) NULL,
        [SubtotalCop] decimal(18,2) NOT NULL,
        [Notas] nvarchar(max) NULL,
        CONSTRAINT [PK_ReciboItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReciboItems_Conceptos_ConceptoId] FOREIGN KEY ([ConceptoId]) REFERENCES [Conceptos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReciboItems_Recibos_ReciboId] FOREIGN KEY ([ReciboId]) REFERENCES [Recibos] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Conceptos_Codigo] ON [Conceptos] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Miembros_Documento] ON [Miembros] ([Documento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pagos_ReciboId] ON [Pagos] ([ReciboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReciboItems_ConceptoId] ON [ReciboItems] ([ConceptoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReciboItems_ReciboId] ON [ReciboItems] ([ReciboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Recibos_MiembroId] ON [Recibos] ([MiembroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TasasCambio_Fecha] ON [TasasCambio] ([Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017210847_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251017210847_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Telefono');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Telefono] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Rango');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Rango] nvarchar(50) COLLATE Modern_Spanish_CI_AS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Nombres');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Nombres] nvarchar(120) COLLATE Modern_Spanish_CI_AS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Direccion');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Direccion] nvarchar(500) COLLATE Modern_Spanish_CI_AS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Cargo');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Cargo] nvarchar(100) COLLATE Modern_Spanish_CI_AS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Miembros]') AND [c].[name] = N'Apellidos');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Miembros] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Miembros] ALTER COLUMN [Apellidos] nvarchar(120) COLLATE Modern_Spanish_CI_AS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    ALTER TABLE [Miembros] ADD [Cedula] nvarchar(40) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    ALTER TABLE [Miembros] ADD [Celular] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    ALTER TABLE [Miembros] ADD [NombreCompleto] nvarchar(250) COLLATE Modern_Spanish_CI_AS NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    ALTER TABLE [Miembros] ADD [NumeroSocio] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'Name');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'LoginProvider');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'ProviderKey');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [ProviderKey] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'LoginProvider');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    CREATE INDEX [IX_Miembros_Cedula] ON [Miembros] ([Cedula]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251019144916_UpdateMiembroModelWithUTF8Support'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251019144916_UpdateMiembroModelWithUTF8Support', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022063256_AddCierreMensual'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TasasCambio]') AND [c].[name] = N'UsdCop');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [TasasCambio] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [TasasCambio] ALTER COLUMN [UsdCop] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022063256_AddCierreMensual'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ReciboItems]') AND [c].[name] = N'TrmAplicada');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ReciboItems] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [ReciboItems] ALTER COLUMN [TrmAplicada] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022063256_AddCierreMensual'
)
BEGIN
    CREATE TABLE [CierresMensuales] (
        [Id] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [Mes] int NOT NULL,
        [FechaCierre] datetime2 NOT NULL,
        [UsuarioCierre] nvarchar(200) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [SaldoInicialCalculado] decimal(18,2) NOT NULL,
        [TotalIngresos] decimal(18,2) NOT NULL,
        [TotalEgresos] decimal(18,2) NOT NULL,
        [SaldoFinal] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CierresMensuales] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022063256_AddCierreMensual'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CierresMensuales_Ano_Mes] ON [CierresMensuales] ([Ano], [Mes]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022063256_AddCierreMensual'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251022063256_AddCierreMensual', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023151037_AddCertificadosDonacion'
)
BEGIN
    CREATE TABLE [CertificadosDonacion] (
        [Id] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [Consecutivo] int NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [FechaDonacion] datetime2 NOT NULL,
        [TipoIdentificacionDonante] nvarchar(10) NOT NULL,
        [IdentificacionDonante] nvarchar(50) NOT NULL,
        [NombreDonante] nvarchar(300) NOT NULL,
        [DireccionDonante] nvarchar(500) NULL,
        [CiudadDonante] nvarchar(150) NULL,
        [TelefonoDonante] nvarchar(50) NULL,
        [EmailDonante] nvarchar(200) NULL,
        [DescripcionDonacion] nvarchar(2000) NOT NULL,
        [ValorDonacionCOP] decimal(18,2) NOT NULL,
        [FormaDonacion] nvarchar(100) NOT NULL,
        [DestinacionDonacion] nvarchar(1000) NULL,
        [Observaciones] nvarchar(max) NULL,
        [ReciboId] uniqueidentifier NULL,
        [NitEntidad] nvarchar(50) NOT NULL,
        [NombreEntidad] nvarchar(300) NOT NULL,
        [EntidadRTE] bit NOT NULL,
        [ResolucionRTE] nvarchar(100) NULL,
        [FechaResolucionRTE] datetime2 NULL,
        [NombreRepresentanteLegal] nvarchar(300) NOT NULL,
        [IdentificacionRepresentante] nvarchar(50) NOT NULL,
        [CargoRepresentante] nvarchar(100) NOT NULL,
        [NombreContador] nvarchar(300) NULL,
        [TarjetaProfesionalContador] nvarchar(50) NULL,
        [NombreRevisorFiscal] nvarchar(300) NULL,
        [TarjetaProfesionalRevisorFiscal] nvarchar(50) NULL,
        [Estado] int NOT NULL,
        [RazonAnulacion] nvarchar(max) NULL,
        [FechaAnulacion] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CertificadosDonacion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CertificadosDonacion_Recibos_ReciboId] FOREIGN KEY ([ReciboId]) REFERENCES [Recibos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023151037_AddCertificadosDonacion'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CertificadosDonacion_Ano_Consecutivo] ON [CertificadosDonacion] ([Ano], [Consecutivo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023151037_AddCertificadosDonacion'
)
BEGIN
    CREATE INDEX [IX_CertificadosDonacion_ReciboId] ON [CertificadosDonacion] ([ReciboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251023151037_AddCertificadosDonacion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251023151037_AddCertificadosDonacion', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024004901_AddAuditLogs'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        [UserName] nvarchar(256) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [IpAddress] nvarchar(50) NULL,
        [AdditionalInfo] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024004901_AddAuditLogs'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024004901_AddAuditLogs'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024004901_AddAuditLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251024004901_AddAuditLogs', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024053700_AddTwoFactorRequiredSince'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [TwoFactorRequiredSince] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251024053700_AddTwoFactorRequiredSince'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251024053700_AddTwoFactorRequiredSince', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [ComprasProductos] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroCompra] nvarchar(50) NOT NULL,
        [FechaCompra] datetime2 NOT NULL,
        [Proveedor] nvarchar(200) NOT NULL,
        [TotalCOP] decimal(18,2) NOT NULL,
        [TotalUSD] decimal(18,2) NULL,
        [TrmAplicada] decimal(18,4) NULL,
        [Estado] int NOT NULL,
        [NumeroFacturaProveedor] nvarchar(100) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [EgresoId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_ComprasProductos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComprasProductos_Egresos_EgresoId] FOREIGN KEY ([EgresoId]) REFERENCES [Egresos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [Ingresos] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroIngreso] nvarchar(50) NOT NULL,
        [FechaIngreso] datetime2 NOT NULL,
        [Categoria] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(1000) NOT NULL,
        [ValorCop] decimal(18,2) NOT NULL,
        [MetodoPago] nvarchar(50) NOT NULL,
        [ReferenciaTransaccion] nvarchar(200) NULL,
        [Observaciones] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Ingresos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Tipo] int NOT NULL,
        [PrecioVentaCOP] decimal(18,2) NOT NULL,
        [PrecioVentaUSD] decimal(18,2) NULL,
        [StockActual] int NOT NULL,
        [StockMinimo] int NOT NULL,
        [Talla] nvarchar(20) NULL,
        [Descripcion] nvarchar(1000) NULL,
        [EsParcheOficial] bit NOT NULL,
        [ImagenUrl] nvarchar(500) NULL,
        [Activo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [VentasProductos] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroVenta] nvarchar(50) NOT NULL,
        [FechaVenta] datetime2 NOT NULL,
        [TipoCliente] int NOT NULL,
        [MiembroId] uniqueidentifier NULL,
        [NombreCliente] nvarchar(200) NULL,
        [IdentificacionCliente] nvarchar(50) NULL,
        [EmailCliente] nvarchar(100) NULL,
        [TotalCOP] decimal(18,2) NOT NULL,
        [TotalUSD] decimal(18,2) NULL,
        [MetodoPago] int NOT NULL,
        [Estado] int NOT NULL,
        [ReciboId] uniqueidentifier NULL,
        [IngresoId] uniqueidentifier NULL,
        [Observaciones] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_VentasProductos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VentasProductos_Ingresos_IngresoId] FOREIGN KEY ([IngresoId]) REFERENCES [Ingresos] ([Id]),
        CONSTRAINT [FK_VentasProductos_Miembros_MiembroId] FOREIGN KEY ([MiembroId]) REFERENCES [Miembros] ([Id]),
        CONSTRAINT [FK_VentasProductos_Recibos_ReciboId] FOREIGN KEY ([ReciboId]) REFERENCES [Recibos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [DetallesComprasProductos] (
        [Id] uniqueidentifier NOT NULL,
        [CompraId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitarioCOP] decimal(18,2) NOT NULL,
        [SubtotalCOP] decimal(18,2) NOT NULL,
        [Notas] nvarchar(500) NULL,
        CONSTRAINT [PK_DetallesComprasProductos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesComprasProductos_ComprasProductos_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [ComprasProductos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesComprasProductos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [DetallesVentasProductos] (
        [Id] uniqueidentifier NOT NULL,
        [VentaId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitarioCOP] decimal(18,2) NOT NULL,
        [DescuentoCOP] decimal(18,2) NULL,
        [SubtotalCOP] decimal(18,2) NOT NULL,
        [Notas] nvarchar(500) NULL,
        CONSTRAINT [PK_DetallesVentasProductos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesVentasProductos_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesVentasProductos_VentasProductos_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [VentasProductos] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE TABLE [MovimientosInventario] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroMovimiento] nvarchar(50) NOT NULL,
        [FechaMovimiento] datetime2 NOT NULL,
        [Tipo] int NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Cantidad] int NOT NULL,
        [StockAnterior] int NOT NULL,
        [StockNuevo] int NOT NULL,
        [CompraId] uniqueidentifier NULL,
        [VentaId] uniqueidentifier NULL,
        [Motivo] nvarchar(500) NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosInventario_ComprasProductos_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [ComprasProductos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_VentasProductos_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [VentasProductos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_ComprasProductos_EgresoId] ON [ComprasProductos] ([EgresoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComprasProductos_NumeroCompra] ON [ComprasProductos] ([NumeroCompra]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_DetallesComprasProductos_CompraId] ON [DetallesComprasProductos] ([CompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_DetallesComprasProductos_ProductoId] ON [DetallesComprasProductos] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_DetallesVentasProductos_ProductoId] ON [DetallesVentasProductos] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_DetallesVentasProductos_VentaId] ON [DetallesVentasProductos] ([VentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ingresos_NumeroIngreso] ON [Ingresos] ([NumeroIngreso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_CompraId] ON [MovimientosInventario] ([CompraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MovimientosInventario_NumeroMovimiento] ON [MovimientosInventario] ([NumeroMovimiento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_ProductoId] ON [MovimientosInventario] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_VentaId] ON [MovimientosInventario] ([VentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_IngresoId] ON [VentasProductos] ([IngresoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_MiembroId] ON [VentasProductos] ([MiembroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VentasProductos_NumeroVenta] ON [VentasProductos] ([NumeroVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_ReciboId] ON [VentasProductos] ([ReciboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107030919_AgregarModuloGerenciaNegocios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251107030919_AgregarModuloGerenciaNegocios', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    ALTER TABLE [VentasProductos] ADD [ClienteId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComprasProductos]') AND [c].[name] = N'Proveedor');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [ComprasProductos] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [ComprasProductos] ALTER COLUMN [Proveedor] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    ALTER TABLE [ComprasProductos] ADD [ProveedorId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Identificacion] nvarchar(50) NULL,
        [Tipo] nvarchar(50) NOT NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(100) NULL,
        [Direccion] nvarchar(200) NULL,
        [Ciudad] nvarchar(100) NULL,
        [LimiteCredito] decimal(18,2) NULL,
        [DiasCredito] int NOT NULL,
        [PuntosFidelizacion] int NOT NULL,
        [Notas] nvarchar(500) NULL,
        [Activo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [ConciliacionesBancarias] (
        [Id] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [Mes] int NOT NULL,
        [SaldoLibros] decimal(18,2) NOT NULL,
        [SaldoBanco] decimal(18,2) NOT NULL,
        [Diferencia] decimal(18,2) NOT NULL,
        [Estado] nvarchar(50) NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [FechaConciliacion] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_ConciliacionesBancarias] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [HistorialesPrecios] (
        [Id] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [PrecioAnteriorCOP] decimal(18,2) NOT NULL,
        [PrecioNuevoCOP] decimal(18,2) NOT NULL,
        [PrecioAnteriorUSD] decimal(18,2) NULL,
        [PrecioNuevoUSD] decimal(18,2) NULL,
        [Motivo] nvarchar(500) NULL,
        [FechaCambio] datetime2 NOT NULL,
        [CambiadoPor] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_HistorialesPrecios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HistorialesPrecios_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [Notificaciones] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Tipo] nvarchar(50) NOT NULL,
        [Titulo] nvarchar(200) NOT NULL,
        [Mensaje] nvarchar(1000) NOT NULL,
        [Url] nvarchar(500) NULL,
        [Leida] bit NOT NULL,
        [FechaLeida] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notificaciones_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [Presupuestos] (
        [Id] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [Mes] int NOT NULL,
        [ConceptoId] int NOT NULL,
        [MontoPresupuestado] decimal(18,2) NOT NULL,
        [MontoEjecutado] decimal(18,2) NOT NULL,
        [Notas] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Presupuestos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Presupuestos_Conceptos_ConceptoId] FOREIGN KEY ([ConceptoId]) REFERENCES [Conceptos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Nit] nvarchar(50) NULL,
        [ContactoNombre] nvarchar(100) NULL,
        [ContactoTelefono] nvarchar(50) NULL,
        [ContactoEmail] nvarchar(100) NULL,
        [Direccion] nvarchar(200) NULL,
        [Ciudad] nvarchar(100) NULL,
        [Pais] nvarchar(50) NULL,
        [TerminosPago] nvarchar(100) NULL,
        [DiasCredito] int NOT NULL,
        [Calificacion] int NULL,
        [Notas] nvarchar(500) NULL,
        [Activo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [Cotizaciones] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroCotizacion] nvarchar(50) NOT NULL,
        [FechaCotizacion] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [ClienteId] uniqueidentifier NULL,
        [MiembroId] uniqueidentifier NULL,
        [NombreCliente] nvarchar(100) NULL,
        [EmailCliente] nvarchar(100) NULL,
        [TelefonoCliente] nvarchar(50) NULL,
        [TotalCOP] decimal(18,2) NOT NULL,
        [TotalUSD] decimal(18,2) NULL,
        [Estado] nvarchar(50) NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [VentaId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_Cotizaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cotizaciones_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]),
        CONSTRAINT [FK_Cotizaciones_Miembros_MiembroId] FOREIGN KEY ([MiembroId]) REFERENCES [Miembros] ([Id]),
        CONSTRAINT [FK_Cotizaciones_VentasProductos_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [VentasProductos] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [ItemsConciliacion] (
        [Id] uniqueidentifier NOT NULL,
        [ConciliacionId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [EsSuma] bit NOT NULL,
        [Conciliado] bit NOT NULL,
        [FechaConciliacion] datetime2 NULL,
        CONSTRAINT [PK_ItemsConciliacion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemsConciliacion_ConciliacionesBancarias_ConciliacionId] FOREIGN KEY ([ConciliacionId]) REFERENCES [ConciliacionesBancarias] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE TABLE [DetallesCotizaciones] (
        [Id] uniqueidentifier NOT NULL,
        [CotizacionId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitarioCOP] decimal(18,2) NOT NULL,
        [DescuentoCOP] decimal(18,2) NOT NULL,
        [SubtotalCOP] decimal(18,2) NOT NULL,
        [Notas] nvarchar(500) NULL,
        CONSTRAINT [PK_DetallesCotizaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesCotizaciones_Cotizaciones_CotizacionId] FOREIGN KEY ([CotizacionId]) REFERENCES [Cotizaciones] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetallesCotizaciones_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_ClienteId] ON [VentasProductos] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_ComprasProductos_ProveedorId] ON [ComprasProductos] ([ProveedorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ConciliacionesBancarias_Ano_Mes] ON [ConciliacionesBancarias] ([Ano], [Mes]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Cotizaciones_ClienteId] ON [Cotizaciones] ([ClienteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Cotizaciones_MiembroId] ON [Cotizaciones] ([MiembroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cotizaciones_NumeroCotizacion] ON [Cotizaciones] ([NumeroCotizacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Cotizaciones_VentaId] ON [Cotizaciones] ([VentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_DetallesCotizaciones_CotizacionId] ON [DetallesCotizaciones] ([CotizacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_DetallesCotizaciones_ProductoId] ON [DetallesCotizaciones] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_HistorialesPrecios_ProductoId] ON [HistorialesPrecios] ([ProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_ItemsConciliacion_ConciliacionId] ON [ItemsConciliacion] ([ConciliacionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_CreatedAt] ON [Notificaciones] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_UsuarioId_Leida] ON [Notificaciones] ([UsuarioId], [Leida]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Presupuestos_Ano_Mes_ConceptoId] ON [Presupuestos] ([Ano], [Mes], [ConceptoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    CREATE INDEX [IX_Presupuestos_ConceptoId] ON [Presupuestos] ([ConceptoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    ALTER TABLE [ComprasProductos] ADD CONSTRAINT [FK_ComprasProductos_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    ALTER TABLE [VentasProductos] ADD CONSTRAINT [FK_VentasProductos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251107092353_AgregarNuevosModulosCompletos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251107092353_AgregarNuevosModulosCompletos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251108064736_AddDescuentoSubtotalToCotizaciones'
)
BEGIN
    ALTER TABLE [Cotizaciones] ADD [DescuentoCOP] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251108064736_AddDescuentoSubtotalToCotizaciones'
)
BEGIN
    ALTER TABLE [Cotizaciones] ADD [SubtotalCOP] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251108064736_AddDescuentoSubtotalToCotizaciones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251108064736_AddDescuentoSubtotalToCotizaciones', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Recibos_FechaEmision] ON [Recibos] ([FechaEmision]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Recibos_Estado] ON [Recibos] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Recibos_FechaEmision_Estado] ON [Recibos] ([FechaEmision], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Egresos_Fecha] ON [Egresos] ([Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Egresos_Categoria] ON [Egresos] ([Categoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Egresos_Fecha_Categoria] ON [Egresos] ([Fecha], [Categoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Miembros_Estado] ON [Miembros] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_Estado] ON [VentasProductos] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_VentasProductos_FechaVenta] ON [VentasProductos] ([FechaVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_ComprasProductos_Estado] ON [ComprasProductos] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_ComprasProductos_FechaCompra] ON [ComprasProductos] ([FechaCompra]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_Tipo] ON [MovimientosInventario] ([Tipo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_FechaMovimiento] ON [MovimientosInventario] ([FechaMovimiento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251112212910_PerformanceIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251112212910_PerformanceIndexes', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE TABLE [CategoriasEgreso] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(60) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [EsGastoSocial] bit NOT NULL,
        [Activa] bit NOT NULL,
        [CuentaContableId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_CategoriasEgreso] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE TABLE [CuentasFinancieras] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Tipo] int NOT NULL,
        [Banco] nvarchar(100) NULL,
        [NumeroCuenta] nvarchar(50) NULL,
        [TitularCuenta] nvarchar(200) NULL,
        [SaldoInicial] decimal(18,2) NOT NULL,
        [SaldoActual] decimal(18,2) NOT NULL,
        [FechaApertura] datetime2 NOT NULL,
        [Activa] bit NOT NULL,
        [Observaciones] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_CuentasFinancieras] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE TABLE [FuentesIngreso] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [Activa] bit NOT NULL,
        [CuentaContableId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_FuentesIngreso] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE TABLE [MovimientosTesoreria] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroMovimiento] nvarchar(50) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Tipo] int NOT NULL,
        [CuentaFinancieraId] uniqueidentifier NOT NULL,
        [FuenteIngresoId] uniqueidentifier NULL,
        [CategoriaEgresoId] uniqueidentifier NULL,
        [Valor] decimal(18,2) NOT NULL,
        [Descripcion] nvarchar(1000) NOT NULL,
        [Medio] int NOT NULL,
        [ReferenciaTransaccion] nvarchar(200) NULL,
        [TerceroId] uniqueidentifier NULL,
        [TerceroNombre] nvarchar(200) NULL,
        [SoporteUrl] nvarchar(500) NULL,
        [Estado] int NOT NULL,
        [FechaAprobacion] datetime2 NULL,
        [UsuarioAprobacion] nvarchar(256) NULL,
        [MotivoAnulacion] nvarchar(500) NULL,
        [FechaAnulacion] datetime2 NULL,
        [UsuarioAnulacion] nvarchar(256) NULL,
        [ReciboId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        [ImportHash] nvarchar(64) NULL,
        [ImportSource] nvarchar(500) NULL,
        [ImportSheet] nvarchar(200) NULL,
        [ImportRowNumber] int NULL,
        [ImportedAtUtc] datetime2 NULL,
        [ImportHasBalanceMismatch] bit NOT NULL,
        [ImportBalanceExpected] decimal(18,2) NULL,
        [ImportBalanceFound] decimal(18,2) NULL,
        CONSTRAINT [PK_MovimientosTesoreria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosTesoreria_CategoriasEgreso_CategoriaEgresoId] FOREIGN KEY ([CategoriaEgresoId]) REFERENCES [CategoriasEgreso] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_CuentasFinancieras_CuentaFinancieraId] FOREIGN KEY ([CuentaFinancieraId]) REFERENCES [CuentasFinancieras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_FuentesIngreso_FuenteIngresoId] FOREIGN KEY ([FuenteIngresoId]) REFERENCES [FuentesIngreso] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosTesoreria_Recibos_ReciboId] FOREIGN KEY ([ReciboId]) REFERENCES [Recibos] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE TABLE [AportesMensuales] (
        [Id] uniqueidentifier NOT NULL,
        [MiembroId] uniqueidentifier NOT NULL,
        [Ano] int NOT NULL,
        [Mes] int NOT NULL,
        [ValorEsperado] decimal(18,2) NOT NULL,
        [Estado] int NOT NULL,
        [FechaPago] datetime2 NULL,
        [MovimientoTesoreriaId] uniqueidentifier NULL,
        [Observaciones] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(256) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(256) NULL,
        CONSTRAINT [PK_AportesMensuales] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AportesMensuales_Miembros_MiembroId] FOREIGN KEY ([MiembroId]) REFERENCES [Miembros] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AportesMensuales_MovimientosTesoreria_MovimientoTesoreriaId] FOREIGN KEY ([MovimientoTesoreriaId]) REFERENCES [MovimientosTesoreria] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Codigo', N'CreatedAt', N'CreatedBy', N'CuentaContableId', N'Descripcion', N'EsGastoSocial', N'Nombre') AND [object_id] = OBJECT_ID(N'[CategoriasEgreso]'))
        SET IDENTITY_INSERT [CategoriasEgreso] ON;
    EXEC(N'INSERT INTO [CategoriasEgreso] ([Id], [Activa], [Codigo], [CreatedAt], [CreatedBy], [CuentaContableId], [Descripcion], [EsGastoSocial], [Nombre])
    VALUES (''30000000-0000-0000-0000-000000000001'', CAST(1 AS bit), N''AYUDA-SOCIAL'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Proyectos de ayuda social (RTE)'', CAST(1 AS bit), N''Ayuda Social''),
    (''30000000-0000-0000-0000-000000000002'', CAST(1 AS bit), N''EVENTO-LOG'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Gastos de organización de eventos'', CAST(0 AS bit), N''Logística de Eventos''),
    (''30000000-0000-0000-0000-000000000003'', CAST(1 AS bit), N''COMPRA-MERCH'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Parches, souvenirs, jerseys'', CAST(0 AS bit), N''Compra Inventario Mercancía''),
    (''30000000-0000-0000-0000-000000000004'', CAST(1 AS bit), N''COMPRA-CLUB-CAFE'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Café, capuchino, etc.'', CAST(0 AS bit), N''Compra Insumos Casa Club - Café''),
    (''30000000-0000-0000-0000-000000000005'', CAST(1 AS bit), N''COMPRA-CLUB-CERV'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Cerveza, bebidas alcohólicas'', CAST(0 AS bit), N''Compra Insumos Casa Club - Cerveza''),
    (''30000000-0000-0000-0000-000000000006'', CAST(1 AS bit), N''COMPRA-CLUB-COMI'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Alimentos para emparedados, snacks'', CAST(0 AS bit), N''Compra Insumos Casa Club - Comida''),
    (''30000000-0000-0000-0000-000000000007'', CAST(1 AS bit), N''COMPRA-CLUB-OTROS'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Artículos moteros para venta'', CAST(0 AS bit), N''Compra Insumos Casa Club - Otros''),
    (''30000000-0000-0000-0000-000000000008'', CAST(1 AS bit), N''ADMIN-PAPEL'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Papelería, oficina'', CAST(0 AS bit), N''Gastos Administrativos - Papelería''),
    (''30000000-0000-0000-0000-000000000009'', CAST(1 AS bit), N''ADMIN-TRANSP'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Transporte, combustible'', CAST(0 AS bit), N''Gastos Administrativos - Transporte''),
    (''30000000-0000-0000-0000-000000000010'', CAST(1 AS bit), N''ADMIN-SERVICIOS'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Internet, telefonía, servicios públicos'', CAST(0 AS bit), N''Gastos Administrativos - Servicios''),
    (''30000000-0000-0000-0000-000000000011'', CAST(1 AS bit), N''MANTENIMIENTO'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Mantenimiento de infraestructura'', CAST(0 AS bit), N''Mantenimiento''),
    (''30000000-0000-0000-0000-000000000012'', CAST(1 AS bit), N''OTROS-GASTOS'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Gastos misceláneos'', CAST(0 AS bit), N''Otros Gastos'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Codigo', N'CreatedAt', N'CreatedBy', N'CuentaContableId', N'Descripcion', N'EsGastoSocial', N'Nombre') AND [object_id] = OBJECT_ID(N'[CategoriasEgreso]'))
        SET IDENTITY_INSERT [CategoriasEgreso] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Banco', N'Codigo', N'CreatedAt', N'CreatedBy', N'FechaApertura', N'Nombre', N'NumeroCuenta', N'Observaciones', N'SaldoActual', N'SaldoInicial', N'Tipo', N'TitularCuenta', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[CuentasFinancieras]'))
        SET IDENTITY_INSERT [CuentasFinancieras] ON;
    EXEC(N'INSERT INTO [CuentasFinancieras] ([Id], [Activa], [Banco], [Codigo], [CreatedAt], [CreatedBy], [FechaApertura], [Nombre], [NumeroCuenta], [Observaciones], [SaldoActual], [SaldoInicial], [Tipo], [TitularCuenta], [UpdatedAt], [UpdatedBy])
    VALUES (''11111111-1111-1111-1111-111111111111'', CAST(1 AS bit), N''Bancolombia'', N''BANCO-BCOL-001'', ''2025-01-01T00:00:00.0000000'', N''seed'', ''2025-01-01T00:00:00.0000000'', N''Bancolombia Cuenta Corriente Principal'', N''****5678'', NULL, 0.0, 0.0, 1, NULL, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Banco', N'Codigo', N'CreatedAt', N'CreatedBy', N'FechaApertura', N'Nombre', N'NumeroCuenta', N'Observaciones', N'SaldoActual', N'SaldoInicial', N'Tipo', N'TitularCuenta', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[CuentasFinancieras]'))
        SET IDENTITY_INSERT [CuentasFinancieras] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Codigo', N'CreatedAt', N'CreatedBy', N'CuentaContableId', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[FuentesIngreso]'))
        SET IDENTITY_INSERT [FuentesIngreso] ON;
    EXEC(N'INSERT INTO [FuentesIngreso] ([Id], [Activa], [Codigo], [CreatedAt], [CreatedBy], [CuentaContableId], [Descripcion], [Nombre])
    VALUES (''20000000-0000-0000-0000-000000000001'', CAST(1 AS bit), N''APORTE-MEN'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''$20.000 COP recurrente'', N''Aporte Mensual Miembro''),
    (''20000000-0000-0000-0000-000000000002'', CAST(1 AS bit), N''VENTA-MERCH'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Souvenirs, jerseys, parches, gorras'', N''Venta Mercancía''),
    (''20000000-0000-0000-0000-000000000003'', CAST(1 AS bit), N''VENTA-CLUB-ART'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Artículos moteros vendidos en casa club'', N''Venta Casa Club - Artículos Moteros''),
    (''20000000-0000-0000-0000-000000000004'', CAST(1 AS bit), N''VENTA-CLUB-CAFE'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Café vendido en casa club'', N''Venta Casa Club - Café''),
    (''20000000-0000-0000-0000-000000000005'', CAST(1 AS bit), N''VENTA-CLUB-CERV'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Cerveza vendida en casa club'', N''Venta Casa Club - Cerveza''),
    (''20000000-0000-0000-0000-000000000006'', CAST(1 AS bit), N''VENTA-CLUB-COMI'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Emparedados, snacks, comida ligera'', N''Venta Casa Club - Comida''),
    (''20000000-0000-0000-0000-000000000007'', CAST(1 AS bit), N''DONACION'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Donaciones recibidas (RTE)'', N''Donación''),
    (''20000000-0000-0000-0000-000000000008'', CAST(1 AS bit), N''EVENTO'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Ingresos por eventos organizados'', N''Evento''),
    (''20000000-0000-0000-0000-000000000009'', CAST(1 AS bit), N''RENOVACION-MEM'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Renovación anual de membresía'', N''Renovación Membresía''),
    (''20000000-0000-0000-0000-000000000010'', CAST(1 AS bit), N''OTROS'', ''2025-01-01T00:00:00.0000000'', N''seed'', NULL, N''Ingresos misceláneos'', N''Otros Ingresos'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Codigo', N'CreatedAt', N'CreatedBy', N'CuentaContableId', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[FuentesIngreso]'))
        SET IDENTITY_INSERT [FuentesIngreso] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AportesMensuales_MiembroId_Ano_Mes] ON [AportesMensuales] ([MiembroId], [Ano], [Mes]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_AportesMensuales_MovimientoTesoreriaId] ON [AportesMensuales] ([MovimientoTesoreriaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CategoriasEgreso_Codigo] ON [CategoriasEgreso] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuentasFinancieras_Codigo] ON [CuentasFinancieras] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FuentesIngreso_Codigo] ON [FuentesIngreso] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_CategoriaEgresoId] ON [MovimientosTesoreria] ([CategoriaEgresoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_CuentaFinancieraId] ON [MovimientosTesoreria] ([CuentaFinancieraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_Fecha_Tipo_CuentaFinancieraId_Estado] ON [MovimientosTesoreria] ([Fecha], [Tipo], [CuentaFinancieraId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_FuenteIngresoId] ON [MovimientosTesoreria] ([FuenteIngresoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_ImportHash] ON [MovimientosTesoreria] ([ImportHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MovimientosTesoreria_NumeroMovimiento] ON [MovimientosTesoreria] ([NumeroMovimiento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    CREATE INDEX [IX_MovimientosTesoreria_ReciboId] ON [MovimientosTesoreria] ([ReciboId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260121233036_AddAnulacionFieldsToMovimientoTesoreria', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122170435_AddImportRowHashForIdempotency'
)
BEGIN
    ALTER TABLE [Recibos] ADD [ImportRowHash] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122170435_AddImportRowHashForIdempotency'
)
BEGIN
    ALTER TABLE [Ingresos] ADD [ImportRowHash] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122170435_AddImportRowHashForIdempotency'
)
BEGIN
    ALTER TABLE [Egresos] ADD [ImportRowHash] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122170435_AddImportRowHashForIdempotency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260122170435_AddImportRowHashForIdempotency', N'8.0.0');
END;
GO

COMMIT;
GO

