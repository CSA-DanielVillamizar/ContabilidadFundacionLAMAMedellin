-- Script para verificar que los miembros se importaron correctamente
-- con tildes, ñ y acentos preservados

USE LamaMedellin;
GO

-- Verificar total de miembros
SELECT COUNT(*) AS TotalMiembros FROM Miembros;
GO

-- Verificar miembros con caracteres especiales (tildes, ñ, acentos)
-- Debemos ver correctamente: González, Rodríguez, Pérez, José, Ramón, etc.
SELECT 
    NumeroSocio,
    NombreCompleto,
    Nombres,
    Apellidos,
    Cargo,
    Rango,
    Estado,
    FechaIngreso
FROM Miembros
ORDER BY NumeroSocio;
GO

-- Verificar específicamente algunos nombres con caracteres especiales
SELECT 
    NombreCompleto,
    CASE 
        WHEN NombreCompleto LIKE '%ñ%' THEN 'Contiene ñ ✓'
        WHEN NombreCompleto LIKE '%á%' OR NombreCompleto LIKE '%é%' OR 
             NombreCompleto LIKE '%í%' OR NombreCompleto LIKE '%ó%' OR 
             NombreCompleto LIKE '%ú%' THEN 'Contiene tildes ✓'
        ELSE 'Sin caracteres especiales'
    END AS TipoCaracter
FROM Miembros
WHERE 
    NombreCompleto LIKE '%ñ%' OR
    NombreCompleto LIKE '%á%' OR NombreCompleto LIKE '%é%' OR 
    NombreCompleto LIKE '%í%' OR NombreCompleto LIKE '%ó%' OR 
    NombreCompleto LIKE '%ú%' OR
    NombreCompleto LIKE '%Á%' OR NombreCompleto LIKE '%É%' OR 
    NombreCompleto LIKE '%Í%' OR NombreCompleto LIKE '%Ó%' OR 
    NombreCompleto LIKE '%Ú%'
ORDER BY NombreCompleto;
GO

-- Casos específicos a verificar
PRINT '=== Verificando casos específicos ===';
PRINT '';

-- Debe mostrar: "Héctor Mario González Henao"
SELECT 'Esperado: Héctor Mario González Henao' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 2;

-- Debe mostrar: "Ramón Antonio González Castaño"
SELECT 'Esperado: Ramón Antonio González Castaño' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 5;

-- Debe mostrar: "César Leonel Rodríguez Galán"
SELECT 'Esperado: César Leonel Rodríguez Galán' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 13;

-- Debe mostrar: "José Edinson Ospina Cruz"
SELECT 'Esperado: José Edinson Ospina Cruz' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 59;

-- Debe mostrar: "Carlos Andrés Pérez Areiza"
SELECT 'Esperado: Carlos Andrés Pérez Areiza' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 49;

-- Debe mostrar: "Ángela Maria Rodríguez Ochoa"
SELECT 'Esperado: Ángela Maria Rodríguez Ochoa' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 46;

-- Debe mostrar: "Milton Darío Gómez Rivera"
SELECT 'Esperado: Milton Darío Gómez Rivera' AS Verificacion, NombreCompleto AS Actual
FROM Miembros WHERE NumeroSocio = 42;

GO
