-- HardeningMiembros.sql
-- Evita duplicados y normaliza comparaciones por nombre
-- 1) Columna computada persistida con nombre normalizado (sin acentos, mayúsculas, trim)
-- 2) Índice único filtrado por Documento <> ''
-- 3) Índice único filtrado por NombreCanon cuando Documento = ''

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    -- 1) Columna computada persistida
    IF COL_LENGTH('dbo.Miembros', 'NombreCanon') IS NULL
    BEGIN
        ALTER TABLE dbo.Miembros
        ADD NombreCanon AS UPPER(LTRIM(RTRIM(NombreCompleto))) COLLATE Latin1_General_CI_AI PERSISTED;
    END

    -- 2) Índice único en Documento no vacío
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Miembros_Documento_NoVacio' AND object_id = OBJECT_ID('dbo.Miembros'))
    BEGIN
        CREATE UNIQUE INDEX UX_Miembros_Documento_NoVacio
        ON dbo.Miembros(Documento)
        WHERE Documento <> N'';
    END

    -- 3) Índice único en NombreCanon para filas sin Documento (vacío)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Miembros_NombreCanon_SinDocumento' AND object_id = OBJECT_ID('dbo.Miembros'))
    BEGIN
        CREATE UNIQUE INDEX UX_Miembros_NombreCanon_SinDocumento
        ON dbo.Miembros(NombreCanon)
        WHERE Documento = N'';
    END

    COMMIT TRAN;

    -- Reporte de salud
    SELECT 'RANGOS' AS Sec; SELECT Rango, COUNT(*) AS Cant FROM dbo.Miembros GROUP BY Rango ORDER BY Cant DESC;
    SELECT 'TOTAL' AS Sec2, COUNT(*) AS Total FROM dbo.Miembros;
    SELECT 'POSIBLES_DUP_POR_DOC' AS Sec3; SELECT Documento, COUNT(*) Cant FROM dbo.Miembros WHERE Documento<>N'' GROUP BY Documento HAVING COUNT(*)>1;
    SELECT 'POSIBLES_DUP_POR_NOMBRE_SIN_DOC' AS Sec4; SELECT NombreCanon, COUNT(*) Cant FROM dbo.Miembros WHERE Documento=N'' GROUP BY NombreCanon HAVING COUNT(*)>1;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
