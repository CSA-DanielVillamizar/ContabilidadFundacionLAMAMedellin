-- Script de validación de saldos Octubre-Noviembre 2025
USE LamaMedellin;
GO

PRINT '=================================================================';
PRINT 'VALIDACIÓN DE SALDOS OCTUBRE-NOVIEMBRE 2025';
PRINT '=================================================================';
PRINT '';

-- Saldo inicial de octubre
DECLARE @SaldoInicialOctubre DECIMAL(18,2);
SELECT @SaldoInicialOctubre = TotalCop 
FROM Recibos 
WHERE Ano = 2025 AND Serie = 'SI' AND Consecutivo = 10;

PRINT '📊 OCTUBRE 2025:';
PRINT '  Saldo inicial: $' + FORMAT(@SaldoInicialOctubre, 'N0', 'es-CO');

-- Ingresos de octubre (serie HT, consecutivos 10000-10999)
DECLARE @IngresosOctubre DECIMAL(18,2);
SELECT @IngresosOctubre = ISNULL(SUM(TotalCop), 0)
FROM Recibos 
WHERE Ano = 2025 AND Serie = 'HT' 
AND Consecutivo >= 10000 AND Consecutivo < 11000;

PRINT '  Ingresos: $' + FORMAT(@IngresosOctubre, 'N0', 'es-CO');

-- Egresos de octubre
DECLARE @EgresosOctubre DECIMAL(18,2);
SELECT @EgresosOctubre = ISNULL(SUM(ValorCop), 0)
FROM Egresos 
WHERE YEAR(Fecha) = 2025 AND MONTH(Fecha) = 10
AND CreatedBy = 'seed-historico';

PRINT '  Egresos: $' + FORMAT(@EgresosOctubre, 'N0', 'es-CO');

-- Saldo final de octubre (calculado)
DECLARE @SaldoFinalOctubre DECIMAL(18,2);
SET @SaldoFinalOctubre = @SaldoInicialOctubre + @IngresosOctubre - @EgresosOctubre;

PRINT '  Saldo final calculado: $' + FORMAT(@SaldoFinalOctubre, 'N0', 'es-CO');
PRINT '';

-- Validar contra el valor esperado
DECLARE @EsperadoOctubre DECIMAL(18,2) = 5138946;
IF @SaldoFinalOctubre = @EsperadoOctubre
    PRINT '  ✅ VALIDACIÓN OCTUBRE: CORRECTO - Saldo final coincide con $5,138,946';
ELSE
BEGIN
    PRINT '  ❌ ERROR OCTUBRE: Saldo final esperado $5,138,946 pero se calculó $' + FORMAT(@SaldoFinalOctubre, 'N0', 'es-CO');
    PRINT '     Diferencia: $' + FORMAT(ABS(@SaldoFinalOctubre - @EsperadoOctubre), 'N0', 'es-CO');
END

PRINT '';
PRINT '-----------------------------------------------------------------';
PRINT '';

-- Saldo inicial de noviembre
DECLARE @SaldoInicialNoviembre DECIMAL(18,2);
SELECT @SaldoInicialNoviembre = TotalCop 
FROM Recibos 
WHERE Ano = 2025 AND Serie = 'SI' AND Consecutivo = 11;

IF @SaldoInicialNoviembre IS NOT NULL
BEGIN
    PRINT '📊 NOVIEMBRE 2025:';
    PRINT '  Saldo inicial: $' + FORMAT(@SaldoInicialNoviembre, 'N0', 'es-CO');
    PRINT '';

    -- Validar que el saldo inicial de noviembre coincida con el final de octubre
    IF @SaldoInicialNoviembre = @SaldoFinalOctubre
        PRINT '  ✅ VALIDACIÓN ARRASTRE: CORRECTO - Saldo inicial noviembre coincide con saldo final octubre';
    ELSE
    BEGIN
        PRINT '  ❌ ERROR ARRASTRE: Saldo inicial noviembre NO coincide con saldo final octubre';
        PRINT '     Octubre final: $' + FORMAT(@SaldoFinalOctubre, 'N0', 'es-CO');
        PRINT '     Noviembre inicial: $' + FORMAT(@SaldoInicialNoviembre, 'N0', 'es-CO');
        PRINT '     Diferencia: $' + FORMAT(ABS(@SaldoInicialNoviembre - @SaldoFinalOctubre), 'N0', 'es-CO');
    END
END
ELSE
BEGIN
    PRINT '  ⚠️ ADVERTENCIA: No se encontró saldo inicial para noviembre 2025';
    PRINT '     Debe crearse con valor: $' + FORMAT(@SaldoFinalOctubre, 'N0', 'es-CO');
END

PRINT '';
PRINT '=================================================================';
PRINT 'DETALLE DE EGRESOS DE OCTUBRE 2025:';
PRINT '=================================================================';

SELECT 
    CONVERT(VARCHAR, Fecha, 103) AS Fecha,
    Categoria,
    Descripcion,
    '$' + FORMAT(ValorCop, 'N0', 'es-CO') AS Valor
FROM Egresos 
WHERE YEAR(Fecha) = 2025 AND MONTH(Fecha) = 10
AND CreatedBy = 'seed-historico'
ORDER BY Fecha, Descripcion;

DECLARE @TotalEgresosOctubre INT;
SELECT @TotalEgresosOctubre = COUNT(*) 
FROM Egresos 
WHERE YEAR(Fecha) = 2025 AND MONTH(Fecha) = 10 
AND CreatedBy = 'seed-historico';

PRINT '';
PRINT 'Total de egresos en octubre: ' + CAST(@TotalEgresosOctubre AS VARCHAR);
GO
