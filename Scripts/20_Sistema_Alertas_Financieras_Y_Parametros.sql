-- =========================================================================================
-- SCRIPT: 20_Sistema_Alertas_Financieras_Y_Parametros.sql
-- Base de Datos: FFC / FGA
-- Objetivo: 
--   1. Registrar parámetros configurables en dbo.Parametros (Porcentaje y Monto).
--   2. Crear tabla dbo.AlertaFinanciera con índices de alto rendimiento.
--   3. Crear SP dbo.FGA_Generar_Alertas_Variacion_Financiera (detección automática).
--   4. Crear SP dbo.FGA_Obtener_Alertas_Financieras (consulta para campana / bandeja).
--   5. Crear SP dbo.FGA_Marcar_Alerta_Leida (marcado individual o masivo).
-- =========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/4] Creando / Verificando Parámetros Globales en dbo.Parametros...';
    PRINT '================================================================================';

    -- 1. Parámetro de Porcentaje de Variación (Default: 15.00 %)
    IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Llave = 'ALERTA_VARIACION_PORC')
    BEGIN
        INSERT INTO dbo.Parametros (Llave, Valor, Descripcion)
        VALUES ('ALERTA_VARIACION_PORC', '15.00', 'Porcentaje mínimo de variación (interanual o mensual) para disparar alerta financiera (%)');
        PRINT '   [OK] Parámetro ALERTA_VARIACION_PORC creado con valor 15.00%';
    END
    ELSE
    BEGIN
        PRINT '   [INFO] Parámetro ALERTA_VARIACION_PORC ya existe.';
    END

    -- 2. Parámetro de Monto Mínimo de Variación (Default: ₡10,000,000.00)
    IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Llave = 'ALERTA_VARIACION_MONTO')
    BEGIN
        INSERT INTO dbo.Parametros (Llave, Valor, Descripcion)
        VALUES ('ALERTA_VARIACION_MONTO', '10000000.00', 'Monto absoluto mínimo de variación para disparar alerta financiera (₡)');
        PRINT '   [OK] Parámetro ALERTA_VARIACION_MONTO creado con valor 10000000.00';
    END
    ELSE
    BEGIN
        PRINT '   [INFO] Parámetro ALERTA_VARIACION_MONTO ya existe.';
    END

    PRINT '================================================================================';
    PRINT '>> [2/4] Creando / Verificando Tabla dbo.AlertaFinanciera...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertaFinanciera' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.AlertaFinanciera (
            Id INT IDENTITY(1,1) NOT NULL,
            IdEntidad NVARCHAR(5) NOT NULL,
            PeriodoActual DATE NOT NULL,
            PeriodoAnterior DATE NOT NULL,
            Cuenta NVARCHAR(50) NOT NULL,
            NombreCuenta NVARCHAR(250) NULL,
            SaldoActual DECIMAL(18,2) NOT NULL DEFAULT 0.00,
            SaldoAnterior DECIMAL(18,2) NOT NULL DEFAULT 0.00,
            VariacionMonto DECIMAL(18,2) NOT NULL DEFAULT 0.00,
            VariacionPorcentaje DECIMAL(18,2) NOT NULL DEFAULT 0.00,
            TipoAlerta NVARCHAR(20) NOT NULL DEFAULT 'ADVERTENCIA', -- 'CRITICA', 'ADVERTENCIA', 'INFO'
            Titulo NVARCHAR(250) NOT NULL,
            Mensaje NVARCHAR(MAX) NOT NULL,
            FechaGeneracion DATETIME NOT NULL DEFAULT GETDATE(),
            Leido BIT NOT NULL DEFAULT 0,
            FechaLeido DATETIME NULL,
            UsuarioLeido NVARCHAR(100) NULL,
            Estado NVARCHAR(20) NOT NULL DEFAULT 'ACTIVA',
            CONSTRAINT PK_AlertaFinanciera PRIMARY KEY CLUSTERED (Id ASC)
        );

        CREATE NONCLUSTERED INDEX IX_AlertaFinanciera_Entidad_Periodo_Cuenta 
            ON dbo.AlertaFinanciera (IdEntidad, PeriodoActual, Cuenta);

        CREATE NONCLUSTERED INDEX IX_AlertaFinanciera_Leido_Fecha 
            ON dbo.AlertaFinanciera (Leido, FechaGeneracion DESC)
            INCLUDE (IdEntidad, TipoAlerta, Titulo, VariacionMonto, VariacionPorcentaje);

        PRINT '   [OK] Tabla dbo.AlertaFinanciera e índices creados exitosamente.';
    END
    ELSE
    BEGIN
        PRINT '   [INFO] Tabla dbo.AlertaFinanciera ya existe.';
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
PRINT '>> [3/4] Creando Stored Procedures de Alertas Financieras...';
PRINT '================================================================================';
GO

-- =========================================================================================
-- SP 1: dbo.FGA_Generar_Alertas_Variacion_Financiera
-- Detecta variaciones que superen los umbrales configurados y las almacena en AlertaFinanciera
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Generar_Alertas_Variacion_Financiera]
    @IDENTIDAD NVARCHAR(5) = '-1',
    @PERIODO DATETIME = NULL,
    @MODALIDAD NVARCHAR(20) = 'Acumulado',        -- 'Acumulado' o 'Mensual'
    @TIPO_COMPARACION NVARCHAR(20) = 'Interanual'  -- 'Interanual' o 'Mensual'
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';

    -- 1. Leer Parámetros Globales de Configuración (con fallback seguro y normalización de formatos)
    DECLARE @UMBRAL_PORC DECIMAL(18,2) = 15.00;
    DECLARE @UMBRAL_MONTO DECIMAL(18,2) = 10000000.00;

    DECLARE @PorcStr NVARCHAR(100), @MontoStr NVARCHAR(100);
    SELECT TOP 1 @PorcStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_PORC', 'ALERTA_PORCENTAJE', 'ALERTA_PORC', 'ALERTA_VARIACION_PORCENTAJE')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%PORC%')
       OR (Descripcion LIKE '%alerta%' AND (Descripcion LIKE '%porcent%' OR Descripcion LIKE '%[%]%'));

    SELECT TOP 1 @MontoStr = LTRIM(RTRIM(Valor)) 
    FROM dbo.Parametros 
    WHERE Llave IN ('ALERTA_VARIACION_MONTO', 'ALERTA_MONTO', 'MONTO_ALERTA')
       OR (Llave LIKE '%ALERTA%' AND Llave LIKE '%MONTO%')
       OR (Descripcion LIKE '%alerta%' AND (Descripcion LIKE '%monto%' OR Descripcion LIKE '%₡%'));

    IF @PorcStr IS NOT NULL
    BEGIN
        SET @PorcStr = REPLACE(REPLACE(REPLACE(REPLACE(@PorcStr, '%', ''), ' ', ''), '$', ''), '₡', '');
        IF CHARINDEX(',', @PorcStr) > 0 AND CHARINDEX('.', @PorcStr) = 0
            SET @PorcStr = REPLACE(@PorcStr, ',', '.');
        SET @UMBRAL_PORC = TRY_CAST(@PorcStr AS DECIMAL(18,2));
    END
    IF @UMBRAL_PORC IS NULL OR @UMBRAL_PORC <= 0 SET @UMBRAL_PORC = 15.00;

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
        ELSE IF CHARINDEX('.', @MontoStr) > 0
        BEGIN
            IF LEN(@MontoStr) - LEN(REPLACE(@MontoStr, '.', '')) > 1
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
            ELSE IF LEN(@MontoStr) - CHARINDEX('.', @MontoStr) = 3 AND LEN(@MontoStr) >= 5
                SET @MontoStr = REPLACE(@MontoStr, '.', '');
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

        SET @UMBRAL_MONTO = TRY_CAST(@MontoStr AS DECIMAL(18,2));
    END
    IF @UMBRAL_MONTO IS NULL OR @UMBRAL_MONTO <= 0 SET @UMBRAL_MONTO = 10000000.00;

    -- 2. Determinar Período Actual (si es null, tomar el más reciente disponible en balanza)
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

    -- Fechas de inicio y fin para filtros
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
            ) AS DECIMAL(18,2)) AS VariacionPorcentaje
        FROM SaldosPeriodos sp
        LEFT JOIN dbo.CATALOGOCUENTA c WITH(NOLOCK) ON sp.CUENTA = c.CUENTA
        -- Considerar cuentas relevantes (con movimiento activo o niveles contables de desglose)
        WHERE (ABS(sp.SaldoActual) > 0.01 OR ABS(sp.SaldoAnterior) > 0.01)
    ),
    AlertasDetectadas AS (
        SELECT 
            v.IdEntidad,
            @PERIODO_ACTUAL AS PeriodoActual,
            @PERIODO_ANTERIOR AS PeriodoAnterior,
            v.Cuenta,
            v.NombreCuenta,
            v.SaldoActual,
            v.SaldoAnterior,
            v.VariacionMonto,
            v.VariacionPorcentaje,
            CASE 
                WHEN ABS(v.VariacionPorcentaje) >= (@UMBRAL_PORC * 2.0) OR ABS(v.VariacionMonto) >= (@UMBRAL_MONTO * 2.5) THEN 'CRITICA'
                ELSE 'ADVERTENCIA'
            END AS TipoAlerta,
            CASE 
                WHEN ABS(v.VariacionPorcentaje) >= (@UMBRAL_PORC * 2.0) OR ABS(v.VariacionMonto) >= (@UMBRAL_MONTO * 2.5) 
                     THEN 'Variación Crítica en ' + RTRIM(v.Cuenta) + ' - ' + v.NombreCuenta
                ELSE 'Variación Significativa en ' + RTRIM(v.Cuenta) + ' - ' + v.NombreCuenta
            END AS Titulo,
            'La cuenta ' + RTRIM(v.Cuenta) + ' (' + v.NombreCuenta + ') registró una variación del ' + 
            CASE WHEN v.VariacionPorcentaje >= 0 THEN '+' ELSE '' END + CAST(v.VariacionPorcentaje AS NVARCHAR(30)) + '% (₡' + 
            FORMAT(v.VariacionMonto, '#,##0.00', 'es-CR') + ') entre ' + 
            FORMAT(@PERIODO_ANTERIOR, 'MM/yyyy') + ' y ' + FORMAT(@PERIODO_ACTUAL, 'MM/yyyy') + 
            '. Saldo anterior: ₡' + FORMAT(v.SaldoAnterior, '#,##0.00', 'es-CR') + ' | Saldo actual: ₡' + FORMAT(v.SaldoActual, '#,##0.00', 'es-CR') + '.' AS Mensaje
        FROM Variaciones v
        -- Filtro estricto: Supera AMBOS umbrales (porcentaje Y monto absoluto)
        WHERE ABS(v.VariacionPorcentaje) >= @UMBRAL_PORC
          AND ABS(v.VariacionMonto) >= @UMBRAL_MONTO
    )
    -- Limpiar alertas existentes para el período y entidad evaluada para reflejar fielmente los nuevos umbrales
    DELETE FROM dbo.AlertaFinanciera
    WHERE (@IDENTIDAD = '-1' OR IdEntidad = @IDENTIDAD)
      AND PeriodoActual = @PERIODO_ACTUAL;

    -- Insertar en la tabla de Alertas las variaciones que califican con los umbrales vigentes
    INSERT INTO dbo.AlertaFinanciera (
        IdEntidad, PeriodoActual, PeriodoAnterior, Cuenta, NombreCuenta, 
        SaldoActual, SaldoAnterior, VariacionMonto, VariacionPorcentaje, 
        TipoAlerta, Titulo, Mensaje, FechaGeneracion, Leido, Estado
    )
    SELECT 
        ad.IdEntidad, ad.PeriodoActual, ad.PeriodoAnterior, ad.Cuenta, ad.NombreCuenta,
        ad.SaldoActual, ad.SaldoAnterior, ad.VariacionMonto, ad.VariacionPorcentaje,
        ad.TipoAlerta, ad.Titulo, ad.Mensaje, GETDATE(), 0, 'ACTIVA'
    FROM AlertasDetectadas ad;

    -- Retornar el resumen de alertas para el período consultado
    EXEC dbo.FGA_Obtener_Alertas_Financieras 
        @IDENTIDAD = @IDENTIDAD, 
        @SOLO_NO_LEIDAS = 0, 
        @TOP = 100;
END;
GO

-- =========================================================================================
-- SP 2: dbo.FGA_Obtener_Alertas_Financieras
-- Consulta las alertas financieras para la campana / bandeja de notificaciones
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Obtener_Alertas_Financieras]
    @IDENTIDAD NVARCHAR(5) = '-1',
    @SOLO_NO_LEIDAS BIT = 0,
    @TOP INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    IF @IDENTIDAD = '99' SET @IDENTIDAD = '13';
    IF @TOP IS NULL OR @TOP <= 0 SET @TOP = 30;

    SELECT TOP (@TOP)
        af.Id,
        af.IdEntidad,
        ISNULL(e.Nombre, 'Entidad ' + af.IdEntidad) AS NombreEntidad,
        af.PeriodoActual,
        af.PeriodoAnterior,
        af.Cuenta,
        af.NombreCuenta,
        af.SaldoActual,
        af.SaldoAnterior,
        af.VariacionMonto,
        af.VariacionPorcentaje,
        af.TipoAlerta,
        af.Titulo,
        af.Mensaje,
        af.FechaGeneracion,
        af.Leido,
        af.FechaLeido,
        af.UsuarioLeido,
        af.Estado
    FROM dbo.AlertaFinanciera af WITH(NOLOCK)
    LEFT JOIN dbo.Entidad e WITH(NOLOCK) ON af.IdEntidad = e.Id
    WHERE (@IDENTIDAD = '-1' OR af.IdEntidad = @IDENTIDAD)
      AND (@SOLO_NO_LEIDAS = 0 OR af.Leido = 0)
      AND af.Estado = 'ACTIVA'
    ORDER BY 
        af.Leido ASC, 
        af.FechaGeneracion DESC, 
        ABS(af.VariacionMonto) DESC;
END;
GO

-- =========================================================================================
-- SP 3: dbo.FGA_Marcar_Alerta_Leida
-- Marca una alerta individual o todas las alertas de una entidad como leídas
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Marcar_Alerta_Leida]
    @ALERTA_ID INT = NULL,
    @IDENTIDAD NVARCHAR(5) = NULL,
    @USUARIO NVARCHAR(100) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;

    IF @USUARIO IS NULL OR RTRIM(@USUARIO) = '' SET @USUARIO = 'SYSTEM';

    IF @ALERTA_ID IS NOT NULL AND @ALERTA_ID > 0
    BEGIN
        UPDATE dbo.AlertaFinanciera
        SET Leido = 1,
            FechaLeido = GETDATE(),
            UsuarioLeido = @USUARIO
        WHERE Id = @ALERTA_ID;

        SELECT @@ROWCOUNT AS RegistrosAfectados;
    END
    ELSE IF @IDENTIDAD IS NOT NULL AND RTRIM(@IDENTIDAD) != ''
    BEGIN
        UPDATE dbo.AlertaFinanciera
        SET Leido = 1,
            FechaLeido = GETDATE(),
            UsuarioLeido = @USUARIO
        WHERE (@IDENTIDAD = '-1' OR IdEntidad = @IDENTIDAD)
          AND Leido = 0;

        SELECT @@ROWCOUNT AS RegistrosAfectados;
    END
    ELSE
    BEGIN
        SELECT 0 AS RegistrosAfectados;
    END
END;
GO

PRINT '================================================================================';
PRINT '>> [4/4] Asignando Permisos de Ejecución a Usuarios y Roles...';
PRINT '================================================================================';

GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Variacion_Financiera] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Variacion_Financiera] TO [public];

GRANT EXECUTE ON [dbo].[FGA_Obtener_Alertas_Financieras] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Obtener_Alertas_Financieras] TO [public];

GRANT EXECUTE ON [dbo].[FGA_Marcar_Alerta_Leida] TO [FGA];
GRANT EXECUTE ON [dbo].[FGA_Marcar_Alerta_Leida] TO [public];
GO

PRINT '================================================================================';
PRINT '>> SCRIPT 20 EJECUTADO Y COMPLETADO EXITOSAMENTE.';
PRINT '================================================================================';
