-- =============================================
-- Script: Actualizar Nombres de Conceptos LAMA
-- Fecha: 2025-10-23
-- Descripción: Ajustar nombres según especificaciones
-- =============================================

USE LamaMedellin;
GO

-- Actualizar nombres de conceptos para que coincidan con la lista solicitada

UPDATE Conceptos SET Nombre = 'Pago Mensualidad' WHERE Codigo = 'MENSUALIDAD';

UPDATE Conceptos SET 
    Codigo = 'MEMB_INT_SOL_CAPITULO',
    Nombre = 'Membresía Internacional (Tarifa de Solicitud) Socio de Capítulo' 
WHERE Codigo = 'INT_SOLICITUD';

-- Insertar conceptos faltantes específicos (con Periodicidad = 0 para no recurrentes)
IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'MEMB_INT_SOL_ESPOSO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('MEMB_INT_SOL_ESPOSO', 'Membresía Internacional (Tarifa de Solicitud) Socio Esposa/o', 'Tarifa inicial para esposa/o de socio', 1, 0, 0, 1, 100.00);

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'MEMB_INT_SOL_ASOCIADO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('MEMB_INT_SOL_ASOCIADO', 'Membresía Internacional (Tarifa de Solicitud) Miembro Asociado', 'Tarifa inicial para miembro asociado', 1, 0, 0, 1, 50.00);

UPDATE Conceptos SET Nombre = 'Membresía Internacional (Renovación) Socio de Capítulo' WHERE Codigo = 'INT_RENOVACION';

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'MEMB_INT_REN_ESPOSO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('MEMB_INT_REN_ESPOSO', 'Membresía Internacional (Renovación) Socio Esposa/o', 'Renovación anual esposa/o', 1, 1, 12, 1, 50.00);

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'MEMB_INT_REN_ASOCIADO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('MEMB_INT_REN_ASOCIADO', 'Membresía Internacional (Renovación) Miembro Asociado', 'Renovación anual miembro asociado', 1, 1, 12, 1, 25.00);

UPDATE Conceptos SET Nombre = 'Membresía Internacional Parche' WHERE Codigo = 'INT_PARCHE';
UPDATE Conceptos SET Nombre = '"P" Parche' WHERE Codigo = 'P_PARCHE';
UPDATE Conceptos SET Nombre = 'LAMA Bandera' WHERE Codigo = 'LAMA_BANDERA';
UPDATE Conceptos SET Nombre = 'DAMA De LAMA Parche' WHERE Codigo = 'DAMA_LAMA';
UPDATE Conceptos SET Nombre = 'L.A.M.A. Parche' WHERE Codigo = 'LAMA_PARCHE';
UPDATE Conceptos SET Nombre = 'Alas Pequeñas' WHERE Codigo = 'ALAS_PEQ';

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'CAMISETA_CAPITULO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('CAMISETA_CAPITULO', 'Camiseta Capítulo', 'Camiseta oficial del capítulo', 1, 0, 0, 0, 40000.00);

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'JERSEY_EVENTO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('JERSEY_EVENTO', 'Jersey Evento', 'Jersey conmemorativo de evento', 1, 0, 0, 0, 80000.00);

IF NOT EXISTS (SELECT 1 FROM Conceptos WHERE Codigo = 'INSCRIPCION_EVENTO')
    INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
    VALUES ('INSCRIPCION_EVENTO', 'Inscripción evento', 'Inscripción a evento o rally', 1, 0, 0, 0, 150000.00);

-- Verificar resultados
SELECT 
    Codigo,
    Nombre,
    CASE Moneda 
        WHEN 0 THEN 'COP' 
        WHEN 1 THEN 'USD' 
        WHEN 2 THEN 'EUR' 
        ELSE 'Otra' 
    END AS Moneda,
    FORMAT(PrecioBase, 'C', 'es-CO') AS PrecioBase,
    CASE EsRecurrente 
        WHEN 1 THEN 'Sí' 
        ELSE 'No' 
    END AS Recurrente
FROM Conceptos
WHERE Codigo LIKE 'MEMB%' 
   OR Codigo IN ('MENSUALIDAD', 'CAMISETA_CAPITULO', 'JERSEY_EVENTO', 'INSCRIPCION_EVENTO')
ORDER BY Codigo;

PRINT '✓ Conceptos actualizados e insertados correctamente';
GO
