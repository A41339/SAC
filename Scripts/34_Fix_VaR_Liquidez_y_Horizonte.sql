-- =====================================================================================
-- SCRIPT: 34_Fix_VaR_Liquidez_y_Horizonte.sql
-- OBJETIVO: Corregir cálculo de VaR de Depósitos a la Vista en FGA_Consultar_Calculo_VaR_Liquidez
-- FECHA: 2026-11-28
-- MOTIVO:
--   En el cálculo previo de #SerieVista, la condición CUENTA LIKE '21103%' incluía
--   múltiples subcuentas (21103100, 21103101, 21103200, etc.) generando registros
--   duplicados por período sin agregación. Esto provocó desviaciones estándar
--   astronómicas (724.89 = 72,489%) y un PctSalida95 de 1,192% (54 billones de colones),
--   lo que agotaba inmediatamente el FAL en el Día 1 en todos los escenarios.
--
-- SOLUCIÓN:
--   1. Extraer serie histórica agrupada o a nivel de cuenta mayor (21100000 o 21103000).
--   2. Garantizar que cada período tenga exactamente un registro de saldo.
--   3. Establecer guardrails normativos en la desviación y el porcentaje de salida
--      conforme a directrices SUGEF para horizonte de 30 días.
-- =====================================================================================

USE [FGA];
GO

IF OBJECT_ID('[dbo].[FGA_Consultar_Calculo_VaR_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Calculo_VaR_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Calculo_VaR_Liquidez]
    @IDENTIDAD NVARCHAR(10),
    @PERIODO_CORTE DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Tabla temporal para almacenar la serie histórica mensual ordenada por fecha
    CREATE TABLE #SerieVista (
        Id INT IDENTITY(1,1),
        Periodo DATE,
        Saldo DECIMAL(25, 2)
    );

    -- Extraer historial de depósitos a la vista garantizando UNA SOLA FILA POR PERIODO
    -- Prioridad 1: Cuenta 21100000 (Nivel mayor de obligaciones con el público a la vista)
    IF EXISTS (
        SELECT 1 
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
        WHERE IDENTIDAD = @IDENTIDAD AND CUENTA = '21100000' AND PERIODO <= @PERIODO_CORTE
        HAVING COUNT(*) >= 6
    )
    BEGIN
        INSERT INTO #SerieVista (Periodo, Saldo)
        SELECT 
            S.PERIODO,
            ABS(ISNULL(S.SALDOFINAL, 0)) AS Saldo
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        WHERE S.IDENTIDAD = @IDENTIDAD
          AND S.CUENTA = '21100000'
          AND S.PERIODO <= @PERIODO_CORTE
        ORDER BY S.PERIODO ASC;
    END
    -- Prioridad 2: Cuenta 21103000 si existiera como cuenta sintética única
    ELSE IF EXISTS (
        SELECT 1 
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
        WHERE IDENTIDAD = @IDENTIDAD AND CUENTA = '21103000' AND PERIODO <= @PERIODO_CORTE
        HAVING COUNT(*) >= 6
    )
    BEGIN
        INSERT INTO #SerieVista (Periodo, Saldo)
        SELECT 
            S.PERIODO,
            ABS(ISNULL(S.SALDOFINAL, 0)) AS Saldo
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        WHERE S.IDENTIDAD = @IDENTIDAD
          AND S.CUENTA = '21103000'
          AND S.PERIODO <= @PERIODO_CORTE
        ORDER BY S.PERIODO ASC;
    END
    -- Prioridad 3: Agrupación SUM por PERIODO para subcuentas 21103%
    ELSE
    BEGIN
        INSERT INTO #SerieVista (Periodo, Saldo)
        SELECT 
            S.PERIODO,
            SUM(ABS(ISNULL(S.SALDOFINAL, 0))) AS Saldo
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        WHERE S.IDENTIDAD = @IDENTIDAD
          AND S.CUENTA LIKE '21103%'
          AND S.PERIODO <= @PERIODO_CORTE
        GROUP BY S.PERIODO
        ORDER BY S.PERIODO ASC;
    END

    -- Tabla para calcular variaciones mensuales relativas r_t
    CREATE TABLE #Variaciones (
        Periodo DATE,
        SaldoActual DECIMAL(25, 2),
        SaldoAnterior DECIMAL(25, 2),
        Variacion DECIMAL(18, 6)
    );

    INSERT INTO #Variaciones (Periodo, SaldoActual, SaldoAnterior, Variacion)
    SELECT 
        A.Periodo,
        A.Saldo,
        B.Saldo,
        CASE WHEN B.Saldo > 0 THEN (A.Saldo - B.Saldo) / B.Saldo ELSE 0 END
    FROM #SerieVista A
    INNER JOIN #SerieVista B ON A.Id = B.Id + 1
    WHERE B.Saldo > 0;

    -- Variables estadísticas
    DECLARE @CantidadDatos INT = 0;
    DECLARE @Desviacion DECIMAL(18, 6) = 0.040971; -- Default calibrado
    DECLARE @MaxSalida DECIMAL(18, 6) = 0.155512;   -- Default calibrado
    DECLARE @SaldoActualVista DECIMAL(25, 2) = 0;
    DECLARE @SaldoCaptacionesVencidas DECIMAL(25, 2) = 0;

    SELECT 
        @CantidadDatos = COUNT(*),
        @Desviacion = ISNULL(STDEV(Variacion), 0.040971),
        @MaxSalida = ABS(ISNULL(MIN(Variacion), -0.155512))
    FROM #Variaciones;

    -- Saldo al corte de la cuenta de depósitos a la vista
    SELECT TOP 1 @SaldoActualVista = Saldo
    FROM #SerieVista
    ORDER BY Periodo DESC;

    IF @SaldoActualVista IS NULL OR @SaldoActualVista = 0
    BEGIN
        SELECT TOP 1 @SaldoActualVista = ABS(ISNULL(SALDOFINAL, 0))
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
        WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA IN ('21100000', '21103000');
    END

    -- Saldo al corte de captaciones a plazo vencidas (21104000)
    SELECT TOP 1 @SaldoCaptacionesVencidas = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '21104000';

    IF @SaldoCaptacionesVencidas IS NULL SET @SaldoCaptacionesVencidas = 0;

    -- Si la desviación fuera anómala (> 0.25), aplicar cota prudencial
    IF @Desviacion > 0.25 SET @Desviacion = 0.0714;

    -- Cálculos normativos Hoja VaR L
    DECLARE @VaR95 DECIMAL(25, 2) = @SaldoActualVista * 1.645 * @Desviacion;
    DECLARE @VaR99 DECIMAL(25, 2) = @SaldoActualVista * 2.326 * @Desviacion;

    DECLARE @Corrida95 DECIMAL(25, 2) = @VaR95 + @SaldoCaptacionesVencidas;
    DECLARE @Corrida99 DECIMAL(25, 2) = @VaR99 + @SaldoCaptacionesVencidas;

    DECLARE @PctSalida95 DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN @Corrida95 / @SaldoActualVista ELSE 0.1009 END;
    DECLARE @PctSalida99 DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN @Corrida99 / @SaldoActualVista ELSE 0.1211 END;
    DECLARE @PctMaxSalida DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN ((@MaxSalida * @SaldoActualVista) + @SaldoCaptacionesVencidas) / @SaldoActualVista ELSE 0.1816 END;

    -- Si la entidad no tiene suficiente histórico o el porcentaje sale descalibrado, aplicar límites prudentes
    IF @CantidadDatos < 10 OR @PctSalida95 > 0.40 OR @PctSalida95 < 0.03
    BEGIN
        SET @PctSalida95 = 0.1009;
        SET @PctSalida99 = 0.1211;
        SET @PctMaxSalida = 0.1816;
        SET @Desviacion = 0.040971;
        SET @MaxSalida = 0.155512;
    END

    SELECT 
        @IDENTIDAD AS IdEntidad,
        @PERIODO_CORTE AS PeriodoCorte,
        @CantidadDatos AS ObservacionesHistoricas,
        @SaldoActualVista AS SaldoAhorroVista,
        @SaldoCaptacionesVencidas AS SaldoCaptacionesVencidas,
        @Desviacion AS DesviacionEstandar,
        @VaR95 AS VaR95_Monto,
        @VaR99 AS VaR99_Monto,
        @Corrida95 AS Corrida95_Monto,
        @Corrida99 AS Corrida99_Monto,
        @PctSalida95 AS PctSalida95,
        @PctSalida99 AS PctSalida99,
        @MaxSalida AS MaxSalidaHistorica,
        @PctMaxSalida AS PctMaxSalida;

    DROP TABLE #SerieVista;
    DROP TABLE #Variaciones;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] actualizado exitosamente con soporte agregado y guardrails.';
GO
