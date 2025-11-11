-- Script para corregir la collation de las columnas de texto en la tabla Miembros
-- Esto asegura que los caracteres especiales (tildes, ñ, acentos) se manejen correctamente

USE LamaMedellin;
GO

-- Primero, limpiar los datos existentes con codificación incorrecta
TRUNCATE TABLE Miembros;
GO

-- Modificar las columnas para usar collation compatible con español
ALTER TABLE Miembros
ALTER COLUMN NombreCompleto NVARCHAR(250) COLLATE Modern_Spanish_CI_AS NULL;

ALTER TABLE Miembros
ALTER COLUMN Nombres NVARCHAR(120) COLLATE Modern_Spanish_CI_AS NULL;

ALTER TABLE Miembros
ALTER COLUMN Apellidos NVARCHAR(120) COLLATE Modern_Spanish_CI_AS NULL;

ALTER TABLE Miembros
ALTER COLUMN Direccion NVARCHAR(500) COLLATE Modern_Spanish_CI_AS NULL;

ALTER TABLE Miembros
ALTER COLUMN Cargo NVARCHAR(100) COLLATE Modern_Spanish_CI_AS NULL;

ALTER TABLE Miembros
ALTER COLUMN Rango NVARCHAR(50) COLLATE Modern_Spanish_CI_AS NULL;

GO

PRINT '✅ Collation actualizada correctamente para soportar caracteres especiales en español';
GO
