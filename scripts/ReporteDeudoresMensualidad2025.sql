-- ReporteDeudoresMensualidad2025.sql
-- Calcula meses adeudados (enero-octubre 2025) por cada miembro según:
-- 1. Rango=Asociado: exentos (no pagan)
-- 2. FechaIngreso >= 2025-01-01: obligados desde mes de ingreso
-- 3. Casos especiales: algunos miembros con pagos parciales hasta cierto mes

SET NOCOUNT ON;

-- Tabla temporal con reglas de pago por miembro (casos especiales conocidos)
IF OBJECT_ID('tempdb..#PagosEspeciales') IS NOT NULL DROP TABLE #PagosEspeciales;
CREATE TABLE #PagosEspeciales (
    NombreCanon NVARCHAR(500) NOT NULL,
    UltimoMesPagado INT NULL, -- NULL = no tiene pagos; 1-12 mes pagado
    MesPrimerPagoRequerido INT NULL -- NULL = desde enero; 1-12 primer mes obligado
);

INSERT INTO #PagosEspeciales (NombreCanon, UltimoMesPagado, MesPrimerPagoRequerido) VALUES
-- Miembros con pagos parciales (hasta cierto mes)
(N'RAMON ANTONIO  GONZALEZ  CASTAÑO', 10, 1), -- pago hasta octubre
(N'CESAR LEONEL RODRIGUEZ GALAN', 9, 1), -- pago hasta septiembre
(N'ANGELA MARIA RODRIGUEZ OCHOA', 9, 1), -- pago hasta septiembre (ingresó nov 2024, debe desde ene)
(N'CARLOS ANDRES PEREZ AREIZA', 6, 1), -- pago hasta junio (debe jul-oct = 4 meses)
(N'DANIEL ANDREY VILLAMIZAR ARAQUE', 6, 1), -- pago hasta junio (debe jul-oct = 4 meses)
(N'MILTON DARIO GOMEZ RIVERA', 6, 1), -- pago hasta junio (debe jul-oct = 4 meses)
(N'GIRLESA MARIA BUITRAGO', 1, 1), -- pago solo enero
(N'CARLOS ALBERTO ARAQUE BETANCUR', 12, 1), -- pago completo ene-dic (al día)

-- Miembros que ingresan en 2025 (obligados desde mes de ingreso)
(N'LAURA VIVIANA SALAZAR MORENO', NULL, 6), -- ingresa junio, empieza a pagar desde junio
(N'JOSE JULIAN VILLAMIZAR ARAQUE', NULL, 6), -- ingresa junio
(N'GUSTAVO ADOLFO GOMEZ ZULUAGA', NULL, 10), -- ingresa octubre, empieza a pagar desde octubre
(N'NELSON AUGUSTO MONTOYA MATAUTE', NULL, 10), -- ingresa octubre, empieza a pagar desde octubre

-- Miembros sin pagos (deben todo desde enero)
(N'HECTOR MARIO GONZALEZ HENAO', NULL, 1),
(N'JHON HARVEY GOMEZ PATIÑO', NULL, 1),
(N'CARLOS MARIO  CEBALLOS', NULL, 1),
(N'JUAN ESTEBAN  SUAREZ CORREA', NULL, 1),
(N'JOSE EDINSON  OSPINA CRUZ', NULL, 1),
(N'JEFFERSON  MONTOYA MUÑOZ', NULL, 1),
(N'ROBINSON ALEJANDRO GALVIS PARRA', NULL, 1),
(N'JHON EMMANUEL ARZUZA PAEZ', NULL, 1),
(N'JUAN ESTEBAN  OSORIO', NULL, 1),
(N'YEFERSON BAIRON  USUGA AGUDELO', NULL, 1),
(N'JHON DAVID SANCHEZ', NULL, 1),
(N'CARLOS JULIO  RENDON DIAZ', NULL, 1),
(N'JENNIFER ANDREA CARDONA BENITEZ', NULL, 1),
(N'WILLIAM HUMBERTO JIMENEZ PEREZ', NULL, 1),
(N'CARLOS MARIO DIAZ DIAZ', NULL, 1);

-- Generar reporte
IF OBJECT_ID('tempdb..#Deuda') IS NOT NULL DROP TABLE #Deuda;
SELECT
    m.Id,
    m.NombreCompleto,
    m.Rango,
    m.FechaIngreso,
    m.NombreCanon,
    ISNULL(pe.UltimoMesPagado, 0) AS UltimoMesPagado,
    ISNULL(pe.MesPrimerPagoRequerido,
           CASE
               WHEN m.FechaIngreso IS NULL OR m.FechaIngreso < '2025-01-01' THEN 1
               ELSE MONTH(m.FechaIngreso)
           END
    ) AS MesPrimerPagoRequerido
INTO #Deuda
FROM dbo.Miembros m
LEFT JOIN #PagosEspeciales pe ON m.NombreCanon = pe.NombreCanon COLLATE Latin1_General_CI_AI
WHERE m.Rango <> N'Asociado' -- Asociados exentos
  AND m.Estado = 1; -- solo activos

-- Calcular meses adeudados
ALTER TABLE #Deuda ADD MesesAdeudados INT;
ALTER TABLE #Deuda ADD PrimerMesAdeudado NVARCHAR(20);

UPDATE #Deuda
SET MesesAdeudados = CASE
        WHEN UltimoMesPagado >= 10 THEN 0 -- al día hasta octubre
        WHEN MesPrimerPagoRequerido > 10 THEN 0 -- ingresó después de octubre, no debe nada de ene-oct
        ELSE
            -- Calcular desde qué mes debe
            (10 - (CASE WHEN MesPrimerPagoRequerido > (UltimoMesPagado + 1) THEN MesPrimerPagoRequerido ELSE UltimoMesPagado + 1 END) + 1)
    END,
    PrimerMesAdeudado = CASE
        WHEN UltimoMesPagado >= 10 THEN NULL
        WHEN MesPrimerPagoRequerido > 10 THEN NULL
        ELSE
            CASE WHEN MesPrimerPagoRequerido > (UltimoMesPagado + 1) THEN MesPrimerPagoRequerido ELSE UltimoMesPagado + 1 END
    END;

-- Correcciones manuales para nombres con variaciones de codificación
UPDATE #Deuda
SET UltimoMesPagado = 6, MesPrimerPagoRequerido = 1
WHERE NombreCanon LIKE '%MILTON%RIVERA%'
     OR NombreCanon LIKE '%CARLOS ANDR%REZ AREIZA%';

UPDATE #Deuda
SET MesesAdeudados = (10 - (CASE WHEN MesPrimerPagoRequerido > (UltimoMesPagado + 1) THEN MesPrimerPagoRequerido ELSE UltimoMesPagado + 1 END) + 1),
    PrimerMesAdeudado = CASE WHEN MesPrimerPagoRequerido > (UltimoMesPagado + 1) THEN MesPrimerPagoRequerido ELSE UltimoMesPagado + 1 END
WHERE NombreCanon LIKE '%MILTON%RIVERA%'
    OR NombreCanon LIKE '%CARLOS ANDR%REZ AREIZA%';

SELECT
    d.NombreCompleto,
    d.Rango,
    FORMAT(d.FechaIngreso, 'yyyy-MM-dd') AS FechaIngreso,
    d.MesesAdeudados,
    CASE d.PrimerMesAdeudado
        WHEN 1 THEN N'Enero - Octubre'
        WHEN 2 THEN N'Febrero - Octubre'
        WHEN 3 THEN N'Marzo - Octubre'
        WHEN 4 THEN N'Abril - Octubre'
        WHEN 5 THEN N'Mayo - Octubre'
        WHEN 6 THEN N'Junio - Octubre'
        WHEN 7 THEN N'Julio - Octubre'
        WHEN 8 THEN N'Agosto - Octubre'
        WHEN 9 THEN N'Septiembre - Octubre'
        WHEN 10 THEN N'Octubre - Octubre'
        ELSE NULL
    END AS RangoAdeudado
FROM #Deuda d
WHERE d.MesesAdeudados > 0
ORDER BY d.MesesAdeudados DESC, d.NombreCompleto;

-- Resumen
SELECT 'RESUMEN' AS Tipo;
SELECT
    COUNT(*) AS TotalDeudores,
    SUM(MesesAdeudados) AS TotalMesesAdeudados,
    AVG(MesesAdeudados) AS PromedioMesesDeuda
FROM #Deuda
WHERE MesesAdeudados > 0;

-- Nota: Miembro Anderson Arlex Betancur Rua (Asociado) no aparece porque está exento
