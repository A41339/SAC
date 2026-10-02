-- =========================================================================================
-- SCRIPT: 05_FGA_Rpt_Balanza_Comprobacion_5Periodos.sql
-- Base de Datos: FFC (o FGA)
-- Objetivo: Stored Procedure optimizado para la Balanza de Comprobación a 5 períodos
--           con soporte para modalidad Mensual vs Acumulado y variaciones.
-- =========================================================================================

USE [FFC];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[FGA_Rpt_Balanza_Comprobacion_5Periodos]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_REF DATETIME,
    @MODALIDAD NVARCHAR(20) = 'Acumulado',       -- 'Acumulado' o 'Mensual'
    @TIPO_COMPARACION NVARCHAR(20) = 'Interanual' -- 'Interanual', 'Trimestral', 'Mensual'
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99'
    BEGIN
        SET @IDENTIDAD = '13';
    END

    -- 1. Normalización de fecha de referencia
    DECLARE @REF_DATE DATE = CAST(@PERIODO_REF AS DATE);
    DECLARE @Y INT = YEAR(@REF_DATE);
    DECLARE @M INT = MONTH(@REF_DATE);

    -- Determinar los 5 períodos
    DECLARE @P1 DATE;
    DECLARE @P2 DATE;
    DECLARE @P3 DATE;
    DECLARE @P4 DATE;
    DECLARE @P5 DATE;

    IF @TIPO_COMPARACION = 'Mensual'
    BEGIN
        @P5 = DATEFROMPARTS(@Y, @M, 1);
        @P4 = DATEADD(MONTH, -1, @P5);
        @P3 = DATEADD(MONTH, -2, @P5);
        @P2 = DATEADD(MONTH, -3, @P5);
        @P1 = DATEADD(MONTH, -4, @P5);
    END
    ELSE IF @TIPO_COMPARACION = 'Trimestral'
    BEGIN
        DECLARE @qMonth INT = ((@M - 1) / 3 + 1) * 3;
        @P5 = DATEFROMPARTS(@Y, @qMonth, 1);
        @P4 = DATEADD(MONTH, -3, @P5);
        @P3 = DATEADD(MONTH, -6, @P5);
        @P2 = DATEADD(MONTH, -9, @P5);
        @P1 = DATEADD(MONTH, -12, @P5);
    END
    ELSE -- Interanual (Default)
    BEGIN
        @P5 = DATEFROMPARTS(@Y, @M, 1);
        @P4 = DATEADD(YEAR, -1, @P5);
        @P3 = DATEADD(YEAR, -2, @P5);
        @P2 = DATEADD(YEAR, -3, @P5);
        @P1 = DATEADD(YEAR, -4, @P5);
    END;

    -- 2. CTE de fechas normalizadas (soporta tanto primer día de mes como fin de mes en BD)
    WITH FechasClave AS (
        SELECT 1 AS Slot, @P1 AS FechaInicio, EOMONTH(@P1) AS FechaFin UNION ALL
        SELECT 2 AS Slot, @P2 AS FechaInicio, EOMONTH(@P2) AS FechaFin UNION ALL
        SELECT 3 AS Slot, @P3 AS FechaInicio, EOMONTH(@P3) AS FechaFin UNION ALL
        SELECT 4 AS Slot, @P4 AS FechaInicio, EOMONTH(@P4) AS FechaFin UNION ALL
        SELECT 5 AS Slot, @P5 AS FechaInicio, EOMONTH(@P5) AS FechaFin
    ),
    -- 3. Movimientos deduplicados de la Balanza de Comprobación
    MovimientosSaldos AS (
        SELECT 
            fc.Slot,
            s.CUENTA,
            -- Según modalidad: Mensual = SaldoFinal (movimiento neto mes) / Acumulado = SaldoAcumulado
            CASE 
                WHEN UPPER(@MODALIDAD) = 'MENSUAL' THEN ISNULL(s.SALDOFINAL, 0)
                ELSE ISNULL(s.SALDOACUMULADO, s.SALDOFINAL)
            END AS Saldo,
            ROW_NUMBER() OVER (
                PARTITION BY fc.Slot, s.CUENTA 
                ORDER BY s.PERIODO DESC
            ) AS rn
        FROM dbo.SALIDA_BALANCE_COMPROBACION s WITH(NOLOCK)
        INNER JOIN FechasClave fc 
            ON s.PERIODO >= fc.FechaInicio AND s.PERIODO <= fc.FechaFin
        WHERE (@IDENTIDAD = '-1' OR s.IDENTIDAD = @IDENTIDAD)
    ),
    SaldosDeduplicados AS (
        SELECT Slot, CUENTA, Saldo
        FROM MovimientosSaldos
        WHERE rn = 1
    ),
    -- 4. Pivot de saldos a 5 períodos en millones de colones
    PivotSaldos AS (
        SELECT 
            CUENTA,
            ISNULL([1], 0) / 1000000.0 AS P1,
            ISNULL([2], 0) / 1000000.0 AS P2,
            ISNULL([3], 0) / 1000000.0 AS P3,
            ISNULL([4], 0) / 1000000.0 AS P4,
            ISNULL([5], 0) / 1000000.0 AS P5
        FROM SaldosDeduplicados
        PIVOT (
            SUM(Saldo) FOR Slot IN ([1], [2], [3], [4], [5])
        ) pvt
    ),
    -- 5. Catálogo de Cuentas completo combinado con los saldos
    CuentasConSaldos AS (
        SELECT 
            c.CUENTA AS Cuenta,
            c.NOMBRE AS Nombre,
            ISNULL(c.NIVEL, 
                CASE 
                    WHEN LEN(RTRIM(c.CUENTA)) <= 2 THEN 1
                    WHEN LEN(RTRIM(c.CUENTA)) <= 4 THEN 2
                    WHEN LEN(RTRIM(c.CUENTA)) <= 6 THEN 3
                    ELSE 4
                END
            ) AS Nivel,
            c.PADRE AS Padre,
            ISNULL(p.P1, 0) AS P1,
            ISNULL(p.P2, 0) AS P2,
            ISNULL(p.P3, 0) AS P3,
            ISNULL(p.P4, 0) AS P4,
            ISNULL(p.P5, 0) AS P5
        FROM dbo.CATALOGOCUENTA c WITH(NOLOCK)
        LEFT JOIN PivotSaldos p ON c.CUENTA = p.CUENTA
    )
    SELECT 
        CAST(ROW_NUMBER() OVER (ORDER BY c.Cuenta ASC) AS INT) AS Orden,
        c.Cuenta,
        c.Nombre,
        c.Nivel,
        c.Padre,
        CAST(ROUND(c.P1, 2) AS DECIMAL(18, 2)) AS P1,
        CAST(ROUND(c.P2, 2) AS DECIMAL(18, 2)) AS P2,
        CAST(ROUND(c.P3, 2) AS DECIMAL(18, 2)) AS P3,
        CAST(ROUND(c.P4, 2) AS DECIMAL(18, 2)) AS P4,
        CAST(ROUND(c.P5, 2) AS DECIMAL(18, 2)) AS P5,
        -- Variación Absoluta = P5 - P4
        CAST(ROUND(c.P5 - c.P4, 2) AS DECIMAL(18, 2)) AS VariacionAbsoluta,
        -- Variación Relativa = (P5 - P4) / |P4| * 100
        CAST(ROUND(
            CASE 
                WHEN c.P4 != 0 THEN ((c.P5 - c.P4) / ABS(c.P4)) * 100.0
                ELSE 0.0 
            END, 2
        ) AS DECIMAL(18, 2)) AS VariacionRelativa
    FROM CuentasConSaldos c
    -- Filtrar únicamente cuentas que tengan saldo en alguno de los 5 períodos o que sean cuentas mayores/padres
    WHERE (ABS(c.P1) > 0.001 OR ABS(c.P2) > 0.001 OR ABS(c.P3) > 0.001 OR ABS(c.P4) > 0.001 OR ABS(c.P5) > 0.001 OR c.Nivel <= 2)
    ORDER BY c.Cuenta ASC;
END;
GO

-- Permisos de Ejecución
GRANT EXECUTE ON [dbo].[FGA_Rpt_Balanza_Comprobacion_5Periodos] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Rpt_Balanza_Comprobacion_5Periodos] TO [public];
GO
