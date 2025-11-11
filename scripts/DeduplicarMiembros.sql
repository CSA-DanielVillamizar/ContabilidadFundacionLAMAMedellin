-- DeduplicarMiembros.sql
-- Remapea Recibos al registro canónico y elimina duplicados por Documento o por Nombre (cuando documento nulo)
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    -- 1) Dedup por Documento
    ;WITH D AS (
        SELECT Documento, MIN(Id) AS IdKeep
        FROM dbo.Miembros
        WHERE Documento IS NOT NULL AND Documento<>''
        GROUP BY Documento
        HAVING COUNT(*) > 1
    ), ToFix AS (
        SELECT m.Id, m.Documento, d.IdKeep
        FROM dbo.Miembros m
        JOIN D d ON m.Documento = d.Documento
        WHERE m.Id <> d.IdKeep
    )
    UPDATE r SET r.MiembroId = f.IdKeep
    FROM dbo.Recibos r
    JOIN ToFix f ON r.MiembroId = f.Id;

    DELETE m
    FROM dbo.Miembros m
    JOIN ToFix f ON m.Id = f.Id;

    -- 2) Dedup por Nombre normalizado (cuando Documento nulo/vacío)
    ;WITH Base AS (
        SELECT Id,
               CASE WHEN ISNULL(NULLIF(Documento,''),'')<>'' THEN NULL ELSE UPPER(LTRIM(RTRIM(NombreCompleto))) COLLATE Latin1_General_CI_AI END AS NKey
        FROM dbo.Miembros
    ), D2 AS (
        SELECT NKey, MIN(Id) AS IdKeep
        FROM Base
        WHERE NKey IS NOT NULL
        GROUP BY NKey
        HAVING COUNT(*) > 1
    ), ToFix2 AS (
        SELECT b.Id, b.NKey, d2.IdKeep
        FROM Base b
        JOIN D2 d2 ON b.NKey = d2.NKey
        WHERE b.Id <> d2.IdKeep
    )
    UPDATE r SET r.MiembroId = f.IdKeep
    FROM dbo.Recibos r
    JOIN ToFix2 f ON r.MiembroId = f.Id;

    DELETE m
    FROM dbo.Miembros m
    JOIN ToFix2 f ON m.Id = f.Id;

    COMMIT TRAN;

    SELECT 'RANGOS' AS Sec;
    SELECT Rango, COUNT(*) AS Cant FROM dbo.Miembros GROUP BY Rango ORDER BY Cant DESC;
    SELECT 'TOTAL' AS Sec2, COUNT(*) AS Total FROM dbo.Miembros;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
