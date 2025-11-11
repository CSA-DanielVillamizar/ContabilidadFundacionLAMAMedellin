-- ========================================
-- ACTUALIZACIÓN DE DEUDORES - OCTUBRE 2025
-- ========================================

USE ContabilidadLAMAMedellin;
GO

PRINT '=== Iniciando actualización de deudores ===';
PRINT '';

-- ========================================
-- 1. NUEVOS MIEMBROS (Octubre 2025)
-- ========================================
PRINT '1️⃣ Actualizando fecha de ingreso nuevos miembros...';

UPDATE Miembros
SET FechaIngreso = '2025-10-01',
    UpdatedAt = GETUTCDATE(),
    UpdatedBy = 'script_actualizacion_octubre_2025'
WHERE UPPER(NombreCompleto) IN (
    'LAURA VIVIAN ASALAZAR MORENO',
    'JOSE JULIAN VILLAMIZAR ARAQUE',
    'GUSTAVO ADOLFO GÓMEZ ZULUAGA',
    'NELSON AUGUSTO MONTOYA MATAUTE'
);

PRINT '  ✓ Fechas de ingreso actualizadas';
PRINT '';

-- ========================================
-- 2. CREAR RECIBOS DE PAGO
-- ========================================
PRINT '2️⃣ Creando recibos de pago...';

DECLARE @ConceptoId INT;
DECLARE @PrecioBase DECIMAL(18,2);

SELECT @ConceptoId = Id, @PrecioBase = PrecioBase
FROM Conceptos
WHERE Codigo = 'MENSUALIDAD';

IF @ConceptoId IS NULL
BEGIN
    PRINT '❌ Error: Concepto MENSUALIDAD no encontrado';
    RETURN;
END

PRINT '  Concepto MENSUALIDAD ID: ' + CAST(@ConceptoId AS VARCHAR);
PRINT '  Precio Base: $' + CAST(@PrecioBase AS VARCHAR);
PRINT '';

-- Función helper para crear recibos
DECLARE @MiembroId UNIQUEIDENTIFIER;
DECLARE @ReciboId UNIQUEIDENTIFIER;
DECLARE @CantidadMeses INT;
DECLARE @FechaEmision DATETIME;
DECLARE @Ano INT;
DECLARE @Total DECIMAL(18,2);

-- RAMÓN ANTONIO GONZALEZ CASTAÑO (10 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'RAMÓN ANTONIO GONZALEZ CASTAÑO';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 10 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 10;
    SET @FechaEmision = '2025-10-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 10 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '10 mensualidad(es)');
    
    PRINT '  ✓ RAMÓN ANTONIO GONZALEZ CASTAÑO: 10 meses';
END

-- CARLOS ALBERTO ARAQUE BETANCUR (12 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'CARLOS ALBERTO ARAQUE BETANCUR';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 12 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 12;
    SET @FechaEmision = '2025-12-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 12 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '12 mensualidad(es)');
    
    PRINT '  ✓ CARLOS ALBERTO ARAQUE BETANCUR: 12 meses';
END

-- MILTON DARIO GOMEZ RIVERA (6 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'MILTON DARIO GOMEZ RIVERA';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 6 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 6;
    SET @FechaEmision = '2025-06-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 6 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '6 mensualidad(es)');
    
    PRINT '  ✓ MILTON DARIO GOMEZ RIVERA: 6 meses';
END

-- DANIEL ANDREY VILLAMIZAR ARAQUE (6 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'DANIEL ANDREY VILLAMIZAR ARAQUE';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 6 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 6;
    SET @FechaEmision = '2025-06-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 6 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '6 mensualidad(es)');
    
    PRINT '  ✓ DANIEL ANDREY VILLAMIZAR ARAQUE: 6 meses';
END

-- ANGELA MARIA RODRIGUEZ (9 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'ANGELA MARIA RODRIGUEZ';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 9 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 9;
    SET @FechaEmision = '2025-09-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 9 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '9 mensualidad(es)');
    
    PRINT '  ✓ ANGELA MARIA RODRIGUEZ: 9 meses';
END

-- CESAR LEONEL RODRIGUEZ GALAN (9 meses)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'CESAR LEONEL RODRIGUEZ GALAN';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 9 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 9;
    SET @FechaEmision = '2025-09-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 9 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '9 mensualidad(es)');
    
    PRINT '  ✓ CESAR LEONEL RODRIGUEZ GALAN: 9 meses';
END

-- GIRLESA MARÍA BUITRAGO (1 mes)
SELECT @MiembroId = Id FROM Miembros WHERE UPPER(NombreCompleto) = 'GIRLESA MARÍA BUITRAGO';
IF @MiembroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Recibos WHERE MiembroId = @MiembroId AND YEAR(FechaEmision) = 2025 AND MONTH(FechaEmision) = 1 AND CreatedBy = 'admin_actualizacion_octubre_2025')
BEGIN
    SET @ReciboId = NEWID();
    SET @CantidadMeses = 1;
    SET @FechaEmision = '2025-01-01';
    SET @Ano = 2025;
    SET @Total = @PrecioBase * @CantidadMeses;
    
    INSERT INTO Recibos (Id, MiembroId, FechaEmision, Ano, Estado, Observaciones, TotalCop, CreatedAt, CreatedBy, Serie, Consecutivo)
    VALUES (@ReciboId, @MiembroId, @FechaEmision, @Ano, 1, 'Actualización octubre 2025 - 1 mes(es)', @Total, GETUTCDATE(), 'admin_actualizacion_octubre_2025', 'LM', 0);
    
    INSERT INTO ReciboItems (ReciboId, ConceptoId, Cantidad, PrecioUnitarioMonedaOrigen, MonedaOrigen, SubtotalCop, Notas)
    VALUES (@ReciboId, @ConceptoId, @CantidadMeses, @PrecioBase, 1, @Total, '1 mensualidad(es)');
    
    PRINT '  ✓ GIRLESA MARÍA BUITRAGO: 1 mes';
END

PRINT '';
PRINT '✅ Actualización completada exitosamente!';
PRINT '';
PRINT 'Verificar resultados en /tesoreria/deudores';

GO
