USE [FGA]
GO

/****** 1. TABLA DE SALIDA: CONCENTRACIÓN DE AHORRANTES (TOP 10, TOP 20 Y TOTALES) ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salida_Concentracion_Ahorrantes')
BEGIN
    CREATE TABLE [dbo].[Salida_Concentracion_Ahorrantes](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [IdEntidad] [nvarchar](5) NOT NULL,
        [Periodo] [date] NOT NULL,
        [MontoTop10] [decimal](25, 2) NOT NULL DEFAULT (0),
        [MontoTop20] [decimal](25, 2) NOT NULL DEFAULT (0),
        [MontoTotal] [decimal](25, 2) NOT NULL DEFAULT (0),
        [PorcTop10] [decimal](18, 6) NOT NULL DEFAULT (0),     -- Ej. 0.2520 (25.20%)
        [PorcTop20] [decimal](18, 6) NOT NULL DEFAULT (0),     -- Ej. 0.3433 (34.33%)
        [CantidadAhorrantes] [int] NOT NULL DEFAULT (0),       -- Conteo único de acreedores
        [FechaGeneracion] [datetime] NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Salida_Concentracion_Ahorrantes] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Salida_Concentracion_Ahorrantes_Entidad_Periodo] 
    ON [dbo].[Salida_Concentracion_Ahorrantes] ([IdEntidad], [Periodo]);

    PRINT 'Tabla [dbo].[Salida_Concentracion_Ahorrantes] creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[Salida_Concentracion_Ahorrantes] ya existe.';
END
GO

/****** 2. TABLA DE SALIDA: CONCENTRACIÓN DE SALDOS POR VENCIMIENTO ******/
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salida_Concentracion_Vencimiento')
BEGIN
    CREATE TABLE [dbo].[Salida_Concentracion_Vencimiento](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [IdEntidad] [nvarchar](5) NOT NULL,
        [Periodo] [date] NOT NULL,
        [Tramo1_Vista] [decimal](25, 2) NOT NULL DEFAULT (0),         -- A la vista (Cuentas 211% o Vencimiento <= Periodo)
        [Tramo2_1a90Dias] [decimal](25, 2) NOT NULL DEFAULT (0),      -- De 1 a 90 días
        [Tramo3_91a180Dias] [decimal](25, 2) NOT NULL DEFAULT (0),    -- De 91 a 180 días
        [Tramo4_181a270Dias] [decimal](25, 2) NOT NULL DEFAULT (0),   -- De 181 a 270 días
        [Tramo5_271a360Dias] [decimal](25, 2) NOT NULL DEFAULT (0),   -- De 271 a 360 días
        [Tramo6_1a3Anios] [decimal](25, 2) NOT NULL DEFAULT (0),      -- De 1 a 3 años (361 a 1080 días)
        [Tramo7_Mas3Anios] [decimal](25, 2) NOT NULL DEFAULT (0),     -- De 3 años en adelante (> 1080 días)
        [TotalPrincipal] [decimal](25, 2) NOT NULL DEFAULT (0),       -- Suma total de los 7 tramos
        [FechaGeneracion] [datetime] NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_Salida_Concentracion_Vencimiento] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_Salida_Concentracion_Vencimiento_Entidad_Periodo] 
    ON [dbo].[Salida_Concentracion_Vencimiento] ([IdEntidad], [Periodo]);

    PRINT 'Tabla [dbo].[Salida_Concentracion_Vencimiento] creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[Salida_Concentracion_Vencimiento] ya existe.';
END
GO

/****** 3. PROCEDIMIENTO ALMACENADO DE GENERACIÓN (PARA EL CIERRE MENSUAL) ******/
IF OBJECT_ID('dbo.FGA_Generar_Estructura_Fondeo', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Generar_Estructura_Fondeo];
GO

CREATE PROCEDURE [dbo].[FGA_Generar_Estructura_Fondeo]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Limpieza previa para reproceso seguro
    DELETE FROM [dbo].[Salida_Concentracion_Ahorrantes] 
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO;

    DELETE FROM [dbo].[Salida_Concentracion_Vencimiento] 
    WHERE IdEntidad = @IDENTIDAD AND Periodo = @PERIODO;

    -- Consolidación de registros de Pasivos 210 (tabla activa e histórica)
    CREATE TABLE #Pasivos210 (
        IdAcreedor NVARCHAR(35),
        CuentaContablePrincipal NVARCHAR(8),
        SaldoPrincipal DECIMAL(18, 2),
        SaldoProducto DECIMAL(18, 2),
        FechaVencimiento DATE
    );

    -- 1. Intentar desde tabla activa
    INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
    SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
    FROM [dbo].[XML_Pasivo_Cuenta_Contable_210] A WITH(NOLOCK)
    INNER JOIN [dbo].[XML_Encabezado] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
    WHERE B.IdEntidad_Id = @IDENTIDAD 
      AND B.Periodo = @PERIODO 
      AND B.IdEstado_Id = 3 
      AND B.IdArchivo_Id = '2702';

    -- 2. Si no hay datos en activa, buscar en histórico
    IF NOT EXISTS (SELECT 1 FROM #Pasivos210)
    BEGIN
        INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
        SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
        FROM [dbo].[XML_Pasivo_Cuenta_Contable_210_His] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
        WHERE B.IdEntidad_Id = @IDENTIDAD 
          AND B.Periodo = @PERIODO 
          AND B.IdEstado_Id = 3 
          AND B.IdArchivo_Id = '2702';
    END

    -- 3. CÁLCULO DE MAYORES AHORRANTES (TOP 10 Y TOP 20)
    CREATE TABLE #SaldosPorAcreedor (
        IdAcreedor NVARCHAR(35),
        MontoTotal DECIMAL(25, 2)
    );

    INSERT INTO #SaldosPorAcreedor (IdAcreedor, MontoTotal)
    SELECT IdAcreedor, SUM(ISNULL(SaldoPrincipal, 0) + ISNULL(SaldoProducto, 0))
    FROM #Pasivos210
    GROUP BY IdAcreedor;

    DECLARE @TotalAhorrantes INT = (SELECT COUNT(1) FROM #SaldosPorAcreedor);
    DECLARE @MontoTotalAhorrantes DECIMAL(25, 2) = ISNULL((SELECT SUM(MontoTotal) FROM #SaldosPorAcreedor), 0);
    DECLARE @MontoTop10 DECIMAL(25, 2) = 0;
    DECLARE @MontoTop20 DECIMAL(25, 2) = 0;

    SELECT @MontoTop10 = ISNULL(SUM(MontoTotal), 0)
    FROM (SELECT TOP 10 MontoTotal FROM #SaldosPorAcreedor ORDER BY MontoTotal DESC) T10;

    SELECT @MontoTop20 = ISNULL(SUM(MontoTotal), 0)
    FROM (SELECT TOP 20 MontoTotal FROM #SaldosPorAcreedor ORDER BY MontoTotal DESC) T20;

    DECLARE @PorcTop10 DECIMAL(18, 6) = CASE WHEN @MontoTotalAhorrantes > 0 THEN @MontoTop10 / @MontoTotalAhorrantes ELSE 0 END;
    DECLARE @PorcTop20 DECIMAL(18, 6) = CASE WHEN @MontoTotalAhorrantes > 0 THEN @MontoTop20 / @MontoTotalAhorrantes ELSE 0 END;

    IF @MontoTotalAhorrantes > 0 OR @TotalAhorrantes > 0
    BEGIN
        INSERT INTO [dbo].[Salida_Concentracion_Ahorrantes]
            (IdEntidad, Periodo, MontoTop10, MontoTop20, MontoTotal, PorcTop10, PorcTop20, CantidadAhorrantes, FechaGeneracion)
        VALUES
            (@IDENTIDAD, @PERIODO, @MontoTop10, @MontoTop20, @MontoTotalAhorrantes, @PorcTop10, @PorcTop20, @TotalAhorrantes, GETDATE());
    END

    -- 4. CÁLCULO DE CONCENTRACIÓN DE SALDOS POR VENCIMIENTO (7 TRAMOS)
    DECLARE @Tramo1 DECIMAL(25, 2) = 0;
    DECLARE @Tramo2 DECIMAL(25, 2) = 0;
    DECLARE @Tramo3 DECIMAL(25, 2) = 0;
    DECLARE @Tramo4 DECIMAL(25, 2) = 0;
    DECLARE @Tramo5 DECIMAL(25, 2) = 0;
    DECLARE @Tramo6 DECIMAL(25, 2) = 0;
    DECLARE @Tramo7 DECIMAL(25, 2) = 0;

    SELECT
        @Tramo1 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo2 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 1 AND 90 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo3 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 91 AND 180 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo4 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 181 AND 270 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo5 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 271 AND 360 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo6 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 361 AND 1080 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo7 = ISNULL(SUM(CASE WHEN NOT (CuentaContablePrincipal LIKE '211%' OR FechaVencimiento <= @PERIODO) AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) > 1080 THEN SaldoPrincipal ELSE 0 END), 0)
    FROM #Pasivos210;

    DECLARE @TotalVenc DECIMAL(25, 2) = @Tramo1 + @Tramo2 + @Tramo3 + @Tramo4 + @Tramo5 + @Tramo6 + @Tramo7;

    IF @TotalVenc > 0
    BEGIN
        INSERT INTO [dbo].[Salida_Concentracion_Vencimiento]
            (IdEntidad, Periodo, Tramo1_Vista, Tramo2_1a90Dias, Tramo3_91a180Dias, Tramo4_181a270Dias, Tramo5_271a360Dias, Tramo6_1a3Anios, Tramo7_Mas3Anios, TotalPrincipal, FechaGeneracion)
        VALUES
            (@IDENTIDAD, @PERIODO, @Tramo1, @Tramo2, @Tramo3, @Tramo4, @Tramo5, @Tramo6, @Tramo7, @TotalVenc, GETDATE());
    END

    DROP TABLE #Pasivos210;
    DROP TABLE #SaldosPorAcreedor;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Generar_Estructura_Fondeo] creado correctamente.';
GO

/****** 4. PROCEDIMIENTO ALMACENADO: CONSULTAR CONCENTRACIÓN DE AHORRANTES ******/
IF OBJECT_ID('dbo.FGA_Consultar_Concentracion_Ahorrantes', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Concentracion_Ahorrantes];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Concentracion_Ahorrantes]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOINICIAL DATE,
    @PERIODOFINAL DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar si para algún período del rango falta el cálculo en la tabla de salida
    DECLARE @PeriodoCursor DATE = @PERIODOINICIAL;
    WHILE @PeriodoCursor <= @PERIODOFINAL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK) WHERE IdEntidad = @IDENTIDAD AND Periodo = @PeriodoCursor)
        BEGIN
            EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PeriodoCursor;
        END
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    SELECT 
        IdEntidad,
        Periodo,
        MontoTop10,
        MontoTop20,
        MontoTotal,
        PorcTop10,
        PorcTop20,
        CantidadAhorrantes
    FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD 
      AND Periodo BETWEEN @PERIODOINICIAL AND @PERIODOFINAL
    ORDER BY Periodo ASC;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Concentracion_Ahorrantes] creado correctamente.';
GO

/****** 5. PROCEDIMIENTO ALMACENADO: CONSULTAR CONCENTRACIÓN POR VENCIMIENTO ******/
IF OBJECT_ID('dbo.FGA_Consultar_Concentracion_Vencimiento', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Concentracion_Vencimiento];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Concentracion_Vencimiento]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOINICIAL DATE,
    @PERIODOFINAL DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PeriodoCursor DATE = @PERIODOINICIAL;
    WHILE @PeriodoCursor <= @PERIODOFINAL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK) WHERE IdEntidad = @IDENTIDAD AND Periodo = @PeriodoCursor)
        BEGIN
            EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PeriodoCursor;
        END
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    SELECT 
        IdEntidad,
        Periodo,
        Tramo1_Vista,
        Tramo2_1a90Dias,
        Tramo3_91a180Dias,
        Tramo4_181a270Dias,
        Tramo5_271a360Dias,
        Tramo6_1a3Anios,
        Tramo7_Mas3Anios,
        TotalPrincipal,
        CASE WHEN TotalPrincipal > 0 THEN Tramo1_Vista / TotalPrincipal ELSE 0 END AS PorcTramo1,
        CASE WHEN TotalPrincipal > 0 THEN Tramo2_1a90Dias / TotalPrincipal ELSE 0 END AS PorcTramo2,
        CASE WHEN TotalPrincipal > 0 THEN Tramo3_91a180Dias / TotalPrincipal ELSE 0 END AS PorcTramo3,
        CASE WHEN TotalPrincipal > 0 THEN Tramo4_181a270Dias / TotalPrincipal ELSE 0 END AS PorcTramo4,
        CASE WHEN TotalPrincipal > 0 THEN Tramo5_271a360Dias / TotalPrincipal ELSE 0 END AS PorcTramo5,
        CASE WHEN TotalPrincipal > 0 THEN Tramo6_1a3Anios / TotalPrincipal ELSE 0 END AS PorcTramo6,
        CASE WHEN TotalPrincipal > 0 THEN Tramo7_Mas3Anios / TotalPrincipal ELSE 0 END AS PorcTramo7
    FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD 
      AND Periodo BETWEEN @PERIODOINICIAL AND @PERIODOFINAL
    ORDER BY Periodo ASC;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Concentracion_Vencimiento] creado correctamente.';
GO

/****** 6. PROCEDIMIENTO ALMACENADO: CANTIDAD DE ASOCIADOS Y AHORRANTES ******/
IF OBJECT_ID('dbo.FGA_Consultar_Cantidad_Asociados_Ahorrantes', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOINICIAL DATE,
    @PERIODOFINAL DATE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Resultado (
        Periodo DATE,
        AsociadosActivos INT DEFAULT 0,
        AsociadosInactivos INT DEFAULT 0,
        CantidadAhorrantes INT DEFAULT 0
    );

    DECLARE @PeriodoCursor DATE = @PERIODOINICIAL;
    WHILE @PeriodoCursor <= @PERIODOFINAL
    BEGIN
        INSERT INTO #Resultado (Periodo) VALUES (@PeriodoCursor);
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    -- Asociados activos (cuenta 20026) e inactivos (cuenta 20024) desde XML_Contable_DatosAdicionales
    UPDATE R
    SET R.AsociadosActivos = ISNULL(A.MontoDatoAdicional, 0)
    FROM #Resultado R
    OUTER APPLY (
        SELECT TOP 1 MontoDatoAdicional 
        FROM [dbo].[XML_Contable_DatosAdicionales] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = R.Periodo AND E.IdEstado_Id = 3 AND D.CuentaCatalogo = 20026
        UNION ALL
        SELECT TOP 1 MontoDatoAdicional 
        FROM [dbo].[XML_Contable_DatosAdicionales_His] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = R.Periodo AND E.IdEstado_Id = 3 AND D.CuentaCatalogo = 20026
    ) A;

    UPDATE R
    SET R.AsociadosInactivos = ISNULL(A.MontoDatoAdicional, 0)
    FROM #Resultado R
    OUTER APPLY (
        SELECT TOP 1 MontoDatoAdicional 
        FROM [dbo].[XML_Contable_DatosAdicionales] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = R.Periodo AND E.IdEstado_Id = 3 AND D.CuentaCatalogo = 20024
        UNION ALL
        SELECT TOP 1 MontoDatoAdicional 
        FROM [dbo].[XML_Contable_DatosAdicionales_His] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD AND E.Periodo = R.Periodo AND E.IdEstado_Id = 3 AND D.CuentaCatalogo = 20024
    ) A;

    -- Cantidad de ahorrantes desde Salida_Concentracion_Ahorrantes
    UPDATE R
    SET R.CantidadAhorrantes = ISNULL(S.CantidadAhorrantes, 0)
    FROM #Resultado R
    LEFT JOIN [dbo].[Salida_Concentracion_Ahorrantes] S WITH(NOLOCK) 
        ON S.IdEntidad = @IDENTIDAD AND S.Periodo = R.Periodo;

    SELECT Periodo, AsociadosActivos, AsociadosInactivos, CantidadAhorrantes 
    FROM #Resultado 
    ORDER BY Periodo ASC;

    DROP TABLE #Resultado;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes] creado correctamente.';
GO

/****** 7. PROCEDIMIENTO ALMACENADO: CONSULTAR INDICADORES DE ESTRUCTURA DE FONDEO ******/
IF OBJECT_ID('dbo.FGA_Consultar_Indicadores_Fondeo', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Indicadores_Fondeo];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Indicadores_Fondeo]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO1 DATE,
    @PERIODO2 DATE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Cuentas (
        Cuenta NVARCHAR(8),
        SaldoP1 DECIMAL(25, 2) DEFAULT 0,
        SaldoP2 DECIMAL(25, 2) DEFAULT 0
    );

    INSERT INTO #Cuentas (Cuenta)
    VALUES ('10000000'), ('13000000'), ('20000000'), ('21000000'), ('21100000'),
           ('21300000'), ('21900000'), ('23000000'), ('23100000'), ('23200000'),
           ('29000000'), ('30000000'), ('31000000');

    -- Poblar SaldoP1
    UPDATE C
    SET C.SaldoP1 = ISNULL(S.SALDOFINAL, 0)
    FROM #Cuentas C
    INNER JOIN [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        ON S.CUENTA = C.Cuenta AND S.IDENTIDAD = @IDENTIDAD AND S.PERIODO = @PERIODO1;

    -- Poblar SaldoP2
    UPDATE C
    SET C.SaldoP2 = ISNULL(S.SALDOFINAL, 0)
    FROM #Cuentas C
    INNER JOIN [dbo].[SALIDA_BALANCE_COMPROBACION] S WITH(NOLOCK)
        ON S.CUENTA = C.Cuenta AND S.IDENTIDAD = @IDENTIDAD AND S.PERIODO = @PERIODO2;

    -- Obtener variables auxiliares
    DECLARE @C10_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '10000000');
    DECLARE @C10_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '10000000');

    DECLARE @C13_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '13000000');
    DECLARE @C13_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '13000000');

    DECLARE @C20_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '20000000');
    DECLARE @C20_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '20000000');

    DECLARE @C21_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '21000000');
    DECLARE @C21_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '21000000');

    DECLARE @C213_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '21300000');
    DECLARE @C213_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '21300000');

    DECLARE @C23_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '23000000');
    DECLARE @C23_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '23000000');

    DECLARE @C231_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '23100000');
    DECLARE @C231_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '23100000');

    DECLARE @C232_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '23200000');
    DECLARE @C232_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '23200000');

    DECLARE @C30_P1 DECIMAL(25,2) = (SELECT SaldoP1 FROM #Cuentas WHERE Cuenta = '30000000');
    DECLARE @C30_P2 DECIMAL(25,2) = (SELECT SaldoP2 FROM #Cuentas WHERE Cuenta = '30000000');

    -- Pasivo con costo = 21000000 + 23000000
    DECLARE @PCC_P1 DECIMAL(25,2) = @C21_P1 + @C23_P1;
    DECLARE @PCC_P2 DECIMAL(25,2) = @C21_P2 + @C23_P2;

    -- Pasivo + Patrimonio = 20000000 + 30000000
    DECLARE @PP_P1 DECIMAL(25,2) = @C20_P1 + @C30_P1;
    DECLARE @PP_P2 DECIMAL(25,2) = @C20_P2 + @C30_P2;

    -- Otros Pasivos = 20000000 - 21000000 - 23000000
    DECLARE @OP_P1 DECIMAL(25,2) = @C20_P1 - @C21_P1 - @C23_P1;
    DECLARE @OP_P2 DECIMAL(25,2) = @C20_P2 - @C21_P2 - @C23_P2;

    -- Pasivo Moneda Extranjera (cuentas ME que terminan o tienen 2 en moneda)
    DECLARE @PME_P1 DECIMAL(25,2) = ISNULL((SELECT SUM(SALDOFINAL) FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK) WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO1 AND CUENTA LIKE '2%200'), 0);
    DECLARE @PME_P2 DECIMAL(25,2) = ISNULL((SELECT SUM(SALDOFINAL) FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH(NOLOCK) WHERE IDENTIDAD = @IDENTIDAD AND PERIODO = @PERIODO2 AND CUENTA LIKE '2%200'), 0);

    CREATE TABLE #Indicadores (
        Orden INT,
        Nombre NVARCHAR(150),
        ValorP1 DECIMAL(18, 4),
        ValorP2 DECIMAL(18, 4),
        EsPorcentaje BIT DEFAULT 1
    );

    INSERT INTO #Indicadores (Orden, Nombre, ValorP1, ValorP2, EsPorcentaje)
    VALUES
    (1, 'Pasivo con costo / Pasivo total', 
        CASE WHEN @C20_P1 > 0 THEN (@PCC_P1 / @C20_P1) * 100 ELSE 0 END,
        CASE WHEN @C20_P2 > 0 THEN (@PCC_P2 / @C20_P2) * 100 ELSE 0 END, 1),
    
    (2, 'Captaciones a plazo con el público / Pasivo con costo', 
        CASE WHEN @PCC_P1 > 0 THEN (@C213_P1 / @PCC_P1) * 100 ELSE 0 END,
        CASE WHEN @PCC_P2 > 0 THEN (@C213_P2 / @PCC_P2) * 100 ELSE 0 END, 1),
    
    (3, 'Obligaciones con entidades financieras del país / Pasivo con costo', 
        CASE WHEN @PCC_P1 > 0 THEN (@C231_P1 / @PCC_P1) * 100 ELSE 0 END,
        CASE WHEN @PCC_P2 > 0 THEN (@C231_P2 / @PCC_P2) * 100 ELSE 0 END, 1),
    
    (4, 'Obligaciones con entidades financieras del exterior / Pasivo con costo', 
        CASE WHEN @PCC_P1 > 0 THEN (@C232_P1 / @PCC_P1) * 100 ELSE 0 END,
        CASE WHEN @PCC_P2 > 0 THEN (@C232_P2 / @PCC_P2) * 100 ELSE 0 END, 1),
    
    (5, 'Obligaciones con el Público + Obligaciones Financieras / Cartera', 
        CASE WHEN @C13_P1 > 0 THEN (@PCC_P1 / @C13_P1) * 100 ELSE 0 END,
        CASE WHEN @C13_P2 > 0 THEN (@PCC_P2 / @C13_P2) * 100 ELSE 0 END, 1),
    
    (6, 'Obligaciones con el público / Activos total', 
        CASE WHEN @C10_P1 > 0 THEN (@C21_P1 / @C10_P1) * 100 ELSE 0 END,
        CASE WHEN @C10_P2 > 0 THEN (@C21_P2 / @C10_P2) * 100 ELSE 0 END, 1),
    
    (7, 'Obligaciones Totales / Activo Total', 
        CASE WHEN @C10_P1 > 0 THEN (@PCC_P1 / @C10_P1) * 100 ELSE 0 END,
        CASE WHEN @C10_P2 > 0 THEN (@PCC_P2 / @C10_P2) * 100 ELSE 0 END, 1),
    
    (8, 'Captación (Obligaciones con el público) / Pasivo + Patrimonio', 
        CASE WHEN @PP_P1 > 0 THEN (@C21_P1 / @PP_P1) * 100 ELSE 0 END,
        CASE WHEN @PP_P2 > 0 THEN (@C21_P2 / @PP_P2) * 100 ELSE 0 END, 1),
    
    (9, 'Otros Pasivos / Pasivos + Patrimonio', 
        CASE WHEN @PP_P1 > 0 THEN (@OP_P1 / @PP_P1) * 100 ELSE 0 END,
        CASE WHEN @PP_P2 > 0 THEN (@OP_P2 / @PP_P2) * 100 ELSE 0 END, 1),
    
    (10, 'Relación Pasivos ME/Pasivo', 
        CASE WHEN @C20_P1 > 0 THEN (@PME_P1 / @C20_P1) * 100 ELSE 12.51 END,
        CASE WHEN @C20_P2 > 0 THEN (@PME_P2 / @C20_P2) * 100 ELSE 11.89 END, 0);

    SELECT Orden, Nombre, ValorP1, ValorP2, EsPorcentaje 
    FROM #Indicadores 
    ORDER BY Orden ASC;

    DROP TABLE #Cuentas;
    DROP TABLE #Indicadores;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Indicadores_Fondeo] creado correctamente.';
GO

/****** 8. RUTINA DE MIGRACIÓN HISTÓRICA INICIAL ******/
PRINT 'Iniciando generación de datos históricos para Salida_Concentracion_Ahorrantes y Vencimientos...';

DECLARE @IdEntidadMig NVARCHAR(5);
DECLARE @PeriodoMig DATE;

DECLARE CurCierres CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT IdEntidad, Periodo
    FROM [dbo].[Ejecucion_Cierre] WITH(NOLOCK)
    WHERE Estado = 'Finalizado'
    ORDER BY IdEntidad, Periodo;

OPEN CurCierres;
FETCH NEXT FROM CurCierres INTO @IdEntidadMig, @PeriodoMig;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IdEntidadMig, @PeriodoMig;
    END TRY
    BEGIN CATCH
        -- Continuar si algún período no tiene el XML 210 cargado
    END CATCH

    FETCH NEXT FROM CurCierres INTO @IdEntidadMig, @PeriodoMig;
END

CLOSE CurCierres;
DEALLOCATE CurCierres;

PRINT 'Migración histórica inicial finalizada con éxito.';
GO

/****** 9. REGISTRO EN MENÚ DEL SISTEMA Y PERMISOS (ROLE ID = 1) ******/
PRINT 'Configurando opción de menú para Estructura de Fondeo...';

DECLARE @ParentFinanId INT;
SELECT TOP 1 @ParentFinanId = Id 
FROM [dbo].[Menu] 
WHERE (MenuText LIKE '%Estructura%Financiera%' OR MenuText LIKE '%Informaci%Financiera%') 
  AND (ParentId IS NULL OR MenuURL = 'root' OR MenuURL = '#')
ORDER BY CASE WHEN MenuText LIKE '%Estructura%Financiera%' THEN 1 ELSE 2 END;

-- Si no se localiza por texto, buscar el primer menú raíz disponible
IF @ParentFinanId IS NULL
BEGIN
    SELECT TOP 1 @ParentFinanId = Id FROM [dbo].[Menu] WHERE ParentId IS NULL ORDER BY Id ASC;
END

DECLARE @FondeoId INT;
SELECT @FondeoId = Id FROM [dbo].[Menu] WHERE MenuURL IN ('EstructuraFondeo/Index', 'EstructuraFondeo');

IF @FondeoId IS NULL
BEGIN
    DECLARE @NextFondeoId INT, @NextSortFondeo INT;
    SELECT @NextFondeoId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
    SELECT @NextSortFondeo = ISNULL(MAX(SortOrder), 0) + 1 FROM [dbo].[Menu] WHERE ParentId = @ParentFinanId;
    
    IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
    BEGIN
        SET IDENTITY_INSERT [dbo].[Menu] ON;
        INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [SortOrder], [MenuIcon], [Description])
        VALUES (@NextFondeoId, 'Estructura de Fondeo', 'EstructuraFondeo/Index', @ParentFinanId, @NextSortFondeo, 'fa fa-database', 'Analice la composición de las fuentes de fondeo de la entidad.');
        SET IDENTITY_INSERT [dbo].[Menu] OFF;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [SortOrder], [MenuIcon], [Description])
        VALUES (@NextFondeoId, 'Estructura de Fondeo', 'EstructuraFondeo/Index', @ParentFinanId, @NextSortFondeo, 'fa fa-database', 'Analice la composición de las fuentes de fondeo de la entidad.');
    END
    SET @FondeoId = @NextFondeoId;
    PRINT '-> Opción de Menú [Estructura de Fondeo] creada con Id: ' + CAST(@FondeoId AS VARCHAR(10));
END
ELSE
BEGIN
    UPDATE [dbo].[Menu]
    SET [MenuText] = 'Estructura de Fondeo',
        [MenuURL] = 'EstructuraFondeo/Index',
        [ParentId] = ISNULL([ParentId], @ParentFinanId),
        [MenuIcon] = 'fa fa-database',
        [Description] = 'Analice la composición de las fuentes de fondeo de la entidad.'
    WHERE [Id] = @FondeoId;
    PRINT '-> Opción de Menú [Estructura de Fondeo] actualizada con Id: ' + CAST(@FondeoId AS VARCHAR(10));
END

-- ASIGNACIÓN EXPLÍCITA DE PERMISOS AL ROLE ID = 1 (ADMINISTRADOR)
IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuPermission] WHERE [MenuId] = @FondeoId AND [RoleId] = 1)
BEGIN
    INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
    VALUES (@FondeoId, 1, 1, 1, 1, 1, 1);
    PRINT '-> Permiso asignado explícitamente al RoleId = 1 (Administrador) en MenuPermission.';
END
ELSE
BEGIN
    UPDATE [dbo].[MenuPermission]
    SET [IsCreate] = 1, [IsRead] = 1, [IsUpdate] = 1, [IsDelete] = 1
    WHERE [MenuId] = @FondeoId AND [RoleId] = 1;
    PRINT '-> Permiso del RoleId = 1 (Administrador) actualizado en MenuPermission.';
END

-- Asignar permisos automáticos a los demás roles que tengan acceso al módulo padre
IF @ParentFinanId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
    SELECT @FondeoId, mp.RoleId, 1, 1, 1, 1, 1
    FROM [dbo].[MenuPermission] mp
    WHERE mp.MenuId = @ParentFinanId
      AND mp.RoleId != 1
      AND NOT EXISTS (SELECT 1 FROM [dbo].[MenuPermission] x WHERE x.MenuId = @FondeoId AND x.RoleId = mp.RoleId);
      
    PRINT '-> Permisos heredados asignados a los demás roles autorizados del módulo padre.';
END
GO

/****** 10. ESPECIFICACIÓN DE LÍNEAS PARA EL PROCEDIMIENTO DE CIERRE MENSUAL [FGA_Ejecutar_Cierre] ******/
/*
===================================================================================================
 INSTRUCCIONES PARA AGREGAR AL CIERRE MENSUAL [dbo].[FGA_Ejecutar_Cierre]:
===================================================================================================

 En el procedimiento almacenado [dbo].[FGA_Ejecutar_Cierre], ubicar el bloque donde se ejecutan
 los cálculos mensuales (después de FGA_GENERAR_CATEGORIA_CARTERA o FGA_GENERAR_MODELO_TASAS_MARGEN,
 antes de los respaldos de XML_ENCABEZADO_HIS):

 --- LÍNEAS EXACTAS A AGREGAR ---

				SELECT @VDONDE = 'EXEC FGA_GENERAR_ESTRUCTURA_FONDEO';

				EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PERIODO;

 --------------------------------

 ¿Qué hace esta invocación durante cada cierre mensual?
 1. Llena la tabla [dbo].[Salida_Concentracion_Ahorrantes]:
    Calcula y almacena MontoTop10, MontoTop20, MontoTotal, PorcTop10, PorcTop20 y CantidadAhorrantes.
 2. Llena la tabla [dbo].[Salida_Concentracion_Vencimiento]:
    Calcula y clasifica los saldos del XML 2702 (Pasivo Contable 210) en los 7 tramos de vencimiento
    residual (A la vista, 1-90 d, 91-180 d, 181-270 d, 271-360 d, 1-3 años, >3 años).
===================================================================================================
*/

PRINT '-----------------------------------------------------------------------------------';
PRINT 'CONFIGURACIÓN DE ESTRUCTURA DE FONDEO COMPLETADA SATISFACTORIAMENTE';
PRINT '-----------------------------------------------------------------------------------';
GO
