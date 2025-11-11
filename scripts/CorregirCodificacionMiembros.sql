-- CorregirCodificacionMiembros.sql
-- Corrige problemas de codificación (acentos) en columnas Nombres y Apellidos de dbo.Miembros.
-- Incluye manejo de transacción y control de errores para evitar inconsistencias.
-- Requiere: permisos de UPDATE sobre dbo.Miembros.

SET NOCOUNT ON;

-- Requisitos de SET para tablas con índices filtrados y/o columnas calculadas persistidas
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @a1 INT = 0, @a2 INT = 0, @a3 INT = 0, @a4 INT = 0;
    DECLARE @n1 INT = 0, @n2 INT = 0;

    -- Apellidos: RendÃ³n DÃ­az -> Rendón Díaz
    UPDATE dbo.Miembros SET Apellidos = N'Rendón Díaz'
     WHERE LTRIM(RTRIM(Apellidos)) = N'RendÃ³n DÃ­az'
         OR LTRIM(RTRIM(Apellidos)) = N'Rend' + NCHAR(195) + NCHAR(179) + N'n D' + NCHAR(195) + NCHAR(173) + N'az';
    SET @a1 = @@ROWCOUNT;

    -- Apellidos: GÃ³mez Rivera -> Gómez Rivera
    UPDATE dbo.Miembros SET Apellidos = N'Gómez Rivera'
     WHERE LTRIM(RTRIM(Apellidos)) = N'GÃ³mez Rivera'
         OR LTRIM(RTRIM(Apellidos)) = N'G' + NCHAR(195) + NCHAR(179) + N'mez Rivera';
    SET @a2 = @@ROWCOUNT;

    -- Apellidos: SuÃ¡rez Correa -> Suárez Correa
    UPDATE dbo.Miembros SET Apellidos = N'Suárez Correa'
     WHERE LTRIM(RTRIM(Apellidos)) = N'SuÃ¡rez Correa'
         OR LTRIM(RTRIM(Apellidos)) = N'Su' + NCHAR(195) + NCHAR(161) + N'rez Correa';
    SET @a3 = @@ROWCOUNT;

    -- Apellidos: PÃ©rez Areiza -> Pérez Areiza
    UPDATE dbo.Miembros SET Apellidos = N'Pérez Areiza'
     WHERE LTRIM(RTRIM(Apellidos)) = N'PÃ©rez Areiza'
         OR LTRIM(RTRIM(Apellidos)) = N'P' + NCHAR(195) + NCHAR(169) + N'rez Areiza';
    SET @a4 = @@ROWCOUNT;

    -- Nombres: Carlos AndrÃ©s -> Carlos Andrés
    UPDATE dbo.Miembros SET Nombres = N'Carlos Andrés'
     WHERE LTRIM(RTRIM(Nombres)) = N'Carlos AndrÃ©s'
         OR LTRIM(RTRIM(Nombres)) = N'Carlos Andr' + NCHAR(195) + NCHAR(169) + N's';
    SET @n1 = @@ROWCOUNT;

    -- Nombres: RamÃ³n Antonio -> Ramón Antonio
    UPDATE dbo.Miembros SET Nombres = N'Ramón Antonio'
     WHERE LTRIM(RTRIM(Nombres)) = N'RamÃ³n Antonio'
         OR LTRIM(RTRIM(Nombres)) = N'Ram' + NCHAR(195) + NCHAR(179) + N'n Antonio';
    SET @n2 = @@ROWCOUNT;

    -----------------------------------------------------------------------
    -- Corrección global de secuencias comunes de mal codificación UTF-8 -> ANSI
    -- Aplica REPLACE en Nombres y Apellidos para casos frecuentes (Ã±, Ã³, Ã¡, Ã©, Ãº, Ã­, Â)
    -----------------------------------------------------------------------
    -- Remover carácter espurio 'Â'
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Â', N'');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Â', N'');

    -- Cambios de vocales y eñe mal interpretadas (pares UTF-8 mal decodificados)
    -- ó: C3 B3 -> 'Ã' + '³' (U+00B3)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(179), N'ó');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(179), N'ó');
    -- á: C3 A1 -> 'Ã' + '¡' (U+00A1)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(161), N'á');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(161), N'á');
    -- é: C3 A9 -> 'Ã' + '©' (U+00A9)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(169), N'é');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(169), N'é');
    -- ú: C3 BA -> 'Ã' + 'º' (U+00BA)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(186), N'ú');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(186), N'ú');
    -- í: C3 AD -> 'Ã' + '­' (U+00AD)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(173), N'í');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(173), N'í');
    -- ñ: C3 B1 -> 'Ã' + '±' (U+00B1)
    UPDATE dbo.Miembros SET Apellidos = REPLACE(Apellidos, N'Ã' + NCHAR(177), N'ñ');
    UPDATE dbo.Miembros SET Nombres   = REPLACE(Nombres,   N'Ã' + NCHAR(177), N'ñ');

    -- Correcciones específicas por Id para casos aún anómalos (según inspección)
    -- Nota: ajustamos Nombres/Apellidos exactos para evitar nuevas inconsistencias por codificación de consola
    UPDATE dbo.Miembros SET Nombres = N'Milton Darío',   Apellidos = N'Gómez Rivera'     WHERE Id = '436ED58E-D5F0-41A8-9426-475F7684D4FA';
    UPDATE dbo.Miembros SET Nombres = N'Ramón Antonio',  Apellidos = N'González Castaño'  WHERE Id = 'F8ED514E-8E52-4F49-8EB5-69BBA5C218AD';
    UPDATE dbo.Miembros SET Nombres = N'Carlos Andrés',  Apellidos = N'Pérez Areiza'      WHERE Id = '7A697C35-EBC3-4FF7-89CA-191F6B850099';
    UPDATE dbo.Miembros SET Apellidos = N'Rendón Díaz'                                   WHERE Id = 'A6B3001E-95C6-44F1-BE62-2C69E1E98336';
    UPDATE dbo.Miembros SET Apellidos = N'Suárez Correa'                                 WHERE Id = '777ADD98-840C-421C-9BF9-17CC9D70A264';

    -- Resumen de cambios
    PRINT 'Resumen de correcciones en Miembros:';
    PRINT CONCAT(' Apellidos Rendón Díaz actualizados: ', @a1);
    PRINT CONCAT(' Apellidos Gómez Rivera actualizados: ', @a2);
    PRINT CONCAT(' Apellidos Suárez Correa actualizados: ', @a3);
    PRINT CONCAT(' Apellidos Pérez Areiza actualizados: ', @a4);
    PRINT CONCAT(' Nombres Carlos Andrés actualizados: ', @n1);
    PRINT CONCAT(' Nombres Ramón Antonio actualizados: ', @n2);

    -- Verificación rápida (muestra filas afectadas tras el cambio)
    SELECT TOP (50)
        m.Id,
        m.Nombres,
        m.Apellidos
    FROM dbo.Miembros m
    WHERE m.Apellidos IN (N'Rendón Díaz', N'Gómez Rivera', N'Suárez Correa', N'Pérez Areiza', N'González Castaño')
       OR m.Nombres  IN (N'Carlos Andrés', N'Ramón Antonio', N'Milton Darío')
    ORDER BY m.Apellidos, m.Nombres;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrSev INT = ERROR_SEVERITY();
    DECLARE @ErrSta INT = ERROR_STATE();
    RAISERROR('Error corrigiendo codificación de Miembros: %s', @ErrSev, @ErrSta, @ErrMsg);
END CATCH;
