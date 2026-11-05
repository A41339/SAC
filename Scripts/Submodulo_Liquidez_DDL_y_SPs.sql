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
        [RenovacionCDP_Historica] [decimal](8, 4) NOT NULL DEFAULT (0.8500),
        [DisminucionRecaudoManual] [decimal](8, 4) NOT NULL DEFAULT (0.0350),
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
-- 2. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Indicadores_Liquidez (100% DINÁMICO)
-- Consulta saldos de [SALIDA_BALANCE_COMPROBACION], [XML_ICL] y [Salida_Concentracion_Ahorrantes]
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Indicadores_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Indicadores_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Indicadores_Liquidez]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO1 DATE,
    @PERIODO2 DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalizar a inicio de mes
    SET @PERIODO1 = DATEFROMPARTS(YEAR(@PERIODO1), MONTH(@PERIODO1), 1);
    SET @PERIODO2 = DATEFROMPARTS(YEAR(@PERIODO2), MONTH(@PERIODO2), 1);

    -- 1. Tabla temporal para cuentas de balance
    CREATE TABLE #CuentasBalance (
        Cuenta NVARCHAR(8),
        SaldoP1 DECIMAL(25, 2) DEFAULT 0,
        SaldoP2 DECIMAL(25, 2) DEFAULT 0
    );

    INSERT INTO #CuentasBalance (Cuenta)
    VALUES ('10000000'), ('11000000'), ('13000000'), ('20000000'), ('21000000'),
           ('21100000'), ('21300000'), ('23000000'), ('23200000'), ('31000000');

    -- Poblar SaldoP1 desde SALIDA_BALANCE_COMPROBACION
    UPDATE C
    SET C.SaldoP1 = ABS(ISNULL(S.SALDOFINAL, 0))
    FROM #CuentasBalance C
    INNER JOIN [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        ON S.CUENTA = C.Cuenta AND S.IDENTIDAD = @IDENTIDAD AND S.PERIODO = @PERIODO1;

    -- Poblar SaldoP2 desde SALIDA_BALANCE_COMPROBACION
    UPDATE C
    SET C.SaldoP2 = ABS(ISNULL(S.SALDOFINAL, 0))
    FROM #CuentasBalance C
    INNER JOIN [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        ON S.CUENTA = C.Cuenta AND S.IDENTIDAD = @IDENTIDAD AND S.PERIODO = @PERIODO2;

    -- Variables de balance P1
    DECLARE @C10_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '10000000'), 0);
    DECLARE @C11_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '11000000'), 0);
    DECLARE @C13_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '13000000'), 0);
    DECLARE @C210_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '21000000'), 0);
    DECLARE @C211_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '21100000'), 0);
    DECLARE @C213_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '21300000'), 0);
    DECLARE @C230_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '23000000'), 0);
    DECLARE @C232_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '23200000'), 0);
    DECLARE @C31_P1 DECIMAL(25,2) = ISNULL((SELECT SaldoP1 FROM #CuentasBalance WHERE Cuenta = '31000000'), 0);

    -- Variables de balance P2
    DECLARE @C10_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '10000000'), 0);
    DECLARE @C11_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '11000000'), 0);
    DECLARE @C13_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '13000000'), 0);
    DECLARE @C210_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '21000000'), 0);
    DECLARE @C211_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '21100000'), 0);
    DECLARE @C213_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '21300000'), 0);
    DECLARE @C230_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '23000000'), 0);
    DECLARE @C232_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '23200000'), 0);
    DECLARE @C31_P2 DECIMAL(25,2) = ISNULL((SELECT SaldoP2 FROM #CuentasBalance WHERE Cuenta = '31000000'), 0);

    -- 2. Consultar dinámicamente FAL de XML_ICL (CuentaCatalogo = '200000' o '100000')
    DECLARE @FAL_P1 DECIMAL(25,2) = 0;
    DECLARE @FAL_P2 DECIMAL(25,2) = 0;

    SELECT TOP 1 @FAL_P1 = ISNULL(B.MontoPonderado, B.Monto)
    FROM [dbo].[XML_ICL] B WITH(NOLOCK)
    INNER JOIN [dbo].[XML_Encabezado] A WITH(NOLOCK) ON A.Id = B.IdEncabezado_Id
    WHERE (B.CuentaCatalogo = '200000' OR B.CuentaCatalogo = '100000')
      AND A.IdEntidad_Id = @IDENTIDAD AND A.Periodo = @PERIODO1;

    IF @FAL_P1 = 0
    BEGIN
        SELECT TOP 1 @FAL_P1 = ISNULL(B.MontoPonderado, B.Monto)
        FROM [dbo].[XML_ICL_His] B WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] A WITH(NOLOCK) ON A.Id = B.IdEncabezado_Id
        WHERE (B.CuentaCatalogo = '200000' OR B.CuentaCatalogo = '100000')
          AND A.IdEntidad_Id = @IDENTIDAD AND A.Periodo = @PERIODO1;
    END

    SELECT TOP 1 @FAL_P2 = ISNULL(B.MontoPonderado, B.Monto)
    FROM [dbo].[XML_ICL] B WITH(NOLOCK)
    INNER JOIN [dbo].[XML_Encabezado] A WITH(NOLOCK) ON A.Id = B.IdEncabezado_Id
    WHERE (B.CuentaCatalogo = '200000' OR B.CuentaCatalogo = '100000')
      AND A.IdEntidad_Id = @IDENTIDAD AND A.Periodo = @PERIODO2;

    IF @FAL_P2 = 0
    BEGIN
        SELECT TOP 1 @FAL_P2 = ISNULL(B.MontoPonderado, B.Monto)
        FROM [dbo].[XML_ICL_His] B WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] A WITH(NOLOCK) ON A.Id = B.IdEncabezado_Id
        WHERE (B.CuentaCatalogo = '200000' OR B.CuentaCatalogo = '100000')
          AND A.IdEntidad_Id = @IDENTIDAD AND A.Periodo = @PERIODO2;
    END

    -- 3. Consultar dinámicamente Top 20 depositantes desde Salida_Concentracion_Ahorrantes
    DECLARE @Top20_P1 DECIMAL(25,2) = 0;
    DECLARE @Top20_P2 DECIMAL(25,2) = 0;

    SELECT @Top20_P1 = ISNULL(MontoTop20, 0)
    FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO1;

    SELECT @Top20_P2 = ISNULL(MontoTop20, 0)
    FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO2;

    -- 4. Consultar Pasivos exigibles CP (<= 30 días) desde Salida_Concentracion_Vencimiento (precalculado en Cierre)
    DECLARE @PasCP_P1 DECIMAL(25,2) = 0;
    DECLARE @PasCP_P2 DECIMAL(25,2) = 0;

    SELECT @PasCP_P1 = ISNULL(Tramo1_Vista, 0)
    FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO1;

    SELECT @PasCP_P2 = ISNULL(Tramo1_Vista, 0)
    FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO2;

    -- Si para algún periodo aún no se ha corrido el cierre de vencimientos, usar el saldo de balance a la vista (21100000)
    -- para evitar escaneos pesados sobre XML_Pasivo_Cuenta_Contable_210 en tiempo real
    IF @PasCP_P1 = 0 SET @PasCP_P1 = @C211_P1;
    IF @PasCP_P2 = 0 SET @PasCP_P2 = @C211_P2;

    -- 5. Armar resultado con los 8 indicadores normativos de Liquidez
    CREATE TABLE #Resultados (
        Orden INT,
        Indicador NVARCHAR(150),
        Formula NVARCHAR(250),
        QueMide NVARCHAR(250),
        ValorP1 DECIMAL(18, 6),
        ValorP2 DECIMAL(18, 6),
        EsPorcentaje BIT
    );

    -- 1. Liquidez inmediata: 11000000 / 21100000
    INSERT INTO #Resultados VALUES (
        1, 'Liquidez inmediata', 'Disponibilidades (11000000) / Captaciones a la vista (21100000)',
        'Capacidad de atender retiros inmediatos',
        CASE WHEN @C211_P1 > 0 THEN (@C11_P1 / @C211_P1) ELSE 0 END,
        CASE WHEN @C211_P2 > 0 THEN (@C11_P2 / @C211_P2) ELSE 0 END,
        1
    );

    -- 2. Activos líquidos / Captaciones: FAL / 21000000
    INSERT INTO #Resultados VALUES (
        2, 'Activos líquidos / Captaciones', 'Fondo de Activos Líquidos ICL / Saldo captaciones totales (21000000)',
        'Cobertura general de depósitos con activos líquidos',
        CASE WHEN @C210_P1 > 0 THEN (@FAL_P1 / @C210_P1) ELSE 0 END,
        CASE WHEN @C210_P2 > 0 THEN (@FAL_P2 / @C210_P2) ELSE 0 END,
        1
    );

    -- 3. Activos líquidos / Pasivos exigibles CP
    INSERT INTO #Resultados VALUES (
        3, 'Activos líquidos / Pasivos exigibles CP', 'Activos líquidos (FAL ICL) / Pasivos exigibles <= 30 días',
        'Cobertura de obligaciones de corto plazo',
        CASE WHEN @PasCP_P1 > 0 THEN (@FAL_P1 / @PasCP_P1) ELSE 0 END,
        CASE WHEN @PasCP_P2 > 0 THEN (@FAL_P2 / @PasCP_P2) ELSE 0 END,
        1
    );

    -- 4. Cartera / Captaciones: 13000000 / (21300000 + 21100000)
    DECLARE @Den4_P1 DECIMAL(25,2) = @C213_P1 + @C211_P1;
    DECLARE @Den4_P2 DECIMAL(25,2) = @C213_P2 + @C211_P2;
    INSERT INTO #Resultados VALUES (
        4, 'Cartera / Captaciones', 'Cartera neta (13000000) / (21300000 + 21100000)',
        'Qué proporción de los depósitos está colocada en cartera',
        CASE WHEN @Den4_P1 > 0 THEN (@C13_P1 / @Den4_P1) ELSE 0 END,
        CASE WHEN @Den4_P2 > 0 THEN (@C13_P2 / @Den4_P2) ELSE 0 END,
        1
    );

    -- 5. Cartera / Fondeo estable: 13000000 / (21300000 + 23200000 + 31000000)
    DECLARE @Den5_P1 DECIMAL(25,2) = @C213_P1 + @C232_P1 + @C31_P1;
    DECLARE @Den5_P2 DECIMAL(25,2) = @C213_P2 + @C232_P2 + @C31_P2;
    INSERT INTO #Resultados VALUES (
        5, 'Cartera / Fondeo estable', 'Cartera neta / (Captaciones LP + deuda LP + patrimonio)',
        'Dependencia de fondeo estable para financiar cartera',
        CASE WHEN @Den5_P1 > 0 THEN (@C13_P1 / @Den5_P1) ELSE 0 END,
        CASE WHEN @Den5_P2 > 0 THEN (@C13_P2 / @Den5_P2) ELSE 0 END,
        1
    );

    -- 6. Activos líquidos / Activos: FAL / 10000000
    INSERT INTO #Resultados VALUES (
        6, 'Activos líquidos / Activos', 'FAL ICL / Activo total',
        'Buffer de liquidez respecto al tamaño de la entidad',
        CASE WHEN @C10_P1 > 0 THEN (@FAL_P1 / @C10_P1) ELSE 0 END,
        CASE WHEN @C10_P2 > 0 THEN (@FAL_P2 / @C10_P2) ELSE 0 END,
        1
    );

    -- 7. Activos líquidos / 20 mayores depositantes
    INSERT INTO #Resultados VALUES (
        7, 'Activos líquidos / 20 mayores depositantes', 'Activos líquidos (FAL ICL) / Saldo Top 20',
        'Capacidad de absorber concentración de fondeo',
        CASE WHEN @Top20_P1 > 0 THEN (@FAL_P1 / @Top20_P1) ELSE 0 END,
        CASE WHEN @Top20_P2 > 0 THEN (@FAL_P2 / @Top20_P2) ELSE 0 END,
        1
    );

    -- 8. Fondeo mayorista / Captaciones totales: 23000000 / 21000000
    INSERT INTO #Resultados VALUES (
        8, 'Fondeo mayorista / Captaciones totales', 'Deuda institucional (23000000) / Fondeo total (21000000)',
        'Dependencia de fuentes institucionales',
        CASE WHEN @C210_P1 > 0 THEN (@C230_P1 / @C210_P1) ELSE 0 END,
        CASE WHEN @C210_P2 > 0 THEN (@C230_P2 / @C210_P2) ELSE 0 END,
        1
    );

    SELECT Orden, Indicador, Formula, QueMide, ValorP1, ValorP2, EsPorcentaje
    FROM #Resultados
    ORDER BY Orden ASC;

    DROP TABLE #CuentasBalance;
    DROP TABLE #Resultados;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Indicadores_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 3. PROCEDIMIENTO ALMACENADO: FGA_Consultar_Serie_VaR_Liquidez (100% DINÁMICO)
-- Extrae los últimos N cierres de la cuenta 21103000 desde [SALIDA_BALANCE_COMPROBACION]
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_Serie_VaR_Liquidez]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Serie_VaR_Liquidez];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Serie_VaR_Liquidez]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_CORTE DATE,
    @CANTIDAD INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@CANTIDAD)
        S.PERIODO AS Periodo,
        ABS(ISNULL(S.SALDOFINAL, 0)) AS SaldoAhorroVista
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
    WHERE S.IDENTIDAD = @IDENTIDAD
      AND (S.CUENTA LIKE '21103%' OR S.CUENTA LIKE '211%')
      AND S.PERIODO <= @PERIODO_CORTE
    ORDER BY S.PERIODO DESC;
END
GO
PRINT 'SP [dbo].[FGA_Consultar_Serie_VaR_Liquidez] creado exitosamente.';
GO

-- =====================================================================================
-- 4. TABLA DE SALIDA: GAP CONTRACTUAL (PRECALCULADA EN CIERRE)
-- =====================================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salida_GAP_Contractual')
BEGIN
    CREATE TABLE [dbo].[Salida_GAP_Contractual](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [IdEntidad] [nvarchar](5) NOT NULL,
        [Periodo] [date] NOT NULL,
        [BandaId] [int] NOT NULL,
        [Banda] [nvarchar](50) NOT NULL,
        [Entradas] [decimal](25, 2) NOT NULL DEFAULT (0),
        [Salidas] [decimal](25, 2) NOT NULL DEFAULT (0),
        [BrechaBanda] [decimal](25, 2) NOT NULL DEFAULT (0),
        [BrechaAcumulada] [decimal](25, 2) NOT NULL DEFAULT (0),
        [FechaGeneracion] [datetime] NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Salida_GAP_Contractual] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Salida_GAP_Contractual_Entidad_Periodo] 
    ON [dbo].[Salida_GAP_Contractual] ([IdEntidad], [Periodo]);

    PRINT 'Tabla [dbo].[Salida_GAP_Contractual] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[Salida_GAP_Contractual] ya existe.';
END
GO

-- =====================================================================================
-- 4.1 PROCEDIMIENTO DE GENERACIÓN DE CIERRE: FGA_Generar_GAP_Contractual
-- Procesa los XML de pasivos e inversiones una única vez durante el cierre y los almacena
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Generar_GAP_Contractual]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Generar_GAP_Contractual];
GO

CREATE PROCEDURE [dbo].[FGA_Generar_GAP_Contractual]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_CORTE DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODO_CORTE = DATEFROMPARTS(YEAR(@PERIODO_CORTE), MONTH(@PERIODO_CORTE), 1);

    -- Limpieza previa del período para reproceso
    DELETE FROM [dbo].[Salida_GAP_Contractual]
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO_CORTE;

    -- Tabla temporal para armar los 7 tramos de vencimiento contractual
    CREATE TABLE #Bandas (
        BandaId INT,
        Banda NVARCHAR(50),
        DiasInicio INT,
        DiasFin INT,
        Entradas DECIMAL(25,2) DEFAULT 0,
        Salidas DECIMAL(25,2) DEFAULT 0
    );

    INSERT INTO #Bandas (BandaId, Banda, DiasInicio, DiasFin)
    VALUES 
        (1, '1 a 30 días', 1, 30),
        (2, '31 a 60 días', 31, 60),
        (3, '61 a 90 días', 61, 90),
        (4, '91 a 180 días', 91, 180),
        (5, '181 a 270 días', 181, 270),
        (6, '271 a 365 días', 271, 365),
        (7, '> 365 días', 366, 99999);

    -- 1. Salidas desde XML_Pasivo_Cuenta_Contable_210
    UPDATE B
    SET B.Salidas = ISNULL(P.TotalSalida, 0) / 1000000.0 -- En millones de colones
    FROM #Bandas B
    LEFT JOIN (
        SELECT 
            CASE 
                WHEN A.FechaVencimiento IS NULL OR A.FechaVencimiento <= DATEADD(DAY, 30, @PERIODO_CORTE) THEN 1
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 31 AND 60 THEN 2
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 61 AND 90 THEN 3
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 91 AND 180 THEN 4
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 181 AND 270 THEN 5
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 271 AND 365 THEN 6
                ELSE 7
            END AS TramoId,
            SUM(ISNULL(A.SaldoPrincipal, 0)) AS TotalSalida
        FROM [dbo].[XML_Pasivo_Cuenta_Contable_210] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = A.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = @PERIODO_CORTE
        GROUP BY 
            CASE 
                WHEN A.FechaVencimiento IS NULL OR A.FechaVencimiento <= DATEADD(DAY, 30, @PERIODO_CORTE) THEN 1
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 31 AND 60 THEN 2
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 61 AND 90 THEN 3
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 91 AND 180 THEN 4
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 181 AND 270 THEN 5
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 271 AND 365 THEN 6
                ELSE 7
            END
    ) P ON P.TramoId = B.BandaId;

    -- Si no hubo salidas de XML, usar como contingencia los tramos de Salida_Concentracion_Vencimiento si existen
    IF (SELECT SUM(Salidas) FROM #Bandas) = 0
    BEGIN
        UPDATE B
        SET B.Salidas = CASE B.BandaId
            WHEN 1 THEN ISNULL(V.Tramo1_Vista, 0) / 1000000.0
            WHEN 2 THEN (ISNULL(V.Tramo2_1a90Dias, 0) / 3.0) / 1000000.0
            WHEN 3 THEN (ISNULL(V.Tramo2_1a90Dias, 0) / 3.0) / 1000000.0
            WHEN 4 THEN ISNULL(V.Tramo3_91a180Dias, 0) / 1000000.0
            WHEN 5 THEN ISNULL(V.Tramo4_181a270Dias, 0) / 1000000.0
            WHEN 6 THEN ISNULL(V.Tramo5_271a360Dias, 0) / 1000000.0
            ELSE (ISNULL(V.Tramo6_1a3Anios, 0) + ISNULL(V.Tramo7_Mas3Anios, 0)) / 1000000.0
        END
        FROM #Bandas B
        CROSS JOIN [dbo].[Salida_Concentracion_Vencimiento] V WITH(NOLOCK)
        WHERE V.IdEntidad = @IDENTIDAD AND V.Periodo = @PERIODO_CORTE;
    END

    -- 2. Entradas desde XML_Inversion_Activa
    UPDATE B
    SET B.Entradas = ISNULL(I.TotalEntrada, 0) / 1000000.0
    FROM #Bandas B
    LEFT JOIN (
        SELECT 
            CASE 
                WHEN A.FechaVencimiento IS NULL OR A.FechaVencimiento <= DATEADD(DAY, 30, @PERIODO_CORTE) THEN 1
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 31 AND 60 THEN 2
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 61 AND 90 THEN 3
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 91 AND 180 THEN 4
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 181 AND 270 THEN 5
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 271 AND 365 THEN 6
                ELSE 7
            END AS TramoId,
            SUM(ISNULL(A.SaldoPrincipal, 0)) AS TotalEntrada
        FROM [dbo].[XML_Inversion_Activa] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = A.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = @PERIODO_CORTE
        GROUP BY 
            CASE 
                WHEN A.FechaVencimiento IS NULL OR A.FechaVencimiento <= DATEADD(DAY, 30, @PERIODO_CORTE) THEN 1
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 31 AND 60 THEN 2
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 61 AND 90 THEN 3
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 91 AND 180 THEN 4
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 181 AND 270 THEN 5
                WHEN DATEDIFF(DAY, @PERIODO_CORTE, A.FechaVencimiento) BETWEEN 271 AND 365 THEN 6
                ELSE 7
            END
    ) I ON I.TramoId = B.BandaId;

    -- FAL inicial para acumulación
    DECLARE @FAL_Inicial DECIMAL(25,2) = 0;
    SELECT TOP 1 @FAL_Inicial = ISNULL(B.MontoPonderado, B.Monto) / 1000000.0
    FROM [dbo].[XML_ICL] B WITH(NOLOCK)
    INNER JOIN [dbo].[XML_Encabezado] A WITH(NOLOCK) ON A.Id = B.IdEncabezado_Id
    WHERE (B.CuentaCatalogo = '200000' OR B.CuentaCatalogo = '100000')
      AND A.IdEntidad_Id = @IDENTIDAD AND A.Periodo = @PERIODO_CORTE;

    IF @FAL_Inicial = 0
    BEGIN
        SET @FAL_Inicial = ISNULL((SELECT ABS(SALDOFINAL) / 1000000.0 FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO_CORTE AND CUENTA = '11000000'), 1203.53);
    END

    -- Insertar en la tabla de salida definitiva
    INSERT INTO [dbo].[Salida_GAP_Contractual]
    (
        IdEntidad,
        Periodo,
        BandaId,
        Banda,
        Entradas,
        Salidas,
        BrechaBanda,
        BrechaAcumulada,
        FechaGeneracion
    )
    SELECT
        @IDENTIDAD,
        @PERIODO_CORTE,
        BandaId,
        Banda,
        Entradas,
        Salidas,
        (Entradas - Salidas) AS BrechaBanda,
        @FAL_Inicial + SUM(Entradas - Salidas) OVER (ORDER BY BandaId ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS BrechaAcumulada,
        GETDATE()
    FROM #Bandas
    ORDER BY BandaId ASC;

    DROP TABLE #Bandas;
END
GO
PRINT 'SP [dbo].[FGA_Generar_GAP_Contractual] creado exitosamente.';
GO

-- =====================================================================================
-- 4.2 PROCEDIMIENTO DE CONSULTA: FGA_Consultar_GAP_Contractual (ULTRA RÁPIDO)
-- Lee de la tabla de salida de cierre; si no existe, la genera y guarda de inmediato
-- =====================================================================================
IF OBJECT_ID('[dbo].[FGA_Consultar_GAP_Contractual]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_GAP_Contractual];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_GAP_Contractual]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO_CORTE DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODO_CORTE = DATEFROMPARTS(YEAR(@PERIODO_CORTE), MONTH(@PERIODO_CORTE), 1);

    -- Si no existe cálculo previo de cierre, generarlo una sola vez
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_GAP_Contractual] WITH(NOLOCK) WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO_CORTE)
    BEGIN
        EXEC [dbo].[FGA_Generar_GAP_Contractual] @IDENTIDAD, @PERIODO_CORTE;
    END

    -- Retornar instantáneamente desde la tabla de salida
    SELECT
        BandaId,
        Banda,
        Entradas,
        Salidas,
        BrechaBanda,
        BrechaAcumulada
    FROM [dbo].[Salida_GAP_Contractual] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO_CORTE
    ORDER BY BandaId ASC;
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
    @IDENTIDAD NVARCHAR(10),
    @RENOVACION DECIMAL(8,4),
    @DISMINUCION DECIMAL(8,4),
    @LINEAS DECIMAL(18,2),
    @PAGO_DEUDA DECIMAL(18,2),
    @CUPONES DECIMAL(18,2),
    @USUARIO NVARCHAR(50) = NULL
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
            FechaCreacion,
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
            GETDATE(),
            @USUARIO
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
GRANT EXECUTE ON [dbo].[FGA_Generar_GAP_Contractual] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Consultar_GAP_Contractual] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [FGA];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [FGA];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_GAP_Contractual] TO [FGA];
PRINT 'Permisos asignados exitosamente.';
GO

-- =====================================================================================
-- 7. CONFIGURACIÓN DEL MENÚ Y PERMISOS: SUBMÓDULO DE LIQUIDEZ (ParentId = 1500)
-- =====================================================================================
PRINT 'Configurando menú y permisos para el Submódulo de Liquidez...';

IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5000)
BEGIN
    UPDATE dbo.Menu
    SET MenuText = 'Liquidez',
        MenuURL = 'Liquidez/Index',
        ParentId = 1500,
        SortOrder = 3,
        MenuIcon = '<i class="fa fa-tint"></i>',
        Description = 'Reconfiguración del submódulo: Liquidez Contractual y Escenarios de Estrés.'
    WHERE Id = 5000;
    PRINT '   -> Opción de menú 5000 [Liquidez] actualizada bajo Estructura Financiera (ParentId: 1500).';
END
ELSE
BEGIN
    SET IDENTITY_INSERT dbo.Menu ON;
    INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
    VALUES (5000, 'Liquidez', 'Liquidez/Index', 1500, 3, '<i class="fa fa-tint"></i>', 'Reconfiguración del submódulo: Liquidez Contractual y Escenarios de Estrés.');
    SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Opción de menú 5000 [Liquidez] creada exitosamente.';
END

UPDATE dbo.Menu SET ParentId = NULL WHERE ParentId = 5000 AND Id NOT IN (5000);

IF NOT EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 5000 AND RoleId = 1)
BEGIN
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    VALUES (5000, 1, 3, 1, 1, 1, 1);
    PRINT '   -> Permiso asignado al Role 1 (Administrador) para el Menú 5000.';
END
ELSE
BEGIN
    UPDATE dbo.MenuPermission
    SET IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1, SortOrder = 3
    WHERE MenuId = 5000 AND RoleId = 1;
    PRINT '   -> Permiso actualizado para el Role 1.';
END

INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
SELECT 5000, r.Id, 3, 1, 1, 1, 1
FROM dbo.Role r
WHERE r.Id IN (2, 4, 5, 6, 7, 10, 12)
  AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 5000 AND mp.RoleId = r.Id);

PRINT 'Permisos de menú para Liquidez asignados exitosamente.';
GO
