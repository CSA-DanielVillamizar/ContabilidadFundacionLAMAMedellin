-- =============================================
-- Script: Insertar Conceptos Predefinidos LAMA Medellín
-- Fecha: 2025-10-23
-- Descripción: Conceptos más comunes para recibos
-- =============================================

USE LamaMedellin;
GO

-- Limpiar conceptos existentes (OPCIONAL - comentar si quieres conservar los actuales)
-- DELETE FROM ReciboItems WHERE ConceptoId IN (SELECT Id FROM Conceptos);
-- DELETE FROM Conceptos;
-- GO

-- =============================================
-- CONCEPTOS DE MEMBRESÍA
-- =============================================

INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
VALUES 
    ('MENSUALIDAD', 'Pago Mensualidad', 'Cuota mensual de membresía del capítulo', 1, 1, 1, 0, 80000.00),
    
    ('MEMB_INT_SOL_CAPITULO', 'Membresía Internacional (Tarifa de Solicitud) Socio de Capítulo', 'Tarifa inicial para socio de capítulo', 1, 0, NULL, 1, 350.00),
    ('MEMB_INT_SOL_ESPOSO', 'Membresía Internacional (Tarifa de Solicitud) Socio Esposa/o', 'Tarifa inicial para esposa/o de socio', 1, 0, NULL, 1, 100.00),
    ('MEMB_INT_SOL_ASOCIADO', 'Membresía Internacional (Tarifa de Solicitud) Miembro Asociado', 'Tarifa inicial para miembro asociado', 1, 0, NULL, 1, 50.00),
    
    ('MEMB_INT_REN_CAPITULO', 'Membresía Internacional (Renovación) Socio de Capítulo', 'Renovación anual socio de capítulo', 1, 1, 12, 1, 125.00),
    ('MEMB_INT_REN_ESPOSO', 'Membresía Internacional (Renovación) Socio Esposa/o', 'Renovación anual esposa/o de socio', 1, 1, 12, 1, 50.00),
    ('MEMB_INT_REN_ASOCIADO', 'Membresía Internacional (Renovación) Miembro Asociado', 'Renovación anual miembro asociado', 1, 1, 12, 1, 25.00);

-- =============================================
-- PARCHES Y ARTÍCULOS
-- =============================================

INSERT INTO Conceptos (Codigo, Nombre, Descripcion, EsIngreso, EsRecurrente, Periodicidad, Moneda, PrecioBase)
VALUES 
    ('PARCHE_INT', 'Membresía Internacional Parche', 'Parche de membresía internacional', 1, 0, NULL, 0, 25000.00),
    ('PARCHE_P', '"P" Parche', 'Parche letra P', 1, 0, NULL, 0, 15000.00),
    ('ARCOS', 'Arcos', 'Arcos decorativos', 1, 0, NULL, 0, 20000.00),
    ('ALAS', 'Alas', 'Alas bordadas', 1, 0, NULL, 0, 30000.00),
    ('PARCHE_PAIS', 'Parche de País', 'Parche con bandera del país', 1, 0, NULL, 0, 18000.00),
    ('PARCHE_CAPITULO', 'Parche del Capítulo', 'Parche identificación del capítulo', 1, 0, NULL, 0, 22000.00),
    ('PARCHE_ESTADO', 'Parche Estado/Provincia', 'Parche con nombre del estado o provincia', 1, 0, NULL, 0, 18000.00),
    ('LAMA_BANDERA', 'LAMA Bandera', 'Bandera oficial de LAMA', 1, 0, NULL, 0, 45000.00),
    ('DAMA_PARCHE', 'DAMA De LAMA Parche', 'Parche para DAMA de LAMA', 1, 0, NULL, 0, 20000.00),
    ('PARCHE_JUVENTUD', 'Parche de Juventud', 'Parche para miembros jóvenes', 1, 0, NULL, 0, 15000.00),
    ('LAMA_PARCHE', 'L.A.M.A. Parche', 'Parche oficial con logo L.A.M.A.', 1, 0, NULL, 0, 25000.00),
    ('ALAS_PEQUENAS', 'Alas Pequeñas', 'Alas bordadas tamaño pequeño', 1, 0, NULL, 0, 20000.00),
    ('MILLAS_PARCHE', 'Millas Parche', 'Parche de millas recorridas', 1, 0, NULL, 0, 12000.00),
    ('PARCHE_RALLY_RECIENTE', 'Parche Rally Reciente', 'Parche del rally más reciente', 1, 0, NULL, 0, 35000.00),
    ('PARCHE_RALLY_PASADO', 'Parche Rally Pasado', 'Parche de rallies anteriores', 1, 0, NULL, 0, 25000.00),
    ('PINS', 'Pins', 'Pins coleccionables LAMA', 1, 0, NULL, 0, 8000.00),
    ('HEBILLAS', 'Hebillas', 'Hebillas para cinturón', 1, 0, NULL, 0, 35000.00),
    ('CALCOMAN IA', 'Calcomanía', 'Calcomanías adhesivas LAMA', 1, 0, NULL, 0, 5000.00),
    ('CAMISETA_CAPITULO', 'Camiseta Capítulo', 'Camiseta oficial del capítulo', 1, 0, NULL, 0, 40000.00),
    ('JERSEY_EVENTO', 'Jersey Evento', 'Jersey conmemorativo de evento', 1, 0, NULL, 0, 80000.00),
    ('INSCRIPCION_EVENTO', 'Inscripción evento', 'Inscripción a evento o rally', 1, 0, NULL, 0, 150000.00);

-- =============================================
-- VERIFICACIÓN
-- =============================================

SELECT 
    Codigo,
    Nombre,
    CASE Moneda 
        WHEN 0 THEN 'COP' 
        WHEN 1 THEN 'USD' 
        ELSE 'Otra' 
    END AS Moneda,
    PrecioBase,
    CASE EsRecurrente 
        WHEN 1 THEN 'Sí' 
        ELSE 'No' 
    END AS Recurrente
FROM Conceptos
ORDER BY Codigo;

PRINT '✓ Se insertaron ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' conceptos exitosamente';
GO
