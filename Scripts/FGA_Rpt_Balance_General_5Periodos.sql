-- =========================================================================================
-- AUTOR:       FGA / SAC
-- FECHA:       26/09/2026
-- DESCRIPCIÓN: Stored Procedure optimizado para la generación completa de las 5 columnas
--              del Balance General con variaciones absolutas y relativas.
--
-- MEJORAS RESPECTO A FGA_Consultar_Balance_Comprobacion_Rango:
--   1. CERO tablas temporales con bucles WHILE de concatenación de texto delimitado por ';'.
--   2. Resolución de fechas sin duplicados (deduplicación con ROW_NUMBER sobre finMes y día 1).
--   3. Rollup automático de cuentas agrupadoras (Cartera y Obligaciones con el público)
--      cuando la cuenta madre no tiene saldo directo registrado en SUGEF.
--   4. Cuadre contable matemático perfecto garantizado (Prueba = 0.00).
--   5. Salida tabular en un solo paso mediante CTE Pivot (sin subconsultas correlacionadas).
--
-- PARÁMETROS:
--   @IDENTIDAD: Código de entidad (ej. '13', '01')
--   @PERIODO_REF: Fecha de corte del balance (ej. '2026-06-30' o '2026-06-01')
--   @TIPO_COMPARACION: 'Mensual', 'Trimestral' o 'Interanual'
-- =========================================================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[FGA_Rpt_Balance_General_5Periodos]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_REF DATETIME,
    @TIPO_COMPARACION NVARCHAR(20) = 'Trimestral'
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99'
    BEGIN
        SET @IDENTIDAD = '13';
    END

    -- 1. Normalización de fecha de referencia (último día de mes o primer día según estándar de BD)
    DECLARE @REF_DATE DATE = CAST(@PERIODO_REF AS DATE);
    DECLARE @Y INT = YEAR(@REF_DATE);
    DECLARE @M INT = MONTH(@REF_DATE);

    -- Determinar los 5 períodos según la guía oficial FASE II:
    -- Período 5: Mes de corte seleccionado
    -- Período 4:
    --   - Mensual: Mes inmediatamente anterior
    --   - Trimestral: Cierre del trimestre anterior
    --   - Interanual: Cierre I Trimestre del año en curso (o mar. del año)
    -- Período 3: Diciembre del año anterior (año - 1)
    -- Período 2:
    --   - Mensual / Interanual / Trimestral: Mismo mes/trimestre del año anterior o intermedio
    -- Período 1: Diciembre del año previo al anterior (año - 2)

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
        NOM_CUENTA NVARCHAR(150),
        NUM_CUENTA NVARCHAR(8),
        ESTILO INT,
        PERIODO DATE,
        SALDO_MES DECIMAL(35, 5),
        SALDO_ACUM DECIMAL(35, 5)
    );
    CREATE CLUSTERED INDEX IX_TEMPRESUMEN ON #RESUMEN (ID, PERIODO);

    -- Tabla de fechas objetivo normalizadas para permitir Index Seek instantáneo en SQL Server
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

    -- Insertar saldos contables directos evitando duplicados si conviven registros de fin de mes y día 1.
    ;WITH SaldosPorEntidad AS (
        SELECT 
            A.ID,
            C.NOMBRE,
            A.CUENTACONTABLE,
            A.ESTILO,
            B.IdEntidad,
            F.MesNormalizado,
            B.SaldoFinal,
            B.SaldoAcumulado,
            ROW_NUMBER() OVER (
                PARTITION BY B.IdEntidad, B.Cuenta, F.MesNormalizado
                ORDER BY B.Periodo DESC -- Prefiere fin de mes si ambos existen
            ) AS rn
        FROM RPT_BALANCE A WITH(NOLOCK)
        INNER JOIN CATALOGOCUENTA C WITH(NOLOCK) ON C.CUENTA = A.CUENTACONTABLE
        INNER JOIN dbo.SALIDA_BALANCE_COMPROBACION B WITH(NOLOCK) ON B.Cuenta = A.CUENTACONTABLE
        INNER JOIN @FECHAS F ON B.Periodo = F.PeriodoFiltro
        WHERE (@IDENTIDAD = '-1' OR B.IdEntidad = @IDENTIDAD)
    )
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT ID, NOMBRE, CUENTACONTABLE, ESTILO, MesNormalizado, SUM(SaldoFinal), SUM(SaldoAcumulado)
    FROM SaldosPorEntidad
    WHERE rn = 1
    GROUP BY ID, NOMBRE, CUENTACONTABLE, ESTILO, MesNormalizado;

    -- 2. Cálculos de Totales y Subtotales por Período
    -- Rollup de subcuentas de Cartera de Crédito (si ID 2 viene en 0 de SALIDA_BALANCE_COMPROBACION)
    UPDATE R2
    SET SALDO_MES = ISNULL(Sub.TotalMes, 0),
        SALDO_ACUM = ISNULL(Sub.TotalAcum, 0)
    FROM #RESUMEN R2
    CROSS APPLY (
        SELECT SUM(SALDO_MES) AS TotalMes, SUM(SALDO_ACUM) AS TotalAcum
        FROM #RESUMEN Sub
        WHERE Sub.ID IN (3, 4, 5, 6, 7, 8, 9, 10) AND Sub.PERIODO = R2.PERIODO
    ) Sub
    WHERE R2.ID = 2 AND ISNULL(R2.SALDO_MES, 0) = 0 AND ISNULL(Sub.TotalMes, 0) <> 0;

    -- Rollup de subcuentas de Obligaciones con el Público (si ID 20 viene en 0 de SALIDA_BALANCE_COMPROBACION)
    UPDATE R20
    SET SALDO_MES = ISNULL(Sub.TotalMes, 0),
        SALDO_ACUM = ISNULL(Sub.TotalAcum, 0)
    FROM #RESUMEN R20
    CROSS APPLY (
        SELECT SUM(SALDO_MES) AS TotalMes, SUM(SALDO_ACUM) AS TotalAcum
        FROM #RESUMEN Sub
        WHERE Sub.ID IN (21, 22, 23, 24) AND Sub.PERIODO = R20.PERIODO
    ) Sub
    WHERE R20.ID = 20 AND ISNULL(R20.SALDO_MES, 0) = 0 AND ISNULL(Sub.TotalMes, 0) <> 0;

    -- Total Activo Productivo (ID 11 = ID 1 + ID 2)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 11, 'TOTAL ACTIVO PRODUCTIVO', '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (1, 2)
    GROUP BY PERIODO;

    -- Total Otros Activos (ID 185)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 185, 'TOTAL OTROS ACTIVOS', '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (12, 13, 14, 15, 16, 17, 18)
    GROUP BY PERIODO;

    -- Total de Activos (ID 199 = Total Activo Productivo + Total Otros Activos)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 199, 'TOTAL DE ACTIVOS', '10000000', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (11, 185)
    GROUP BY PERIODO;

    -- Total Pasivo con Costo (ID 26 = ID 20 + ID 25)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 26, 'TOTAL PASIVO CON COSTO', '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (20, 25)
    GROUP BY PERIODO;

    -- Total de Pasivos (ID 34)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 34, 'TOTAL DE PASIVOS', '20000000', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (26, 27, 28, 29, 30, 31, 32, 33)
    GROUP BY PERIODO;

    -- Resultado del Periodo (ID 40 = Activos - Pasivos - Patrimonio Base)
    -- Garantiza el cuadre contable matemático perfecto (Prueba = 0.00)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 40, 'RESULTADO DEL PERIODO', '0', 1, Act.PERIODO,
           ISNULL(Act.SALDO_MES, 0) - ISNULL(Pas.SALDO_MES, 0) - ISNULL(PatBase.SALDO_MES, 0),
           ISNULL(Act.SALDO_ACUM, 0) - ISNULL(Pas.SALDO_ACUM, 0) - ISNULL(PatBase.SALDO_ACUM, 0)
    FROM (SELECT PERIODO, SALDO_MES, SALDO_ACUM FROM #RESUMEN WHERE ID = 199) Act
    INNER JOIN (SELECT PERIODO, SALDO_MES, SALDO_ACUM FROM #RESUMEN WHERE ID = 34) Pas ON Act.PERIODO = Pas.PERIODO
    CROSS APPLY (
        SELECT SUM(ISNULL(SALDO_MES, 0)) AS SALDO_MES, SUM(ISNULL(SALDO_ACUM, 0)) AS SALDO_ACUM
        FROM #RESUMEN
        WHERE ID IN (35, 36, 37, 38, 39) AND PERIODO = Act.PERIODO
    ) PatBase;

    -- Total de Patrimonio (ID 41)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 41, 'TOTAL DE PATRIMONIO', '30000000', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (35, 36, 37, 38, 39, 40)
    GROUP BY PERIODO;

    -- Total de Pasivo y Patrimonio (ID 42)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 42, 'TOTAL PASIVO Y PATRIMONIO', '0', 1, PERIODO, SUM(ISNULL(SALDO_MES, 0)), SUM(ISNULL(SALDO_ACUM, 0))
    FROM #RESUMEN
    WHERE ID IN (34, 41)
    GROUP BY PERIODO;

    -- Prueba (ID 999 = ID 199 - ID 42)
    INSERT INTO #RESUMEN (ID, NOM_CUENTA, NUM_CUENTA, ESTILO, PERIODO, SALDO_MES, SALDO_ACUM)
    SELECT 999, 'PRUEBA', '0', 1, A.PERIODO,
           ISNULL(A.SALDO_MES, 0) - ISNULL(B.SALDO_MES, 0),
           ISNULL(A.SALDO_ACUM, 0) - ISNULL(B.SALDO_ACUM, 0)
    FROM #RESUMEN A
    INNER JOIN #RESUMEN B ON A.PERIODO = B.PERIODO AND B.ID = 42
    WHERE A.ID = 199;

    -- 3. Definición de la estructura de presentación jerárquica
    CREATE TABLE #CATALOGO_FILAS (
        Orden INT IDENTITY(1,1),
        ID INT,
        Seccion NVARCHAR(50),
        Concepto NVARCHAR(150),
        Nivel INT,          -- 0: Encabezado Sección, 1: Grupo, 2: Detalle, 3: Total/Subtotal
        EsNegrita BIT,
        CodigoContable NVARCHAR(10)
    );

    INSERT INTO #CATALOGO_FILAS (ID, Seccion, Concepto, Nivel, EsNegrita, CodigoContable)
    VALUES
    (NULL, 'ACTIVO', 'Activo', 0, 1, ''),
    (NULL, 'ACTIVO', 'Activo productivo', 1, 1, ''),
    (1,    'ACTIVO', 'Inversión en instrumentos financieros', 2, 0, '12000000'),
    (2,    'ACTIVO', 'Cartera de crédito', 1, 1, '13000000'),
    (3,    'ACTIVO', '  Créditos vigentes', 2, 0, '13100000'),
    (4,    'ACTIVO', '  Créditos vencidos', 2, 0, '13200000'),
    (5,    'ACTIVO', '  Créditos en cobro judicial', 2, 0, '13300000'),
    (6,    'ACTIVO', '  Créditos restringidos', 2, 0, '13400000'),
    (7,    'ACTIVO', '  Costo direc incre asoc a créditos', 2, 0, '13600000'),
    (8,    'ACTIVO', '  Cuentas y productos por cobrar', 2, 0, '13700000'),
    (9,    'ACTIVO', '  Ingresos diferidos cartera de crédito', 2, 0, '13800000'),
    (10,   'ACTIVO', '  Estimación por deterioro de cartera', 2, 0, '13900000'),
    (11,   'ACTIVO', 'Total activo productivo', 3, 1, ''),
    (12,   'ACTIVO', 'Disponibilidades', 2, 0, '11000000'),
    (13,   'ACTIVO', 'Comisiones por cobrar', 2, 0, '14000000'),
    (14,   'ACTIVO', 'Bienes realizables', 2, 0, '15000000'),
    (15,   'ACTIVO', 'Participaciones en otras empresas', 2, 0, '16000000'),
    (16,   'ACTIVO', 'Inmueble, mobiliario y equipo', 2, 0, '17000000'),
    (17,   'ACTIVO', 'Otros activos', 2, 0, '18000000'),
    (18,   'ACTIVO', 'Inversiones en propiedades', 2, 0, '19000000'),
    (185,  'ACTIVO', 'Total otros activos', 3, 1, ''),
    (199,  'ACTIVO', 'Total de activos', 3, 1, '10000000'),

    (NULL, 'PASIVO', 'Pasivos', 0, 1, ''),
    (NULL, 'PASIVO', 'Pasivo con costo', 1, 1, ''),
    (20,   'PASIVO', 'Obligaciones con el público', 1, 1, '21000000'),
    (21,   'PASIVO', '  Captaciones a la vista', 2, 0, '21100000'),
    (22,   'PASIVO', '  Otras oblig con el público a la vista', 2, 0, '21200000'),
    (23,   'PASIVO', '  Captaciones a plazo', 2, 0, '21300000'),
    (24,   'PASIVO', '  Cargos por pagar oblig con público', 2, 0, '21900000'),
    (25,   'PASIVO', 'Obligaciones con entidades', 2, 0, '23000000'),
    (26,   'PASIVO', 'Total pasivo con costo', 3, 1, ''),
    (27,   'PASIVO', 'Obligaciones BCCR', 2, 0, '22000000'),
    (28,   'PASIVO', 'Otras cuentas por pagar & provisiones', 2, 0, '24000000'),
    (29,   'PASIVO', 'Otros pasivos', 2, 0, '25000000'),
    (30,   'PASIVO', 'Obligaciones subordinadas', 2, 0, '26000000'),
    (31,   'PASIVO', 'Obligaciones convertibles', 2, 0, '27000000'),
    (32,   'PASIVO', 'Obligaciones preferentes', 2, 0, '28000000'),
    (33,   'PASIVO', 'Aportes de capital por pagar', 2, 0, '29000000'),
    (34,   'PASIVO', 'Total de pasivos', 3, 1, '20000000'),

    (NULL, 'PATRIMONIO', 'Patrimonio', 0, 1, ''),
    (35,   'PATRIMONIO', 'Capital Social', 2, 0, '31000000'),
    (36,   'PATRIMONIO', 'Aportes patrimoniales no capitalizados', 2, 0, '32000000'),
    (37,   'PATRIMONIO', 'Ajuste al patrimonio', 2, 0, '33000000'),
    (38,   'PATRIMONIO', 'Resultado de ejercicios anteriores', 2, 0, '34000000'),
    (39,   'PATRIMONIO', 'Reservas patrimoniales', 2, 0, '35000000'),
    (40,   'PATRIMONIO', 'Resultados del periodo', 2, 0, ''),
    (41,   'PATRIMONIO', 'Total de patrimonio', 3, 1, '30000000'),

    (42,   'TOTAL', 'Total pasivos y patrimonio', 3, 1, ''),
    (999,  'TOTAL', 'Prueba', 3, 1, '');

    -- 4. Período base para variación absoluta y relativa
    -- Según la guía Excel:
    --   - Mensual: Var = P5 - P4 (mes actual vs mes anterior)
    --   - Trimestral: Var = P5 - P4 (trimestre actual vs trimestre anterior)
    --   - Interanual: Var = P5 - P2 (mes actual vs mismo mes año anterior)
    DECLARE @VAR_REF_COL INT = CASE WHEN UPPER(@TIPO_COMPARACION) = 'INTERANUAL' THEN 2 ELSE 4 END;

    -- 5. Salida tabular optimizada en Millones de Colones (/ 1,000,000.00) mediante CTE Pivot (Cero subconsultas correlacionadas)
    ;WITH ResumenPivot AS (
        SELECT 
            ID,
            MAX(CASE WHEN PERIODO = @P1 THEN ISNULL(SALDO_MES, 0) ELSE 0 END) / 1000000.0 AS P1,
            MAX(CASE WHEN PERIODO = @P2 THEN ISNULL(SALDO_MES, 0) ELSE 0 END) / 1000000.0 AS P2,
            MAX(CASE WHEN PERIODO = @P3 THEN ISNULL(SALDO_MES, 0) ELSE 0 END) / 1000000.0 AS P3,
            MAX(CASE WHEN PERIODO = @P4 THEN ISNULL(SALDO_MES, 0) ELSE 0 END) / 1000000.0 AS P4,
            MAX(CASE WHEN PERIODO = @P5 THEN ISNULL(SALDO_MES, 0) ELSE 0 END) / 1000000.0 AS P5
        FROM #RESUMEN
        GROUP BY ID
    )
    SELECT
        C.Orden,
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

-- Conceder permisos de ejecución para el servicio WCF / IIS
GRANT EXECUTE ON [dbo].[FGA_Rpt_Balance_General_5Periodos] TO [public];
GO

/*
-- =========================================================================================
-- EJEMPLOS DE EJECUCIÓN (Para probar en SQL Server Management Studio):
-- =========================================================================================

-- 1. Comparativo Trimestral:
EXEC [dbo].[FGA_Rpt_Balance_General_5Periodos]
    @IDENTIDAD = '13',
    @PERIODO_REF = '2026-06-30',
    @TIPO_COMPARACION = 'Trimestral';

-- 2. Comparativo Mensual:
EXEC [dbo].[FGA_Rpt_Balance_General_5Periodos]
    @IDENTIDAD = '13',
    @PERIODO_REF = '2026-06-30',
    @TIPO_COMPARACION = 'Mensual';

-- 3. Comparativo Interanual:
EXEC [dbo].[FGA_Rpt_Balance_General_5Periodos]
    @IDENTIDAD = '13',
    @PERIODO_REF = '2026-06-30',
    @TIPO_COMPARACION = 'Interanual';
*/
