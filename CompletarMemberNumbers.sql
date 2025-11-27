-- Actualizar MemberNumbers faltantes y datos temporales
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE [LamaMedellin];
GO

-- Asignar MemberNumbers faltantes basándose en nombres
UPDATE Miembros SET MemberNumber = 87, Email = 'gustavo.gomez.temp@fundacionlamamedellin.org'
WHERE NombreCompleto LIKE '%Gustavo%Gomez%Zuluaga%' AND MemberNumber IS NULL;

UPDATE Miembros SET MemberNumber = 89, Email = 'nelson.montoya.temp@fundacionlamamedellin.org'
WHERE NombreCompleto LIKE '%Nelson%Montoya%Mataute%' AND MemberNumber IS NULL;

UPDATE Miembros SET MemberNumber = 85
WHERE NombreCompleto LIKE '%Jennifer%Cardona%Benitez%' AND MemberNumber IS NULL;

UPDATE Miembros SET MemberNumber = 86
WHERE NombreCompleto LIKE '%Laura%Salazar%Moreno%' AND MemberNumber IS NULL;

UPDATE Miembros SET MemberNumber = 88
WHERE NombreCompleto LIKE '%Anderson%Betancur%Rua%' AND MemberNumber IS NULL;

PRINT '✅ MemberNumbers asignados correctamente';

-- Verificar resultado final
SELECT COUNT(*) as TotalMiembros FROM Miembros;
SELECT COUNT(*) as ConMemberNumber FROM Miembros WHERE MemberNumber IS NOT NULL;
SELECT MemberNumber, LEFT(NombreCompleto, 35) as Nombre, Cedula, LEFT(Email, 45) as Email
FROM Miembros
WHERE (Cedula LIKE '1000000%' AND LEN(Cedula) = 10) OR Email LIKE '%.temp@%'
ORDER BY MemberNumber;
GO
