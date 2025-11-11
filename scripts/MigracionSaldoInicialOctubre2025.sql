-- ============================================================================
-- Script de Migración: Saldo Inicial desde Informe Manual Octubre 2025
-- ============================================================================
-- Este script registra el saldo inicial histórico de $4,718,042 (septiembre 2025)
-- y las transacciones manuales de octubre 2025 para que el sistema parta 
-- desde el saldo real de $6,311,342
-- ============================================================================

USE LamaMedellin;
GO

PRINT '🔄 Iniciando migración de saldo inicial...';

-- ============================================================================
-- 1. CREAR CONCEPTO PARA SALDO INICIAL (si no existe)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'SALDO_INICIAL')
BEGIN
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Moneda, PrecioBase, Periodicidad)
    VALUES (
        'SALDO_INICIAL',
        'Saldo Inicial - Migración',
        'Saldo inicial del sistema proveniente de registros manuales anteriores',
        1, -- Es ingreso
        0, -- No es recurrente
        0, -- Moneda.COP
        0,
        0  -- Periodicidad.None
    );
    PRINT '✅ Concepto SALDO_INICIAL creado';
END
ELSE
BEGIN
    PRINT '⏭️ Concepto SALDO_INICIAL ya existe';
END

-- ============================================================================
-- 2. CREAR CONCEPTO PARA VENTAS DE MERCANCÍA
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'VENTA_MERCHANDISING')
BEGIN
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Moneda, PrecioBase, Periodicidad)
    VALUES (
        'VENTA_MERCHANDISING',
        'Venta de Merchandising',
        'Venta de jerseys, camisetas, balaclavas y otros artículos de la fundación',
        1, -- Es ingreso
        0, -- No es recurrente
        0, -- Moneda.COP
        0,
        0  -- Periodicidad.None
    );
    PRINT '✅ Concepto VENTA_MERCHANDISING creado';
END
ELSE
BEGIN
    PRINT '⏭️ Concepto VENTA_MERCHANDISING ya existe';
END

-- ============================================================================
-- 3. REGISTRAR SALDO INICIAL (Septiembre 2025)
-- ============================================================================
-- El saldo efectivo del mes anterior (septiembre) era $4,718,042
-- Lo registramos como un "Recibo" de ajuste al 30 de septiembre 2025

DECLARE @ConceptoSaldoInicial INT = (SELECT Id FROM Conceptos WHERE Codigo = 'SALDO_INICIAL');
DECLARE @ConceptoVenta INT = (SELECT Id FROM Conceptos WHERE Codigo = 'VENTA_MERCHANDISING');

-- Verificar si ya existe el registro de saldo inicial
IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 1)
BEGIN
    DECLARE @ReciboSaldoInicial UNIQUEIDENTIFIER = NEWID();
    
    -- Insertar recibo de ajuste para saldo inicial
    INSERT INTO Recibos (
        Id, Serie, Ano, Consecutivo, FechaEmision, Estado, 
        TerceroLibre, TotalCop, Observaciones, CreatedAt, CreatedBy
    )
    VALUES (
        @ReciboSaldoInicial,
        'AJUSTE',           -- Serie especial para ajustes
        2025,
        1,                  -- Consecutivo 1
        '2025-09-30',       -- Último día de septiembre
        1,                  -- EstadoRecibo.Emitido
        'Sistema - Migración desde registros manuales',
        4718042.00,         -- $4,718,042
        'Saldo inicial proveniente de informe manual de tesorería. Representa el saldo efectivo al cierre de septiembre 2025.',
        GETDATE(),
        'Sistema'
    );

    -- Insertar item del recibo
    INSERT INTO ReciboItems (
        ReciboId, ConceptoId, Cantidad, MonedaOrigen, 
        PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas
    )
    VALUES (
        @ReciboSaldoInicial,
        @ConceptoSaldoInicial,
        1,
            0, -- Moneda.COP
        4718042.00,
        1.00,
        4718042.00,
        'Saldo efectivo del mes anterior (septiembre 2025)'
    );

    PRINT '✅ Saldo inicial de $4,718,042 registrado al 30/09/2025';
END
ELSE
BEGIN
    PRINT '⏭️ Saldo inicial ya estaba registrado';
END

-- ============================================================================
-- 4. REGISTRAR TRANSACCIONES DE OCTUBRE 2025
-- ============================================================================

-- 4.1 INGRESO: 01/Oct/2025 - Venta 30 Jersey por $50,000 en Rally Sudamericano
IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 2)
BEGIN
    DECLARE @ReciboId1 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Recibos (
        Id, Serie, Ano, Consecutivo, FechaEmision, Estado, 
        TerceroLibre, TotalCop, Observaciones, CreatedAt, CreatedBy
    )
    VALUES (
        @ReciboId1, 'AJUSTE', 2025, 2, '2025-10-01', 1,
        'Rally Sudamericano - Aniversario',
        1913300.00,
        'Venta de 30 jerseys por $50,000 c/u + $1,413,300 adicional en Rally Sudamericano (Aniversario)',
        GETDATE(), 'Sistema'
    );

    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, MonedaOrigen, PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas)
        VALUES (@ReciboId1, @ConceptoVenta, 1, 0, 1913300.00, 1.00, 1913300.00, '30 jerseys por $50,000 + otros artículos');

    PRINT '✅ Ingreso registrado: 01/Oct - Venta Rally ($1,913,300)';
END

-- 4.2 INGRESO: 02/Oct/2025 - Venta 02 Camisetas LM
IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 3)
BEGIN
    DECLARE @ReciboId2 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Recibos (
        Id, Serie, Ano, Consecutivo, FechaEmision, Estado, 
        TerceroLibre, TotalCop, Observaciones, CreatedAt, CreatedBy
    )
    VALUES (
        @ReciboId2, 'AJUSTE', 2025, 3, '2025-10-02', 1,
        'Cliente - Venta Camisetas',
        120000.00,
        'Venta de 02 camisetas LM',
        GETDATE(), 'Sistema'
    );

    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, MonedaOrigen, PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas)
        VALUES (@ReciboId2, @ConceptoVenta, 2, 0, 60000.00, 1.00, 120000.00, 'Camisetas LM');

    PRINT '✅ Ingreso registrado: 02/Oct - Camisetas ($120,000)';
END

-- 4.3 INGRESO: 03/Oct/2025 - Venta 02 Balaclavas
IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 4)
BEGIN
    DECLARE @ReciboId3 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Recibos (
        Id, Serie, Ano, Consecutivo, FechaEmision, Estado, 
        TerceroLibre, TotalCop, Observaciones, CreatedAt, CreatedBy
    )
    VALUES (
        @ReciboId3, 'AJUSTE', 2025, 4, '2025-10-03', 1,
        'Cliente - Venta Balaclavas',
        40000.00,
        'Venta de 02 balaclavas',
        GETDATE(), 'Sistema'
    );

    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, MonedaOrigen, PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas)
        VALUES (@ReciboId3, @ConceptoVenta, 2, 0, 20000.00, 1.00, 40000.00, 'Balaclavas');

    PRINT '✅ Ingreso registrado: 03/Oct - Balaclavas ($40,000)';
END

-- 4.4 EGRESO: 04/Oct/2025 - Pago Parches Apoyo Cuba
IF NOT EXISTS (SELECT 1 FROM Egresos WHERE Fecha = '2025-10-04' AND ValorCop = 600000.00)
BEGIN
    INSERT INTO Egresos (
        Id, Fecha, Categoria, Descripcion, Proveedor, 
        ValorCop, UsuarioRegistro, CreatedAt, CreatedBy
    )
    VALUES (
        NEWID(),
        '2025-10-04',
        'Merchandising',
        'Pago por fabricación de parches de apoyo a Cuba',
        'Proveedor de Parches',
        600000.00,
        'Sistema',
        GETDATE(),
        'Sistema'
    );

    PRINT '✅ Egreso registrado: 04/Oct - Parches Cuba ($600,000)';
END

-- 4.5 INGRESO: 05/Oct/2025 - Venta 02 Camisetas LM Robinson
IF NOT EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 5)
BEGIN
    DECLARE @ReciboId4 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Recibos (
        Id, Serie, Ano, Consecutivo, FechaEmision, Estado, 
        TerceroLibre, TotalCop, Observaciones, CreatedAt, CreatedBy
    )
    VALUES (
        @ReciboId4, 'AJUSTE', 2025, 5, '2025-10-05', 1,
        'Robinson - Venta Camisetas',
        120000.00,
        'Venta de 02 camisetas LM a Robinson',
        GETDATE(), 'Sistema'
    );

    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, MonedaOrigen, PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas)
        VALUES (@ReciboId4, @ConceptoVenta, 2, 0, 60000.00, 1.00, 120000.00, 'Camisetas LM');

    PRINT '✅ Ingreso registrado: 05/Oct - Camisetas Robinson ($120,000)';
END

-- ============================================================================
-- 4.6 ASEGURAR ITEM DEL SALDO INICIAL (IDEMPOTENTE)
-- ============================================================================
IF EXISTS (SELECT 1 FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 1)
BEGIN
    DECLARE @ReciboSaldoFix UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025 AND Consecutivo = 1);
    DECLARE @ConceptoSaldoFix INT = (SELECT TOP 1 Id FROM Conceptos WHERE Codigo = 'SALDO_INICIAL');

    IF @ReciboSaldoFix IS NOT NULL AND @ConceptoSaldoFix IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ReciboItems WHERE ReciboId = @ReciboSaldoFix)
    BEGIN
        INSERT INTO ReciboItems (
            ReciboId, ConceptoId, Cantidad, MonedaOrigen,
            PrecioUnitarioMonedaOrigen, TrmAplicada, SubtotalCop, Notas
        )
        VALUES (
            @ReciboSaldoFix, @ConceptoSaldoFix, 1, 0,
            4718042.00, 1.00, 4718042.00,
            'Saldo efectivo del mes anterior (septiembre 2025)'
        );

        PRINT '✅ Item del saldo inicial asegurado (AJUSTE-2025-0001)';
    END
END

-- ============================================================================
-- 5. VERIFICACIÓN FINAL
-- ============================================================================
PRINT '';
PRINT '📊 VERIFICACIÓN DE SALDOS:';
PRINT '==================================================';

-- Calcular totales
DECLARE @TotalIngresos DECIMAL(18,2);
DECLARE @TotalEgresos DECIMAL(18,2);
DECLARE @SaldoFinal DECIMAL(18,2);

-- Ingresos de octubre (excluyendo saldo inicial de septiembre)
SELECT @TotalIngresos = ISNULL(SUM(TotalCop), 0)
FROM Recibos
WHERE YEAR(FechaEmision) = 2025 
  AND MONTH(FechaEmision) = 10
  AND Estado = 1;

-- Egresos de octubre
SELECT @TotalEgresos = ISNULL(SUM(ValorCop), 0)
FROM Egresos
WHERE YEAR(Fecha) = 2025 
  AND MONTH(Fecha) = 10;

-- Saldo inicial (septiembre)
DECLARE @SaldoInicial DECIMAL(18,2) = 4718042.00;

-- Calcular saldo final
SET @SaldoFinal = @SaldoInicial + @TotalIngresos - @TotalEgresos;

PRINT '💰 Saldo Inicial (Sep 2025):  ' + FORMAT(@SaldoInicial, 'C', 'es-CO');
PRINT '📈 Total Ingresos (Oct 2025): ' + FORMAT(@TotalIngresos, 'C', 'es-CO');
PRINT '📉 Total Egresos (Oct 2025):  ' + FORMAT(@TotalEgresos, 'C', 'es-CO');
PRINT '✅ Saldo Final (Oct 2025):    ' + FORMAT(@SaldoFinal, 'C', 'es-CO');
PRINT '';

IF @SaldoFinal = 6311342.00
BEGIN
    PRINT '✅ ¡VERIFICACIÓN EXITOSA! El saldo final coincide con el informe manual: $6,311,342';
END
ELSE
BEGIN
    PRINT '⚠️ ADVERTENCIA: Diferencia detectada';
    PRINT '   Esperado: $6,311,342';
    PRINT '   Calculado: ' + FORMAT(@SaldoFinal, 'C', 'es-CO');
    PRINT '   Diferencia: ' + FORMAT(6311342.00 - @SaldoFinal, 'C', 'es-CO');
END

PRINT '';
PRINT '==================================================';
PRINT '✅ Migración completada exitosamente';
PRINT '📝 El sistema ahora parte desde el saldo histórico de octubre 2025';
PRINT '💡 Los nuevos pagos de miembros se sumarán a este saldo base';
PRINT '==================================================';
GO

-- ============================================================================
-- 6. ROLLBACK (OPCIONAL) - SOLO USAR SI SE NECESITA REVERTIR LA MIGRACIÓN
-- ============================================================================
-- NOTA: Estas sentencias están comentadas para evitar ejecuciones accidentales.
--       Descomenta y ejecútalas si necesitas deshacer la migración de octubre 2025.
--
-- BEGIN TRAN;
--     -- Eliminar ítems y recibos de la serie AJUSTE del 2025
--     DELETE RI
--     FROM ReciboItems RI
--     INNER JOIN Recibos R ON R.Id = RI.ReciboId
--     WHERE R.Serie = 'AJUSTE' AND R.Ano = 2025;
--
--     DELETE FROM Recibos WHERE Serie = 'AJUSTE' AND Ano = 2025;
--
--     -- Eliminar egreso del 04/Oct/2025 por $600,000
--     DELETE FROM Egresos WHERE Fecha = '2025-10-04' AND ValorCop = 600000.00;
--
--     -- Eliminar conceptos de migración (si no están en uso por otros registros)
--     DELETE FROM Conceptos WHERE Codigo IN ('SALDO_INICIAL', 'VENTA_MERCHANDISING')
--         AND Id NOT IN (SELECT DISTINCT ConceptoId FROM ReciboItems);
--
-- COMMIT TRAN;
