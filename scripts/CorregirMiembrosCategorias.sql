-- CorregirMiembrosCategorias.sql
-- Normaliza categorías de miembros según requerimiento del 2025-11-06
-- Reglas:
-- - Un (1) Asociado: Anderson Arlex Betancur Rua
-- - Tres (3) Prospectos: Nelson Augusto Montoya Mataute, Gustavo Adolfo Gómez Zuluaga, Jennifer Andrea Cardona Benítez
-- - Un (1) Rockets: José Julián Villamizar Araque
-- - Todos los demás: Full Color
-- - Elimina duplicados con problemas de codificación encontrados en la carga inicial (si existen)

SET NOCOUNT ON;
BEGIN TRAN;

-- Eliminar duplicados con codificación corrupta (si existen)
DELETE FROM Miembros WHERE Id IN (
    'ED680BC0-8FE5-4C6F-A6B6-604D5F9EE814', -- Gustavo Adolfo GA3mez
    '65BD9B7A-A94B-41BA-A26D-4F561BDF8C7D', -- Jennifer Andrea BenA-tez
    '9972A6B7-046B-4CCA-B3F1-A5E93E2FEFB4', -- JosAc JuliA¡n
    'D25CD982-D783-4637-A633-C6160EBF6B85', -- Nelson Augusto (dup)
    '47B6A110-9CEF-4DB8-B328-27C306CAF8CC'  -- Anderson Arlex (dup)
);

-- Forzar categoría Rockets solo para José Julián
UPDATE Miembros SET Rango = 'Rockets'
WHERE Id = 'DA187AE1-303B-4724-957F-7AF66CDAEF4B';

-- Asegurar Prospectos correctos
UPDATE Miembros SET Rango = 'Prospecto'
WHERE Id IN (
    '6C8E7D6A-4959-4988-B7BF-A514122A52AA', -- Gustavo Adolfo Gómez Zuluaga
    '9BCC5605-5B41-4D88-BB5D-01B8258E8FA4', -- Jennifer Andrea Cardona Benítez
    'BD16B088-22FB-4227-BF69-91989918D175'  -- Nelson Augusto Montoya Mataute
);

-- Asegurar el único Asociado correcto
UPDATE Miembros SET Rango = 'Asociado'
WHERE Id = '3ADEBE59-3210-44C4-AC34-BC9F3F0DCBAB'; -- Anderson Arlex Betancur Rua

-- Cambiar a Full Color quienes quedaron marcados como Rockets por error
UPDATE Miembros SET Rango = 'Full Color'
WHERE Id IN (
    'F1907C52-8660-46A0-BF07-41262155DD5A', -- Jhon David SAnchez (sin acentos)
    '636E28FA-D120-48BA-8106-A12AEB86206D'  -- Jhon David Sánchez
);

-- Reporte final
SELECT 'DISTINCT_RANGOS' AS Sec;
SELECT Rango, COUNT(*) AS Cant FROM Miembros GROUP BY Rango ORDER BY Cant DESC;
SELECT 'Prospectos' AS Sec;
SELECT NombreCompleto FROM Miembros WHERE Rango='Prospecto' ORDER BY NombreCompleto;
SELECT 'Rockets' AS Sec;
SELECT NombreCompleto FROM Miembros WHERE Rango='Rockets' ORDER BY NombreCompleto;
SELECT 'Asociado' AS Sec;
SELECT NombreCompleto FROM Miembros WHERE Rango='Asociado' ORDER BY NombreCompleto;

COMMIT TRAN;
