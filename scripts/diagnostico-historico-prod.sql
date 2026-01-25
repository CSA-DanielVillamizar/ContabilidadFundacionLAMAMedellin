-- Script de diagnóstico para verificar data histórica en producción
-- Base de datos: sqldb-tesorerialamamedellin-prod

PRINT '========================================';
PRINT 'DIAGNÓSTICO: Data Histórica Producción';
PRINT '========================================';
PRINT '';

-- 1. Verificar DB actual
PRINT '1. Base de datos actual:';
SELECT DB_NAME() AS DatabaseActual;
PRINT '';

-- 2. Listar tablas principales
PRINT '2. Tablas principales:';
SELECT name FROM sys.tables 
WHERE name IN ('MovimientosTesoreria', 'CuentasFinancieras', 'Miembros', 'CategoriasEgresos', 'FuentesIngresos', 'Recibos')
ORDER BY name;
PRINT '';

-- 3. Conteo total de movimientos
PRINT '3. Total de movimientos en MovimientosTesoreria:';
SELECT COUNT(*) AS TotalMovimientos FROM MovimientosTesoreria;
PRINT '';

-- 4. Rango de fechas
PRINT '4. Rango de fechas en MovimientosTesoreria:';
SELECT 
    MIN(Fecha) AS FechaMinima,
    MAX(Fecha) AS FechaMaxima,
    DATEDIFF(day, MIN(Fecha), MAX(Fecha)) AS DiasRango
FROM MovimientosTesoreria;
PRINT '';

-- 5. Conteo por mes 2025
PRINT '5. Movimientos por mes en 2025:';
SELECT 
    FORMAT(Fecha, 'yyyy-MM') AS Mes,
    COUNT(*) AS CantidadMovimientos,
    SUM(CASE WHEN Tipo = 1 THEN 1 ELSE 0 END) AS Ingresos,
    SUM(CASE WHEN Tipo = 2 THEN 1 ELSE 0 END) AS Egresos
FROM MovimientosTesoreria
WHERE Fecha >= '2025-01-01' AND Fecha < '2025-12-01'
GROUP BY FORMAT(Fecha, 'yyyy-MM')
ORDER BY Mes;
PRINT '';

-- 6. Conteo por estado
PRINT '6. Movimientos por estado:';
SELECT 
    Estado,
    CASE Estado
        WHEN 0 THEN 'Borrador'
        WHEN 1 THEN 'Aprobado'
        WHEN 2 THEN 'Anulado'
        ELSE 'Desconocido'
    END AS NombreEstado,
    COUNT(*) AS Cantidad
FROM MovimientosTesoreria
WHERE Fecha >= '2025-01-01' AND Fecha < '2025-12-01'
GROUP BY Estado
ORDER BY Estado;
PRINT '';

-- 7. Cuentas financieras
PRINT '7. Cuentas financieras disponibles:';
SELECT Id, Codigo, Nombre, Activo FROM CuentasFinancieras ORDER BY Codigo;
PRINT '';

-- 8. Movimientos por cuenta financiera
PRINT '8. Movimientos por cuenta financiera (2025):';
SELECT 
    cf.Codigo,
    cf.Nombre,
    COUNT(mt.Id) AS CantidadMovimientos
FROM CuentasFinancieras cf
LEFT JOIN MovimientosTesoreria mt ON cf.Id = mt.CuentaFinancieraId
    AND mt.Fecha >= '2025-01-01' AND mt.Fecha < '2025-12-01'
GROUP BY cf.Codigo, cf.Nombre
ORDER BY cf.Codigo;
PRINT '';

-- 9. Primeros 10 movimientos de 2025
PRINT '9. Primeros 10 movimientos de 2025:';
SELECT TOP 10
    NumeroMovimiento,
    Fecha,
    Tipo,
    Estado,
    Valor,
    Concepto
FROM MovimientosTesoreria
WHERE Fecha >= '2025-01-01' AND Fecha < '2025-12-01'
ORDER BY Fecha, NumeroMovimiento;
PRINT '';

-- 10. Verificar columnas ImportSource/ImportSheet
PRINT '10. Movimientos importados vs manuales:';
SELECT 
    CASE 
        WHEN ImportSource IS NOT NULL THEN 'Importado'
        ELSE 'Manual'
    END AS Origen,
    COUNT(*) AS Cantidad
FROM MovimientosTesoreria
WHERE Fecha >= '2025-01-01' AND Fecha < '2025-12-01'
GROUP BY CASE WHEN ImportSource IS NOT NULL THEN 'Importado' ELSE 'Manual' END;
PRINT '';

PRINT 'Diagnóstico completado.';
