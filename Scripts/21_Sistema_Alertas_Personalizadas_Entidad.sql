-- =========================================================================================
-- SCRIPT: 21_Sistema_Alertas_Personalizadas_Entidad.sql
-- Base de Datos: FFC / FGA
-- Objetivo:
--   1. Crear tabla dbo.AlertaParametrosEntidad (umbrales y modo de monitoreo por cooperativa).
--   2. Crear tabla dbo.AlertaCuentaMonitoreada (Watchlist / Blacklist de cuentas contables).
--   3. Procedimientos almacenados para gestión de configuración y cuentas por entidad.
--   4. Actualizar dbo.FGA_Generar_Alertas_Variacion_Financiera para aplicar herencia multi-tenant
--      y filtrado de cuentas monitoreadas o excluidas.
-- =========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/5] Creando / Verificando Tabla dbo.AlertaParametrosEntidad...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertaParametrosEntidad' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.AlertaParametrosEntidad (
            Id INT IDENTITY(1,1) NOT NULL,
            IdEntidad NVARCHAR(5) NOT NULL,
            UmbralVariacionPorc DECIMAL(18,2) NULL,
            UmbralVariacionMonto DECIMAL(18,2) NULL,
            ModoMonitoreo NVARCHAR(20) NOT NULL CONSTRAINT DF_AlertaParametrosEntidad_Modo DEFAULT 'TODAS', -- 'TODAS', 'SOLO_WATCHLIST', 'EXCLUIR_BLACKLIST'
            Activo BIT NOT NULL CONSTRAINT DF_AlertaParametrosEntidad_Activo DEFAULT 1,
            FechaModificacion DATETIME NOT NULL CONSTRAINT DF_AlertaParametrosEntidad_Fecha DEFAULT GETDATE(),
            UsuarioModificacion NVARCHAR(100) NULL,
            CONSTRAINT PK_AlertaParametrosEntidad PRIMARY KEY CLUSTERED (Id ASC),
            CONSTRAINT UQ_AlertaParametrosEntidad_Entidad UNIQUE (IdEntidad)
        );

        PRINT '   [OK] Tabla dbo.AlertaParametrosEntidad creada exitosamente.';
    END
    ELSE
    BEGIN
        PRINT '   [INFO] Tabla dbo.AlertaParametrosEntidad ya existe.';
    END

    PRINT '================================================================================';
    PRINT '>> [2/5] Creando / Verificando Tabla dbo.AlertaCuentaMonitoreada...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertaCuentaMonitoreada' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.AlertaCuentaMonitoreada (
            Id INT IDENTITY(1,1) NOT NULL,
            IdEntidad NVARCHAR(5) NOT NULL,
            Cuenta NVARCHAR(50) NOT NULL,
            NombreCuenta NVARCHAR(250) NULL,
            TipoRegla NVARCHAR(20) NOT NULL CONSTRAINT DF_AlertaCuenta_Tipo DEFAULT 'MONITOREAR', -- 'MONITOREAR' (Watchlist) o 'IGNORAR' (Blacklist)
            UmbralPorcPersonalizado DECIMAL(18,2) NULL,
            UmbralMontoPersonalizado DECIMAL(18,2) NULL,
            Activo BIT NOT NULL CONSTRAINT DF_AlertaCuenta_Activo DEFAULT 1,
            FechaRegistro DATETIME NOT NULL CONSTRAINT DF_AlertaCuenta_Fecha DEFAULT GETDATE(),
            UsuarioRegistro NVARCHAR(100) NULL,
            CONSTRAINT PK_AlertaCuentaMonitoreada PRIMARY KEY CLUSTERED (Id ASC),
            CONSTRAINT UQ_AlertaCuenta_Entidad_Cuenta UNIQUE (IdEntidad, Cuenta)
        );

        CREATE NONCLUSTERED INDEX IX_AlertaCuentaMonitoreada_Entidad_Activo 
            ON dbo.AlertaCuentaMonitoreada (IdEntidad, Activo, TipoRegla)
            INCLUDE (Cuenta, NombreCuenta, UmbralPorcPersonalizado, UmbralMontoPersonalizado);

        PRINT '   [OK] Tabla dbo.AlertaCuentaMonitoreada e índices creados exitosamente.';
    END
    ELSE
    BEGIN
        PRINT '   [INFO] Tabla dbo.AlertaCuentaMonitoreada ya existe.';
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrMsg, 16, 1);
    RETURN;
END CATCH;
GO

PRINT '================================================================================';
PRINT '>> [3/5] Creando Procedimientos de Parámetros y Cuentas por Entidad...';
PRINT '================================================================================';
GO

-- =========================================================================================
-- SP 1: dbo.FGA_Obtener_Configuracion_Alerta_Entidad
-- Retorna los parámetros activos de una entidad, con fallback a los globales de dbo.Parametros
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Obtener_Configuracion_Alerta_Entidad]
    @IDENTIDAD NVARCHAR(5) = '-1'
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';

    -- 1. Leer parámetros globales con normalización flexible
    DECLARE @GlobalPorc DECIMAL(18,2) = 15.00;
    DECLARE @GlobalMonto DECIMAL(18,2) = 10000000.00;

    DECLARE @PorcStr NVARCHAR(100), @MontoStr NVARCHAR(100);
    SELECT TOP 1 @PorcStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_PORC', 'ALERTA_PORCENTAJE', 'ALERTA_PORC', 'ALERTA_VARIACION_PORCENTAJE')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%PORC%');

    SELECT TOP 1 @MontoStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_MONTO', 'ALERTA_MONTO', 'MONTO_ALERTA')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%MONTO%');

    IF @PorcStr IS NOT NULL
    BEGIN
        SET @PorcStr = REPLACE(REPLACE(REPLACE(REPLACE(@PorcStr, '%', ''), ' ', ''), '$', ''), '₡', '');
        IF CHARINDEX(',', @PorcStr) > 0 AND CHARINDEX('.', @PorcStr) = 0
            SET @PorcStr = REPLACE(@PorcStr, ',', '.');
        SET @GlobalPorc = TRY_CAST(@PorcStr AS DECIMAL(18,2));
    END
    IF @GlobalPorc IS NULL OR @GlobalPorc <= 0 SET @GlobalPorc = 15.00;

    IF @MontoStr IS NOT NULL
    BEGIN
        SET @MontoStr = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@MontoStr, '₡', ''), '$', ''), '%', ''), ' ', ''), 'M', '000000');
        IF CHARINDEX(',', @MontoStr) > 0 AND CHARINDEX('.', @MontoStr) > 0
        BEGIN
            IF CHARINDEX('.', @MontoStr) > CHARINDEX(',', @MontoStr)
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE
                SET @MontoStr = REPLACE(REPLACE(@MontoStr, '.', ''), ',', '.');
        END
        ELSE IF CHARINDEX(',', @MontoStr) > 0
        BEGIN
            IF LEN(@MontoStr) - LEN(REPLACE(@MontoStr, ',', '')) > 1
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE IF LEN(@MontoStr) - CHARINDEX(',', @MontoStr) = 3 AND LEN(@MontoStr) >= 5
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE
                SET @MontoStr = REPLACE(@MontoStr, ',', '.');
        END
        ELSE IF CHARINDEX('.', @MontoStr) > 0
        BEGIN
            IF LEN(@MontoStr) - LEN(REPLACE(@MontoStr, '.', '')) > 1
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
            ELSE IF LEN(@MontoStr) - CHARINDEX('.', @MontoStr) = 3 AND LEN(@MontoStr) >= 5
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
        END
        SET @GlobalMonto = TRY_CAST(@MontoStr AS DECIMAL(18,2));
    END
    IF @GlobalMonto IS NULL OR @GlobalMonto <= 0 SET @GlobalMonto = 10000000.00;

    -- 2. Consultar parámetros de la entidad
    SELECT 
        @IDENTIDAD AS IdEntidad,
        ISNULL(ape.UmbralVariacionPorc, @GlobalPorc) AS UmbralVariacionPorc,
        ISNULL(ape.UmbralVariacionMonto, @GlobalMonto) AS UmbralVariacionMonto,
        ISNULL(ape.ModoMonitoreo, 'TODAS') AS ModoMonitoreo,
        CASE 
            WHEN ape.Id IS NOT NULL AND ape.Activo = 1 AND (ape.UmbralVariacionPorc IS NOT NULL OR ape.UmbralVariacionMonto IS NOT NULL OR ape.ModoMonitoreo != 'TODAS') 
            THEN CAST(1 AS BIT) 
            ELSE CAST(0 AS BIT) 
        END AS EsPersonalizado,
        @GlobalPorc AS GlobalPorc,
        @GlobalMonto AS GlobalMonto,
        ISNULL(ape.Activo, CAST(1 AS BIT)) AS Activo,
        ape.FechaModificacion,
        ape.UsuarioModificacion
    FROM (SELECT 1 AS dummy) d
    LEFT JOIN dbo.AlertaParametrosEntidad ape ON ape.IdEntidad = @IDENTIDAD AND ape.Activo = 1;
END;
GO

-- =========================================================================================
-- SP 2: dbo.FGA_Guardar_Configuracion_Alerta_Entidad
-- Guarda o actualiza los umbrales y modo de monitoreo de una entidad
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Guardar_Configuracion_Alerta_Entidad]
    @IDENTIDAD NVARCHAR(5),
    @UMBRAL_PORC DECIMAL(18,2) = NULL,
    @UMBRAL_MONTO DECIMAL(18,2) = NULL,
    @MODO_MONITOREO NVARCHAR(20) = 'TODAS',
    @ACTIVO BIT = 1,
    @USUARIO NVARCHAR(100) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';
    IF @IDENTIDAD IS NULL OR @IDENTIDAD = '-1' OR @IDENTIDAD = ''
    BEGIN
        RAISERROR('Debe especificar una entidad válida para guardar sus parámetros de alerta.', 16, 1);
        RETURN;
    END

    IF @MODO_MONITOREO NOT IN ('TODAS', 'SOLO_WATCHLIST', 'EXCLUIR_BLACKLIST')
        SET @MODO_MONITOREO = 'TODAS';

    IF EXISTS (SELECT 1 FROM dbo.AlertaParametrosEntidad WHERE IdEntidad = @IDENTIDAD)
    BEGIN
        UPDATE dbo.AlertaParametrosEntidad
        SET UmbralVariacionPorc = @UMBRAL_PORC,
            UmbralVariacionMonto = @UMBRAL_MONTO,
            ModoMonitoreo = @MODO_MONITOREO,
            Activo = @ACTIVO,
            FechaModificacion = GETDATE(),
            UsuarioModificacion = @USUARIO
        WHERE IdEntidad = @IDENTIDAD;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AlertaParametrosEntidad (
            IdEntidad, UmbralVariacionPorc, UmbralVariacionMonto, ModoMonitoreo, Activo, FechaModificacion, UsuarioModificacion
        )
        VALUES (
            @IDENTIDAD, @UMBRAL_PORC, @UMBRAL_MONTO, @MODO_MONITOREO, @ACTIVO, GETDATE(), @USUARIO
        );
    END

    -- Retornar la configuración resultante
    EXEC dbo.FGA_Obtener_Configuracion_Alerta_Entidad @IDENTIDAD = @IDENTIDAD;
END;
GO

-- =========================================================================================
-- SP 3: dbo.FGA_Obtener_Cuentas_Monitoreadas_Entidad
-- Retorna las cuentas activas registradas en la Watchlist o Blacklist de una entidad
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad]
    @IDENTIDAD NVARCHAR(5) = '-1',
    @SOLO_ACTIVAS BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';

    SELECT 
        acm.Id,
        acm.IdEntidad,
        ISNULL(e.Nombre, 'Entidad ' + acm.IdEntidad) AS NombreEntidad,
        acm.Cuenta,
        ISNULL(acm.NombreCuenta, ISNULL(c.NOMBRE, 'Cuenta ' + acm.Cuenta)) AS NombreCuenta,
        ISNULL(c.NIVEL, 3) AS Nivel,
        acm.TipoRegla,
        acm.UmbralPorcPersonalizado,
        acm.UmbralMontoPersonalizado,
        acm.Activo,
        acm.FechaRegistro,
        acm.UsuarioRegistro
    FROM dbo.AlertaCuentaMonitoreada acm WITH(NOLOCK)
    LEFT JOIN dbo.CATALOGOCUENTA c WITH(NOLOCK) ON acm.Cuenta = c.CUENTA
    LEFT JOIN dbo.Entidad e WITH(NOLOCK) ON acm.IdEntidad = e.Id
    WHERE (@IDENTIDAD = '-1' OR acm.IdEntidad = @IDENTIDAD)
      AND (@SOLO_ACTIVAS = 0 OR acm.Activo = 1)
    ORDER BY acm.TipoRegla ASC, acm.Cuenta ASC;
END;
GO

-- =========================================================================================
-- SP 4: dbo.FGA_Guardar_Cuenta_Monitoreada_Entidad
-- Agrega o actualiza una cuenta contable en la lista de seguimiento / exclusión de una entidad
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad]
    @IDENTIDAD NVARCHAR(5),
    @CUENTA NVARCHAR(50),
    @NOMBRE_CUENTA NVARCHAR(250) = NULL,
    @TIPO_REGLA NVARCHAR(20) = 'MONITOREAR', -- 'MONITOREAR' o 'IGNORAR'
    @UMBRAL_PORC DECIMAL(18,2) = NULL,
    @UMBRAL_MONTO DECIMAL(18,2) = NULL,
    @USUARIO NVARCHAR(100) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';
    IF @IDENTIDAD IS NULL OR @IDENTIDAD = '-1' OR @IDENTIDAD = ''
    BEGIN
        RAISERROR('Debe especificar una entidad válida.', 16, 1);
        RETURN;
    END

    SET @CUENTA = LTRIM(RTRIM(@CUENTA));
    IF @CUENTA IS NULL OR @CUENTA = ''
    BEGIN
        RAISERROR('Debe especificar un código de cuenta contable.', 16, 1);
        RETURN;
    END

    -- Autocompletar nombre si no se suministró
    IF @NOMBRE_CUENTA IS NULL OR @NOMBRE_CUENTA = ''
    BEGIN
        SELECT TOP 1 @NOMBRE_CUENTA = NOMBRE 
        FROM dbo.CATALOGOCUENTA WITH(NOLOCK) 
        WHERE CUENTA = @CUENTA;

        IF @NOMBRE_CUENTA IS NULL SET @NOMBRE_CUENTA = 'Cuenta ' + @CUENTA;
    END

    IF @TIPO_REGLA NOT IN ('MONITOREAR', 'IGNORAR')
        SET @TIPO_REGLA = 'MONITOREAR';

    IF EXISTS (SELECT 1 FROM dbo.AlertaCuentaMonitoreada WHERE IdEntidad = @IDENTIDAD AND Cuenta = @CUENTA)
    BEGIN
        UPDATE dbo.AlertaCuentaMonitoreada
        SET NombreCuenta = @NOMBRE_CUENTA,
            TipoRegla = @TIPO_REGLA,
            UmbralPorcPersonalizado = @UMBRAL_PORC,
            UmbralMontoPersonalizado = @UMBRAL_MONTO,
            Activo = 1,
            FechaRegistro = GETDATE(),
            UsuarioRegistro = @USUARIO
        WHERE IdEntidad = @IDENTIDAD AND Cuenta = @CUENTA;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AlertaCuentaMonitoreada (
            IdEntidad, Cuenta, NombreCuenta, TipoRegla, UmbralPorcPersonalizado, UmbralMontoPersonalizado, Activo, FechaRegistro, UsuarioRegistro
        )
        VALUES (
            @IDENTIDAD, @CUENTA, @NOMBRE_CUENTA, @TIPO_REGLA, @UMBRAL_PORC, @UMBRAL_MONTO, 1, GETDATE(), @USUARIO
        );
    END

    -- Retornar la lista de cuentas actualizada
    EXEC dbo.FGA_Obtener_Cuentas_Monitoreadas_Entidad @IDENTIDAD = @IDENTIDAD, @SOLO_ACTIVAS = 1;
END;
GO

-- =========================================================================================
-- SP 5: dbo.FGA_Eliminar_Cuenta_Monitoreada_Entidad
-- Desactiva o remueve una cuenta contable de la lista de una entidad
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad]
    @ID INT,
    @IDENTIDAD NVARCHAR(5) = NULL,
    @USUARIO NVARCHAR(100) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';

    DELETE FROM dbo.AlertaCuentaMonitoreada
    WHERE Id = @ID
      AND (@IDENTIDAD IS NULL OR @IDENTIDAD = '-1' OR IdEntidad = @IDENTIDAD);

    SELECT @@ROWCOUNT AS FilasEliminadas;
END;
GO

PRINT '================================================================================';
PRINT '>> [4/5] Actualizando Procedimiento dbo.FGA_Generar_Alertas_Variacion_Financiera...';
PRINT '================================================================================';
GO

-- =========================================================================================
-- SP 6: dbo.FGA_Generar_Alertas_Variacion_Financiera (ACTUALIZADO)
-- Evalúa variaciones respetando los umbrales y la Watchlist / Blacklist de cada entidad
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Generar_Alertas_Variacion_Financiera]
    @IDENTIDAD NVARCHAR(5) = '-1',
    @PERIODO DATETIME = NULL,
    @MODALIDAD NVARCHAR(20) = 'Acumulado',        -- 'Acumulado' o 'Mensual'
    @TIPO_COMPARACION NVARCHAR(20) = 'Interanual', -- 'Interanual' o 'Mensual'
    @FECHA DATETIME = NULL                        -- Alias retrocompatible
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @PERIODO IS NULL AND @FECHA IS NOT NULL
        SET @PERIODO = @FECHA;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';

    -- 1. Leer Parámetros Globales de Configuración (Fallback Seguro)
    DECLARE @GLOBAL_PORC DECIMAL(18,2) = 15.00;
    DECLARE @GLOBAL_MONTO DECIMAL(18,2) = 10000000.00;

    DECLARE @PorcStr NVARCHAR(100), @MontoStr NVARCHAR(100);
    SELECT TOP 1 @PorcStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_PORC', 'ALERTA_PORCENTAJE', 'ALERTA_PORC', 'ALERTA_VARIACION_PORCENTAJE')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%PORC%');

    SELECT TOP 1 @MontoStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_MONTO', 'ALERTA_MONTO', 'MONTO_ALERTA')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%MONTO%');

    IF @PorcStr IS NOT NULL
    BEGIN
        SET @PorcStr = REPLACE(REPLACE(REPLACE(REPLACE(@PorcStr, '%', ''), ' ', ''), '$', ''), '₡', '');
        IF CHARINDEX(',', @PorcStr) > 0 AND CHARINDEX('.', @PorcStr) = 0
            SET @PorcStr = REPLACE(@PorcStr, ',', '.');
        SET @GLOBAL_PORC = TRY_CAST(@PorcStr AS DECIMAL(18,2));
    END
    IF @GLOBAL_PORC IS NULL OR @GLOBAL_PORC <= 0 SET @GLOBAL_PORC = 15.00;

    IF @MontoStr IS NOT NULL
    BEGIN
        SET @MontoStr = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@MontoStr, '₡', ''), '$', ''), '%', ''), ' ', ''), 'M', '000000');
        IF CHARINDEX(',', @MontoStr) > 0 AND CHARINDEX('.', @MontoStr) > 0
        BEGIN
            IF CHARINDEX('.', @MontoStr) > CHARINDEX(',', @MontoStr)
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE
                SET @MontoStr = REPLACE(REPLACE(@MontoStr, '.', ''), ',', '.');
        END
        ELSE IF CHARINDEX(',', @MontoStr) > 0
        BEGIN
            IF LEN(@MontoStr) - LEN(REPLACE(@MontoStr, ',', '')) > 1
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE IF LEN(@MontoStr) - CHARINDEX(',', @MontoStr) = 3 AND LEN(@MontoStr) >= 5
                SET @MontoStr = REPLACE(@MontoStr, ',', '');
            ELSE
                SET @MontoStr = REPLACE(@MontoStr, ',', '.');
        END
        ELSE IF CHARINDEX('.', @MontoStr) > 0
        BEGIN
            IF LEN(@MontoStr) - LEN(REPLACE(@MontoStr, '.', '')) > 1
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
            ELSE IF LEN(@MontoStr) - CHARINDEX('.', @MontoStr) = 3 AND LEN(@MontoStr) >= 5
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
        END
        SET @GLOBAL_MONTO = TRY_CAST(@MontoStr AS DECIMAL(18,2));
    END
    IF @GLOBAL_MONTO IS NULL OR @GLOBAL_MONTO <= 0 SET @GLOBAL_MONTO = 10000000.00;

    -- 2. Determinar Período Actual
    DECLARE @PERIODO_ACTUAL DATE;
    IF @PERIODO IS NOT NULL
    BEGIN
        SET @PERIODO_ACTUAL = DATEFROMPARTS(YEAR(@PERIODO), MONTH(@PERIODO), 1);
    END
    ELSE
    BEGIN
        SELECT @PERIODO_ACTUAL = DATEFROMPARTS(YEAR(MAX(PERIODO)), MONTH(MAX(PERIODO)), 1)
        FROM dbo.SALIDA_BALANCE_COMPROBACION WITH(NOLOCK)
        WHERE (@IDENTIDAD = '-1' OR IDENTIDAD = @IDENTIDAD);

        IF @PERIODO_ACTUAL IS NULL SET @PERIODO_ACTUAL = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    END

    -- 3. Determinar Período Anterior
    DECLARE @PERIODO_ANTERIOR DATE;
    IF UPPER(@TIPO_COMPARACION) = 'MENSUAL'
    BEGIN
        SET @PERIODO_ANTERIOR = DATEADD(MONTH, -1, @PERIODO_ACTUAL);
    END
    ELSE -- Interanual (Default)
    BEGIN
        SET @PERIODO_ANTERIOR = DATEADD(YEAR, -1, @PERIODO_ACTUAL);
    END

    DECLARE @IniAct DATE = @PERIODO_ACTUAL;
    DECLARE @FinAct DATE = EOMONTH(@PERIODO_ACTUAL);
    DECLARE @IniAnt DATE = @PERIODO_ANTERIOR;
    DECLARE @FinAnt DATE = EOMONTH(@PERIODO_ANTERIOR);

    -- 4. Extraer saldos deduplicados para ambos períodos
    ;WITH SaldosBase AS (
        SELECT 
            s.IDENTIDAD,
            s.CUENTA,
            CASE 
                WHEN UPPER(@MODALIDAD) = 'MENSUAL' THEN ISNULL(s.SALDOFINAL, 0)
                ELSE ISNULL(s.SALDOACUMULADO, s.SALDOFINAL)
            END AS Saldo,
            CASE 
                WHEN s.PERIODO >= @IniAct AND s.PERIODO <= @FinAct THEN 'ACT'
                WHEN s.PERIODO >= @IniAnt AND s.PERIODO <= @FinAnt THEN 'ANT'
            END AS PeriodoTipo,
            ROW_NUMBER() OVER (
                PARTITION BY s.IDENTIDAD, s.CUENTA, 
                             CASE WHEN s.PERIODO >= @IniAct AND s.PERIODO <= @FinAct THEN 'ACT' ELSE 'ANT' END
                ORDER BY s.PERIODO DESC
            ) AS rn
        FROM dbo.SALIDA_BALANCE_COMPROBACION s WITH(NOLOCK)
        WHERE (@IDENTIDAD = '-1' OR s.IDENTIDAD = @IDENTIDAD)
          AND ((s.PERIODO >= @IniAct AND s.PERIODO <= @FinAct) OR (s.PERIODO >= @IniAnt AND s.PERIODO <= @FinAnt))
    ),
    SaldosPeriodos AS (
        SELECT 
            IDENTIDAD,
            CUENTA,
            MAX(CASE WHEN PeriodoTipo = 'ACT' THEN Saldo ELSE 0.00 END) AS SaldoActual,
            MAX(CASE WHEN PeriodoTipo = 'ANT' THEN Saldo ELSE 0.00 END) AS SaldoAnterior
        FROM SaldosBase
        WHERE rn = 1
        GROUP BY IDENTIDAD, CUENTA
    ),
    Variaciones AS (
        SELECT 
            sp.IDENTIDAD AS IdEntidad,
            sp.CUENTA AS Cuenta,
            ISNULL(c.NOMBRE, 'Cuenta ' + sp.CUENTA) AS NombreCuenta,
            ISNULL(c.NIVEL, 3) AS Nivel,
            CAST(ROUND(sp.SaldoActual, 2) AS DECIMAL(18,2)) AS SaldoActual,
            CAST(ROUND(sp.SaldoAnterior, 2) AS DECIMAL(18,2)) AS SaldoAnterior,
            CAST(ROUND(sp.SaldoActual - sp.SaldoAnterior, 2) AS DECIMAL(18,2)) AS VariacionMonto,
            CAST(ROUND(
                CASE 
                    WHEN sp.SaldoAnterior != 0 THEN ((sp.SaldoActual - sp.SaldoAnterior) / ABS(sp.SaldoAnterior)) * 100.0
                    ELSE 0.00 
                END, 2
            ) AS DECIMAL(18,2)) AS VariacionPorcentaje,
            -- Resolver umbrales efectivos por entidad o herencia global
            ISNULL(acm.UmbralPorcPersonalizado, ISNULL(ape.UmbralVariacionPorc, @GLOBAL_PORC)) AS UmbralPorcEfectivo,
            ISNULL(acm.UmbralMontoPersonalizado, ISNULL(ape.UmbralVariacionMonto, @GLOBAL_MONTO)) AS UmbralMontoEfectivo,
            ISNULL(ape.ModoMonitoreo, 'TODAS') AS ModoMonitoreo,
            acm.TipoRegla AS ReglaCuenta,
            acm.Activo AS CuentaActiva
        FROM SaldosPeriodos sp
        LEFT JOIN dbo.CATALOGOCUENTA c WITH(NOLOCK) ON sp.CUENTA = c.CUENTA
        LEFT JOIN dbo.AlertaParametrosEntidad ape WITH(NOLOCK) ON sp.IDENTIDAD = ape.IdEntidad AND ape.Activo = 1
        LEFT JOIN dbo.AlertaCuentaMonitoreada acm WITH(NOLOCK) ON sp.IDENTIDAD = acm.IdEntidad AND sp.CUENTA = acm.Cuenta AND acm.Activo = 1
        WHERE (ABS(sp.SaldoActual) > 0.01 OR ABS(sp.SaldoAnterior) > 0.01)
    ),
    VariacionesFiltradas AS (
        SELECT *
        FROM Variaciones v
        WHERE 
            -- Regla 1: Modo Catálogo Completo (o si la cuenta está explícitamente en Watchlist)
            (
                v.ModoMonitoreo = 'TODAS' 
                AND (v.ReglaCuenta IS NULL OR v.ReglaCuenta != 'IGNORAR')
            )
            -- Regla 2: Modo Solo Watchlist (exclusivamente cuentas marcadas como MONITOREAR)
            OR (
                v.ModoMonitoreo = 'SOLO_WATCHLIST' 
                AND v.ReglaCuenta = 'MONITOREAR' 
                AND v.CuentaActiva = 1
            )
            -- Regla 3: Modo Excluir Blacklist (todas excepto las marcadas como IGNORAR)
            OR (
                v.ModoMonitoreo = 'EXCLUIR_BLACKLIST' 
                AND (v.ReglaCuenta IS NULL OR v.ReglaCuenta != 'IGNORAR')
            )
    ),
    AlertasDetectadas AS (
        SELECT 
            vf.IdEntidad,
            @PERIODO_ACTUAL AS PeriodoActual,
            @PERIODO_ANTERIOR AS PeriodoAnterior,
            vf.Cuenta,
            vf.NombreCuenta,
            vf.SaldoActual,
            vf.SaldoAnterior,
            vf.VariacionMonto,
            vf.VariacionPorcentaje,
            CASE 
                WHEN ABS(vf.VariacionPorcentaje) >= (vf.UmbralPorcEfectivo * 2.0) OR ABS(vf.VariacionMonto) >= (vf.UmbralMontoEfectivo * 2.5) THEN 'CRITICA'
                ELSE 'ADVERTENCIA'
            END AS TipoAlerta,
            CASE 
                WHEN ABS(vf.VariacionPorcentaje) >= (vf.UmbralPorcEfectivo * 2.0) OR ABS(vf.VariacionMonto) >= (vf.UmbralMontoEfectivo * 2.5) 
                     THEN 'Variación Crítica en ' + RTRIM(vf.Cuenta) + ' - ' + vf.NombreCuenta
                ELSE 'Variación Significativa en ' + RTRIM(vf.Cuenta) + ' - ' + vf.NombreCuenta
            END AS Titulo,
            'La cuenta ' + RTRIM(vf.Cuenta) + ' (' + vf.NombreCuenta + ') registró una variación del ' + 
            CASE WHEN vf.VariacionPorcentaje >= 0 THEN '+' ELSE '' END + CAST(vf.VariacionPorcentaje AS NVARCHAR(30)) + '% (₡' + 
            FORMAT(vf.VariacionMonto, '#,##0.00', 'es-CR') + ') entre ' + 
            FORMAT(@PERIODO_ANTERIOR, 'MM/yyyy') + ' y ' + FORMAT(@PERIODO_ACTUAL, 'MM/yyyy') + 
            '. Saldo anterior: ₡' + FORMAT(vf.SaldoAnterior, '#,##0.00', 'es-CR') + ' | Saldo actual: ₡' + FORMAT(vf.SaldoActual, '#,##0.00', 'es-CR') + '.' AS Mensaje
        FROM VariacionesFiltradas vf
        -- Filtro contra umbrales efectivos (de cuenta, de entidad o global)
        WHERE ABS(vf.VariacionPorcentaje) >= vf.UmbralPorcEfectivo
          AND ABS(vf.VariacionMonto) >= vf.UmbralMontoEfectivo
    )
    -- Guardar resultados calculados en tabla temporal para evitar alcance inválido de CTE
    SELECT 
        ad.IdEntidad, ad.PeriodoActual, ad.PeriodoAnterior, ad.Cuenta, ad.NombreCuenta,
        ad.SaldoActual, ad.SaldoAnterior, ad.VariacionMonto, ad.VariacionPorcentaje,
        ad.TipoAlerta, ad.Titulo, ad.Mensaje
    INTO #AlertasDetectadas
    FROM AlertasDetectadas ad;

    -- Limpiar alertas existentes para el período y entidad evaluada para reflejar fielmente los nuevos umbrales
    DELETE FROM dbo.AlertaFinanciera
    WHERE (@IDENTIDAD = '-1' OR IdEntidad = @IDENTIDAD)
      AND PeriodoActual = @PERIODO_ACTUAL;

    -- Insertar en la tabla de Alertas las variaciones calificadas
    INSERT INTO dbo.AlertaFinanciera (
        IdEntidad, PeriodoActual, PeriodoAnterior, Cuenta, NombreCuenta, 
        SaldoActual, SaldoAnterior, VariacionMonto, VariacionPorcentaje, 
        TipoAlerta, Titulo, Mensaje, FechaGeneracion, Leido, Estado
    )
    SELECT 
        ad.IdEntidad, ad.PeriodoActual, ad.PeriodoAnterior, ad.Cuenta, ad.NombreCuenta,
        ad.SaldoActual, ad.SaldoAnterior, ad.VariacionMonto, ad.VariacionPorcentaje,
        ad.TipoAlerta, ad.Titulo, ad.Mensaje, GETDATE(), 0, 'ACTIVA'
    FROM #AlertasDetectadas ad;

    DROP TABLE IF EXISTS #AlertasDetectadas;

    -- Retornar el resumen de alertas para el período consultado
    EXEC dbo.FGA_Obtener_Alertas_Financieras 
        @IDENTIDAD = @IDENTIDAD, 
        @SOLO_NO_LEIDAS = 0, 
        @TOP = 100;
END;
GO

PRINT '================================================================================';
PRINT '>> [5/5] Asignando Permisos de Ejecución a los Nuevos Procedimientos...';
PRINT '================================================================================';

GRANT EXECUTE ON [dbo].[FGA_Obtener_Configuracion_Alerta_Entidad] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Obtener_Configuracion_Alerta_Entidad] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Configuracion_Alerta_Entidad] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Configuracion_Alerta_Entidad] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad] TO [public];
GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Variacion_Financiera] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Variacion_Financiera] TO [public];
GO

PRINT '================================================================================';
PRINT '>> [COMPLETADO] Sistema de Parámetros y Cuentas Monitoreadas por Entidad listo.';
PRINT '================================================================================';
