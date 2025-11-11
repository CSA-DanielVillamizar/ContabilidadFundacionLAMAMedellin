-- Script para limpiar datos de Octubre 2025 y permitir re-seed
-- Ejecutar este script para recargar los egresos faltantes de octubre

USE LamaMedellin;
GO

PRINT 'Iniciando limpieza de datos de Octubre 2025...';

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Eliminar items de recibos de octubre 2025 (serie HT y SI consecutivo 10)
    DELETE FROM ReciboItems 
    WHERE ReciboId IN (
        SELECT Id FROM Recibos 
        WHERE Ano = 2025 
        AND (
            (Serie = 'HT' AND Consecutivo >= 10000 AND Consecutivo < 11000)
            OR (Serie = 'SI' AND Consecutivo = 10)
        )
    );
    PRINT '  ✓ Items de recibos eliminados';

    -- 2. Eliminar recibos de octubre 2025
    DELETE FROM Recibos 
    WHERE Ano = 2025 
    AND (
        (Serie = 'HT' AND Consecutivo >= 10000 AND Consecutivo < 11000)
        OR (Serie = 'SI' AND Consecutivo = 10)
    );
    PRINT '  ✓ Recibos eliminados';

    -- 3. Eliminar egresos de octubre 2025
    DELETE FROM Egresos 
    WHERE YEAR(Fecha) = 2025 AND MONTH(Fecha) = 10
    AND CreatedBy = 'seed-historico';
    PRINT '  ✓ Egresos eliminados';

    -- 4. Eliminar auditoría relacionada (opcional, solo si quieres limpiar todo)
    DELETE FROM Auditoria 
    WHERE YEAR(FechaHora) = 2025 AND MONTH(FechaHora) = 10
    AND (TipoEntidad = 'Recibo' OR TipoEntidad = 'Egreso')
    AND Usuario = 'seed-historico';
    PRINT '  ✓ Auditoría eliminada';

    COMMIT TRANSACTION;
    PRINT '✓ Limpieza completada exitosamente';
    PRINT 'Ahora puedes reiniciar el servidor para que se ejecute el seed actualizado';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✗ Error durante la limpieza:';
    PRINT ERROR_MESSAGE();
END CATCH;
GO
