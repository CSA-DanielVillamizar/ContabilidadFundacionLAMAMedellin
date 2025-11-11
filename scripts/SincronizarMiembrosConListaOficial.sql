-- SincronizarMiembrosConListaOficial.sql
-- Deja exactamente los 28 miembros de la lista oficial y elimina duplicados/no listados
-- Fecha: 2025-11-06
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    -- 1) Respaldo
    IF OBJECT_ID('dbo.Miembros_Backup_20251106','U') IS NULL
    SELECT * INTO dbo.Miembros_Backup_20251106 FROM dbo.Miembros;

    -- 2) Canon: Lista oficial de 28
    IF OBJECT_ID('tempdb..#Canon') IS NOT NULL DROP TABLE #Canon;
    CREATE TABLE #Canon (
        Documento NVARCHAR(50) NULL,
        NombreCompleto NVARCHAR(200) NOT NULL,
        Rango NVARCHAR(50) NOT NULL
    );

    INSERT INTO #Canon (Documento, NombreCompleto, Rango) VALUES
    (N'8336963', N'Héctor Mario González Henao', N'Full Color'),
    (N'15432593', N'Ramón Antonio  González  Castaño', N'Full Color'),
    (N'74182011', N'César Leonel RodrÍguez Galán', N'Full Color'),
    (N'9528949', N'Jhon Harvey Gómez Patiño', N'Full Color'),
    (N'98496540', N'William Humberto Jiménez Perez', N'Full Color'),
    (N'71334468', N'Carlos Alberto Araque Betancur', N'Full Color'),
    (N'98589814', N'Milton Darío Gómez Rivera', N'Full Color'),
    (N'75049349', N'Carlos Mario  Ceballos', N'Full Color'),
    (N'98699136', N'Carlos Andrés Pérez Areiza', N'Full Color'),
    (N'1095808546', N'Juan Esteban  Suárez Correa', N'Full Color'),
    (N'51983082', N'Girlesa María Buitrago', N'Full Color'),
    (N'72345562', N'Jhon Emmanuel Arzuza Páez', N'Full Color'),
    (N'8335981', N'José Edinson  Ospina Cruz', N'Full Color'),
    (N'1128406344', N'Jefferson  Montoya Muñoz', N'Full Color'),
    (N'71380596', N'Robinson Alejandro Galvis Parra', N'Full Color'),
    (N'15506596', N'Carlos Mario Díaz Díaz', N'Full Color'),
    (N'1128399797', N'Juan Esteban  Osorio', N'Full Color'),
    (N'8162536', N'Carlos Julio  Rendón Díaz', N'Full Color'),
    (N'8106002', N'Daniel Andrey Villamizar Araque', N'Full Color'),
    (NULL, N'Jhon David Sánchez', N'Full Color'),
    (N'43703788', N'Ángela Maria Rodríguez Ochoa', N'Full Color'),
    (NULL, N'Yeferson Bairon  Úsuga Agudelo', N'Full Color'),
    (N'1035424338', N'Jennifer Andrea Cardona Benítez', N'Prospecto'),
    (N'1090419626', N'Laura Viviana Salazar Moreno', N'Full Color'),
    (N'8033065', N'José Julián Villamizar Araque', N'Rockets'),
    (N'1094923731', N'Gustavo Adolfo Gómez Zuluaga', N'Prospecto'),
    (N'1036634452', N'Anderson Arlex Betancur Rua', N'Asociado'),
    (N'98472306', N'Nelson Augusto Montoya Mataute', N'Prospecto');

    -- 3) Actualizar rangos desde canon (join por documento o por nombre, sin acentos)
    ;WITH M AS (
        SELECT m.*
        FROM dbo.Miembros m
    ),
    C AS (
        SELECT c.* FROM #Canon c
    )
    UPDATE m SET m.Rango = c.Rango
    FROM M m
    JOIN C c
      ON (c.Documento IS NOT NULL AND c.Documento <> '' AND m.Documento = c.Documento)
      OR (
           (c.Documento IS NULL OR c.Documento = '')
           AND UPPER(LTRIM(RTRIM(m.NombreCompleto))) COLLATE Latin1_General_CI_AI = UPPER(LTRIM(RTRIM(c.NombreCompleto))) COLLATE Latin1_General_CI_AI
         );

    -- 4) Eliminar todo Miembros que no esté en la lista oficial (match por documento o nombre normalizado)
    DELETE m
    FROM dbo.Miembros m
    WHERE NOT EXISTS (
        SELECT 1
        FROM #Canon c
        WHERE (c.Documento IS NOT NULL AND c.Documento <> '' AND m.Documento = c.Documento)
           OR (
                (c.Documento IS NULL OR c.Documento = '')
                AND UPPER(LTRIM(RTRIM(m.NombreCompleto))) COLLATE Latin1_General_CI_AI = UPPER(LTRIM(RTRIM(c.NombreCompleto))) COLLATE Latin1_General_CI_AI
              )
    );

    -- 5) Deduplicar: si por alguna razón quedaron múltiples filas por el mismo Documento o mismo Nombre, dejar una
    ;WITH Base AS (
        SELECT *,
               CASE WHEN ISNULL(NULLIF(Documento,''), '') <> '' THEN Documento
                    ELSE UPPER(LTRIM(RTRIM(NombreCompleto))) COLLATE Latin1_General_CI_AI END AS DedupKey
        FROM dbo.Miembros
    ), KeepOne AS (
        SELECT MIN(Id) AS IdKeep
        FROM Base
        GROUP BY DedupKey
    )
    DELETE FROM dbo.Miembros
    WHERE Id NOT IN (SELECT IdKeep FROM KeepOne);

    COMMIT TRAN;

    -- Reporte final
    SELECT 'DISTINCT_RANGOS' AS Sec;
    SELECT Rango, COUNT(*) AS Cant FROM dbo.Miembros GROUP BY Rango ORDER BY Cant DESC;
    SELECT 'TOTAL' AS Sec2, COUNT(*) AS Total FROM dbo.Miembros;
    SELECT 'LISTA' AS Sec3;
    SELECT NombreCompleto, Documento, Rango FROM dbo.Miembros ORDER BY NombreCompleto;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
