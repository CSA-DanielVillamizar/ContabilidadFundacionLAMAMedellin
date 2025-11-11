-- Script para importar miembros desde CSV a la base de datos LamaMedellin
USE LamaMedellin;
GO

-- Insertar miembros
INSERT INTO Miembros (Id, Nombres, Apellidos, Documento, Email, Telefono, Direccion, MemberNumber, Cargo, Rango, Estado, FechaIngreso, DatosIncompletos, CreatedAt, CreatedBy)
VALUES
(NEWID(), 'Héctor Mario', 'González Henao', '8336963', 'hecmarg@yahoo.com', '3104363831', 'Calle 53 # 50a-24', 2, 'SOCIO', 'Full Color', 0, '2013-05-01', 0, GETDATE(), 'System'),
(NEWID(), 'Ramón Antonio', 'González Castaño', '15432593', 'raangoca@gmail.com', '3137672573', 'Calle 51 # 83 96', 5, 'SOCIO', 'Full Color', 0, '2013-05-01', 0, GETDATE(), 'System'),
(NEWID(), 'César Leonel', 'Rodríguez Galán', '74182011', 'ce-galan@hotmail.com', '3192259796', 'Carrera 99 A # 48 A-13 Apto 1812', 13, 'SOCIO', 'Full Color', 0, '2015-02-01', 0, GETDATE(), 'System'),
(NEWID(), 'Jhon Harvey', 'Gómez Patiño', '9528949', 'jhongo01@hotmail.com', '3006155416', 'Circular 1 # 66 B 154', 19, 'SOCIO', 'Full Color', 0, '2015-09-01', 0, GETDATE(), 'System'),
(NEWID(), 'William Humberto', 'Jiménez Perez', '98496540', 'williamhjp@hotmail.com', '3017969572', 'Calle 32A # 55-33 Int. 301', 30, 'SOCIO', 'Full Color', 0, '2017-06-01', 0, GETDATE(), 'System'),
(NEWID(), 'Carlos Alberto', 'Araque Betancur', '71334468', 'cocoloquisimo@gmail.com', '3206693638', 'Carrera 80 # 41 Sur-31 SADEP', 35, 'SOCIO', 'Full Color', 0, '2019-02-07', 0, GETDATE(), 'System'),
(NEWID(), 'Milton Darío', 'Gómez Rivera', '98589814', 'miltondariog@gmail.com', '3183507127', 'Carrera 55 # 58-43', 42, 'SOCIO', 'Full Color', 0, '2019-06-19', 0, GETDATE(), 'System'),
(NEWID(), 'Carlos Mario', 'Ceballos', '75049349', 'carmace7@gmail.com', '3147244972', 'Carrera 60 # 55-56', 47, 'SARGENTO DE ARMAS', 'Full Color', 0, '2020-04-30', 0, GETDATE(), 'System'),
(NEWID(), 'Carlos Andrés', 'Pérez Areiza', '98699136', 'carlosap@gmail.com', '3017560517', 'Carrera 47 # 19 Sur 136 In 203', 49, 'VICEPRESIDENTE', 'Full Color', 0, '2020-04-30', 0, GETDATE(), 'System'),
(NEWID(), 'Juan Esteban', 'Suárez Correa', '1095808546', 'suarezcorreaj@gmail.com', '3156160015', 'Carrera 32A # 77 Sur - 73', 50, 'SOCIO', 'Full Color', 0, '2020-04-30', 0, GETDATE(), 'System'),
(NEWID(), 'Girlesa María', 'Buitrago', '51983082', 'girlesa@gmail.com', '3124739736', 'Carrera 72 # 80A -43 Apto 1110 Urbanización la Toscana', 54, 'SOCIO', 'Full Color', 0, '2021-05-26', 0, GETDATE(), 'System'),
(NEWID(), 'Jhon Emmanuel', 'Arzuza Páez', '72345562', 'jhonarzuza@gmail.com', '3003876340', 'Calle 45 AA Sur # 36D-10 Apto 602, Edificio MIrador de las Antillas', 56, 'REPORTE RO - SARGENTO DE ARMAS NACIONAL', 'Full Color', 0, '2021-06-30', 0, GETDATE(), 'System'),
(NEWID(), 'José Edinson', 'Ospina Cruz', '8335981', 'chattu.1964@hotmail.com', '3008542336', 'Carrera 86 # 48 BB - 19, Medellin', 59, 'GERENTE DE NEGOCIOS', 'Full Color', 0, '2021-10-03', 0, GETDATE(), 'System'),
(NEWID(), 'Jefferson', 'Montoya Muñoz', '1128406344', 'majayura2011@hotmail.com', '3508319246', 'Calle 45 # 83-12', 60, 'SOCIO', 'Full Color', 0, '2021-10-03', 0, GETDATE(), 'System'),
(NEWID(), 'Robinson Alejandro', 'Galvis Parra', '71380596', 'robin11952@hotmail.com', '3105127314', 'Carrera 86 C # 53C 41 Apto 1014', 66, 'TESORERO', 'Full Color', 0, '2022-07-01', 0, GETDATE(), 'System'),
(NEWID(), 'Carlos Mario', 'Díaz Díaz', '15506596', 'carlosmario.diazdiaz@gmail.com', '3213167406', 'Carrera 46D # 48 - 04', 67, 'SECRETARIO', 'Full Color', 0, '2022-08-01', 0, GETDATE(), 'System'),
(NEWID(), 'Juan Esteban', 'Osorio', '1128399797', 'Juan.osorio1429@correo.policia.gov.co', '3112710782', 'Calle 50 # 38-12 Apto 801 Barrio Boston Sector Placita de flores', 68, 'SOCIO', 'Full Color', 0, '2021-10-01', 0, GETDATE(), 'System'),
(NEWID(), 'Carlos Julio', 'Rendón Díaz', '8162536', 'movie.cj@gmail.com', '3507757020', 'Avenida 40 Diagonal 51-110, Interior 2222, Torre 1. Unidad Nuevo Milenio. Sector Niquia', 69, 'MTO', 'Full Color', 0, '2021-10-03', 0, GETDATE(), 'System'),
(NEWID(), 'Daniel Andrey', 'Villamizar Araque', '8106002', 'dvillamizara@gmail.com', '3106328171', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', 84, 'PRESIDENTE', 'Full Color', 0, '2024-02-01', 0, GETDATE(), 'System'),
(NEWID(), 'Jhon David', 'Sánchez', '', 'jhonda361@gmail.com', '3013424220', 'Carrera 26 CC  # 38 A 10 Barrio La Milagrosa', 72, 'SOCIO', 'Rockets', 0, '2023-09-01', 1, GETDATE(), 'System'),
(NEWID(), 'Ángela Maria', 'Rodríguez Ochoa', '43703788', 'angelarodriguez40350@gmail.com', '3104490476', 'Calle 85 # 57-62  Itagui', 46, 'SOCIO', 'Full Color', 0, '2024-11-01', 0, GETDATE(), 'System'),
(NEWID(), 'Yeferson Bairon', 'Úsuga Agudelo', '', 'yeferson915@hotmail.com', '3002891509', 'Calle 40 Sur # 75 - 62', 71, 'SOCIO', 'Full Color', 0, '2023-09-01', 1, GETDATE(), 'System'),
(NEWID(), 'Jennifer Andrea', 'Cardona Benítez', '1035424338', 'tucoach21@gmail.com', '3014005382', 'Carrera 45 # 47 A 85 Interior 1303 Edificio Vicenza, Barrio Fátima', NULL, 'SOCIO', 'Prospecto', 0, '2025-01-01', 0, GETDATE(), 'System'),
(NEWID(), 'Laura Viviana', 'Salazar Moreno', '1090419626', 'laura.s.enf@hotmail.com', '3014307375', 'Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna', NULL, 'SOCIO', 'Full Color', 0, '2024-02-01', 0, GETDATE(), 'System'),
(NEWID(), 'José Julián', 'Villamizar Araque', '8033065', 'julianvilllamizar@outlook.com', '3014873771', '', 51, 'SOCIO', 'Prospecto', 0, '2025-06-04', 1, GETDATE(), 'System'),
(NEWID(), 'Gustavo Adolfo', 'Gómez Zuluaga', '1094923731', '', '3132672208', 'Carrera 45A # 80 Sur - 75 Apto. 1116, Sabaneta', NULL, 'SOCIO', 'Prospecto', 0, '2025-10-14', 1, GETDATE(), 'System');

GO

-- Verificar inserción
SELECT COUNT(*) AS TotalMiembrosImportados FROM Miembros WHERE CreatedBy = 'System';
SELECT TOP 10 Nombres, Apellidos, Email, Cargo, Estado FROM Miembros ORDER BY FechaIngreso;
GO
