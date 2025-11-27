-- Actualizar datos temporales específicos
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE [LamaMedellin];
GO

-- Actualizar cédulas temporales
UPDATE Miembros SET Cedula = '1000000071' WHERE MemberNumber = 71;
UPDATE Miembros SET Cedula = '1000000072' WHERE MemberNumber = 72;

-- Actualizar emails temporales
UPDATE Miembros SET Email = 'gustavo.gomez.temp@fundacionlamamedellin.org' WHERE MemberNumber = 87;
UPDATE Miembros SET Email = 'nelson.montoya.temp@fundacionlamamedellin.org' WHERE MemberNumber = 89;

PRINT '✅ Datos temporales actualizados correctamente';

-- Verificar
SELECT MemberNumber, NombreCompleto, Cedula, Email
FROM Miembros
WHERE MemberNumber IN (71, 72, 87, 89)
ORDER BY MemberNumber;
GO
