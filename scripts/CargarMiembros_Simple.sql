-- Script SIMPLIFICADO para cargar miembros (versión corregida)
USE LamaMedellin;
GO

PRINT 'Cargando 28 miembros normalizados...';

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @Now DATETIME = GETUTCDATE();
    
    -- Función helper: concatenar nombre completo
    -- Miembros donde Cedula puede ser NULL usarán 'SIN-DOCUMENTO' en Documento
    
    -- Limpiar duplicados por Nombres/Apellidos antes de insertar
    DELETE FROM Miembros WHERE CreatedBy = 'seed-miembros-2025';
    
    -- Insertar los 28 miembros
    INSERT INTO Miembros (Id, NombreCompleto, Nombres, Apellidos, Cedula, Documento, Email, Celular, Telefono, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy, DatosIncompletos)
    VALUES
    (NEWID(), 'Héctor Mario González Henao', 'Héctor Mario', 'González Henao', '8336963', '8336963', 'hecmarg@yahoo.com', '3104363831', '3104363831', 'Calle 53 # 50a-24', 2, 'SOCIO', 'Full Color', 1, '2013-05-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Ramón Antonio González  Castaño', 'Ramón Antonio', 'González  Castaño', '15432593', '15432593', 'raangoca@gmail.com', '3137672573', '3137672573', 'Calle 51 # 83 96', 5, 'SOCIO', 'Full Color', 1, '2013-05-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'César Leonel RodrÍguez Galán', 'César Leonel', 'RodrÍguez Galán', '74182011', '74182011', 'ce-galan@hotmail.com', '3192259796', '3192259796', 'Carrera 99 A # 48 A-13 Apto 1812', 13, 'SOCIO', 'Full Color', 1, '2015-02-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Jhon Harvey Gómez Patiño', 'Jhon Harvey', 'Gómez Patiño', '9528949', '9528949', 'jhongo01@hotmail.com', '3006155416', '3006155416', 'Circular 1 # 66 B 154', 19, 'SOCIO', 'Full Color', 1, '2015-09-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'William Humberto Jiménez Perez', 'William Humberto', 'Jiménez Perez', '98496540', '98496540', 'williamhjp@hotmail.com', '3017969572', '3017969572', 'Calle 32A # 55-33 Int. 301', 30, 'SOCIO', 'Full Color', 1, '2017-06-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Carlos Alberto Araque Betancur', 'Carlos Alberto', 'Araque Betancur', '71334468', '71334468', 'cocoloquisimo@gmail.com', '3206693638', '3206693638', 'Carrera 80 # 41 Sur-31 SADEP', 35, 'SOCIO', 'Full Color', 1, '2019-02-07', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Milton Darío Gómez Rivera', 'Milton Darío', 'Gómez Rivera', '98589814', '98589814', 'miltondariog@gmail.com', '3183507127', '3183507127', 'Carrera 55 # 58-43', 42, 'SOCIO', 'Full Color', 1, '2019-06-19', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Carlos Mario Ceballos', 'Carlos Mario', 'Ceballos', '75049349', '75049349', 'carmace7@gmail.com', '3147244972', '3147244972', 'Carrera 60 # 55-56', 47, 'SARGENTO DE ARMAS', 'Full Color', 1, '2020-04-30', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Carlos Andrés Pérez Areiza', 'Carlos Andrés', 'Pérez Areiza', '98699136', '98699136', 'carlosap@gmail.com', '3017560517', '3017560517', 'Carrera 47 # 19 Sur 136 In 203', 49, 'VICEPRESIDENTE', 'Full Color', 1, '2020-04-30', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Juan Esteban Suárez Correa', 'Juan Esteban', 'Suárez Correa', '1095808546', '1095808546', 'suarezcorreaj@gmail.com', '3156160015', '3156160015', 'Carrera 32A # 77 Sur - 73', 50, 'SOCIO', 'Full Color', 1, '2020-04-30', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Girlesa María Buitrago', 'Girlesa María', 'Buitrago', '51983082', '51983082', 'girlesa@gmail.com', '3124739736', '3124739736', 'Carrera 72 # 80A -43 Apto 1110 Urbanización la Toscana', 54, 'SOCIO', 'Full Color', 1, '2021-05-26', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Jhon Emmanuel Arzuza Páez', 'Jhon Emmanuel', 'Arzuza Páez', '72345562', '72345562', 'jhonarzuza@gmail.com', '3003876340', '3003876340', 'Calle 45 AA Sur # 36D-10 Apto 602, Edificio MIrador de las Antillas', 56, 'REPORTE RO - SARGENTO DE ARMAS NACIONAL', 'Full Color', 1, '2021-06-30', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'José Edinson Ospina Cruz', 'José Edinson', 'Ospina Cruz', '8335981', '8335981', 'chattu.1964@hotmail.com', '3008542336', '3008542336', 'Carrera 86 # 48 BB - 19, Medellin', 59, 'GERENTE DE NEGOCIOS', 'Full Color', 1, '2021-10-03', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Jefferson Montoya Muñoz', 'Jefferson', 'Montoya Muñoz', '1128406344', '1128406344', 'majayura2011@hotmail.com', '3508319246', '3508319246', 'Calle 45 # 83-12', 60, 'SOCIO', 'Full Color', 1, '2021-10-03', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Robinson Alejandro Galvis Parra', 'Robinson Alejandro', 'Galvis Parra', '71380596', '71380596', 'robin11952@hotmail.com', '3105127314', '3105127314', 'Carrera 86 C # 53C 41 Apto 1014', 66, 'TESORERO', 'Full Color', 1, '2022-07-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Carlos Mario Díaz Díaz', 'Carlos Mario', 'Díaz Díaz', '15506596', '15506596', 'carlosmario.diazdiaz@gmail.com', '3213167406', '3213167406', 'Carrera 46D # 48 - 04', 67, 'SECRETARIO', 'Full Color', 1, '2022-08-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Juan Esteban Osorio', 'Juan Esteban', 'Osorio', '1128399797', '1128399797', 'juan.osorio1429@correo.policia.gov.co', '3112710782', '3112710782', 'Calle 50 # 38-12 Apto 801 Barrio Boston Sector Placita de flores', 68, 'SOCIO', 'Full Color', 1, '2021-10-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Carlos Julio Rendón Díaz', 'Carlos Julio', 'Rendón Díaz', '8162536', '8162536', 'movie.cj@gmail.com', '3507757020', '3507757020', 'Avenida 40 Diagonal 51-110, Interior 2222, Torre 1. Unidad Nuevo Milenio. Sector Niquia', 69, 'MTO', 'Full Color', 1, '2021-10-03', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Daniel Andrey Villamizar Araque', 'Daniel Andrey', 'Villamizar Araque', '8106002', '8106002', 'dvillamizara@gmail.com', '3106328171', '3106328171', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', 84, 'PRESIDENTE', 'Full Color', 1, '2024-02-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Jhon David Sánchez', 'Jhon David', 'Sánchez', 'SIN-DOCUMENTO', 'SIN-DOCUMENTO', 'jhonda361@gmail.com', '3013424220', '3013424220', 'Carrera 26 CC  # 38 A 10 Barrio La Milagrosa', 72, 'SOCIO', 'Rockets', 1, '2023-09-01', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'Ángela Maria Rodríguez Ochoa', 'Ángela Maria', 'Rodríguez Ochoa', '43703788', '43703788', 'angelarodriguez40350@gmail.com', '3104490476', '3104490476', 'Calle 85 # 57-62  Itagui', 46, 'SOCIO', 'Full Color', 1, '2024-11-01', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Yeferson Bairon Úsuga Agudelo', 'Yeferson Bairon', 'Úsuga Agudelo', 'SIN-DOCUMENTO', 'SIN-DOCUMENTO', 'yeferson915@hotmail.com', '3002891509', '3002891509', 'Calle 40 Sur # 75 - 62', 71, 'SOCIO', 'Full Color', 1, '2023-09-01', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'Jennifer Andrea Cardona Benítez', 'Jennifer Andrea', 'Cardona Benítez', '1035424338', '1035424338', 'tucoach21@gmail.com', '3014005382', '3014005382', 'Carrera 45 # 47 A 85 Interior 1303 Edificio Vicenza, Barrio Fátima', NULL, 'SOCIO', 'Prospecto', 1, '2025-01-01', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'Laura Viviana Salazar Moreno', 'Laura Viviana', 'Salazar Moreno', '1090419626', '1090419626', 'laura.s.enf@hotmail.com', '3014307375', '3014307375', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', NULL, 'SOCIO', 'Full Color', 1, '2025-06-04', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'José Julián Villamizar Araque', 'José Julián', 'Villamizar Araque', '8033065', '8033065', 'julianvilllamizar@outlook.com', '3014873771', '3014873771', '', 51, 'SOCIO', 'Prospecto', 1, '2025-06-04', @Now, 'seed-miembros-2025', 0),
    (NEWID(), 'Gustavo Adolfo Gómez Zuluaga', 'Gustavo Adolfo', 'Gómez Zuluaga', '1094923731', '1094923731', 'SIN-EMAIL', '3132672208', '3132672208', 'Carrera 45A # 80 Sur - 75 Apto. 1116, Sabaneta', NULL, 'SOCIO', 'Prospecto', 1, '2025-10-14', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'Anderson Arlex Betancur Rua', 'Anderson Arlex', 'Betancur Rua', '1036634452', '1036634452', 'SIN-EMAIL', '3194207889', '3194207889', 'Calle 42 Sur # 65 A - 84', NULL, 'SOCIO', 'Asociado', 1, '2021-10-03', @Now, 'seed-miembros-2025', 1),
    (NEWID(), 'Nelson Augusto Montoya Mataute', 'Nelson Augusto', 'Montoya Mataute', '98472306', '98472306', 'SIN-EMAIL', '3137100335', '3137100335', 'Carrera 24 A # 59 B - 103', NULL, 'SOCIO', 'Prospecto', 1, '2025-10-20', @Now, 'seed-miembros-2025', 1);
    
    COMMIT TRANSACTION;
    PRINT '✅ 28 miembros cargados exitosamente';
    
    -- Resumen
    SELECT 
        COUNT(*) AS TotalMiembros,
        SUM(CASE WHEN Rango = 'Full Color' THEN 1 ELSE 0 END) AS FullColor,
        SUM(CASE WHEN Rango = 'Prospecto' THEN 1 ELSE 0 END) AS Prospectos,
        SUM(CASE WHEN Rango = 'Rockets' THEN 1 ELSE 0 END) AS Rockets,
        SUM(CASE WHEN Rango = 'Asociado' THEN 1 ELSE 0 END) AS Asociados
    FROM Miembros;
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '❌ Error: ' + ERROR_MESSAGE();
END CATCH;
GO
