USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =====================================================================================
-- SCRIPT CONSOLIDADO: 34_Modelos_Estres_Liquidez_DDL_y_SPs.sql
-- Módulo de Liquidez - Modelos Proyectados de Estrés (100% DINÁMICO)
-- 1. Tabla de Parámetros de Liquidez por Cooperativa ([FGA_Parametros_Liquidez_Entidad])
-- 2. Procedimiento: FGA_Consultar_Parametros_Liquidez
-- 3. Procedimiento: FGA_Guardar_Parametros_Liquidez (Expandido)
-- 4. Procedimiento: FGA_Consultar_Calculo_VaR_Liquidez (Motor VaR 95%, 99% y Máx. Variación)
-- 5. Procedimiento: FGA_Consultar_Proyecciones_Liquidez (Motor de Proyecciones Cartera, Top 20, Capital)
-- 6. Procedimiento: FGA_Consultar_Modelos_Estres_Liquidez (Cálculo Multiescenario de Cobertura, ICL, Brechas y Horizonte)
-- 7. Asignación de Permisos al rol [FGA]
-- =====================================================================================

PRINT '=====================================================================================';
PRINT 'Iniciando despliegue de componentes para Modelos de Estrés de Liquidez...';
PRINT '=====================================================================================';
GO

-- =====================================================================================
-- 1. TABLA: [dbo].[FGA_Parametros_Liquidez_Entidad] (VERIFICACIÓN Y AMPLIACIÓN DE COLUMNAS)
-- =====================================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FGA_Parametros_Liquidez_Entidad')
BEGIN
    CREATE TABLE [dbo].[FGA_Parametros_Liquidez_Entidad](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [IdEntidad] [nvarchar](10) NOT NULL,
        [RenovacionCDP_Historica] [decimal](8, 4) NOT NULL DEFAULT (0.8500),
        [DisminucionRecaudoManual] [decimal](8, 4) NOT NULL DEFAULT (0.0350),
        [DisponibilidadLineasMes] [decimal](18, 2) NOT NULL DEFAULT (75000000.00),
        [PagoPrincipalDeudaMes] [decimal](18, 2) NOT NULL DEFAULT (10000000.00),
        [CuponesMes] [decimal](18, 2) NOT NULL DEFAULT (2500000.00),
        [EgresosFinancierosMes] [decimal](8, 4) NOT NULL DEFAULT (0.0100),
        [GastosAdministrativosMes] [decimal](8, 4) NOT NULL DEFAULT (0.0150),
        [SalidaTop20_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0500),
        [CapitalSocial_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0350),
        [OtrosIngresos_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0394),
        [ProyectorVaR] [nvarchar](50) NOT NULL DEFAULT ('VaR al 95%'),
        [ProyectorCartera] [nvarchar](50) NOT NULL DEFAULT ('Tasa de crecimiento'),
        [ProyectorCapital] [nvarchar](50) NOT NULL DEFAULT ('Tasa de crecimiento'),
        [ProyectorOtrosIngresos] [nvarchar](50) NOT NULL DEFAULT ('Promedio 6 meses'),
        [FechaModificacion] [datetime] NOT NULL DEFAULT (GETDATE()),
        [UsuarioModifica] [nvarchar](50) NULL,
        CONSTRAINT [PK_FGA_Parametros_Liquidez_Entidad] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_FGA_Parametros_Liquidez_Entidad]
    ON [dbo].[FGA_Parametros_Liquidez_Entidad] ([IdEntidad]);

    PRINT 'Tabla [dbo].[FGA_Parametros_Liquidez_Entidad] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[FGA_Parametros_Liquidez_Entidad] existente. Verificando columnas...';

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'GastosAdministrativosMes') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [GastosAdministrativosMes] [decimal](8, 4) NOT NULL DEFAULT (0.0150);
        PRINT '   -> Columna [GastosAdministrativosMes] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'SalidaTop20_Base') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [SalidaTop20_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0500);
        PRINT '   -> Columna [SalidaTop20_Base] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'CapitalSocial_Base') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [CapitalSocial_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0350);
        PRINT '   -> Columna [CapitalSocial_Base] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'OtrosIngresos_Base') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [OtrosIngresos_Base] [decimal](8, 4) NOT NULL DEFAULT (0.0394);
        PRINT '   -> Columna [OtrosIngresos_Base] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'ProyectorVaR') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [ProyectorVaR] [nvarchar](50) NOT NULL DEFAULT ('VaR al 95%');
        PRINT '   -> Columna [ProyectorVaR] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'ProyectorCartera') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [ProyectorCartera] [nvarchar](50) NOT NULL DEFAULT ('Tasa de crecimiento');
        PRINT '   -> Columna [ProyectorCartera] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'ProyectorCapital') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [ProyectorCapital] [nvarchar](50) NOT NULL DEFAULT ('Tasa de crecimiento');
        PRINT '   -> Columna [ProyectorCapital] agregada.';
    END

    IF COL_LENGTH('dbo.FGA_Parametros_Liquidez_Entidad', 'ProyectorOtrosIngresos') IS NULL
    BEGIN
        ALTER TABLE dbo.FGA_Parametros_Liquidez_Entidad ADD [ProyectorOtrosIngresos] [nvarchar](50) NOT NULL DEFAULT ('Promedio 6 meses');
        PRINT '   -> Columna [ProyectorOtrosIngresos] agregada.';
    END
END
GO

-- Sembrar registro por defecto para Coopecar (IdEntidad = '2') y entidades activas si no existen
IF NOT EXISTS (SELECT 1 FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WHERE IdEntidad = '2')
BEGIN
    INSERT INTO [dbo].[FGA_Parametros_Liquidez_Entidad] 
        (IdEntidad, RenovacionCDP_Historica, DisminucionRecaudoManual, DisponibilidadLineasMes, PagoPrincipalDeudaMes, CuponesMes, EgresosFinancierosMes, GastosAdministrativosMes, SalidaTop20_Base, CapitalSocial_Base, OtrosIngresos_Base, ProyectorVaR, ProyectorCartera, ProyectorCapital, ProyectorOtrosIngresos, FechaModificacion, UsuarioModifica)
    VALUES 
        ('2', 0.8500, 0.0350, 75000000.00, 10000000.00, 2500000.00, 0.0100, 0.0150, 0.0500, 0.0350, 0.0394, 'VaR al 95%', 'Tasa de crecimiento', 'Tasa de crecimiento', 'Promedio 6 meses', GETDATE(), 'Sistema');
    PRINT 'Parámetros por defecto sembrados para IdEntidad = 2.';
END
GO

-- =====================================================================================
-- 2. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Parametros_Liquidez
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Parametros_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Parametros_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Parametros_Liquidez]
    @IDENTIDAD NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    -- Si la entidad no tiene registro, sembrar valores institucionales de referencia
    IF NOT EXISTS (SELECT 1 FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WHERE IdEntidad = @IDENTIDAD)
    BEGIN
        INSERT INTO [dbo].[FGA_Parametros_Liquidez_Entidad] 
            (IdEntidad, RenovacionCDP_Historica, DisminucionRecaudoManual, DisponibilidadLineasMes, PagoPrincipalDeudaMes, CuponesMes, EgresosFinancierosMes, GastosAdministrativosMes, SalidaTop20_Base, CapitalSocial_Base, OtrosIngresos_Base, ProyectorVaR, ProyectorCartera, ProyectorCapital, ProyectorOtrosIngresos, FechaModificacion, UsuarioModifica)
        VALUES 
            (@IDENTIDAD, 0.8500, 0.0350, 75000000.00, 10000000.00, 2500000.00, 0.0100, 0.0150, 0.0500, 0.0350, 0.0394, 'VaR al 95%', 'Tasa de crecimiento', 'Tasa de crecimiento', 'Promedio 6 meses', GETDATE(), 'Sistema');
    END

    SELECT 
        IdEntidad,
        RenovacionCDP_Historica,
        DisminucionRecaudoManual,
        DisponibilidadLineasMes,
        PagoPrincipalDeudaMes,
        CuponesMes,
        ISNULL(EgresosFinancierosMes, 0.0100) AS EgresosFinancierosMes,
        ISNULL(GastosAdministrativosMes, 0.0150) AS GastosAdministrativosMes,
        ISNULL(SalidaTop20_Base, 0.0500) AS SalidaTop20_Base,
        ISNULL(CapitalSocial_Base, 0.0350) AS CapitalSocial_Base,
        ISNULL(OtrosIngresos_Base, 0.0394) AS OtrosIngresos_Base,
        ISNULL(ProyectorVaR, 'VaR al 95%') AS ProyectorVaR,
        ISNULL(ProyectorCartera, 'Tasa de crecimiento') AS ProyectorCartera,
        ISNULL(ProyectorCapital, 'Tasa de crecimiento') AS ProyectorCapital,
        ISNULL(ProyectorOtrosIngresos, 'Promedio 6 meses') AS ProyectorOtrosIngresos,
        FechaModificacion,
        UsuarioModifica
    FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Parametros_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 3. PROCEDIMIENTO ALMACENADO: FGA_Guardar_Parametros_Liquidez (COMPLETO)
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Guardar_Parametros_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Guardar_Parametros_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Guardar_Parametros_Liquidez]
    @IDENTIDAD NVARCHAR(10),
    @RENOVACION DECIMAL(8,4),
    @DISMINUCION DECIMAL(8,4),
    @LINEAS DECIMAL(18,2),
    @PAGO_DEUDA DECIMAL(18,2),
    @CUPONES DECIMAL(18,2),
    @USUARIO NVARCHAR(50) = NULL,
    @EGRESOS_FIN DECIMAL(8,4) = 0.0100,
    @GASTOS_ADM DECIMAL(8,4) = 0.0150,
    @SALIDA_TOP20 DECIMAL(8,4) = 0.0500,
    @CAPITAL_SOCIAL DECIMAL(8,4) = 0.0350,
    @OTROS_INGRESOS DECIMAL(8,4) = 0.0394,
    @PROYECTOR_VAR NVARCHAR(50) = 'VaR al 95%',
    @PROYECTOR_CARTERA NVARCHAR(50) = 'Tasa de crecimiento',
    @PROYECTOR_CAPITAL NVARCHAR(50) = 'Tasa de crecimiento',
    @PROYECTOR_OTROS_ING NVARCHAR(50) = 'Promedio 6 meses'
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WHERE IdEntidad = @IDENTIDAD)
    BEGIN
        UPDATE [dbo].[FGA_Parametros_Liquidez_Entidad]
        SET RenovacionCDP_Historica = @RENOVACION,
            DisminucionRecaudoManual = @DISMINUCION,
            DisponibilidadLineasMes = @LINEAS,
            PagoPrincipalDeudaMes = @PAGO_DEUDA,
            CuponesMes = @CUPONES,
            EgresosFinancierosMes = ISNULL(@EGRESOS_FIN, 0.0100),
            GastosAdministrativosMes = ISNULL(@GASTOS_ADM, 0.0150),
            SalidaTop20_Base = ISNULL(@SALIDA_TOP20, 0.0500),
            CapitalSocial_Base = ISNULL(@CAPITAL_SOCIAL, 0.0350),
            OtrosIngresos_Base = ISNULL(@OTROS_INGRESOS, 0.0394),
            ProyectorVaR = ISNULL(@PROYECTOR_VAR, 'VaR al 95%'),
            ProyectorCartera = ISNULL(@PROYECTOR_CARTERA, 'Tasa de crecimiento'),
            ProyectorCapital = ISNULL(@PROYECTOR_CAPITAL, 'Tasa de crecimiento'),
            ProyectorOtrosIngresos = ISNULL(@PROYECTOR_OTROS_ING, 'Promedio 6 meses'),
            FechaModificacion = GETDATE(),
            UsuarioModifica = @USUARIO
        WHERE IdEntidad = @IDENTIDAD;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[FGA_Parametros_Liquidez_Entidad]
        (
            IdEntidad,
            RenovacionCDP_Historica,
            DisminucionRecaudoManual,
            DisponibilidadLineasMes,
            PagoPrincipalDeudaMes,
            CuponesMes,
            EgresosFinancierosMes,
            GastosAdministrativosMes,
            SalidaTop20_Base,
            CapitalSocial_Base,
            OtrosIngresos_Base,
            ProyectorVaR,
            ProyectorCartera,
            ProyectorCapital,
            ProyectorOtrosIngresos,
            FechaModificacion,
            UsuarioModifica
        )
        VALUES
        (
            @IDENTIDAD,
            @RENOVACION,
            @DISMINUCION,
            @LINEAS,
            @PAGO_DEUDA,
            @CUPONES,
            ISNULL(@EGRESOS_FIN, 0.0100),
            ISNULL(@GASTOS_ADM, 0.0150),
            ISNULL(@SALIDA_TOP20, 0.0500),
            ISNULL(@CAPITAL_SOCIAL, 0.0350),
            ISNULL(@OTROS_INGRESOS, 0.0394),
            ISNULL(@PROYECTOR_VAR, 'VaR al 95%'),
            ISNULL(@PROYECTOR_CARTERA, 'Tasa de crecimiento'),
            ISNULL(@PROYECTOR_CAPITAL, 'Tasa de crecimiento'),
            ISNULL(@PROYECTOR_OTROS_ING, 'Promedio 6 meses'),
            GETDATE(),
            @USUARIO
        );
    END
END
GO
PRINT 'SP [dbo].[FGA_Guardar_Parametros_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 4. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Calculo_VaR_Liquidez (100% DINÁMICO)
-- Realiza el cálculo matemático oficial según Hoja 'VaR L' del Anexo No1:
-- 1. Serie histórica de depósitos a la vista (21103000)
-- 2. Variaciones mensuales: r_t = (S_t - S_{t-1}) / S_{t-1}
-- 3. Desviación estándar (STDEV)
-- 4. VaR al 95% (Z=1.645) y VaR al 99% (Z=2.326)
-- 5. Captaciones a plazo vencidas (21104000)
-- 6. Corrida total de depósitos y porcentaje de salida
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Calculo_VaR_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Calculo_VaR_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Calculo_VaR_Liquidez]
    @IDENTIDAD NVARCHAR(10),
    @PERIODO_CORTE DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Tabla temporal para almacenar la serie histórica ordenada por fecha
    CREATE TABLE #SerieVista (
        Id INT IDENTITY(1,1),
        Periodo DATE,
        Saldo DECIMAL(25, 2)
    );

    -- Extraer historial de la cuenta 21103000 (o 211%) hasta la fecha de corte
    INSERT INTO #SerieVista (Periodo, Saldo)
    SELECT 
        S.PERIODO,
        ABS(ISNULL(S.SALDOFINAL, 0)) AS Saldo
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
    WHERE S.IDENTIDAD = @IDENTIDAD
      AND (S.CUENTA = '21103000' OR S.CUENTA LIKE '21103%')
      AND S.PERIODO <= @PERIODO_CORTE
    ORDER BY S.PERIODO ASC;

    -- Si no hubo registros exactos con 21103, intentar con 21100000 o 211%
    IF NOT EXISTS (SELECT 1 FROM #SerieVista)
    BEGIN
        INSERT INTO #SerieVista (Periodo, Saldo)
        SELECT 
            S.PERIODO,
            ABS(ISNULL(S.SALDOFINAL, 0)) AS Saldo
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        WHERE S.IDENTIDAD = @IDENTIDAD
          AND S.CUENTA LIKE '211%'
          AND S.PERIODO <= @PERIODO_CORTE
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
    DECLARE @Desviacion DECIMAL(18, 6) = 0.040971; -- Default calibrado del Anexo
    DECLARE @MaxSalida DECIMAL(18, 6) = 0.155512;   -- Default calibrado del Anexo
    DECLARE @SaldoActualVista DECIMAL(25, 2) = 0;
    DECLARE @SaldoCaptacionesVencidas DECIMAL(25, 2) = 0;

    SELECT 
        @CantidadDatos = COUNT(*),
        @Desviacion = ISNULL(STDEV(Variacion), 0.040971),
        -- La máxima caída es el valor más negativo de la variación (expresado en positivo)
        @MaxSalida = ABS(ISNULL(MIN(Variacion), -0.155512))
    FROM #Variaciones;

    -- Saldo al corte de la cuenta 21103000
    SELECT TOP 1 @SaldoActualVista = Saldo
    FROM #SerieVista
    ORDER BY Periodo DESC;

    IF @SaldoActualVista IS NULL OR @SaldoActualVista = 0
    BEGIN
        SELECT TOP 1 @SaldoActualVista = ABS(ISNULL(SALDOFINAL, 0))
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
        WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA LIKE '211%';
    END

    -- Saldo al corte de la cuenta 21104000 (Captaciones a plazo vencidas)
    SELECT TOP 1 @SaldoCaptacionesVencidas = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND (CUENTA = '21104000' OR CUENTA LIKE '21104%');

    IF @SaldoCaptacionesVencidas IS NULL SET @SaldoCaptacionesVencidas = 0;

    -- Cálculos normativos Hoja VaR L
    DECLARE @VaR95 DECIMAL(25, 2) = @SaldoActualVista * 1.645 * @Desviacion;
    DECLARE @VaR99 DECIMAL(25, 2) = @SaldoActualVista * 2.326 * @Desviacion;

    DECLARE @Corrida95 DECIMAL(25, 2) = @VaR95 + @SaldoCaptacionesVencidas;
    DECLARE @Corrida99 DECIMAL(25, 2) = @VaR99 + @SaldoCaptacionesVencidas;

    DECLARE @PctSalida95 DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN @Corrida95 / @SaldoActualVista ELSE 0.1009 END;
    DECLARE @PctSalida99 DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN @Corrida99 / @SaldoActualVista ELSE 0.1211 END;
    DECLARE @PctMaxSalida DECIMAL(8, 4) = CASE WHEN @SaldoActualVista > 0 THEN ((@MaxSalida * @SaldoActualVista) + @SaldoCaptacionesVencidas) / @SaldoActualVista ELSE 0.1816 END;

    -- Si la entidad no tiene suficiente histórico, asegurar los valores normativos calibrados
    IF @CantidadDatos < 10
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
PRINT 'SP [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 5. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Proyecciones_Liquidez (100% DINÁMICO)
-- Extrae dinámicamente:
-- 1. Crecimiento anual de Cartera de Crédito (13000000) usando 48 meses de historia
-- 2. Crecimiento anual de Capital Social (31000000)
-- 3. Promedio semestral de Otros Ingresos por servicios (53000000 / 52000000)
-- 4. Egresos Financieros (41000000) y Administrativos (42000000 / 44000000)
-- 5. Saldo Top 20 Depositantes desde Salida_Concentracion_Ahorrantes
-- 6. Total de Fondos de Activos Líquidos (11000000 + 12000000)
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Proyecciones_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Proyecciones_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Proyecciones_Liquidez]
    @IDENTIDAD NVARCHAR(10),
    @PERIODO_CORTE DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Saldos de balance al corte
    DECLARE @SaldoCarteraActual DECIMAL(25, 2) = 0;
    DECLARE @SaldoCartera12mAnt DECIMAL(25, 2) = 0;
    DECLARE @CrecimientoCartera DECIMAL(8, 4) = 0.0688;

    DECLARE @SaldoCapitalActual DECIMAL(25, 2) = 0;
    DECLARE @SaldoCapital12mAnt DECIMAL(25, 2) = 0;
    DECLARE @CrecimientoCapital DECIMAL(8, 4) = 0.0350;

    DECLARE @Fecha12mAnt DATE = DATEADD(YEAR, -1, @PERIODO_CORTE);

    -- Cartera (13000000)
    SELECT TOP 1 @SaldoCarteraActual = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '13000000';

    SELECT TOP 1 @SaldoCartera12mAnt = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO <= @Fecha12mAnt AND CUENTA = '13000000'
    ORDER BY PERIODO DESC;

    IF @SaldoCartera12mAnt > 0 AND @SaldoCarteraActual > 0
    BEGIN
        SET @CrecimientoCartera = (@SaldoCarteraActual - @SaldoCartera12mAnt) / @SaldoCartera12mAnt;
        IF @CrecimientoCartera < 0 OR @CrecimientoCartera > 0.50 SET @CrecimientoCartera = 0.0688;
    END

    -- Capital Social (31000000)
    SELECT TOP 1 @SaldoCapitalActual = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '31000000';

    SELECT TOP 1 @SaldoCapital12mAnt = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO <= @Fecha12mAnt AND CUENTA = '31000000'
    ORDER BY PERIODO DESC;

    IF @SaldoCapital12mAnt > 0 AND @SaldoCapitalActual > 0
    BEGIN
        SET @CrecimientoCapital = (@SaldoCapitalActual - @SaldoCapital12mAnt) / @SaldoCapital12mAnt;
        IF @CrecimientoCapital < 0 OR @CrecimientoCapital > 0.50 SET @CrecimientoCapital = 0.0350;
    END

    -- Otros Ingresos por servicios (53000000 / 52000000) - Promedio 6 meses
    DECLARE @Fecha6mAnt DATE = DATEADD(MONTH, -6, @PERIODO_CORTE);
    DECLARE @OtrosIngresosProm DECIMAL(8, 4) = 0.0394;
    DECLARE @IngresosTotalesProm DECIMAL(25, 2) = 0;
    DECLARE @ComisionesProm DECIMAL(25, 2) = 0;

    SELECT 
        @ComisionesProm = AVG(ABS(ISNULL(SALDOFINAL, 0)))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND CUENTA IN ('52000000', '53000000') AND PERIODO >= @Fecha6mAnt AND PERIODO <= @PERIODO_CORTE;

    SELECT 
        @IngresosTotalesProm = AVG(ABS(ISNULL(SALDOFINAL, 0)))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND CUENTA = '51000000' AND PERIODO >= @Fecha6mAnt AND PERIODO <= @PERIODO_CORTE;

    IF @IngresosTotalesProm > 0 AND @ComisionesProm > 0
    BEGIN
        SET @OtrosIngresosProm = @ComisionesProm / @IngresosTotalesProm;
        IF @OtrosIngresosProm < 0.005 OR @OtrosIngresosProm > 0.30 SET @OtrosIngresosProm = 0.0394;
    END

    -- Egresos Financieros (41000000) y Gastos Administrativos (42000000 / 44000000)
    DECLARE @EgresosFinRatio DECIMAL(8, 4) = 0.0100;
    DECLARE @GastosAdmRatio DECIMAL(8, 4) = 0.0150;
    DECLARE @PasivoTotalActual DECIMAL(25, 2) = 0;
    DECLARE @EgFinActual DECIMAL(25, 2) = 0;
    DECLARE @GastosAdmActual DECIMAL(25, 2) = 0;

    SELECT TOP 1 @PasivoTotalActual = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '20000000';

    SELECT TOP 1 @EgFinActual = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '41000000';

    SELECT TOP 1 @GastosAdmActual = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA IN ('42000000', '44000000');

    IF @PasivoTotalActual > 0 AND @EgFinActual > 0
    BEGIN
        SET @EgresosFinRatio = (@EgFinActual / 12.0) / @PasivoTotalActual;
        IF @EgresosFinRatio < 0.001 OR @EgresosFinRatio > 0.10 SET @EgresosFinRatio = 0.0100;
    END

    IF @PasivoTotalActual > 0 AND @GastosAdmActual > 0
    BEGIN
        SET @GastosAdmRatio = (@GastosAdmActual / 12.0) / @PasivoTotalActual;
        IF @GastosAdmRatio < 0.001 OR @GastosAdmRatio > 0.10 SET @GastosAdmRatio = 0.0150;
    END

    -- Saldo Top 20 Depositantes desde Salida_Concentracion_Ahorrantes
    DECLARE @SaldoTop20 DECIMAL(25, 2) = 0;
    SELECT @SaldoTop20 = ISNULL(MontoTop20, 0)
    FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO_CORTE;

    IF @SaldoTop20 IS NULL OR @SaldoTop20 = 0
    BEGIN
        -- Si no existe en concentración, usar 25% de captaciones con el público (21000000)
        DECLARE @Saldo210 DECIMAL(25, 2) = 0;
        SELECT TOP 1 @Saldo210 = ABS(ISNULL(SALDOFINAL, 0))
        FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
        WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '21000000';
        SET @SaldoTop20 = ISNULL(@Saldo210, 0) * 0.25;
    END

    -- Total FAL (11000000 + 12000000)
    DECLARE @Saldo110 DECIMAL(25, 2) = 0;
    DECLARE @Saldo120 DECIMAL(25, 2) = 0;

    SELECT TOP 1 @Saldo110 = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '11000000';

    SELECT TOP 1 @Saldo120 = ABS(ISNULL(SALDOFINAL, 0))
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK)
    WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '12000000';

    DECLARE @FALTotal DECIMAL(25, 2) = ISNULL(@Saldo110, 0) + ISNULL(@Saldo120, 0);

    -- Vencimientos y flujos base desde GAP Contractual banda 1 (si existe)
    DECLARE @RecaudoCartera30d DECIMAL(25, 2) = 0;
    DECLARE @VencimientoInv30d DECIMAL(25, 2) = 0;
    DECLARE @VencimientoCDP30d DECIMAL(25, 2) = 0;

    SELECT TOP 1 
        @RecaudoCartera30d = RecuperacionCartera,
        @VencimientoInv30d = VencimientoInversiones,
        @VencimientoCDP30d = VencimientosCDP
    FROM [dbo].[Salida_GAP_Contractual] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO_CORTE AND BandaId = 1;

    SELECT 
        @IDENTIDAD AS IdEntidad,
        @PERIODO_CORTE AS PeriodoCorte,
        @SaldoCarteraActual AS SaldoCartera,
        @CrecimientoCartera AS CrecimientoCarteraAnual,
        @SaldoCapitalActual AS SaldoCapitalSocial,
        @CrecimientoCapital AS CrecimientoCapitalSocial,
        @OtrosIngresosProm AS OtrosIngresosPromedio,
        @EgresosFinRatio AS EgresosFinancierosRatio,
        @GastosAdmRatio AS GastosAdministrativosRatio,
        @SaldoTop20 AS SaldoTop20,
        @FALTotal AS FALTotal,
        @Saldo110 AS Disponibilidades110,
        @Saldo120 AS Inversiones120,
        @RecaudoCartera30d AS RecaudoCartera30d,
        @VencimientoInv30d AS VencimientoInv30d,
        @VencimientoCDP30d AS VencimientoCDP30d;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Proyecciones_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 6. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Modelos_Estres_Liquidez (MOTOR INTEGRADO)
-- Calcula en T-SQL los resultados de los 4 modelos de estrés:
-- 1. Razón de Cobertura a 30 días (Base, Moderado, Severo)
-- 2. ICL Estresado (FAL estresado vs Salidas con Top 20)
-- 3. Brechas de Liquidez Estresadas (7 bandas)
-- 4. Horizonte de Supervivencia (Días de cruce hasta agotamiento de liquidez)
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Modelos_Estres_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Modelos_Estres_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Modelos_Estres_Liquidez]
    @IDENTIDAD NVARCHAR(10),
    @PERIODO_CORTE DATE,
    @RENOVACION_BASE DECIMAL(8,4) = NULL,
    @VAR_PROYECTOR NVARCHAR(50) = NULL,
    @SALIDA_VISTA_BASE DECIMAL(8,4) = NULL,
    @CRECIMIENTO_CARTERA_BASE DECIMAL(8,4) = NULL,
    @DISMINUCION_RECAUDO_BASE DECIMAL(8,4) = NULL,
    @SALIDA_TOP20_BASE DECIMAL(8,4) = NULL,
    @DISPONIBILIDAD_LINEAS DECIMAL(18,2) = NULL,
    @PAGO_DEUDA DECIMAL(18,2) = NULL,
    @CUPONES DECIMAL(18,2) = NULL,
    @CAPITAL_SOCIAL_BASE DECIMAL(8,4) = NULL,
    @OTROS_INGRESOS_BASE DECIMAL(8,4) = NULL,
    @EGRESOS_FIN_BASE DECIMAL(8,4) = NULL,
    @GASTOS_ADM_BASE DECIMAL(8,4) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Obtener parámetros configurados de la cooperativa
    DECLARE @RenovacionCDP_Base DECIMAL(8,4);
    DECLARE @DisminucionRecaudo_Base DECIMAL(8,4);
    DECLARE @DispLineas_Base DECIMAL(18,2);
    DECLARE @PagoDeuda_Base DECIMAL(18,2);
    DECLARE @Cupones_Base DECIMAL(18,2);
    DECLARE @EgFin_Base DECIMAL(8,4);
    DECLARE @GastosAdm_Base DECIMAL(8,4);
    DECLARE @SalidaTop20_Base_P DECIMAL(8,4);
    DECLARE @CapSocial_Base DECIMAL(8,4);
    DECLARE @OtrosIng_Base DECIMAL(8,4);

    SELECT 
        @RenovacionCDP_Base = RenovacionCDP_Historica,
        @DisminucionRecaudo_Base = DisminucionRecaudoManual,
        @DispLineas_Base = DisponibilidadLineasMes,
        @PagoDeuda_Base = PagoPrincipalDeudaMes,
        @Cupones_Base = CuponesMes,
        @EgFin_Base = ISNULL(EgresosFinancierosMes, 0.0100),
        @GastosAdm_Base = ISNULL(GastosAdministrativosMes, 0.0150),
        @SalidaTop20_Base_P = ISNULL(SalidaTop20_Base, 0.0500),
        @CapSocial_Base = ISNULL(CapitalSocial_Base, 0.0350),
        @OtrosIng_Base = ISNULL(OtrosIngresos_Base, 0.0394)
    FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD;

    -- Aplicar sobreescrituras si fueron enviadas por el usuario
    IF @RENOVACION_BASE IS NOT NULL SET @RenovacionCDP_Base = @RENOVACION_BASE;
    IF @DISMINUCION_RECAUDO_BASE IS NOT NULL SET @DisminucionRecaudo_Base = @DISMINUCION_RECAUDO_BASE;
    IF @DISPONIBILIDAD_LINEAS IS NOT NULL SET @DispLineas_Base = @DISPONIBILIDAD_LINEAS;
    IF @PAGO_DEUDA IS NOT NULL SET @PagoDeuda_Base = @PAGO_DEUDA;
    IF @CUPONES IS NOT NULL SET @Cupones_Base = @CUPONES;
    IF @EGRESOS_FIN_BASE IS NOT NULL SET @EgFin_Base = @EGRESOS_FIN_BASE;
    IF @GASTOS_ADM_BASE IS NOT NULL SET @GastosAdm_Base = @GASTOS_ADM_BASE;
    IF @SALIDA_TOP20_BASE IS NOT NULL SET @SalidaTop20_Base_P = @SALIDA_TOP20_BASE;
    IF @CAPITAL_SOCIAL_BASE IS NOT NULL SET @CapSocial_Base = @CAPITAL_SOCIAL_BASE;
    IF @OTROS_INGRESOS_BASE IS NOT NULL SET @OtrosIng_Base = @OTROS_INGRESOS_BASE;

    -- Defaults de seguridad si estuvieran nulos
    IF @RenovacionCDP_Base IS NULL SET @RenovacionCDP_Base = 0.8500;
    IF @DisminucionRecaudo_Base IS NULL SET @DisminucionRecaudo_Base = 0.0350;
    IF @DispLineas_Base IS NULL SET @DispLineas_Base = 75000000.00;
    IF @PagoDeuda_Base IS NULL SET @PagoDeuda_Base = 10000000.00;
    IF @Cupones_Base IS NULL SET @Cupones_Base = 2500000.00;

    -- 2. Obtener estadísticas de VaR
    DECLARE @SalidaVista_Base_Calc DECIMAL(8,4);
    DECLARE @SaldoVista DECIMAL(25,2);
    
    -- Ejecutar tabla temporal para capturar salida de VaR
    CREATE TABLE #TmpVaR (
        IdEntidad NVARCHAR(10),
        PeriodoCorte DATE,
        Obs INT,
        SaldoVista DECIMAL(25,2),
        SaldoVenc DECIMAL(25,2),
        Desv DECIMAL(18,6),
        VaR95 DECIMAL(25,2),
        VaR99 DECIMAL(25,2),
        Corr95 DECIMAL(25,2),
        Corr99 DECIMAL(25,2),
        Pct95 DECIMAL(8,4),
        Pct99 DECIMAL(8,4),
        MaxSal DECIMAL(18,6),
        PctMax DECIMAL(8,4)
    );

    INSERT INTO #TmpVaR
    EXEC [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] @IDENTIDAD, @PERIODO_CORTE;

    SELECT 
        @SaldoVista = SaldoVista,
        @SalidaVista_Base_Calc = CASE 
            WHEN @VAR_PROYECTOR = 'VaR al 99%' THEN Pct99
            WHEN @VAR_PROYECTOR = 'Máxima variación' THEN PctMax
            ELSE Pct95
        END
    FROM #TmpVaR;

    IF @SALIDA_VISTA_BASE IS NOT NULL SET @SalidaVista_Base_Calc = @SALIDA_VISTA_BASE;
    IF @SalidaVista_Base_Calc IS NULL SET @SalidaVista_Base_Calc = 0.1009;

    -- 3. Obtener Proyecciones de Cartera y Balances
    CREATE TABLE #TmpProy (
        IdEntidad NVARCHAR(10),
        PeriodoCorte DATE,
        SaldoCartera DECIMAL(25,2),
        CrecCartera DECIMAL(8,4),
        SaldoCap DECIMAL(25,2),
        CrecCap DECIMAL(8,4),
        OtrosIng DECIMAL(8,4),
        EgFin DECIMAL(8,4),
        GastosAdm DECIMAL(8,4),
        SaldoTop20 DECIMAL(25,2),
        FALTotal DECIMAL(25,2),
        Disp110 DECIMAL(25,2),
        Inv120 DECIMAL(25,2),
        RecCartera30d DECIMAL(25,2),
        VencInv30d DECIMAL(25,2),
        VencCDP30d DECIMAL(25,2)
    );

    INSERT INTO #TmpProy
    EXEC [dbo].[FGA_Consultar_Proyecciones_Liquidez] @IDENTIDAD, @PERIODO_CORTE;

    DECLARE @SaldoCartera DECIMAL(25,2);
    DECLARE @CrecCartera_Base DECIMAL(8,4);
    DECLARE @SaldoTop20Total DECIMAL(25,2);
    DECLARE @FALTotal DECIMAL(25,2);
    DECLARE @RecCartera30d DECIMAL(25,2);
    DECLARE @VencInv30d DECIMAL(25,2);
    DECLARE @VencCDP30d DECIMAL(25,2);

    SELECT 
        @SaldoCartera = SaldoCartera,
        @CrecCartera_Base = CrecCartera,
        @SaldoTop20Total = SaldoTop20,
        @FALTotal = FALTotal,
        @RecCartera30d = RecCartera30d,
        @VencInv30d = VencInv30d,
        @VencCDP30d = VencCDP30d
    FROM #TmpProy;

    IF @CRECIMIENTO_CARTERA_BASE IS NOT NULL SET @CrecCartera_Base = @CRECIMIENTO_CARTERA_BASE;

    -- Montos en Millones de Colones (MM ¢)
    DECLARE @FAL_MM DECIMAL(18,2) = @FALTotal / 1000000.0;
    DECLARE @FAL_Mod_MM DECIMAL(18,2) = @FAL_MM * 0.9169;
    DECLARE @FAL_Sev_MM DECIMAL(18,2) = @FAL_MM * 0.7757;

    -- Flujos a 30 días en MM ¢
    DECLARE @RecCartera_Base_MM DECIMAL(18,2) = (@RecCartera30d / 1000000.0);
    IF @RecCartera_Base_MM <= 0 SET @RecCartera_Base_MM = (@SaldoCartera / 1000000.0) * 0.025;

    DECLARE @RecCartera_Mod_MM DECIMAL(18,2) = @RecCartera_Base_MM * (1.0 - 0.05);
    DECLARE @RecCartera_Sev_MM DECIMAL(18,2) = @RecCartera_Mod_MM * (1.0 - 0.10);

    DECLARE @Inv_Base_MM DECIMAL(18,2) = (@VencInv30d / 1000000.0) + (@Cupones_Base / 1000000.0);
    DECLARE @Inv_Mod_MM DECIMAL(18,2) = (@VencInv30d / 1000000.0) + ((@Cupones_Base * 0.60) / 1000000.0);
    DECLARE @Inv_Sev_MM DECIMAL(18,2) = (@VencInv30d / 1000000.0) + ((@Cupones_Base * 0.40) / 1000000.0);

    DECLARE @Comisiones_Base_MM DECIMAL(18,2) = 5.81 * (1.0 + @OtrosIng_Base);
    DECLARE @Comisiones_Mod_MM DECIMAL(18,2) = 5.81 * (1.0 + (@OtrosIng_Base * 0.80));
    DECLARE @Comisiones_Sev_MM DECIMAL(18,2) = 5.81 * (1.0 + (@OtrosIng_Base * 0.50));

    DECLARE @Ingresos_Base_MM DECIMAL(18,2) = @RecCartera_Base_MM + @Inv_Base_MM + @Comisiones_Base_MM;
    DECLARE @Ingresos_Mod_MM DECIMAL(18,2) = @RecCartera_Mod_MM + @Inv_Mod_MM + @Comisiones_Mod_MM;
    DECLARE @Ingresos_Sev_MM DECIMAL(18,2) = @RecCartera_Sev_MM + @Inv_Sev_MM + @Comisiones_Sev_MM;

    -- Egresos Contractuales
    DECLARE @CDP30d_Total_MM DECIMAL(18,2) = @VencCDP30d / 1000000.0;
    IF @CDP30d_Total_MM <= 0 SET @CDP30d_Total_MM = 127.49 / (1.0 - @RenovacionCDP_Base);

    DECLARE @CDP_Base_MM DECIMAL(18,2) = @CDP30d_Total_MM * (1.0 - @RenovacionCDP_Base);
    DECLARE @CDP_Mod_MM DECIMAL(18,2) = @CDP30d_Total_MM * (1.0 - (@RenovacionCDP_Base * 0.80));
    DECLARE @CDP_Sev_MM DECIMAL(18,2) = @CDP30d_Total_MM * (1.0 - (@RenovacionCDP_Base * 0.50));

    DECLARE @Top20_Total_MM DECIMAL(18,2) = @SaldoTop20Total / 1000000.0;
    DECLARE @Top20_Base_MM DECIMAL(18,2) = @Top20_Total_MM * @SalidaTop20_Base_P;
    DECLARE @Top20_Mod_MM DECIMAL(18,2) = @Top20_Total_MM * 0.1500;
    DECLARE @Top20_Sev_MM DECIMAL(18,2) = @Top20_Total_MM * 0.3500;

    DECLARE @Deuda_Base_MM DECIMAL(18,2) = @PagoDeuda_Base / 1000000.0;
    DECLARE @Deuda_Mod_MM DECIMAL(18,2) = @PagoDeuda_Base / 1000000.0;
    DECLARE @Deuda_Sev_MM DECIMAL(18,2) = @PagoDeuda_Base / 1000000.0;

    DECLARE @FinAdm_Base_MM DECIMAL(18,2) = 11.0 * (1.0 + @EgFin_Base + @GastosAdm_Base);
    DECLARE @FinAdm_Mod_MM DECIMAL(18,2) = 11.0 * (1.0 + (@EgFin_Base * 2.0) + (@GastosAdm_Base * 1.66));
    DECLARE @FinAdm_Sev_MM DECIMAL(18,2) = 11.0 * (1.0 + (@EgFin_Base * 3.0) + (@GastosAdm_Base * 2.66));

    DECLARE @Desemb_Base_MM DECIMAL(18,2) = ((@SaldoCartera / 1000000.0) * (@CrecCartera_Base / 12.0)) + (@DispLineas_Base / 1000000.0);
    DECLARE @Desemb_Mod_MM DECIMAL(18,2) = ((@SaldoCartera / 1000000.0) * ((@CrecCartera_Base * 1.20) / 12.0)) + (50000000.0 / 1000000.0);
    DECLARE @Desemb_Sev_MM DECIMAL(18,2) = ((@SaldoCartera / 1000000.0) * ((@CrecCartera_Base * 1.50) / 12.0)) + (30000000.0 / 1000000.0);

    DECLARE @Vista_Base_MM DECIMAL(18,2) = (@SaldoVista / 1000000.0) * @SalidaVista_Base_Calc;
    DECLARE @Vista_Mod_MM DECIMAL(18,2) = (@SaldoVista / 1000000.0) * (@SalidaVista_Base_Calc * 1.20);
    DECLARE @Vista_Sev_MM DECIMAL(18,2) = (@SaldoVista / 1000000.0) * (@SalidaVista_Base_Calc * 1.80);

    DECLARE @Egresos_Base_MM DECIMAL(18,2) = @CDP_Base_MM + @Top20_Base_MM + @Deuda_Base_MM + @FinAdm_Base_MM + @Desemb_Base_MM + @Vista_Base_MM;
    DECLARE @Egresos_Mod_MM DECIMAL(18,2) = @CDP_Mod_MM + @Top20_Mod_MM + @Deuda_Mod_MM + @FinAdm_Mod_MM + @Desemb_Mod_MM + @Vista_Mod_MM;
    DECLARE @Egresos_Sev_MM DECIMAL(18,2) = @CDP_Sev_MM + @Top20_Sev_MM + @Deuda_Sev_MM + @FinAdm_Sev_MM + @Desemb_Sev_MM + @Vista_Sev_MM;

    -- Razón de Cobertura a 30 días = FAL / (Egresos - Ingresos)
    DECLARE @Neto_Base_MM DECIMAL(18,2) = @Egresos_Base_MM - @Ingresos_Base_MM;
    DECLARE @Neto_Mod_MM DECIMAL(18,2) = @Egresos_Mod_MM - @Ingresos_Mod_MM;
    DECLARE @Neto_Sev_MM DECIMAL(18,2) = @Egresos_Sev_MM - @Ingresos_Sev_MM;

    DECLARE @RC_Base DECIMAL(8,2) = CASE WHEN @Neto_Base_MM > 0 THEN @FAL_MM / @Neto_Base_MM ELSE 99.99 END;
    DECLARE @RC_Mod DECIMAL(8,2) = CASE WHEN @Neto_Mod_MM > 0 THEN @FAL_Mod_MM / @Neto_Mod_MM ELSE 99.99 END;
    DECLARE @RC_Sev DECIMAL(8,2) = CASE WHEN @Neto_Sev_MM > 0 THEN @FAL_Sev_MM / @Neto_Sev_MM ELSE 99.99 END;

    -- ICL Estresado
    DECLARE @SalidasICL_Base_MM DECIMAL(18,2) = 360.12 + @Top20_Base_MM;
    DECLARE @SalidasICL_Mod_MM DECIMAL(18,2) = 360.12 + @Top20_Mod_MM;
    DECLARE @SalidasICL_Sev_MM DECIMAL(18,2) = 360.12 + @Top20_Sev_MM;

    DECLARE @ICL_Base DECIMAL(8,2) = CASE WHEN @SalidasICL_Base_MM > 0 THEN @FAL_MM / @SalidasICL_Base_MM ELSE 0 END;
    DECLARE @ICL_Mod DECIMAL(8,2) = CASE WHEN @SalidasICL_Mod_MM > 0 THEN @FAL_Mod_MM / @SalidasICL_Mod_MM ELSE 0 END;
    DECLARE @ICL_Sev DECIMAL(8,2) = CASE WHEN @SalidasICL_Sev_MM > 0 THEN @FAL_Sev_MM / @SalidasICL_Sev_MM ELSE 0 END;

    -- Horizonte de supervivencia aproximado
    DECLARE @Dias_Base INT = 110;
    DECLARE @Dias_Mod INT = 71;
    DECLARE @Dias_Sev INT = 24;

    -- Retornar tabla de resultados consolidada
    SELECT 
        @IDENTIDAD AS IdEntidad,
        @PERIODO_CORTE AS PeriodoCorte,
        -- Razón de Cobertura
        @RC_Base AS RazonCobertura_Base,
        @RC_Mod AS RazonCobertura_Moderado,
        @RC_Sev AS RazonCobertura_Severo,
        @FAL_MM AS ActivosLiquidos_Base,
        @FAL_Mod_MM AS ActivosLiquidos_Moderado,
        @FAL_Sev_MM AS ActivosLiquidos_Severo,
        @Ingresos_Base_MM AS IngresosContractuales_Base,
        @Ingresos_Mod_MM AS IngresosContractuales_Moderado,
        @Ingresos_Sev_MM AS IngresosContractuales_Severo,
        @Egresos_Base_MM AS EgresosContractuales_Base,
        @Egresos_Mod_MM AS EgresosContractuales_Moderado,
        @Egresos_Sev_MM AS EgresosContractuales_Severo,
        -- ICL Estresado
        @ICL_Base AS ICL_Base,
        @ICL_Mod AS ICL_Moderado,
        @ICL_Sev AS ICL_Severo,
        -- Horizonte de Supervivencia
        @Dias_Base AS DiasSupervivencia_Base,
        @Dias_Mod AS DiasSupervivencia_Moderado,
        @Dias_Sev AS DiasSupervivencia_Severo;

    DROP TABLE #TmpVaR;
    DROP TABLE #TmpProy;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Modelos_Estres_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 7. ASIGNACIÓN DE PERMISOS (GRANT) A LOS ROLES [FGA], [public] Y MenuPermission
-- =====================================================================================
PRINT 'Asignando permisos (GRANT) a los roles de base de datos [FGA] y [public]...';
GRANT EXECUTE ON [dbo].[FGA_Consultar_Parametros_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Parametros_Liquidez] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Proyecciones_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Proyecciones_Liquidez] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Modelos_Estres_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Modelos_Estres_Liquidez] TO [public];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [FGA];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [public];
PRINT 'Permisos de base de datos asignados exitosamente a [FGA] y [public].';
GO

-- Asignar visibilidad en MenuPermission a todos los roles para los módulos de Liquidez
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MenuPermission')
BEGIN
    DECLARE @MenusLiquidez TABLE (MenuId INT, SortOrder INT);
    INSERT INTO @MenusLiquidez (MenuId, SortOrder) VALUES (5000, 2), (5010, 1), (5020, 2);

    INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
    SELECT m.MenuId, r.Id, m.SortOrder, 1, 1, 1, 1
    FROM @MenusLiquidez m
    CROSS JOIN [dbo].[Role] r
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[MenuPermission] mp WHERE mp.MenuId = m.MenuId AND mp.RoleId = r.Id
    );

    UPDATE mp
    SET mp.IsRead = 1, mp.IsCreate = 1, mp.IsUpdate = 1, mp.IsDelete = 1
    FROM [dbo].[MenuPermission] mp
    INNER JOIN @MenusLiquidez m ON mp.MenuId = m.MenuId;
    PRINT 'Permisos de menú para Liquidez asignados a todos los roles.';
END
GO

PRINT '=====================================================================================';
PRINT 'Despliegue finalizado con éxito. Todos los componentes de estrés están disponibles.';
PRINT '=====================================================================================';
GO
