USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =====================================================================================
-- SCRIPT: 35_Grants_Permisos_Roles_SAC.sql (CORREGIDO)
-- Módulo SAC: Asignación Integral de Permisos de Base de Datos y de Aplicación
-- 1. Permisos EXECUTE sobre todos los Procedimientos Almacenados a [FGA] y [public]
-- 2. Permisos SELECT, INSERT, UPDATE, DELETE sobre todas las Tablas a [FGA] y [public]
-- 3. Permisos en [dbo].[MenuPermission] con cálculo seguro de [Id] (MAX(Id) + ROW_NUMBER)
--    Compatible tanto si [Id] es IDENTITY como si NO ES IDENTITY
-- =====================================================================================

PRINT '=====================================================================================';
PRINT 'Iniciando asignación integral de permisos (Grants y MenuPermission)...';
PRINT '=====================================================================================';
GO

-- =====================================================================================
-- 1. PERMISOS DE EJECUCIÓN (GRANT EXECUTE) SOBRE PROCEDIMIENTOS ALMACENADOS
-- =====================================================================================
PRINT '1. Asignando permisos GRANT EXECUTE a los roles [FGA] y [public]...';

-- A. Submódulo Liquidez Contractual y GAP
IF OBJECT_ID('[dbo].[FGA_Generar_GAP_Contractual]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Generar_GAP_Contractual] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Generar_GAP_Contractual] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Generar_GAP_Contractual].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_GAP_Contractual]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_GAP_Contractual] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_GAP_Contractual] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_GAP_Contractual].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Indicadores_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Indicadores_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Indicadores_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Indicadores_Liquidez].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Serie_VaR_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Serie_VaR_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Serie_VaR_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Serie_VaR_Liquidez].';
END

-- B. Submódulo de Riesgo de Crédito (Alertas Tempranas SUGEF 14-21)
IF OBJECT_ID('[dbo].[FGA_Generar_Alertas_Tempranas_Credito]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Tempranas_Credito] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Generar_Alertas_Tempranas_Credito] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Generar_Alertas_Tempranas_Credito].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Alertas_Tempranas_Credito]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Alertas_Tempranas_Credito] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Alertas_Tempranas_Credito] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Alertas_Tempranas_Credito].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Serie_Alerta_Temprana]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Serie_Alerta_Temprana] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Serie_Alerta_Temprana] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Serie_Alerta_Temprana].';
END

-- C. Escenarios de Estrés de Liquidez
IF OBJECT_ID('[dbo].[FGA_Consultar_Parametros_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Parametros_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Parametros_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Parametros_Liquidez].';
END

IF OBJECT_ID('[dbo].[FGA_Guardar_Parametros_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Guardar_Parametros_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Guardar_Parametros_Liquidez].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Calculo_VaR_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Calculo_VaR_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Calculo_VaR_Liquidez].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Proyecciones_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Proyecciones_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Proyecciones_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Proyecciones_Liquidez].';
END

IF OBJECT_ID('[dbo].[FGA_Consultar_Modelos_Estres_Liquidez]', 'P') IS NOT NULL
BEGIN
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Modelos_Estres_Liquidez] TO [FGA];
    GRANT EXECUTE ON [dbo].[FGA_Consultar_Modelos_Estres_Liquidez] TO [public];
    PRINT '   -> Permiso EXECUTE concedido en [FGA_Consultar_Modelos_Estres_Liquidez].';
END
GO

-- =====================================================================================
-- 2. PERMISOS DE TABLAS (GRANT SELECT, INSERT, UPDATE, DELETE)
-- =====================================================================================
PRINT '2. Asignando permisos SELECT, INSERT, UPDATE, DELETE sobre tablas a [FGA] y [public]...';

IF OBJECT_ID('[dbo].[Salida_GAP_Contractual]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_GAP_Contractual] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_GAP_Contractual] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Salida_GAP_Contractual].';
END

IF OBJECT_ID('[dbo].[Config_Alertas_Tempranas_Credito]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Config_Alertas_Tempranas_Credito] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Config_Alertas_Tempranas_Credito] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Config_Alertas_Tempranas_Credito].';
END

IF OBJECT_ID('[dbo].[Salida_Alertas_Tempranas_Credito]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Alertas_Tempranas_Credito] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Alertas_Tempranas_Credito] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Salida_Alertas_Tempranas_Credito].';
END

IF OBJECT_ID('[dbo].[FGA_Parametros_Liquidez_Entidad]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[FGA_Parametros_Liquidez_Entidad] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [FGA_Parametros_Liquidez_Entidad].';
END

IF OBJECT_ID('[dbo].[Salida_Concentracion_Ahorrantes]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Concentracion_Ahorrantes] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Concentracion_Ahorrantes] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Salida_Concentracion_Ahorrantes].';
END

IF OBJECT_ID('[dbo].[Salida_Concentracion_Vencimiento]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Concentracion_Vencimiento] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Salida_Concentracion_Vencimiento] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Salida_Concentracion_Vencimiento].';
END

IF OBJECT_ID('[dbo].[SALIDA_BALANCE_COMPROBACION]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT ON [dbo].[SALIDA_BALANCE_COMPROBACION] TO [FGA];
    GRANT SELECT ON [dbo].[SALIDA_BALANCE_COMPROBACION] TO [public];
    PRINT '   -> Permiso SELECT concedido en [SALIDA_BALANCE_COMPROBACION].';
END

IF OBJECT_ID('[dbo].[SALIDA_MATRICES]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT ON [dbo].[SALIDA_MATRICES] TO [FGA];
    GRANT SELECT ON [dbo].[SALIDA_MATRICES] TO [public];
    PRINT '   -> Permiso SELECT concedido en [SALIDA_MATRICES].';
END

IF OBJECT_ID('[dbo].[Menu]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Menu] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Menu] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [Menu].';
END

IF OBJECT_ID('[dbo].[MenuPermission]', 'U') IS NOT NULL
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[MenuPermission] TO [FGA];
    GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[MenuPermission] TO [public];
    PRINT '   -> Permisos CRUD concedidos en [MenuPermission].';
END
GO

-- =====================================================================================
-- 3. PERMISOS DE MENÚ EN LA APLICACIÓN (MenuPermission PARA TODOS LOS ROLES)
-- Maneja con seguridad la columna [Id] de [dbo].[MenuPermission]:
--   - Si [Id] NO es IDENTITY: calcula (@MaxId + ROW_NUMBER) para evitar conflicto de PK.
--   - Si [Id] ES IDENTITY: inserta sin la columna [Id].
-- =====================================================================================
PRINT '3. Asignando permisos en [dbo].[MenuPermission] para todos los roles de usuario...';

IF OBJECT_ID('[dbo].[MenuPermission]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL
BEGIN
    -- Lista de menús requeridos
    DECLARE @MenusRequeridos TABLE (MenuId INT, SortOrder INT);
    INSERT INTO @MenusRequeridos (MenuId, SortOrder) VALUES
        (1500, 2),  -- Estructura Financiera
        (4000, 4),  -- Cartera de crédito
        (4001, 1),  -- Calidad de la cartera
        (7500, 3),  -- Evaluación Riesgos
        (7510, 1),  -- Riesgo de crédito
        (7511, 1),  -- Indicadores de alerta
        (7512, 2),  -- Modelo de matrices
        (7513, 3),  -- Modelo de cosechas
        (7514, 4),  -- Simulaciones de mora
        (7515, 5),  -- Evaluación de riesgo FFC
        (5000, 2),  -- Liquidez
        (5010, 1),  -- Liquidez Contractual
        (5020, 2);  -- Escenarios de Estrés

    -- Identificar combinaciones faltantes
    CREATE TABLE #PermisosFaltantes (
        Fila INT IDENTITY(1,1),
        MenuId INT,
        RoleId INT,
        SortOrder INT
    );

    INSERT INTO #PermisosFaltantes (MenuId, RoleId, SortOrder)
    SELECT 
        m.MenuId, 
        r.Id AS RoleId, 
        m.SortOrder
    FROM @MenusRequeridos m
    CROSS JOIN [dbo].[Role] r
    WHERE NOT EXISTS (
        SELECT 1 
        FROM [dbo].[MenuPermission] mp 
        WHERE mp.MenuId = m.MenuId AND mp.RoleId = r.Id
    );

    DECLARE @CantidadFaltantes INT = 0;
    SELECT @CantidadFaltantes = COUNT(*) FROM #PermisosFaltantes;

    IF @CantidadFaltantes > 0
    BEGIN
        -- Detectar si la columna Id es IDENTITY
        DECLARE @IsIdentity INT = COLUMNPROPERTY(OBJECT_ID('[dbo].[MenuPermission]'), 'Id', 'IsIdentity');

        IF @IsIdentity = 1
        BEGIN
            PRINT '   -> Columna [Id] es IDENTITY. Insertando sin especificar Id...';
            INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
            SELECT MenuId, RoleId, SortOrder, 1, 1, 1, 1
            FROM #PermisosFaltantes;
        END
        ELSE
        BEGIN
            PRINT '   -> Columna [Id] NO es IDENTITY. Calculando Id autoincremental seguro...';
            DECLARE @MaxId INT = 0;
            SELECT @MaxId = ISNULL(MAX(Id), 0) FROM [dbo].[MenuPermission];

            INSERT INTO [dbo].[MenuPermission] ([Id], [MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
            SELECT 
                (@MaxId + Fila) AS Id,
                MenuId, 
                RoleId, 
                SortOrder, 
                1, 1, 1, 1
            FROM #PermisosFaltantes;
        END

        PRINT '   -> ' + CAST(@CantidadFaltantes AS NVARCHAR(10)) + ' permisos nuevos insertados exitosamente.';
    END
    ELSE
    BEGIN
        PRINT '   -> Todos los roles ya contaban con registros de permisos asignados.';
    END

    -- Asegurar que los permisos tengan lectura y operación habilitada (IsRead = 1)
    UPDATE mp
    SET mp.IsRead = 1,
        mp.IsCreate = 1,
        mp.IsUpdate = 1,
        mp.IsDelete = 1
    FROM [dbo].[MenuPermission] mp
    INNER JOIN @MenusRequeridos m ON mp.MenuId = m.MenuId;

    PRINT '   -> Permisos de visualización garantizados (IsRead = 1) para todos los roles.';

    DROP TABLE #PermisosFaltantes;
END
GO

PRINT '=====================================================================================';
PRINT 'Asignación de permisos completada exitosamente. Todos los roles pueden ver y ejecutar.';
PRINT '=====================================================================================';
GO
