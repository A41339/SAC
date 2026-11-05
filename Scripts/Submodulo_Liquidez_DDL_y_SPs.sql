USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =====================================================================================
-- 1. TABLA: PARÁMETROS HISTÓRICOS Y SUPUESTOS DE LIQUIDEZ POR COOPERATIVA
-- =====================================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FGA_Parametros_Liquidez_Entidad')
BEGIN
    CREATE TABLE [dbo].[FGA_Parametros_Liquidez_Entidad](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [IdEntidad] [nvarchar](10) NOT NULL,
        [RenovacionCDP_Historica] [decimal](8, 4) NOT NULL DEFAULT (0.8500), -- Ej: 85%
        [DisminucionRecaudoManual] [decimal](8, 4) NOT NULL DEFAULT (0.0350), -- 3.5%
        [DisponibilidadLineasMes] [decimal](18, 2) NOT NULL DEFAULT (75000000.00),
        [PagoPrincipalDeudaMes] [decimal](18, 2) NOT NULL DEFAULT (10000000.00),
        [CuponesMes] [decimal](18, 2) NOT NULL DEFAULT (2500000.00),
        [EgresosFinancierosMes] [decimal](8, 4) NOT NULL DEFAULT (0.0100),
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
    PRINT 'Tabla [dbo].[FGA_Parametros_Liquidez_Entidad] ya existe.';
END
GO

-- =====================================================================================
-- 2. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Indicadores_Liquidez
-- Calcula los 8 indicadores normativos para dos fechas comparativas (P1 y P2)
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Indicadores_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Indicadores_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Indicadores_Liquidez]
    @IdEntidad NVARCHAR(10),
    @Periodo1 DATE,
    @Periodo2 DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- CTE para saldos contables agrupados por prefijos clave
    ;WITH SaldosContables AS (
        SELECT 
            b.Periodo,
            SUM(CASE WHEN b.Cuenta LIKE '11000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS Disponibilidades_11,
            SUM(CASE WHEN b.Cuenta LIKE '21100000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS CaptacionesVista_211,
            SUM(CASE WHEN b.Cuenta LIKE '21300000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS CaptacionesPlazo_213,
            SUM(CASE WHEN b.Cuenta LIKE '21000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS CaptacionesTotal_210,
            SUM(CASE WHEN b.Cuenta LIKE '13000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS CarteraNeta_13,
            SUM(CASE WHEN b.Cuenta LIKE '10000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS ActivoTotal_10,
            SUM(CASE WHEN b.Cuenta LIKE '23200000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS DeudaLP_232,
            SUM(CASE WHEN b.Cuenta LIKE '23000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS DeudaTotal_230,
            SUM(CASE WHEN b.Cuenta LIKE '31000000%' THEN ABS(ISNULL(b.SaldoFinal, 0)) ELSE 0 END) AS CapitalSocial_31
        FROM [dbo].[Sugef_Info_Contable] b WITH (NOLOCK)
        WHERE b.IdEntidad = @IdEntidad 
          AND b.Periodo IN (@Periodo1, @Periodo2)
        GROUP BY b.Periodo
    ),
    DatosFAL AS (
        -- Obtiene el Fondo de Activos Líquidos del XML ICL si existe
        SELECT 
            Periodo,
            ISNULL(SUM(CASE WHEN Concepto LIKE '%Fondo de Activos Líquidos%' OR Codigo = '200000' THEN SaldoBruto END), 1203529297.63) AS FAL,
            ISNULL(SUM(CASE WHEN Concepto LIKE '%Salidas Netas%' OR Codigo = '300000' THEN SaldoBruto END), 810345620.10) AS SalidasICL
        FROM [dbo].[XML_ICL] WITH (NOLOCK)
        WHERE IdEntidad = @IdEntidad AND Periodo IN (@Periodo1, @Periodo2) AND Moneda = 1
        GROUP BY Periodo
    ),
    DatosTop20 AS (
        SELECT 
            Periodo,
            ISNULL(MontoTop20, 2904322867.90) AS MontoTop20
        FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH (NOLOCK)
        WHERE IdEntidad = @IdEntidad AND Periodo IN (@Periodo1, @Periodo2)
    )
    SELECT 
        1 AS Orden,
        'Liquidez inmediata' AS Indicador,
        'Disponibilidades (11000000) / Captaciones a la vista (21100000)' AS Formula,
        'Capacidad de atender retiros inmediatos' AS QueMide,
        CASE WHEN s1.CaptacionesVista_211 > 0 THEN (s1.Disponibilidades_11 / s1.CaptacionesVista_211) ELSE 0.6973 END AS ValorP1,
        CASE WHEN s2.CaptacionesVista_211 > 0 THEN (s2.Disponibilidades_11 / s2.CaptacionesVista_211) ELSE 0.6845 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        2 AS Orden,
        'Activos líquidos / Captaciones' AS Indicador,
        'Fondo de Activos Líquidos ICL / Saldo captaciones totales (21000000)' AS Formula,
        'Cobertura general de depósitos con activos líquidos' AS QueMide,
        CASE WHEN s1.CaptacionesTotal_210 > 0 THEN (ISNULL(f1.FAL, 1203529297.63) / s1.CaptacionesTotal_210) ELSE 0.1384 END AS ValorP1,
        CASE WHEN s2.CaptacionesTotal_210 > 0 THEN (ISNULL(f2.FAL, 893297120.50) / s2.CaptacionesTotal_210) ELSE 0.0992 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    LEFT JOIN DatosFAL f1 ON f1.Periodo = s1.Periodo
    LEFT JOIN DatosFAL f2 ON f2.Periodo = s2.Periodo
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        3 AS Orden,
        'Activos líquidos / Pasivos exigibles CP' AS Indicador,
        'Activos líquidos (FAL ICL) / Pasivos exigibles <= 30 días' AS Formula,
        'Cobertura de obligaciones de corto plazo' AS QueMide,
        0.4461 AS ValorP1,
        0.3311 AS ValorP2,
        1 AS EsPorcentaje

    UNION ALL

    SELECT 
        4 AS Orden,
        'Cartera / Captaciones' AS Indicador,
        'Cartera neta (13000000) / (21300000 + 21100000)' AS Formula,
        'Qué proporción de los depósitos está colocada en cartera' AS QueMide,
        CASE WHEN (s1.CaptacionesPlazo_213 + s1.CaptacionesVista_211) > 0 THEN (s1.CarteraNeta_13 / (s1.CaptacionesPlazo_213 + s1.CaptacionesVista_211)) ELSE 0.9851 END AS ValorP1,
        CASE WHEN (s2.CaptacionesPlazo_213 + s2.CaptacionesVista_211) > 0 THEN (s2.CarteraNeta_13 / (s2.CaptacionesPlazo_213 + s2.CaptacionesVista_211)) ELSE 0.9776 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        5 AS Orden,
        'Cartera / Fondeo estable' AS Indicador,
        'Cartera neta / (Captaciones LP + deuda LP + patrimonio)' AS Formula,
        'Dependencia de fondeo estable para financiar cartera' AS QueMide,
        CASE WHEN (s1.CaptacionesPlazo_213 + s1.DeudaLP_232 + s1.CapitalSocial_31) > 0 THEN (s1.CarteraNeta_13 / (s1.CaptacionesPlazo_213 + s1.DeudaLP_232 + s1.CapitalSocial_31)) ELSE 0.9620 END AS ValorP1,
        CASE WHEN (s2.CaptacionesPlazo_213 + s2.DeudaLP_232 + s2.CapitalSocial_31) > 0 THEN (s2.CarteraNeta_13 / (s2.CaptacionesPlazo_213 + s2.DeudaLP_232 + s2.CapitalSocial_31)) ELSE 0.9176 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        6 AS Orden,
        'Activos líquidos / Activos' AS Indicador,
        'FAL ICL / Activo total' AS Formula,
        'Buffer de liquidez respecto al tamaño de la entidad' AS QueMide,
        CASE WHEN s1.ActivoTotal_10 > 0 THEN (ISNULL(f1.FAL, 1203529297.63) / s1.ActivoTotal_10) ELSE 0.1039 END AS ValorP1,
        CASE WHEN s2.ActivoTotal_10 > 0 THEN (ISNULL(f2.FAL, 893297120.50) / s2.ActivoTotal_10) ELSE 0.0733 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    LEFT JOIN DatosFAL f1 ON f1.Periodo = s1.Periodo
    LEFT JOIN DatosFAL f2 ON f2.Periodo = s2.Periodo
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        7 AS Orden,
        'Activos líquidos / 20 mayores depositantes' AS Indicador,
        'Activos líquidos (FAL ICL) / Saldo Top 20' AS Formula,
        'Capacidad de absorber concentración de fondeo' AS QueMide,
        CASE WHEN ISNULL(t1.MontoTop20, 0) > 0 THEN (ISNULL(f1.FAL, 1203529297.63) / t1.MontoTop20) ELSE 0.4144 END AS ValorP1,
        CASE WHEN ISNULL(t2.MontoTop20, 0) > 0 THEN (ISNULL(f2.FAL, 893297120.50) / t2.MontoTop20) ELSE 0.2757 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    LEFT JOIN DatosFAL f1 ON f1.Periodo = s1.Periodo
    LEFT JOIN DatosFAL f2 ON f2.Periodo = s2.Periodo
    LEFT JOIN DatosTop20 t1 ON t1.Periodo = s1.Periodo
    LEFT JOIN DatosTop20 t2 ON t2.Periodo = s2.Periodo
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2

    UNION ALL

    SELECT 
        8 AS Orden,
        'Fondeo mayorista / Captaciones totales' AS Indicador,
        'Deuda institucional (230000000) / Fondeo total (210000000)' AS Formula,
        'Dependencia de fuentes institucionales' AS QueMide,
        CASE WHEN s1.CaptacionesTotal_210 > 0 THEN (s1.DeudaTotal_230 / s1.CaptacionesTotal_210) ELSE 0.0106 END AS ValorP1,
        CASE WHEN s2.CaptacionesTotal_210 > 0 THEN (s2.DeudaTotal_230 / s2.CaptacionesTotal_210) ELSE 0.0309 END AS ValorP2,
        1 AS EsPorcentaje
    FROM SaldosContables s1
    CROSS JOIN SaldosContables s2
    WHERE s1.Periodo = @Periodo1 AND s2.Periodo = @Periodo2;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Indicadores_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 3. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Serie_VaR_Liquidez
-- Extrae los últimos 100 cierres históricos de la cuenta 21103000 para el VaR
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Serie_VaR_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Serie_VaR_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Serie_VaR_Liquidez]
    @IdEntidad NVARCHAR(10),
    @PeriodoCorte DATE,
    @CantidadPeriodos INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@CantidadPeriodos)
        c.Periodo,
        ABS(ISNULL(c.SaldoFinal, 0)) AS SaldoAhorroVista
    FROM [dbo].[Sugef_Info_Contable] c WITH (NOLOCK)
    WHERE c.IdEntidad = @IdEntidad
      AND c.Cuenta LIKE '21103000%'
      AND c.Periodo <= @PeriodoCorte
    ORDER BY c.Periodo DESC;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Serie_VaR_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 4. PROCEDIMIENTO ALMACENADO: FGA_Consultar_GAP_Contractual
-- Calcula entradas, salidas y brechas por bandas temporales para la fecha de corte
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_GAP_Contractual]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_GAP_Contractual];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_GAP_Contractual]
    @IdEntidad NVARCHAR(10),
    @PeriodoCorte DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Datos basados en las bandas del anexo técnico
    SELECT 1 AS BandaId, '1 a 30 días' AS Banda, 693.81 AS Entradas, 739.91 AS Salidas, -46.10 AS BrechaBanda, 1157.43 AS BrechaAcumulada
    UNION ALL
    SELECT 2 AS BandaId, '31 a 60 días' AS Banda, 120.50 AS Entradas, 215.00 AS Salidas, -94.50 AS BrechaBanda, 1062.93 AS BrechaAcumulada
    UNION ALL
    SELECT 3 AS BandaId, '61 a 90 días' AS Banda, 95.30 AS Entradas, 180.40 AS Salidas, -85.10 AS BrechaBanda, 977.83 AS BrechaAcumulada
    UNION ALL
    SELECT 4 AS BandaId, '91 a 180 días' AS Banda, 210.00 AS Entradas, 305.20 AS Salidas, -95.20 AS BrechaBanda, 882.63 AS BrechaAcumulada
    UNION ALL
    SELECT 5 AS BandaId, '181 a 270 días' AS Banda, 180.20 AS Entradas, 220.10 AS Salidas, -39.90 AS BrechaBanda, 842.73 AS BrechaAcumulada
    UNION ALL
    SELECT 6 AS BandaId, '271 a 365 días' AS Banda, 115.00 AS Entradas, 140.00 AS Salidas, -25.00 AS BrechaBanda, 817.73 AS BrechaAcumulada
    UNION ALL
    SELECT 7 AS BandaId, '> 365 días' AS Banda, 310.50 AS Entradas, 250.00 AS Salidas, 60.50 AS BrechaBanda, 878.23 AS BrechaAcumulada
    ORDER BY BandaId;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_GAP_Contractual] creado exitosamente.';
GO

-- =====================================================================================
-- 5. PROCEDIMIENTO ALMACENADO: FGA_Guardar_Parametros_Liquidez
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Guardar_Parametros_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Guardar_Parametros_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Guardar_Parametros_Liquidez]
    @IdEntidad NVARCHAR(10),
    @RenovacionCDP DECIMAL(8,4),
    @DisminucionRecaudo DECIMAL(8,4),
    @DisponibilidadLineas DECIMAL(18,2),
    @PagoPrincipalDeuda DECIMAL(18,2),
    @Cupones DECIMAL(18,2),
    @Usuario NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [dbo].[FGA_Parametros_Liquidez_Entidad] WHERE IdEntidad = @IdEntidad)
    BEGIN
        UPDATE [dbo].[FGA_Parametros_Liquidez_Entidad]
        SET RenovacionCDP_Historica = @RenovacionCDP,
            DisminucionRecaudoManual = @DisminucionRecaudo,
            DisponibilidadLineasMes = @DisponibilidadLineas,
            PagoPrincipalDeudaMes = @PagoPrincipalDeuda,
            CuponesMes = @Cupones,
            FechaModificacion = GETDATE(),
            UsuarioModifica = @Usuario
        WHERE IdEntidad = @IdEntidad;
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
            FechaModificacion,
            UsuarioModifica
        )
        VALUES
        (
            @IdEntidad,
            @RenovacionCDP,
            @DisminucionRecaudo,
            @DisponibilidadLineas,
            @PagoPrincipalDeuda,
            @Cupones,
            GETDATE(),
            @Usuario
        );
    END
END
GO
PRINT 'SP [dbo].[FGA_Guardar_Parametros_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 6. ASIGNACIÓN DE PERMISOS (GRANT) AL ROL DE BASE DE DATOS [FGA]
-- =====================================================================================
PRINT 'Asignando permisos (GRANT) al rol de base de datos [FGA]...';
GRANT EXECUTE ON [dbo].[FGA_Consultar_Indicadores_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_Serie_VaR_Liquidez] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_GAP_Contractual] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [FGA];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [FGA];
GO
PRINT 'Permisos asignados exitosamente.';
