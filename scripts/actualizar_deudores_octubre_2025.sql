-- Script de actualización de estado de mensualidades - Octubre 2025
-- Fecha de ejecución: 27 de octubre de 2025
-- Este script NO modifica FechaIngreso, ya que representa la fecha de entrada al capítulo
-- La lógica de cálculo de deudas debe estar en el servicio DeudoresService

USE ContabilidadLAMAMedellin;
GO

-- =====================================================
-- NOTA IMPORTANTE:
-- =====================================================
-- El campo FechaIngreso NO se debe modificar para reflejar pagos
-- FechaIngreso representa la fecha de ingreso al capítulo
-- 
-- Para el cálculo de deudas de mensualidad, el sistema debe:
-- 1. Consultar los recibos de ingreso del miembro
-- 2. Identificar el último mes pagado según los recibos registrados
-- 3. Calcular la deuda desde ese mes hasta el mes actual
--
-- Este script documenta el estado ESPERADO de pagos,
-- pero la actualización real debe hacerse registrando RECIBOS
-- =====================================================

-- Resumen de estados de pago (octubre 2025):
-- 
-- MIEMBROS AL DÍA O CON DEUDA MENOR:
-- - RAMÓN ANTONIO GONZALEZ CASTAÑO: Al día (octubre 2025)
-- - CARLOS ALBERTO ARAQUE BETANCUR: Al día + 2 meses (diciembre 2025)
-- 
-- MIEMBROS CON DEUDA MODERADA (1-5 meses):
-- - MILTON DARIO GOMEZ RIVERA: Debe 4 meses (julio-octubre)
-- - DANIEL ANDREY VILLAMIZAR ARAQUE: Debe 4 meses (julio-octubre)
-- - ANGELA MARIA RODRIGUEZ: Debe 1 mes (octubre)
-- - CESAR LEONEL RODRIGUEZ GALAN: Debe 1 mes (octubre)
-- 
-- MIEMBROS CON DEUDA ALTA (6+ meses):
-- - GIRLESA MARÍA BUITRAGO: Debe 9 meses (febrero-octubre)
-- 
-- NUEVOS MIEMBROS (ingresaron octubre 2025 - NO DEBEN):
-- - LAURA VIVIAN ASALAZAR MORENO
-- - JOSE JULIAN VILLAMIZAR ARAQUE
-- - GUSTAVO ADOLFO GÓMEZ ZULUAGA
-- - Nelson Augusto Montoya Mataute
--
-- MIEMBROS QUE DEBEN TODO EL AÑO (enero-octubre = 10 meses):
-- - HECTOR MARIO GONZALEZ HENAO
-- - JHON JARVEY GÓMEZ PATIÑO
-- - CARLOS MARIO CEBALLOS
-- - CARLOS ANDRES PEREZ AREIZA
-- - JUAN ESTEBAN SUAREZ CORREA
-- - JOSÉ EDINSON OSPINA CRUZ
-- - JEFFERSON MONTOYA MUÑOZ
-- - ROBINSON ALEHANDRO GALVIS PARRA
-- - JHON ENMANUEL ARZUZA PÁEZ
-- - JUAN ESTEBAN OSORIO
-- - YEFERSON BAIRÓN USUGA AGUDELO
-- - JHON DAVID SANCHEZ
-- - CARLOS JULIO RENDÓN DÍAZ
-- - JENNIFER ANDREA CARDONA BENITEZ
-- - WILLIAM HUMBERTO JIMENEZ PEREZ
-- - CARLOS MARIO DIAZ DIAZ

-- =====================================================
-- VERIFICACIÓN DE MIEMBROS
-- =====================================================
-- Consulta para verificar si los miembros existen en la BD
SELECT 
    NombreCompleto,
    FechaIngreso,
    Estado,
    Cargo
FROM Miembros
WHERE NombreCompleto IN (
    'RAMÓN ANTONIO GONZALEZ CASTAÑO',
    'CARLOS ALBERTO ARAQUE BETANCUR',
    'MILTON DARIO GOMEZ RIVERA',
    'DANIEL ANDREY VILLAMIZAR ARAQUE',
    'GIRLESA MARÍA BUITRAGO',
    'ANGELA MARIA RODRIGUEZ',
    'CESAR LEONEL RODRIGUEZ GALAN',
    'LAURA VIVIAN ASALAZAR MORENO',
    'JOSE JULIAN VILLAMIZAR ARAQUE',
    'GUSTAVO ADOLFO GÓMEZ ZULUAGA',
    'Nelson Augusto Montoya Mataute',
    'HECTOR MARIO GONZALEZ HENAO',
    'JHON JARVEY GÓMEZ PATIÑO',
    'CARLOS MARIO CEBALLOS',
    'CARLOS ANDRES PEREZ AREIZA',
    'JUAN ESTEBAN SUAREZ CORREA',
    'JOSÉ EDINSON OSPINA CRUZ',
    'JEFFERSON MONTOYA MUÑOZ',
    'ROBINSON ALEHANDRO GALVIS PARRA',
    'JHON ENMANUEL ARZUZA PÁEZ',
    'JUAN ESTEBAN OSORIO',
    'YEFERSON BAIRÓN USUGA AGUDELO',
    'JHON DAVID SANCHEZ',
    'CARLOS JULIO RENDÓN DÍAZ',
    'JENNIFER ANDREA CARDONA BENITEZ',
    'WILLIAM HUMBERTO JIMENEZ PEREZ',
    'CARLOS MARIO DIAZ DIAZ'
)
ORDER BY NombreCompleto;

-- =====================================================
-- ACTUALIZAR FECHA DE INGRESO PARA NUEVOS MIEMBROS
-- =====================================================
-- Estos miembros ingresaron en octubre 2025
UPDATE Miembros
SET 
    FechaIngreso = '2025-10-01',
    UpdatedAt = GETUTCDATE(),
    UpdatedBy = 'admin_actualizacion_octubre_2025'
WHERE NombreCompleto IN (
    'LAURA VIVIAN ASALAZAR MORENO',
    'JOSE JULIAN VILLAMIZAR ARAQUE',
    'GUSTAVO ADOLFO GÓMEZ ZULUAGA',
    'Nelson Augusto Montoya Mataute'
)
AND (FechaIngreso IS NULL OR FechaIngreso > '2025-10-01');

-- Verificar actualización
SELECT 
    NombreCompleto,
    FechaIngreso,
    UpdatedAt,
    UpdatedBy
FROM Miembros
WHERE NombreCompleto IN (
    'LAURA VIVIAN ASALAZAR MORENO',
    'JOSE JULIAN VILLAMIZAR ARAQUE',
    'GUSTAVO ADOLFO GÓMEZ ZULUAGA',
    'Nelson Augusto Montoya Mataute'
);

GO
