-- Script para eliminar todos los recibos de 2025 y permitir que el seed se ejecute nuevamente
-- Ejecutar este script en SQL Server antes de iniciar la aplicación

USE LamaMedellin;
GO

PRINT '=== Iniciando eliminación de recibos 2025 ===';
GO

-- Primero eliminar los items de recibos (por la foreign key)
PRINT 'Eliminando items de recibos...';
DELETE FROM ReciboItems 
WHERE ReciboId IN (SELECT Id FROM Recibos WHERE Ano = 2025);
PRINT 'Items eliminados.';
GO

-- Luego eliminar los recibos
PRINT 'Eliminando recibos...';
DELETE FROM Recibos 
WHERE Ano = 2025;
PRINT 'Recibos eliminados.';
GO

-- Verificar que se eliminaron
PRINT 'Verificando eliminación...';
SELECT COUNT(*) AS RecibosRestantes2025 FROM Recibos WHERE Ano = 2025;
-- Debe mostrar 0
GO

PRINT '=== Recibos de 2025 eliminados exitosamente ===';
PRINT 'Ahora puedes ejecutar la aplicación con: dotnet run --project src\Server\Server.csproj';
PRINT 'El seed se ejecutará nuevamente incluyendo a CARLOS ALBERTO ARAQUE BETANCUR';
GO
