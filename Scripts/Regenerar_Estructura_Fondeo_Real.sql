USE [FGA];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

PRINT '================================================================================';
PRINT '   REGENERACIÓN COMPLETA DE ESTRUCTURA DE FONDEO CON DATOS 100% REALES';
PRINT '================================================================================';
GO

/****** 1. LIMPIEZA DE CUENTAS AUXILIARES Y LÍNEAS DE BALANCE EXTRAS ******/
PRINT 'Eliminando líneas y cuentas auxiliares extras de Salida_Balance_Comprobacion y CatalogoCuenta...';
DELETE FROM dbo.Salida_Balance_Comprobacion WHERE Cuenta LIKE '99900%';
DELETE FROM dbo.CatalogoCuenta WHERE Cuenta LIKE '99900%';
PRINT 'Cuentas auxiliares eliminadas correctamente.';
GO

/****** 2. TABLA: SALIDA DE CONCENTRACIÓN DE AHORRANTES ******/
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salida_Concentracion_Ahorrantes')
BEGIN
    CREATE TABLE [dbo].[Salida_Concentracion_Ahorrantes] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdEntidad] NVARCHAR(5) NOT NULL,
        [Periodo] DATE NOT NULL,
        [MontoTop10] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [MontoTop20] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [MontoTotal] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [PorcTop10] DECIMAL(18, 6) NOT NULL DEFAULT 0,
        [PorcTop20] DECIMAL(18, 6) NOT NULL DEFAULT 0,
        [CantidadAhorrantes] INT NOT NULL DEFAULT 0,
        [FechaGeneracion] DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE NONCLUSTERED INDEX [IX_Salida_Concentracion_Ahorrantes_Busqueda] 
    ON [dbo].[Salida_Concentracion_Ahorrantes] ([IdEntidad], [Periodo]);
    
    PRINT 'Tabla [dbo].[Salida_Concentracion_Ahorrantes] creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[Salida_Concentracion_Ahorrantes] ya existe.';
END
GO

/****** 3. TABLA: SALIDA DE CONCENTRACIÓN DE VENCIMIENTOS ******/
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salida_Concentracion_Vencimiento')
BEGIN
    CREATE TABLE [dbo].[Salida_Concentracion_Vencimiento] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IdEntidad] NVARCHAR(5) NOT NULL,
        [Periodo] DATE NOT NULL,
        [Tramo1_Vista] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo2_1a90Dias] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo3_91a180Dias] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo4_181a270Dias] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo5_271a360Dias] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo6_1a3Anios] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [Tramo7_Mas3Anios] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [TotalPrincipal] DECIMAL(25, 2) NOT NULL DEFAULT 0,
        [FechaGeneracion] DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE NONCLUSTERED INDEX [IX_Salida_Concentracion_Vencimiento_Busqueda] 
    ON [dbo].[Salida_Concentracion_Vencimiento] ([IdEntidad], [Periodo]);
    
    PRINT 'Tabla [dbo].[Salida_Concentracion_Vencimiento] creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla [dbo].[Salida_Concentracion_Vencimiento] ya existe.';
END
GO

/****** 4. SP DE CÁLCULO REAL POR ACREEDOR Y VENCIMIENTO ******/
IF OBJECT_ID('dbo.FGA_Generar_Estructura_Fondeo', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Generar_Estructura_Fondeo];
GO

CREATE PROCEDURE [dbo].[FGA_Generar_Estructura_Fondeo]
    @IDENTIDAD NVARCHAR(5),
    @PERIODO DATE
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODO = DATEFROMPARTS(YEAR(@PERIODO), MONTH(@PERIODO), 1);

    -- Limpieza previa para reproceso seguro
    DELETE FROM [dbo].[Salida_Concentracion_Ahorrantes] 
    WHERE IdEntidad = @IDENTIDAD 
      AND (Periodo = @PERIODO OR (YEAR(Periodo) = YEAR(@PERIODO) AND MONTH(Periodo) = MONTH(@PERIODO)));

    DELETE FROM [dbo].[Salida_Concentracion_Vencimiento] 
    WHERE IdEntidad = @IDENTIDAD 
      AND (Periodo = @PERIODO OR (YEAR(Periodo) = YEAR(@PERIODO) AND MONTH(Periodo) = MONTH(@PERIODO)));

    -- Consolidación de registros de Pasivos 210 (tabla activa e histórica)
    CREATE TABLE #Pasivos210 (
        IdAcreedor NVARCHAR(35),
        CuentaContablePrincipal NVARCHAR(8),
        SaldoPrincipal DECIMAL(18, 2),
        SaldoProducto DECIMAL(18, 2),
        FechaVencimiento DATE
    );

    -- 1. Intentar desde tabla activa (Estado = 3 Aprobado)
    INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
    SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
    FROM [dbo].[XML_Pasivo_Cuenta_Contable_210] A WITH(NOLOCK)
    INNER JOIN [dbo].[XML_Encabezado] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
    WHERE B.IdEntidad_Id = @IDENTIDAD 
      AND (B.Periodo = @PERIODO OR (YEAR(B.Periodo) = YEAR(@PERIODO) AND MONTH(B.Periodo) = MONTH(@PERIODO)))
      AND B.IdEstado_Id = 3 
      AND B.IdArchivo_Id = '2702';

    -- Si no hay con Estado = 3, buscar cualquier estado no anulado (!= 12)
    IF NOT EXISTS (SELECT 1 FROM #Pasivos210)
    BEGIN
        INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
        SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
        FROM [dbo].[XML_Pasivo_Cuenta_Contable_210] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
        WHERE B.IdEntidad_Id = @IDENTIDAD 
          AND (B.Periodo = @PERIODO OR (YEAR(B.Periodo) = YEAR(@PERIODO) AND MONTH(B.Periodo) = MONTH(@PERIODO)))
          AND B.IdEstado_Id != 12 
          AND B.IdArchivo_Id = '2702';
    END

    -- 2. Si no hay datos en activa, buscar en histórico
    IF NOT EXISTS (SELECT 1 FROM #Pasivos210)
    BEGIN
        INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
        SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
        FROM [dbo].[XML_Pasivo_Cuenta_Contable_210_His] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
        WHERE B.IdEntidad_Id = @IDENTIDAD 
          AND (B.Periodo = @PERIODO OR (YEAR(B.Periodo) = YEAR(@PERIODO) AND MONTH(B.Periodo) = MONTH(@PERIODO)))
          AND B.IdEstado_Id = 3 
          AND B.IdArchivo_Id = '2702';
    END

    IF NOT EXISTS (SELECT 1 FROM #Pasivos210)
    BEGIN
        INSERT INTO #Pasivos210 (IdAcreedor, CuentaContablePrincipal, SaldoPrincipal, SaldoProducto, FechaVencimiento)
        SELECT A.IdAcreedor, A.CuentaContablePrincipal, A.SaldoPrincipal, A.SaldoProducto, A.FechaVencimiento
        FROM [dbo].[XML_Pasivo_Cuenta_Contable_210_His] A WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] B WITH(NOLOCK) ON B.Id = A.IdEncabezado_Id
        WHERE B.IdEntidad_Id = @IDENTIDAD 
          AND (B.Periodo = @PERIODO OR (YEAR(B.Periodo) = YEAR(@PERIODO) AND MONTH(B.Periodo) = MONTH(@PERIODO)))
          AND B.IdEstado_Id != 12 
          AND B.IdArchivo_Id = '2702';
    END

    -- 3. CÁLCULO DE MAYORES AHORRANTES (TOP 10 Y TOP 20 REALES)
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

    DECLARE @PorcTop10 DECIMAL(18, 6) = CASE WHEN @MontoTotalAhorrantes > 0 THEN (@MontoTop10 / @MontoTotalAhorrantes) * 100.0 ELSE 0 END;
    DECLARE @PorcTop20 DECIMAL(18, 6) = CASE WHEN @MontoTotalAhorrantes > 0 THEN (@MontoTop20 / @MontoTotalAhorrantes) * 100.0 ELSE 0 END;

    IF @MontoTotalAhorrantes > 0 OR @TotalAhorrantes > 0
    BEGIN
        INSERT INTO [dbo].[Salida_Concentracion_Ahorrantes]
            (IdEntidad, Periodo, MontoTop10, MontoTop20, MontoTotal, PorcTop10, PorcTop20, CantidadAhorrantes, FechaGeneracion)
        VALUES
            (@IDENTIDAD, @PERIODO, @MontoTop10, @MontoTop20, @MontoTotalAhorrantes, @PorcTop10, @PorcTop20, @TotalAhorrantes, GETDATE());
    END

    -- 4. CÁLCULO DE CONCENTRACIÓN DE SALDOS POR VENCIMIENTO (7 TRAMOS REALES)
    DECLARE @Tramo1 DECIMAL(25, 2) = 0;
    DECLARE @Tramo2 DECIMAL(25, 2) = 0;
    DECLARE @Tramo3 DECIMAL(25, 2) = 0;
    DECLARE @Tramo4 DECIMAL(25, 2) = 0;
    DECLARE @Tramo5 DECIMAL(25, 2) = 0;
    DECLARE @Tramo6 DECIMAL(25, 2) = 0;
    DECLARE @Tramo7 DECIMAL(25, 2) = 0;

    SELECT
        @Tramo1 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal LIKE '211%' OR FechaVencimiento IS NULL OR FechaVencimiento <= @PERIODO THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo2 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 1 AND 90 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo3 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 91 AND 180 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo4 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 181 AND 270 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo5 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 271 AND 360 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo6 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) BETWEEN 361 AND 1080 THEN SaldoPrincipal ELSE 0 END), 0),
        @Tramo7 = ISNULL(SUM(CASE WHEN CuentaContablePrincipal NOT LIKE '211%' AND FechaVencimiento > @PERIODO AND DATEDIFF(DAY, @PERIODO, FechaVencimiento) > 1080 THEN SaldoPrincipal ELSE 0 END), 0)
    FROM #Pasivos210;

    DECLARE @TotalVenc DECIMAL(25, 2) = @Tramo1 + @Tramo2 + @Tramo3 + @Tramo4 + @Tramo5 + @Tramo6 + @Tramo7;

    IF @TotalVenc > 0
    BEGIN
        INSERT INTO [dbo].[Salida_Concentracion_Vencimiento]
            (IdEntidad, Periodo, Tramo1_Vista, Tramo2_1a90Dias, Tramo3_91a180Dias, Tramo4_181a270Dias, Tramo5_271a360Dias, Tramo6_1a3Anios, Tramo7_Mas3Anios, TotalPrincipal, FechaGeneracion)
        VALUES
            (@IDENTIDAD, @PERIODO, @Tramo1, @Tramo2, @Tramo3, @Tramo4, @Tramo5, @Tramo6, @Tramo7, @TotalVenc, GETDATE());
    END

    -- 5. OBTENER ASOCIADOS ACTIVOS DE DATOS ADICIONALES (CUENTA 20026)
    DECLARE @CantidadAsociados INT = 0;
    SELECT TOP 1 @CantidadAsociados = ISNULL(MontoDatoAdicional, 0)
    FROM (
        SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
        FROM [dbo].[XML_Contable_DatosAdicionales] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD 
          AND (E.Periodo = @PERIODO OR (YEAR(E.Periodo) = YEAR(@PERIODO) AND MONTH(E.Periodo) = MONTH(@PERIODO)))
          AND E.IdEstado_Id != 12 
          AND D.CuentaCatalogo = 20026
        UNION ALL
        SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
        FROM [dbo].[XML_Contable_DatosAdicionales_His] D WITH(NOLOCK)
        INNER JOIN [dbo].[XML_Encabezado_His] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
        WHERE E.IdEntidad_Id = @IDENTIDAD 
          AND (E.Periodo = @PERIODO OR (YEAR(E.Periodo) = YEAR(@PERIODO) AND MONTH(E.Periodo) = MONTH(@PERIODO)))
          AND E.IdEstado_Id != 12 
          AND D.CuentaCatalogo = 20026
    ) X
    ORDER BY CASE WHEN X.IdEstado_Id = 3 THEN 0 ELSE 1 END, X.EncId DESC;

    DROP TABLE #Pasivos210;
    DROP TABLE #SaldosPorAcreedor;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Generar_Estructura_Fondeo] creado correctamente.';
GO

/****** 5. SP CONSULTA CONCENTRACIÓN DE AHORRANTES ******/
IF OBJECT_ID('dbo.FGA_Consultar_Concentracion_Ahorrantes', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Concentracion_Ahorrantes];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Concentracion_Ahorrantes]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOI DATE = NULL,
    @PERIODOF DATE = NULL,
    @PERIODOINICIAL DATE = NULL,
    @PERIODOFINAL DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODOI = DATEFROMPARTS(YEAR(ISNULL(@PERIODOI, @PERIODOINICIAL)), MONTH(ISNULL(@PERIODOI, @PERIODOINICIAL)), 1);
    SET @PERIODOF = DATEFROMPARTS(YEAR(ISNULL(@PERIODOF, @PERIODOFINAL)), MONTH(ISNULL(@PERIODOF, @PERIODOFINAL)), 1);

    -- Si no existe cálculo para algún período, generarlo dinámicamente
    DECLARE @PeriodoCursor DATE = @PERIODOI;
    WHILE @PeriodoCursor <= @PERIODOF
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK) 
                       WHERE IdEntidad = @IDENTIDAD AND Periodo = @PeriodoCursor)
        BEGIN
            EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PeriodoCursor;
        END
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    SELECT 
        Periodo,
        MontoTop10,
        MontoTop20,
        MontoTotal,
        PorcTop10,
        PorcTop20,
        CantidadAhorrantes
    FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD
      AND Periodo BETWEEN @PERIODOI AND @PERIODOF
    ORDER BY Periodo ASC;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Concentracion_Ahorrantes] creado correctamente.';
GO

/****** 6. SP CONSULTA CONCENTRACIÓN POR VENCIMIENTO ******/
IF OBJECT_ID('dbo.FGA_Consultar_Concentracion_Vencimiento', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Concentracion_Vencimiento];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Concentracion_Vencimiento]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOI DATE = NULL,
    @PERIODOF DATE = NULL,
    @PERIODOINICIAL DATE = NULL,
    @PERIODOFINAL DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODOI = DATEFROMPARTS(YEAR(ISNULL(@PERIODOI, @PERIODOINICIAL)), MONTH(ISNULL(@PERIODOI, @PERIODOINICIAL)), 1);
    SET @PERIODOF = DATEFROMPARTS(YEAR(ISNULL(@PERIODOF, @PERIODOFINAL)), MONTH(ISNULL(@PERIODOF, @PERIODOFINAL)), 1);

    DECLARE @PeriodoCursor DATE = @PERIODOI;
    WHILE @PeriodoCursor <= @PERIODOF
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK) 
                       WHERE IdEntidad = @IDENTIDAD AND Periodo = @PeriodoCursor)
        BEGIN
            EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PeriodoCursor;
        END
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    SELECT 
        Periodo,
        Tramo1_Vista AS ALaVista,
        Tramo2_1a90Dias AS De1A90Dias,
        Tramo3_91a180Dias AS De91A180Dias,
        Tramo4_181a270Dias AS De181A270Dias,
        Tramo5_271a360Dias AS De271A360Dias,
        Tramo6_1a3Anios AS De1A3Anos,
        Tramo7_Mas3Anios AS De3AnosEnAdelante,
        TotalPrincipal AS Total,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo1_Vista / TotalPrincipal) * 100.0 ELSE 0 END AS PorcALaVista,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo2_1a90Dias / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe1A90Dias,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo3_91a180Dias / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe91A180Dias,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo4_181a270Dias / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe181A270Dias,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo5_271a360Dias / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe271A360Dias,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo6_1a3Anios / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe1A3Anos,
        CASE WHEN TotalPrincipal > 0 THEN (Tramo7_Mas3Anios / TotalPrincipal) * 100.0 ELSE 0 END AS PorcDe3AnosEnAdelante
    FROM [dbo].[Salida_Concentracion_Vencimiento] WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD
      AND Periodo BETWEEN @PERIODOI AND @PERIODOF
    ORDER BY Periodo ASC;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Concentracion_Vencimiento] creado correctamente.';
GO

/****** 7. SP CONSULTA CANTIDAD DE ASOCIADOS Y AHORRANTES ******/
IF OBJECT_ID('dbo.FGA_Consultar_Cantidad_Asociados_Ahorrantes', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes];
GO

CREATE PROCEDURE [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes]
    @IDENTIDAD NVARCHAR(5),
    @PERIODOI DATE = NULL,
    @PERIODOF DATE = NULL,
    @PERIODOINICIAL DATE = NULL,
    @PERIODOFINAL DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @PERIODOI = DATEFROMPARTS(YEAR(ISNULL(@PERIODOI, @PERIODOINICIAL)), MONTH(ISNULL(@PERIODOI, @PERIODOINICIAL)), 1);
    SET @PERIODOF = DATEFROMPARTS(YEAR(ISNULL(@PERIODOF, @PERIODOFINAL)), MONTH(ISNULL(@PERIODOF, @PERIODOFINAL)), 1);

    DECLARE @PeriodoCursor DATE = @PERIODOI;
    WHILE @PeriodoCursor <= @PERIODOF
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Salida_Concentracion_Ahorrantes] WITH(NOLOCK) 
                       WHERE IdEntidad = @IDENTIDAD AND Periodo = @PeriodoCursor)
        BEGIN
            EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @IDENTIDAD, @PeriodoCursor;
        END
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    CREATE TABLE #Resultado (
        Periodo DATE,
        AsociadosActivos INT DEFAULT 0,
        AsociadosInactivos INT DEFAULT 0,
        CantidadAhorrantes INT DEFAULT 0
    );

    SET @PeriodoCursor = @PERIODOI;
    WHILE @PeriodoCursor <= @PERIODOF
    BEGIN
        INSERT INTO #Resultado (Periodo) VALUES (@PeriodoCursor);
        SET @PeriodoCursor = DATEADD(MONTH, 1, @PeriodoCursor);
    END

    -- Asociados activos (cuenta 20026)
    UPDATE R
    SET R.AsociadosActivos = ISNULL(A.MontoDatoAdicional, 0)
    FROM #Resultado R
    OUTER APPLY (
        SELECT TOP 1 MontoDatoAdicional 
        FROM (
            SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
            FROM [dbo].[XML_Contable_DatosAdicionales] D WITH(NOLOCK)
            INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
            WHERE E.IdEntidad_Id = @IDENTIDAD 
              AND (E.Periodo = R.Periodo OR (YEAR(E.Periodo) = YEAR(R.Periodo) AND MONTH(E.Periodo) = MONTH(R.Periodo)))
              AND E.IdEstado_Id != 12 
              AND D.CuentaCatalogo = 20026
            UNION ALL
            SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
            FROM [dbo].[XML_Contable_DatosAdicionales_His] D WITH(NOLOCK)
            INNER JOIN [dbo].[XML_Encabezado_His] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
            WHERE E.IdEntidad_Id = @IDENTIDAD 
              AND (E.Periodo = R.Periodo OR (YEAR(E.Periodo) = YEAR(R.Periodo) AND MONTH(E.Periodo) = MONTH(R.Periodo)))
              AND E.IdEstado_Id != 12 
              AND D.CuentaCatalogo = 20026
        ) X
        ORDER BY CASE WHEN X.IdEstado_Id = 3 THEN 0 ELSE 1 END, X.EncId DESC
    ) A;

    -- Asociados inactivos (cuenta 20024)
    UPDATE R
    SET R.AsociadosInactivos = ISNULL(A.MontoDatoAdicional, 0)
    FROM #Resultado R
    OUTER APPLY (
        SELECT TOP 1 MontoDatoAdicional 
        FROM (
            SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
            FROM [dbo].[XML_Contable_DatosAdicionales] D WITH(NOLOCK)
            INNER JOIN [dbo].[XML_Encabezado] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
            WHERE E.IdEntidad_Id = @IDENTIDAD 
              AND (E.Periodo = R.Periodo OR (YEAR(E.Periodo) = YEAR(R.Periodo) AND MONTH(E.Periodo) = MONTH(R.Periodo)))
              AND E.IdEstado_Id != 12 
              AND D.CuentaCatalogo = 20024
            UNION ALL
            SELECT D.MontoDatoAdicional, E.IdEstado_Id, E.Id AS EncId
            FROM [dbo].[XML_Contable_DatosAdicionales_His] D WITH(NOLOCK)
            INNER JOIN [dbo].[XML_Encabezado_His] E WITH(NOLOCK) ON E.Id = D.IdEncabezado_Id
            WHERE E.IdEntidad_Id = @IDENTIDAD 
              AND (E.Periodo = R.Periodo OR (YEAR(E.Periodo) = YEAR(R.Periodo) AND MONTH(E.Periodo) = MONTH(R.Periodo)))
              AND E.IdEstado_Id != 12 
              AND D.CuentaCatalogo = 20024
        ) X
        ORDER BY CASE WHEN X.IdEstado_Id = 3 THEN 0 ELSE 1 END, X.EncId DESC
    ) A;

    -- Cantidad de ahorrantes desde Salida_Concentracion_Ahorrantes
    UPDATE R
    SET R.CantidadAhorrantes = ISNULL(S.CantidadAhorrantes, 0)
    FROM #Resultado R
    LEFT JOIN [dbo].[Salida_Concentracion_Ahorrantes] S WITH(NOLOCK) 
        ON S.IdEntidad = @IDENTIDAD 
       AND (S.Periodo = R.Periodo OR (YEAR(S.Periodo) = YEAR(R.Periodo) AND MONTH(S.Periodo) = MONTH(R.Periodo)));

    SELECT Periodo, AsociadosActivos, AsociadosInactivos, CantidadAhorrantes 
    FROM #Resultado 
    ORDER BY Periodo ASC;

    DROP TABLE #Resultado;
END
GO
PRINT 'Procedimiento [dbo].[FGA_Consultar_Cantidad_Asociados_Ahorrantes] creado correctamente.';
GO

/****** 8. BUCLE DE REGENERACIÓN MASIVA PARA TODAS LAS ENTIDADES Y PERIODOS ******/
PRINT 'Iniciando procesamiento de todos los archivos 2702 existentes en base de datos...';

DECLARE @EntReg INT;
DECLARE @PerReg DATE;
DECLARE @TotalProcesados INT = 0;

DECLARE curCortes CURSOR LOCAL FAST_FORWARD FOR
SELECT DISTINCT 
    IdEntidad_Id, 
    DATEFROMPARTS(YEAR(Periodo), MONTH(Periodo), 1) AS PeriodoNorm
FROM (
    SELECT IdEntidad_Id, Periodo FROM [dbo].[XML_Encabezado] WITH(NOLOCK) WHERE IdArchivo_Id = '2702' AND IdEstado_Id != 12
    UNION
    SELECT IdEntidad_Id, Periodo FROM [dbo].[XML_Encabezado_His] WITH(NOLOCK) WHERE IdArchivo_Id = '2702' AND IdEstado_Id != 12
) X
ORDER BY PeriodoNorm DESC, IdEntidad_Id ASC;

OPEN curCortes;
FETCH NEXT FROM curCortes INTO @EntReg, @PerReg;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC [dbo].[FGA_Generar_Estructura_Fondeo] @EntReg, @PerReg;
    SET @TotalProcesados = @TotalProcesados + 1;
    FETCH NEXT FROM curCortes INTO @EntReg, @PerReg;
END

CLOSE curCortes;
DEALLOCATE curCortes;

PRINT 'Se procesaron exitosamente ' + CAST(@TotalProcesados AS VARCHAR(10)) + ' cortes de pasivos 2702.';

PRINT 'Cálculo de estructura de fondeo 100% real finalizado exitosamente.';
GO

/****** 9. CONSULTA DE VALIDACIÓN FINAL ******/
PRINT 'Resumen de datos reales generados:';
SELECT 
    A.IdEntidad,
    E.Nombre AS NombreEntidad,
    A.Periodo,
    A.CantidadAhorrantes AS TotalAhorrantes_Real,
    A.MontoTotal AS SaldoTotal_Real,
    A.PorcTop10 AS PorcTop10_Real,
    A.PorcTop20 AS PorcTop20_Real,
    A.FechaGeneracion
FROM [dbo].[Salida_Concentracion_Ahorrantes] A WITH(NOLOCK)
LEFT JOIN [dbo].[Entidad] E WITH(NOLOCK) ON E.Id = A.IdEntidad
ORDER BY A.Periodo DESC, A.PorcTop10 DESC;
GO
