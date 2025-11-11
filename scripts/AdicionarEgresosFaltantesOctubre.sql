-- Script para adicionar los egresos faltantes de Octubre 2025
-- y crear el saldo inicial de Noviembre 2025

USE LamaMedellin;
GO

PRINT '==================================================================';
PRINT 'ADICIÓN DE EGRESOS FALTANTES DE OCTUBRE 2025';
PRINT '==================================================================';

BEGIN TRANSACTION;

BEGIN TRY
    -- Obtener el ID del concepto FIESTA
    DECLARE @ConceptoFiesta INT;
    SELECT @ConceptoFiesta = Id FROM Conceptos WHERE Codigo = 'FIESTA';

    -- Obtener el ID del concepto RECONOCIMIENTOS
    DECLARE @ConceptoReconocimientos INT;
    SELECT @ConceptoReconocimientos = Id FROM Conceptos WHERE Codigo = 'RECONOCIMIENTOS';

    -- Obtener el ID del concepto PUBLICIDAD
    DECLARE @ConceptoPublicidad INT;
    SELECT @ConceptoPublicidad = Id FROM Conceptos WHERE Codigo = 'PUBLICIDAD';

    DECLARE @FechaOctubre DATETIME = '2025-10-31';
    DECLARE @Now DATETIME = GETUTCDATE();

    -- Egreso 1: COMPRA DE 2 POLLOS FRISBY - $133,200
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Compra de 2 pollos Frisby',
        133200,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 1: Compra de 2 pollos Frisby ($133,200)';

    -- Egreso 2: COMPRA DE GASEOSAS - $17,290
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Compra de gaseosas',
        17290,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 2: Compra de gaseosas ($17,290)';

    -- Egreso 3: SANCOCHO DONDE MILTON - $211,762
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Sancocho donde Milton',
        211762,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 3: Sancocho donde Milton ($211,762)';

    -- Egreso 4: REVUELTO - $53,144
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Revuelto',
        53144,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 4: Revuelto ($53,144)';

    -- Egreso 5: GASEOSAS Y CERVEZAS - $39,800
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Gaseosas y cervezas',
        39800,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 5: Gaseosas y cervezas ($39,800)';

    -- Egreso 6: GASEOSAS Y CERVEZAS - $63,200
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Gastos de Fiesta',
        'Histórico',
        'Histórico Octubre 2025 - Gaseosas y cervezas',
        63200,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 6: Gaseosas y cervezas ($63,200)';

    -- Egreso 7: ARREGLO FLORAL MADRE DE MARIA PAEZ - $190,000
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Reconocimientos/Recordatorios',
        'Histórico',
        'Histórico Octubre 2025 - Arreglo floral madre de Maria Paez',
        190000,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 7: Arreglo floral madre de Maria Paez ($190,000)';

    -- Egreso 8: PAGO INSCRIPCION CAMARA Y COMERCIO - $464,000
    INSERT INTO Egresos (Id, Fecha, Categoria, Proveedor, Descripcion, ValorCop, UsuarioRegistro, CreatedAt, CreatedBy)
    VALUES (
        NEWID(),
        @FechaOctubre,
        'Publicidad/Marketing',
        'Histórico',
        'Histórico Octubre 2025 - Pago inscripción Cámara de Comercio',
        464000,
        'seed-historico',
        @Now,
        'seed-historico'
    );
    PRINT '  ✓ Egreso 8: Pago inscripción Cámara de Comercio ($464,000)';

    PRINT '';
    PRINT '==================================================================';
    PRINT 'CREACIÓN DE SALDO INICIAL NOVIEMBRE 2025';
    PRINT '==================================================================';

    -- Verificar si ya existe el saldo inicial de noviembre
    IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Ano = 2025 AND Serie = 'SI' AND Consecutivo = 11)
    BEGIN
        -- Obtener el ID del concepto SALDO_INICIAL
        DECLARE @ConceptoSaldoInicial INT;
        SELECT @ConceptoSaldoInicial = Id FROM Conceptos WHERE Codigo = 'SALDO_INICIAL';

        DECLARE @NuevoReciboId UNIQUEIDENTIFIER = NEWID();
        DECLARE @MontoSaldoNoviembre DECIMAL(18,2) = 5138946; -- Saldo final de octubre
        DECLARE @FechaNoviembre DATETIME = '2025-11-01';

        -- Crear el recibo de saldo inicial
        INSERT INTO Recibos (Id, Serie, Ano, Consecutivo, FechaEmision, Estado, TotalCop, Observaciones, TerceroLibre, CreatedAt, CreatedBy)
        VALUES (
            @NuevoReciboId,
            'SI',
            2025,
            11,
            @FechaNoviembre,
            1, -- EstadoRecibo.Emitido
            @MontoSaldoNoviembre,
            'Saldo inicial Noviembre 2025 - Arrastre automático del saldo final de Octubre 2025',
            'TESORERÍA',
            @Now,
            'seed-produccion'
        );

        -- Crear el item del recibo
        INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, MonedaOrigen, PrecioUnitarioMonedaOrigen, SubtotalCop, Notas)
        VALUES (
            @NuevoReciboId,
            @ConceptoSaldoInicial,
            1,
            1, -- Moneda.COP
            @MontoSaldoNoviembre,
            @MontoSaldoNoviembre,
            'Saldo inicial Noviembre 2025 - Arrastre automático del saldo final de Octubre 2025'
        );

        PRINT '  ✓ Saldo inicial noviembre $5,138,946 registrado (arrastre de octubre)';
    END
    ELSE
    BEGIN
        PRINT '  ℹ️ Saldo inicial noviembre ya existe';
    END

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '✅ Proceso completado exitosamente';
    PRINT '';
    PRINT '==================================================================';
    PRINT 'Para validar los saldos, ejecuta:';
    PRINT 'sqlcmd -S localhost -d LamaMedellin -E -i ValidarSaldosOctubreNoviembre.sql';
    PRINT '==================================================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '❌ Error durante el proceso:';
    PRINT ERROR_MESSAGE();
END CATCH;
GO
