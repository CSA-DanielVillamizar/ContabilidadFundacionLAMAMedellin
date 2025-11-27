-- =====================================================================
-- Script de ImportaciÃ³n de Miembros desde CSV Normalizado
-- Archivo fuente: miembros_lama_medellin_clean.csv
-- Fecha: 11 de noviembre de 2025
-- Total registros: 28 miembros
-- =====================================================================

-- IMPORTANTE: 
-- 1. Este script asume que la tabla Miembros ya existe con el schema correcto
-- 2. Verificar que no haya referencias FK antes de MERGE/UPDATE
-- 3. Backup de la tabla antes de ejecutar si hay datos existentes

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE [LamaMedellin];
GO

-- Tabla temporal para staging
IF OBJECT_ID('tempdb..#MiembrosTemp') IS NOT NULL DROP TABLE #MiembrosTemp;
CREATE TABLE #MiembrosTemp (
    NombreCompleto NVARCHAR(200),
    Nombres NVARCHAR(100),
    Apellidos NVARCHAR(100),
    Cedula NVARCHAR(20),
    Email NVARCHAR(150),
    Celular NVARCHAR(20),
    Direccion NVARCHAR(300),
    MemberNumber INT,
    Cargo NVARCHAR(100),
    Rango NVARCHAR(50),
    Estado INT, -- 1 = Activo, 0 = Inactivo
    FechaIngreso DATE
);

-- Insertar datos normalizados (28 registros)
INSERT INTO #MiembrosTemp (NombreCompleto, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso)
VALUES
    (N'HÃ©ctor Mario GonzÃ¡lez Henao', N'HÃ©ctor Mario', N'GonzÃ¡lez Henao', '8336963', 'hecmarg@yahoo.com', '3104363831', 'Calle 53 # 50a-24', 2, 'SOCIO', 'Full Color', 1, '2013-05-01'),
    (N'RamÃ³n Antonio GonzÃ¡lez CastaÃ±o', N'RamÃ³n Antonio', N'GonzÃ¡lez CastaÃ±o', '15432593', 'raangoca@gmail.com', '3137672573', 'Calle 51 # 83 96', 5, 'SOCIO', 'Full Color', 1, '2013-05-01'),
    (N'CÃ©sar Leonel RodrÃ­guez GalÃ¡n', N'CÃ©sar Leonel', N'RodrÃ­guez GalÃ¡n', '74182011', 'ce-galan@hotmail.com', '3192259796', 'Carrera 99 A # 48 A-13 Apto 1812', 13, 'SOCIO', 'Full Color', 1, '2015-02-01'),
    (N'Jhon Harvey GÃ³mez PatiÃ±o', N'Jhon Harvey', N'GÃ³mez PatiÃ±o', '9528949', 'jhongo01@hotmail.com', '3006155416', 'Circular 1 # 66 B 154', 19, 'SOCIO', 'Full Color', 1, '2015-09-01'),
    (N'William Humberto JimÃ©nez Perez', N'William Humberto', N'JimÃ©nez Perez', '98496540', 'williamhjp@hotmail.com', '3017969572', 'Calle 32A # 55-33 Int. 301', 30, 'SOCIO', 'Full Color', 1, '2017-06-01'),
    (N'Carlos Alberto Araque Betancur', N'Carlos Alberto', N'Araque Betancur', '71334468', 'cocoloquisimo@gmail.com', '3206693638', 'Carrera 80 # 41 Sur-31 SADEP', 35, 'SOCIO', 'Full Color', 1, '2019-02-07'),
    (N'Milton DarÃ­o GÃ³mez Rivera', N'Milton DarÃ­o', N'GÃ³mez Rivera', '98589814', 'miltondariog@gmail.com', '3183507127', 'Carrera 55 # 58-43', 42, 'SOCIO', 'Full Color', 1, '2019-06-19'),
    (N'Carlos Mario Ceballos', N'Carlos Mario', N'Ceballos', '75049349', 'carmace7@gmail.com', '3147244972', 'Carrera 60 # 55-56', 47, 'SARGENTO DE ARMAS', 'Full Color', 1, '2020-04-30'),
    (N'Carlos AndrÃ©s PÃ©rez Areiza', N'Carlos AndrÃ©s', N'PÃ©rez Areiza', '98699136', 'carlosap@gmail.com', '3017560517', 'Carrera 47 # 19 Sur 136 In 203', 49, 'VICEPRESIDENTE', 'Full Color', 1, '2020-04-30'),
    (N'Juan Esteban SuÃ¡rez Correa', N'Juan Esteban', N'SuÃ¡rez Correa', '1095808546', 'suarezcorreaj@gmail.com', '3156160015', 'Carrera 32A # 77 Sur - 73', 50, 'SOCIO', 'Full Color', 1, '2020-04-30'),
    (N'Girlesa MarÃ­a Buitrago', N'Girlesa MarÃ­a', N'Buitrago', '51983082', 'girlesa@gmail.com', '3124739736', 'Carrera 72 # 80A -43 Apto 1110 UrbanizaciÃ³n la Toscana', 54, 'SOCIO', 'Full Color', 1, '2021-05-26'),
    (N'Jhon Emmanuel Arzuza PÃ¡ez', N'Jhon Emmanuel', N'Arzuza PÃ¡ez', '72345562', 'jhonarzuza@gmail.com', '3003876340', 'Calle 45 AA Sur # 36D-10 Apto 602, Edificio MIrador de las Antillas', 56, 'REPORTE RO - SARGENTO DE ARMAS NACIONAL', 'Full Color', 1, '2021-06-30'),
    (N'JosÃ© Edinson Ospina Cruz', N'JosÃ© Edinson', N'Ospina Cruz', '8335981', 'chattu.1964@hotmail.com', '3008542336', 'Carrera 86 # 48 BB - 19, Medellin', 59, 'GERENTE DE NEGOCIOS', 'Full Color', 1, '2021-10-03'),
    (N'Jefferson Montoya MuÃ±oz', N'Jefferson', N'Montoya MuÃ±oz', '1128406344', 'majayura2011@hotmail.com', '3508319246', 'Calle 45 # 83-12', 60, 'SOCIO', 'Full Color', 1, '2021-10-03'),
    (N'Robinson Alejandro Galvis Parra', N'Robinson Alejandro', N'Galvis Parra', '71380596', 'robin11952@hotmail.com', '3105127314', 'Carrera 86 C # 53C 41 Apto 1014', 66, 'TESORERO', 'Full Color', 1, '2022-07-01'),
    (N'Carlos Mario DÃ­az DÃ­az', N'Carlos Mario', N'DÃ­az DÃ­az', '15506596', 'carlosmario.diazdiaz@gmail.com', '3213167406', 'Carrera 46D # 48 - 04', 67, 'SECRETARIO', 'Full Color', 1, '2022-08-01'),
    (N'Juan Esteban Osorio', N'Juan Esteban', N'Osorio', '1128399797', 'Juan.osorio1429@correo.policia.gov.co', '3112710782', 'Calle 50 # 38-12 Apto 801 Barrio Boston Sector Placita de flores', 68, 'SOCIO', 'Full Color', 1, '2021-10-01'),
    (N'Carlos Julio RendÃ³n DÃ­az', N'Carlos Julio', N'RendÃ³n DÃ­az', '8162536', 'movie.cj@gmail.com', '3507757020', 'Avenida 40 Diagonal 51-110, Interior 2222, Torre 1. Unidad Nuevo Milenio. Sector Niquia', 69, 'MTO', 'Full Color', 1, '2021-10-03'),
    (N'Daniel Andrey Villamizar Araque', N'Daniel Andrey', N'Villamizar Araque', '8106002', 'dvillamizara@gmail.com', '3106328171', 'Calle 48F Sur # 40-55 Interior 1308, UrbanizaciÃ³n Puerto Luna', 84, 'PRESIDENTE', 'Full Color', 1, '2024-02-01'),
    
    -- Datos con cÃ©dulas temporales
    (N'Jhon David SÃ¡nchez', N'Jhon David', N'SÃ¡nchez', '1000000072', 'jhonda361@gmail.com', '3013424220', 'Carrera 26 CC # 38 A 10 Barrio La Milagrosa', 72, 'SOCIO', 'Full Color', 1, '2023-09-01'),
    (N'Yeferson Bairon Ãšsuga Agudelo', N'Yeferson Bairon', N'Ãšsuga Agudelo', '1000000071', 'yeferson915@hotmail.com', '3002891509', 'Calle 40 Sur # 75 - 62', 71, 'SOCIO', 'Full Color', 1, '2023-09-01'),
    
    (N'Ãngela Maria RodrÃ­guez Ochoa', N'Ãngela Maria', N'RodrÃ­guez Ochoa', '43703788', 'angelarodriguez40350@gmail.com', '3104490476', 'Calle 85 # 57-62 Itagui', 46, 'SOCIO', 'Full Color', 1, '2024-11-01'),
    (N'Jennifer Andrea Cardona BenÃ­tez', N'Jennifer Andrea', N'Cardona BenÃ­tez', '1035424338', 'tucoach21@gmail.com', '3014005382', 'Carrera 45 # 47 A 85 Interior 1303 Edificio Vicenza, Barrio FÃ¡tima', 85, 'SOCIO', 'Prospecto', 1, '2025-01-01'),
    (N'Laura Viviana Salazar Moreno', N'Laura Viviana', N'Salazar Moreno', '1090419626', 'laura.s.enf@hotmail.com', '3014307375', 'Calle 48F Sur # 40-55 Interior 1308, UrbanizaciÃ³n Puerto Luna', 86, 'SOCIO', 'Full Color', 1, '2025-06-04'),
    (N'JosÃ© JuliÃ¡n Villamizar Araque', N'JosÃ© JuliÃ¡n', N'Villamizar Araque', '8033065', 'julianvilllamizar@outlook.com', '3014873771', '', 51, 'SOCIO', 'Rockets', 1, '2025-06-04'),
    
    -- Datos con emails temporales
    (N'Gustavo Adolfo GÃ³mez Zuluaga', N'Gustavo Adolfo', N'GÃ³mez Zuluaga', '1094923731', 'gustavo.gomez.temp@fundacionlamamedellin.org', '3132672208', 'Carrera 45A # 80 Sur - 75 Apto. 1116, Sabaneta', 87, 'SOCIO', 'Prospecto', 1, '2025-10-14'),
    (N'Anderson Arlex Betancur Rua', N'Anderson Arlex', N'Betancur Rua', '1036634452', 'armigas7@gmail.com', '3194207889', 'Calle 42 Sur # 65 A - 84', 88, 'SOCIO', 'Asociado', 1, '2021-10-03'),
    (N'Nelson Augusto Montoya Mataute', N'Nelson Augusto', N'Montoya Mataute', '98472306', 'nelson.montoya.temp@fundacionlamamedellin.org', '3137100335', 'Carrera 24 A # 59 B - 103', 89, 'SOCIO', 'Prospecto', 1, '2025-10-20');

-- Verificar staging
SELECT 
    MemberNumber,
    NombreCompleto,
    Cedula,
    Email,
    Celular,
    FechaIngreso,
    CASE 
        WHEN Cedula LIKE '1000000%' THEN 'âš ï¸ CEDULA TEMPORAL'
        WHEN Email LIKE '%.temp@%' THEN 'âš ï¸ EMAIL TEMPORAL'
        ELSE 'âœ“ OK'
    END AS Validacion
FROM #MiembrosTemp
ORDER BY MemberNumber;
DECLARE @TotalRegistros INT = (SELECT COUNT(*) FROM #MiembrosTemp);


PRINT '======================================';
PRINT 'Total registros staging: ' + CAST(@TotalRegistros AS NVARCHAR(10));
PRINT 'CÃ©dulas temporales: 2 (MemberNumber 71, 72)';
PRINT 'Emails temporales: 2 (MemberNumber 87, 89)';
PRINT '======================================';

-- =====================================================================
-- OPCIÃ“N 1: MERGE (Actualizar existentes, insertar nuevos)
-- =====================================================================
-- Ejecutar MERGE automÃ¡tico para actualizar/insertar
MERGE INTO Miembros AS target
USING #MiembrosTemp AS source
ON target.MemberNumber = source.MemberNumber
WHEN MATCHED THEN
    UPDATE SET
        target.NombreCompleto = source.NombreCompleto,
        target.Nombres = source.Nombres,
        target.Apellidos = source.Apellidos,
        target.Cedula = source.Cedula,
        target.Email = source.Email,
        target.Celular = source.Celular,
        target.Direccion = source.Direccion,
        target.Cargo = source.Cargo,
        target.Rango = source.Rango,
        target.Estado = source.Estado,
        target.FechaIngreso = source.FechaIngreso
WHEN NOT MATCHED BY TARGET THEN
    INSERT (NombreCompleto, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso)
    VALUES (source.NombreCompleto, source.Nombres, source.Apellidos, source.Cedula, source.Email, source.Celular, source.Direccion, source.MemberNumber, source.Cargo, source.Rango, source.Estado, source.FechaIngreso);

PRINT 'MERGE completado exitosamente';

-- =====================================================================
-- OPCIÃ“N 2: INSERT manual selectivo (recomendado para primera vez)
-- =====================================================================
-- Verificar MemberNumbers existentes en tabla Miembros:
PRINT 'MemberNumbers existentes en tabla Miembros:';
SELECT MemberNumber, NombreCompleto FROM Miembros ORDER BY MemberNumber;

-- Insertar solo los que NO existen (ajustar WHERE segÃºn sea necesario)
/*
INSERT INTO Miembros (NombreCompleto, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso)
SELECT NombreCompleto, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso
FROM #MiembrosTemp t
WHERE NOT EXISTS (SELECT 1 FROM Miembros m WHERE m.MemberNumber = t.MemberNumber);

PRINT 'INSERT completado exitosamente';
*/

-- =====================================================================
-- VALIDACIONES POST-IMPORTACIÃ“N
-- =====================================================================
-- Verificar duplicados de cÃ©dula
SELECT Cedula, COUNT(*) AS Total
FROM Miembros
GROUP BY Cedula
HAVING COUNT(*) > 1;

-- Verificar datos temporales en BD
SELECT MemberNumber, NombreCompleto, Cedula, Email
FROM Miembros
WHERE Cedula LIKE '1000000%' OR Email LIKE '%.temp@%'
ORDER BY MemberNumber;

-- Verificar referencias FK en tabla Recibos
SELECT DISTINCT m.MemberNumber, m.NombreCompleto, COUNT(r.Id) AS TotalRecibos
FROM Miembros m
LEFT JOIN Recibos r ON r.MiembroId = m.Id
GROUP BY m.MemberNumber, m.NombreCompleto
HAVING COUNT(r.Id) > 0
ORDER BY TotalRecibos DESC;

-- Cleanup
DROP TABLE #MiembrosTemp;

PRINT '======================================';
PRINT 'âœ… Script completado';
PRINT 'NOTAS:';
PRINT '- Descomentar secciÃ³n MERGE u INSERT segÃºn necesidad';
PRINT '- Validar datos temporales antes de usar en producciÃ³n';
PRINT '- Actualizar cÃ©dulas 1000000071, 1000000072 con datos reales';
PRINT '- Actualizar emails *.temp@fundacionlamamedellin.org';
PRINT '======================================';
GO

