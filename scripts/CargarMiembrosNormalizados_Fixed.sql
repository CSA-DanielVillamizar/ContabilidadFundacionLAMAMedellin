-- Script para cargar 28 miembros normalizados de LAMA Medellín
-- Generado automáticamente desde miembros_lama_medellin.normalizado.csv
USE LamaMedellin;
GO

PRINT '==================================================================';
PRINT 'CARGA DE MIEMBROS NORMALIZADOS - LAMA MEDELLÍN';
PRINT '==================================================================';
PRINT '';

BEGIN TRANSACTION;
BEGIN TRY

    -- Limpiar miembros existentes del seed anterior (opcional, comentar si no deseas esto)
    -- DELETE FROM Miembros WHERE CreatedBy = 'seed';
    
    DECLARE @Now DATETIME = GETUTCDATE();
    DECLARE @CreatedBy VARCHAR(50) = 'seed-miembros-2025';
    
    -- Miembro 1
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Héctor Mario' AND Apellidos = 'González Henao')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Héctor Mario', 'González Henao', '8336963', 'hecmarg@yahoo.com', '3104363831', 'Calle 53 # 50a-24', 2, 'SOCIO', 'Full Color', 1, '2013-05-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 1: Héctor Mario González Henao';
    END ELSE PRINT '  ℹ️ Miembro 1 ya existe';

    -- Miembro 2
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Ramón Antonio' AND Apellidos = 'González  Castaño')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Ramón Antonio', 'González  Castaño', '15432593', 'raangoca@gmail.com', '3137672573', 'Calle 51 # 83 96', 5, 'SOCIO', 'Full Color', 1, '2013-05-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 2: Ramón Antonio González  Castaño';
    END ELSE PRINT '  ℹ️ Miembro 2 ya existe';

    -- Miembro 3
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'César Leonel' AND Apellidos = 'RodrÍguez Galán')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'César Leonel', 'RodrÍguez Galán', '74182011', 'ce-galan@hotmail.com', '3192259796', 'Carrera 99 A # 48 A-13 Apto 1812', 13, 'SOCIO', 'Full Color', 1, '2015-02-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 3: César Leonel RodrÍguez Galán';
    END ELSE PRINT '  ℹ️ Miembro 3 ya existe';

    -- Miembro 4
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Jhon Harvey' AND Apellidos = 'Gómez Patiño')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Jhon Harvey', 'Gómez Patiño', '9528949', 'jhongo01@hotmail.com', '3006155416', 'Circular 1 # 66 B 154', 19, 'SOCIO', 'Full Color', 1, '2015-09-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 4: Jhon Harvey Gómez Patiño';
    END ELSE PRINT '  ℹ️ Miembro 4 ya existe';

    -- Miembro 5
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'William Humberto' AND Apellidos = 'Jiménez Perez')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'William Humberto', 'Jiménez Perez', '98496540', 'williamhjp@hotmail.com', '3017969572', 'Calle 32A # 55-33 Int. 301', 30, 'SOCIO', 'Full Color', 1, '2017-06-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 5: William Humberto Jiménez Perez';
    END ELSE PRINT '  ℹ️ Miembro 5 ya existe';

    -- Miembro 6
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Carlos Alberto' AND Apellidos = 'Araque Betancur')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Carlos Alberto', 'Araque Betancur', '71334468', 'cocoloquisimo@gmail.com', '3206693638', 'Carrera 80 # 41 Sur-31 SADEP', 35, 'SOCIO', 'Full Color', 1, '2019-02-07', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 6: Carlos Alberto Araque Betancur';
    END ELSE PRINT '  ℹ️ Miembro 6 ya existe';

    -- Miembro 7
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Milton Darío' AND Apellidos = 'Gómez Rivera')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Milton Darío', 'Gómez Rivera', '98589814', 'miltondariog@gmail.com', '3183507127', 'Carrera 55 # 58-43', 42, 'SOCIO', 'Full Color', 1, '2019-06-19', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 7: Milton Darío Gómez Rivera';
    END ELSE PRINT '  ℹ️ Miembro 7 ya existe';

    -- Miembro 8
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Carlos Mario' AND Apellidos = 'Ceballos')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Carlos Mario', 'Ceballos', '75049349', 'carmace7@gmail.com', '3147244972', 'Carrera 60 # 55-56', 47, 'SARGENTO DE ARMAS', 'Full Color', 1, '2020-04-30', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 8: Carlos Mario Ceballos';
    END ELSE PRINT '  ℹ️ Miembro 8 ya existe';

    -- Miembro 9
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Carlos Andrés' AND Apellidos = 'Pérez Areiza')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Carlos Andrés', 'Pérez Areiza', '98699136', 'carlosap@gmail.com', '3017560517', 'Carrera 47 # 19 Sur 136 In 203', 49, 'VICEPRESIDENTE', 'Full Color', 1, '2020-04-30', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 9: Carlos Andrés Pérez Areiza';
    END ELSE PRINT '  ℹ️ Miembro 9 ya existe';

    -- Miembro 10
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Juan Esteban' AND Apellidos = 'Suárez Correa')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Juan Esteban', 'Suárez Correa', '1095808546', 'suarezcorreaj@gmail.com', '3156160015', 'Carrera 32A # 77 Sur - 73', 50, 'SOCIO', 'Full Color', 1, '2020-04-30', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 10: Juan Esteban Suárez Correa';
    END ELSE PRINT '  ℹ️ Miembro 10 ya existe';

    -- Miembro 11
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Girlesa María' AND Apellidos = 'Buitrago')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Girlesa María', 'Buitrago', '51983082', 'girlesa@gmail.com', '3124739736', 'Carrera 72 # 80A -43 Apto 1110 Urbanización la Toscana', 54, 'SOCIO', 'Full Color', 1, '2021-05-26', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 11: Girlesa María Buitrago';
    END ELSE PRINT '  ℹ️ Miembro 11 ya existe';

    -- Miembro 12
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Jhon Emmanuel' AND Apellidos = 'Arzuza Páez')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Jhon Emmanuel', 'Arzuza Páez', '72345562', 'jhonarzuza@gmail.com', '3003876340', 'Calle 45 AA Sur # 36D-10 Apto 602, Edificio MIrador de las Antillas', 56, 'REPORTE RO - SARGENTO DE ARMAS NACIONAL', 'Full Color', 1, '2021-06-30', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 12: Jhon Emmanuel Arzuza Páez';
    END ELSE PRINT '  ℹ️ Miembro 12 ya existe';

    -- Miembro 13
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'José Edinson' AND Apellidos = 'Ospina Cruz')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'José Edinson', 'Ospina Cruz', '8335981', 'chattu.1964@hotmail.com', '3008542336', 'Carrera 86 # 48 BB - 19, Medellin', 59, 'GERENTE DE NEGOCIOS', 'Full Color', 1, '2021-10-03', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 13: José Edinson Ospina Cruz';
    END ELSE PRINT '  ℹ️ Miembro 13 ya existe';

    -- Miembro 14
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Jefferson' AND Apellidos = 'Montoya Muñoz')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Jefferson', 'Montoya Muñoz', '1128406344', 'majayura2011@hotmail.com', '3508319246', 'Calle 45 # 83-12', 60, 'SOCIO', 'Full Color', 1, '2021-10-03', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 14: Jefferson Montoya Muñoz';
    END ELSE PRINT '  ℹ️ Miembro 14 ya existe';

    -- Miembro 15
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Robinson Alejandro' AND Apellidos = 'Galvis Parra')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Robinson Alejandro', 'Galvis Parra', '71380596', 'robin11952@hotmail.com', '3105127314', 'Carrera 86 C # 53C 41 Apto 1014', 66, 'TESORERO', 'Full Color', 1, '2022-07-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 15: Robinson Alejandro Galvis Parra';
    END ELSE PRINT '  ℹ️ Miembro 15 ya existe';

    -- Miembro 16
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Carlos Mario' AND Apellidos = 'Díaz Díaz')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Carlos Mario', 'Díaz Díaz', '15506596', 'carlosmario.diazdiaz@gmail.com', '3213167406', 'Carrera 46D # 48 - 04', 67, 'SECRETARIO', 'Full Color', 1, '2022-08-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 16: Carlos Mario Díaz Díaz';
    END ELSE PRINT '  ℹ️ Miembro 16 ya existe';

    -- Miembro 17
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Juan Esteban' AND Apellidos = 'Osorio')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Juan Esteban', 'Osorio', '1128399797', 'juan.osorio1429@correo.policia.gov.co', '3112710782', 'Calle 50 # 38-12 Apto 801 Barrio Boston Sector Placita de flores', 68, 'SOCIO', 'Full Color', 1, '2021-10-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 17: Juan Esteban Osorio';
    END ELSE PRINT '  ℹ️ Miembro 17 ya existe';

    -- Miembro 18
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Carlos Julio' AND Apellidos = 'Rendón Díaz')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Carlos Julio', 'Rendón Díaz', '8162536', 'movie.cj@gmail.com', '3507757020', 'Avenida 40 Diagonal 51-110, Interior 2222, Torre 1. Unidad Nuevo Milenio. Sector Niquia', 69, 'MTO', 'Full Color', 1, '2021-10-03', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 18: Carlos Julio Rendón Díaz';
    END ELSE PRINT '  ℹ️ Miembro 18 ya existe';

    -- Miembro 19
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Daniel Andrey' AND Apellidos = 'Villamizar Araque')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Daniel Andrey', 'Villamizar Araque', '8106002', 'dvillamizara@gmail.com', '3106328171', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', 84, 'PRESIDENTE', 'Full Color', 1, '2024-02-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 19: Daniel Andrey Villamizar Araque';
    END ELSE PRINT '  ℹ️ Miembro 19 ya existe';

    -- Miembro 20
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Jhon David' AND Apellidos = 'Sánchez')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Jhon David', 'Sánchez', NULL, 'jhonda361@gmail.com', '3013424220', 'Carrera 26 CC  # 38 A 10 Barrio La Milagrosa', 72, 'SOCIO', 'Rockets', 1, '2023-09-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 20: Jhon David Sánchez';
    END ELSE PRINT '  ℹ️ Miembro 20 ya existe';

    -- Miembro 21
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Ángela Maria' AND Apellidos = 'Rodríguez Ochoa')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Ángela Maria', 'Rodríguez Ochoa', '43703788', 'angelarodriguez40350@gmail.com', '3104490476', 'Calle 85 # 57-62  Itagui', 46, 'SOCIO', 'Full Color', 1, '2024-11-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 21: Ángela Maria Rodríguez Ochoa';
    END ELSE PRINT '  ℹ️ Miembro 21 ya existe';

    -- Miembro 22
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Yeferson Bairon' AND Apellidos = 'Úsuga Agudelo')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Yeferson Bairon', 'Úsuga Agudelo', NULL, 'yeferson915@hotmail.com', '3002891509', 'Calle 40 Sur # 75 - 62', 71, 'SOCIO', 'Full Color', 1, '2023-09-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 22: Yeferson Bairon Úsuga Agudelo';
    END ELSE PRINT '  ℹ️ Miembro 22 ya existe';

    -- Miembro 23
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Jennifer Andrea' AND Apellidos = 'Cardona Benítez')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Jennifer Andrea', 'Cardona Benítez', '1035424338', 'tucoach21@gmail.com', '3014005382', 'Carrera 45 # 47 A 85 Interior 1303 Edificio Vicenza, Barrio Fátima', NULL, 'SOCIO', 'Prospecto', 1, '2025-01-01', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 23: Jennifer Andrea Cardona Benítez';
    END ELSE PRINT '  ℹ️ Miembro 23 ya existe';

    -- Miembro 24
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Laura Viviana' AND Apellidos = 'Salazar Moreno')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Laura Viviana', 'Salazar Moreno', '1090419626', 'laura.s.enf@hotmail.com', '3014307375', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', NULL, 'SOCIO', 'Full Color', 1, '2025-06-04', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 24: Laura Viviana Salazar Moreno';
    END ELSE PRINT '  ℹ️ Miembro 24 ya existe';

    -- Miembro 25
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'José Julián' AND Apellidos = 'Villamizar Araque')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'José Julián', 'Villamizar Araque', '8033065', 'julianvilllamizar@outlook.com', '3014873771', '', 51, 'SOCIO', 'Prospecto', 1, '2025-06-04', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 25: José Julián Villamizar Araque';
    END ELSE PRINT '  ℹ️ Miembro 25 ya existe';

    -- Miembro 26
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Gustavo Adolfo' AND Apellidos = 'Gómez Zuluaga')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Gustavo Adolfo', 'Gómez Zuluaga', '1094923731', NULL, '3132672208', 'Carrera 45A # 80 Sur - 75 Apto. 1116, Sabaneta', NULL, 'SOCIO', 'Prospecto', 1, '2025-10-14', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 26: Gustavo Adolfo Gómez Zuluaga';
    END ELSE PRINT '  ℹ️ Miembro 26 ya existe';

    -- Miembro 27
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Anderson Arlex' AND Apellidos = 'Betancur Rua')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Anderson Arlex', 'Betancur Rua', '1036634452', NULL, '3194207889', 'Calle 42 Sur # 65 A - 84', NULL, 'SOCIO', 'Asociado', 1, '2021-10-03', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 27: Anderson Arlex Betancur Rua';
    END ELSE PRINT '  ℹ️ Miembro 27 ya existe';

    -- Miembro 28
    IF NOT EXISTS (SELECT 1 FROM Miembros WHERE Nombres = 'Nelson Augusto' AND Apellidos = 'Montoya Mataute')
    BEGIN
        INSERT INTO Miembros (Id, Nombres, Apellidos, Cedula, Email, Celular, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, CreatedAt, CreatedBy)
        VALUES (NEWID(), 'Nelson Augusto', 'Montoya Mataute', '98472306', NULL, '3137100335', 'Carrera 24 A # 59 B - 103', NULL, 'SOCIO', 'Prospecto', 1, '2025-10-20', @Now, @CreatedBy);
        PRINT '  ✓ Miembro 28: Nelson Augusto Montoya Mataute';
    END ELSE PRINT '  ℹ️ Miembro 28 ya existe';

    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '✅ Carga de miembros completada exitosamente';
    PRINT '';
    
    -- Mostrar resumen
    DECLARE @TotalMiembros INT;
    DECLARE @FullColor INT;
    DECLARE @Prospectos INT;
    DECLARE @Rockets INT;
    DECLARE @Asociados INT;
    
    SELECT @TotalMiembros = COUNT(*) FROM Miembros;
    SELECT @FullColor = COUNT(*) FROM Miembros WHERE Rango = 'Full Color';
    SELECT @Prospectos = COUNT(*) FROM Miembros WHERE Rango = 'Prospecto';
    SELECT @Rockets = COUNT(*) FROM Miembros WHERE Rango = 'Rockets';
    SELECT @Asociados = COUNT(*) FROM Miembros WHERE Rango = 'Asociado';
    
    PRINT '==================================================================';
    PRINT 'RESUMEN DE MIEMBROS';
    PRINT '==================================================================';
    PRINT 'Total de miembros: ' + CAST(@TotalMiembros AS VARCHAR);
    PRINT '  - Full Color: ' + CAST(@FullColor AS VARCHAR);
    PRINT '  - Prospectos: ' + CAST(@Prospectos AS VARCHAR);
    PRINT '  - Rockets: ' + CAST(@Rockets AS VARCHAR);
    PRINT '  - Asociados: ' + CAST(@Asociados AS VARCHAR);
    PRINT '==================================================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ Error durante la carga de miembros:';
    PRINT ERROR_MESSAGE();
    PRINT '';
END CATCH;
GO
