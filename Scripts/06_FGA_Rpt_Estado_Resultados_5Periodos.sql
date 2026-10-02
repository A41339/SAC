-- =========================================================================================
-- AUTOR:       FGA / SAC
-- FECHA:       30/09/2026
-- DESCRIPCIÓN: Stored Procedure optimizado para la generación completa de las 5 columnas
--              del Estado de Resultados con variaciones absolutas y relativas.
--
-- CARACTERÍSTICAS DE ALTO RENDIMIENTO:
--   1. CERO subconsultas correlacionadas pesadas en la salida tabular.
--   2. Normalización de fechas con resolución sin duplicados (fin de mes y día 1).
--   3. Soporte para modalidades: 'Acumulado' (SaldoAcumulado) y 'Mensual' (SaldoFinal).
--   4. Soporte para tipos de comparación: 'Trimestral', 'Mensual' e 'Interanual'.
--   5. Agrupación y jerarquía contable oficial SUGEF / FGA basada en Rpt_EstadoResultado.
--   6. Salida tabular estructurada en Millones de Colones (/ 1,000,000.0).
--
-- PARÁMETROS:
--   @IDENTIDAD: Código de entidad (ej. '13', '01')
--   @PERIODO_REF: Fecha de corte del informe (ej. '2026-06-30' o '2026-06-01')
--   @MODALIDAD: 'Acumulado' o 'Mensual'
--   @TIPO_COMPARACION: 'Trimestral', 'Mensual' o 'Interanual'
-- =========================================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[FGA_Rpt_Estado_Resultados_5Periodos]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_REF DATETIME,
    @MODALIDAD NVARCHAR(20) = 'Acumulado',
    @TIPO_COMPARACION NVARCHAR(20) = 'Trimestral'
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99'
    BEGIN
        SET @IDENTIDAD = '13';
    END

    -- 1. Normalización de fechas de referencia (5 períodos estándar)
    DECLARE @REF_DATE DATE = CAST(@PERIODO_REF AS DATE);
    DECLARE @Y INT = YEAR(@REF_DATE);
    DECLARE @M INT = MONTH(@REF_DATE);

    DECLARE @P1 DATE;
    DECLARE @P2 DATE;
    DECLARE @P3 DATE;
    DECLARE @P4 DATE;
    DECLARE @P5 DATE;

    -- P5 normalizado al primer día del mes para consistencia con P1..P4
    SET @P5 = DATEFROMPARTS(@Y, @M, 1);

    IF UPPER(ISNULL(@TIPO_COMPARACION, 'TRIMESTRAL')) = 'MENSUAL'
    BEGIN
        SET @P5 = DATEFROMPARTS(@Y, @M, 1);
        SET @P4 = DATEADD(MONTH, -1, @P5);
        SET @P3 = DATEADD(MONTH, -2, @P5);
        SET @P2 = DATEADD(MONTH, -3, @P5);
        SET @P1 = DATEADD(MONTH, -4, @P5);
    END
    ELSE IF UPPER(ISNULL(@TIPO_COMPARACION, 'TRIMESTRAL')) = 'TRIMESTRAL'
    BEGIN
        DECLARE @qMonth INT = ((@M - 1) / 3 + 1) * 3;
        SET @P5 = DATEFROMPARTS(@Y, @qMonth, 1);
        SET @P4 = DATEADD(MONTH, -3, @P5);
        SET @P3 = DATEADD(MONTH, -6, @P5);
        SET @P2 = DATEADD(MONTH, -9, @P5);
        SET @P1 = DATEADD(MONTH, -12, @P5);
    END
    ELSE -- INTERANUAL
    BEGIN
        SET @P5 = DATEFROMPARTS(@Y, @M, 1);
        SET @P4 = DATEADD(YEAR, -1, @P5);
        SET @P3 = DATEADD(YEAR, -2, @P5);
        SET @P2 = DATEADD(YEAR, -3, @P5);
        SET @P1 = DATEADD(YEAR, -4, @P5);
    END

    -- Tabla temporal para almacenar los saldos consolidados
    CREATE TABLE #RESUMEN (
        ID INT,
        NUM_CUENTA NVARCHAR(20),
        ESTILO INT,
        PERIODO DATE,
        SALDO_MES DECIMAL(35, 5),
        SALDO_ACUM DECIMAL(35, 5)
    );
    CREATE CLUSTERED INDEX IX_TEMPRESUMEN_ER ON #RESUMEN (ID, PERIODO);

    -- Tabla de fechas objetivo normalizadas para Index Seek instantáneo
    DECLARE @FECHAS TABLE (PeriodoFiltro DATE, MesNormalizado DATE);
    INSERT INTO @FECHAS (PeriodoFiltro, MesNormalizado)
    SELECT DISTINCT PeriodoFiltro, MesNormalizado
    FROM (
        VALUES 
            (@P1, @P1), (EOMONTH(@P1), @P1),
            (@P2, @P2), (EOMONTH(@P2), @P2),
            (@P3, @P3), (EOMONTH(@P3), @P3),
            (@P4, @P4), (EOMONTH(@P4), @P4),
            (@P5, @P5), (EOMONTH(@P5), @P5)
    ) AS F(PeriodoFiltro, MesNormalizado);

    -- 2. Cargar saldos directos deduplicados por entidad, cuenta y mes normalizado
    ;WITH SaldosPorEntidad AS (
        SELECT 
            A.ID,
            A.CUENTACONTABLE,
            A.ESTILO,
            B.IdEntidad,
            F.MesNormalizado,
            B.SaldoFinal,
            B.SaldoAcumulado,
            ROW_NUMBER() OVER (
                PARTITION BY B.IdEntidad, B.Cuenta, F.MesNormalizado
                ORDER BY B.Periodo DESC
            ) AS rn
        FROM Rpt_EstadoResultado A WITH(NOLOCK)
        INNER JOIN dbo.SALIDA_BALANCE_COMPROBACION B WITH(NOLOCK) ON B.Cuenta = A.CUENTACONTABLE
        INNER JOIN @FECHAS F ON B.Periodo = F.PeriodoFiltro
        WHERE (@IDENTIDAD = '-1' OR B.IdEntidad = @IDENTIDAD)
    )
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT ID, CUENTACONTABLE, ESTILO, MesNormalizado, SUM(SaldoFinal), SUM(SaldoAcumulado)
    FROM SaldosPorEntidad
    WHERE rn = 1
    GROUP BY ID, CUENTACONTABLE, ESTILO, MesNormalizado;

    -- 3. Calcular subtotales y agregaciones oficiales según lógica de FGA_Rpt_ER
    -- ID 5: Otros ingresos financieros = SUM(IDs 29..31)
    DELETE FROM #RESUMEN WHERE ID = 5;
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 5, '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN WHERE ID BETWEEN 29 AND 31 GROUP BY PERIODO;

    -- ID 6: Total ingresos financieros = SUM(IDs 1..5)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 6, '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN WHERE ID BETWEEN 1 AND 5 GROUP BY PERIODO;

    -- ID 11: Otros gastos financieros = SUM(IDs 32..35)
    DELETE FROM #RESUMEN WHERE ID = 11;
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 11, '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN WHERE ID BETWEEN 32 AND 35 GROUP BY PERIODO;

    -- ID 12: Total de gastos financieros = SUM(IDs 7..11)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 12, '0', -1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN WHERE ID BETWEEN 7 AND 11 GROUP BY PERIODO;

    -- ID 15: Resultado financiero bruto = Total Ingresos Fin (ID 6) - Total Gasto Fin (ID 12) + Recup Activos (ID 13) - Deterioro (ID 14)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 15, '0', 1, PERIODO, 
           SUM(ISNULL(SALDO_MES, 0) * ESTILO), 
           SUM(ISNULL(SALDO_ACUM, 0) * ESTILO)
    FROM #RESUMEN WHERE ID IN (6, 12, 13, 14) GROUP BY PERIODO;

    -- ID 18: Resultado por DC y UD = Ganancias DF/UD (ID 16) - Pérdidas DC/UD (ID 17)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 18, '0', 1, PERIODO, 
           SUM(ISNULL(SALDO_MES, 0) * ESTILO), 
           SUM(ISNULL(SALDO_ACUM, 0) * ESTILO)
    FROM #RESUMEN WHERE ID BETWEEN 16 AND 17 GROUP BY PERIODO;

    -- ID 21: Resultado operacional bruto = Res Financiero Bruto (ID 15) + Res DC/UD (ID 18) + Otros Ingresos Oper (ID 19) - Otros Gastos Oper (ID 20)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 21, '0', 1, PERIODO, 
           SUM(ISNULL(SALDO_MES, 0) * ESTILO), 
           SUM(ISNULL(SALDO_ACUM, 0) * ESTILO)
    FROM #RESUMEN WHERE ID IN (15, 18, 19, 20) GROUP BY PERIODO;

    -- ID 23: Otros gastos de administración = SUM(IDs 36..39)
    DELETE FROM #RESUMEN WHERE ID = 23;
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 23, '0', -1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN WHERE ID BETWEEN 36 AND 39 GROUP BY PERIODO;

    -- ID 24: Total gastos administrativos = Gastos personal (ID 22) + Otros gastos admin (ID 23)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 24, '0', 1, PERIODO, 
           ABS(SUM(ISNULL(SALDO_MES, 0) * ESTILO)), 
           ABS(SUM(ISNULL(SALDO_ACUM, 0) * ESTILO))
    FROM #RESUMEN WHERE ID IN (22, 23) GROUP BY PERIODO;

    -- ID 25: Resultado operacional neto = Resultado operacional bruto (ID 21) - Total gastos admin (ID 24)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 25, '0', 1, PERIODO, 
           SUM(CASE WHEN ID = 21 THEN SALDO_MES ELSE -SALDO_MES END), 
           SUM(CASE WHEN ID = 21 THEN SALDO_ACUM ELSE -SALDO_ACUM END)
    FROM #RESUMEN WHERE ID IN (21, 24) GROUP BY PERIODO;

    -- ID 28: Resultado del periodo (Oficial FGA/SUGEF: Clase 5 - Clase 4, o Neto - Impuestos - Part)
    INSERT INTO #RESUMEN (ID, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 28, '0', 1, PERIODO, 
           CASE 
               WHEN COUNT(CASE WHEN ID IN (40, 41) THEN 1 END) > 0 THEN 
                   SUM(CASE WHEN ID IN (40, 41) THEN ISNULL(SALDO_MES, 0) * ESTILO ELSE 0 END)
               ELSE 
                   SUM(CASE WHEN ID = 25 THEN ISNULL(SALDO_MES, 0) ELSE -ABS(ISNULL(SALDO_MES, 0)) END)
           END,
           CASE 
               WHEN COUNT(CASE WHEN ID IN (40, 41) THEN 1 END) > 0 THEN 
                   SUM(CASE WHEN ID IN (40, 41) THEN ISNULL(SALDO_ACUM, 0) * ESTILO ELSE 0 END)
               ELSE 
                   SUM(CASE WHEN ID = 25 THEN ISNULL(SALDO_ACUM, 0) ELSE -ABS(ISNULL(SALDO_ACUM, 0)) END)
           END
    FROM #RESUMEN 
    WHERE ID IN (40, 41, 25, 26, 27) 
    GROUP BY PERIODO;

    -- 4. Catálogo estructurado de filas del Estado de Resultados formal
    CREATE TABLE #CATALOGO_FILAS (
        Orden INT IDENTITY(1,1) PRIMARY KEY,
        ID INT NULL,
        Seccion NVARCHAR(100),
        Concepto NVARCHAR(200),
        Nivel INT,
        EsNegrita BIT,
        CodigoContable NVARCHAR(20)
    );

    INSERT INTO #CATALOGO_FILAS (ID, Seccion, Concepto, Nivel, EsNegrita, CodigoContable)
    VALUES
    (NULL, 'INGRESOS FINANCIEROS', 'Ingresos financieros', 0, 1, ''),
    (1,    'INGRESOS FINANCIEROS', 'Ingresos financieros por disponibilidades', 1, 0, '51100000'),
    (2,    'INGRESOS FINANCIEROS', 'Ingresos financieros por instrum. financieros', 1, 0, '51200000'),
    (3,    'INGRESOS FINANCIEROS', 'Productos por cartera de crédito vigente', 1, 0, '51300000'),
    (4,    'INGRESOS FINANCIEROS', 'Productos por cartera de crédito vencida', 1, 0, '51400000'),
    (5,    'INGRESOS FINANCIEROS', 'Otros ingresos financieros', 1, 0, '51900000'),
    (6,    'INGRESOS FINANCIEROS', 'Total de ingresos financieros', 2, 1, ''),

    (NULL, 'GASTOS FINANCIEROS', 'Gastos financieros', 0, 1, ''),
    (7,    'GASTOS FINANCIEROS', 'Gastos financieros por oblig. con público', 1, 0, '41100000'),
    (8,    'GASTOS FINANCIEROS', 'Gastos financieros por oblig. con entidades', 1, 0, '41300000'),
    (9,    'GASTOS FINANCIEROS', 'Gastos financieros cuentas por pagar diversas', 1, 0, '41400000'),
    (10,   'GASTOS FINANCIEROS', 'Gastos por obligaciones subordinadas', 1, 0, '41600000'),
    (11,   'GASTOS FINANCIEROS', 'Otros gastos financieros', 1, 0, '41900000'),
    (12,   'GASTOS FINANCIEROS', 'Total de gastos financieros', 2, 1, ''),

    (13,   'RESULTADO FINANCIERO', 'Ingresos por recuperación de activos', 1, 0, '52000000'),
    (14,   'RESULTADO FINANCIERO', 'Por estimación de deterioro de activos', 1, 0, '42000000'),
    (15,   'RESULTADO FINANCIERO', 'Resultado financiero bruto', 2, 1, ''),

    (16,   'MONEDA EXTRANJERA', 'Ganancias por DF y UD', 1, 0, '51800000'),
    (17,   'MONEDA EXTRANJERA', 'Pérdidas por DC y UD', 1, 0, '41800000'),
    (18,   'MONEDA EXTRANJERA', 'Resultado por DC y UD', 2, 1, ''),

    (19,   'OPERACION', 'Total otros ingresos de operación', 1, 0, '53000000'),
    (20,   'OPERACION', 'Total otros gastos de operación', 1, 0, '43000000'),
    (21,   'OPERACION', 'Resultado operacional bruto', 2, 1, ''),

    (NULL, 'GASTOS ADMINISTRATIVOS', 'Gastos administrativos', 0, 1, ''),
    (22,   'GASTOS ADMINISTRATIVOS', 'Por gastos de personal', 1, 0, '44100000'),
    (23,   'GASTOS ADMINISTRATIVOS', 'Por otros gastos de administración', 1, 0, '44200000'),
    (24,   'GASTOS ADMINISTRATIVOS', 'Total gastos administrativos', 2, 1, ''),

    (25,   'RESULTADO NETO', 'Resultado operacional neto', 2, 1, ''),
    (26,   'RESULTADO NETO', 'DIS de impuesto y participación sobre utilidad', 1, 0, '55000000'),
    (27,   'RESULTADO NETO', 'Participaciones sobre la utilidad', 1, 0, '45000000'),
    (28,   'RESULTADO NETO', 'Resultado del periodo', 3, 1, '');

    -- 5. Período base para variación absoluta y relativa
    DECLARE @VAR_REF_COL INT = CASE WHEN UPPER(ISNULL(@TIPO_COMPARACION, 'TRIMESTRAL')) = 'INTERANUAL' THEN 2 ELSE 4 END;
    DECLARE @USAR_ACUM BIT = CASE WHEN UPPER(ISNULL(@MODALIDAD, 'ACUMULADO')) = 'MENSUAL' THEN 0 ELSE 1 END;

    -- 6. PIVOT tabular en Millones de Colones (/ 1,000,000.0)
    ;WITH ResumenPivot AS (
        SELECT 
            ID,
            MAX(CASE WHEN PERIODO = @P1 THEN (CASE WHEN @USAR_ACUM = 1 THEN ISNULL(SALDO_ACUM, 0) ELSE ISNULL(SALDO_MES, 0) END) ELSE 0 END) / 1000000.0 AS P1,
            MAX(CASE WHEN PERIODO = @P2 THEN (CASE WHEN @USAR_ACUM = 1 THEN ISNULL(SALDO_ACUM, 0) ELSE ISNULL(SALDO_MES, 0) END) ELSE 0 END) / 1000000.0 AS P2,
            MAX(CASE WHEN PERIODO = @P3 THEN (CASE WHEN @USAR_ACUM = 1 THEN ISNULL(SALDO_ACUM, 0) ELSE ISNULL(SALDO_MES, 0) END) ELSE 0 END) / 1000000.0 AS P3,
            MAX(CASE WHEN PERIODO = @P4 THEN (CASE WHEN @USAR_ACUM = 1 THEN ISNULL(SALDO_ACUM, 0) ELSE ISNULL(SALDO_MES, 0) END) ELSE 0 END) / 1000000.0 AS P4,
            MAX(CASE WHEN PERIODO = @P5 THEN (CASE WHEN @USAR_ACUM = 1 THEN ISNULL(SALDO_ACUM, 0) ELSE ISNULL(SALDO_MES, 0) END) ELSE 0 END) / 1000000.0 AS P5
        FROM #RESUMEN
        GROUP BY ID
    )
    SELECT
        CAST(C.Orden AS INT) AS Orden,
        C.Seccion,
        C.Concepto,
        C.Nivel,
        C.EsNegrita,
        C.CodigoContable,
        CAST(ISNULL(R.P1, 0.0) AS DECIMAL(18,2)) AS Periodo1,
        CAST(ISNULL(R.P2, 0.0) AS DECIMAL(18,2)) AS Periodo2,
        CAST(ISNULL(R.P3, 0.0) AS DECIMAL(18,2)) AS Periodo3,
        CAST(ISNULL(R.P4, 0.0) AS DECIMAL(18,2)) AS Periodo4,
        CAST(ISNULL(R.P5, 0.0) AS DECIMAL(18,2)) AS Periodo5,
        -- Variación Absoluta
        CAST(
            ISNULL(R.P5, 0.0) - ISNULL(CASE WHEN @VAR_REF_COL = 2 THEN R.P2 ELSE R.P4 END, 0.0)
            AS DECIMAL(18,2)
        ) AS VariacionAbsoluta,
        -- Variación Relativa (%)
        CAST(
            CASE 
                WHEN ISNULL(CASE WHEN @VAR_REF_COL = 2 THEN R.P2 ELSE R.P4 END, 0.0) = 0.0 THEN 0.0
                ELSE 
                    (
                        (ISNULL(R.P5, 0.0) - ISNULL(CASE WHEN @VAR_REF_COL = 2 THEN R.P2 ELSE R.P4 END, 0.0))
                        / ABS(CASE WHEN @VAR_REF_COL = 2 THEN R.P2 ELSE R.P4 END)
                    ) * 100.0
            END 
            AS DECIMAL(18,2)
        ) AS VariacionRelativa,
        -- Metadatos de fechas
        @P1 AS FechaP1,
        @P2 AS FechaP2,
        @P3 AS FechaP3,
        @P4 AS FechaP4,
        @P5 AS FechaP5
    FROM #CATALOGO_FILAS C
    LEFT JOIN ResumenPivot R ON C.ID = R.ID
    ORDER BY C.Orden;

    DROP TABLE #RESUMEN;
    DROP TABLE #CATALOGO_FILAS;
END
GO

-- Conceder permisos de ejecución para FGA y public
GRANT EXECUTE ON [dbo].[FGA_Rpt_Estado_Resultados_5Periodos] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Rpt_Estado_Resultados_5Periodos] TO [public];
GO
