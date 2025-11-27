-- =====================================================================
-- Script para Corregir Datos Incompletos de Miembros
-- Fecha: 11 de noviembre de 2025
-- =====================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE [LamaMedellin];
GO

PRINT '======================================'
PRINT 'Iniciando correcciones de datos...'
PRINT '======================================'

-- 1. Jennifer Andrea Cardona Benítez - Asignar MemberNumber 85
UPDATE Miembros 
SET MemberNumber = 85
WHERE Cedula = '1035424338' AND MemberNumber IS NULL;

PRINT '✅ Jennifer Andrea: MemberNumber 85 asignado'

-- 2. Nelson Augusto Montoya Mataute - Agregar Telefono y Direccion
UPDATE Miembros 
SET Telefono = '3137100335',
    Celular = '3137100335',
    Direccion = N'Carrera 24 A # 59 B - 103'
WHERE MemberNumber = 89;

PRINT '✅ Nelson Augusto: Teléfono y dirección actualizados'

-- 3. Yeferson Bairon Úsuga Agudelo - Agregar Documento (copia de Cedula temporal)
UPDATE Miembros 
SET Documento = '1000000071'
WHERE MemberNumber = 71 AND (Documento IS NULL OR Documento = '');

PRINT '✅ Yeferson Bairon: Documento actualizado'

-- 4. Anderson Arlex Betancur Rua - Agregar Email real
UPDATE Miembros 
SET Email = 'armigas7@gmail.com'
WHERE MemberNumber = 88;

PRINT '✅ Anderson Arlex: Email actualizado'

PRINT '======================================'
PRINT 'Correcciones completadas'
PRINT '======================================'

-- Verificar resultado final
SELECT 
    MemberNumber,
    LEFT(NombreCompleto, 35) as Nombre,
    Cedula,
    Documento,
    LEFT(Email, 35) as Email,
    Telefono,
    Celular,
    LEFT(Direccion, 35) as Direccion,
    CASE 
        WHEN MemberNumber IS NULL THEN '❌ SIN MemberNumber'
        WHEN Documento IS NULL OR Documento = '' THEN '❌ SIN Documento'
        WHEN Email IS NULL OR Email = '' OR Email = 'SIN-EMAIL' THEN '❌ SIN Email'
        WHEN (Telefono IS NULL OR Telefono = '') AND (Celular IS NULL OR Celular = '') THEN '❌ SIN Teléfono'
        WHEN Direccion IS NULL OR Direccion = '' THEN '❌ SIN Dirección'
        ELSE '✅ COMPLETO'
    END as Estado
FROM Miembros
WHERE NombreCompleto LIKE '%Jennifer%Cardona%' 
   OR NombreCompleto LIKE '%Nelson%Montoya%' 
   OR NombreCompleto LIKE '%Yeferson%Usuga%' 
   OR NombreCompleto LIKE '%Anderson%Betancur%'
ORDER BY MemberNumber;

PRINT ''
PRINT '======================================'
PRINT 'Resumen de correcciones:'
PRINT '1. Jennifer Andrea - MemberNumber: 85'
PRINT '2. Nelson Augusto - Teléfono: 3137100335, Dirección agregada'
PRINT '3. Yeferson Bairon - Documento: 1000000071 (temporal)'
PRINT '4. Anderson Arlex - Email: armigas7@gmail.com'
PRINT '======================================'
GO
