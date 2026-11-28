-- Si se ejecuta en FFC o FGA, respeta el contexto actual de base de datos
IF DB_NAME() NOT IN ('FFC', 'FGA')
BEGIN
    IF DB_ID('FFC') IS NOT NULL USE [FFC];
    ELSE IF DB_ID('FGA') IS NOT NULL USE [FGA];
END
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
-- 3. VERIFICACIÓN Y CREACIÓN DE MENÚS Y ASIGNACIÓN EN dbo.MenuPermission
-- =====================================================================================
PRINT '3. Verificando existencia de menús y asignando permisos en [dbo].[MenuPermission]...';

IF OBJECT_ID('[dbo].[Menu]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[MenuPermission]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL
BEGIN
    -- 3.0 Catálogo de menús requeridos para SAC (Riesgo y Liquidez)
    IF OBJECT_ID('tempdb..#DefinicionMenus') IS NOT NULL DROP TABLE #DefinicionMenus;

    CREATE TABLE #DefinicionMenus (
        Id INT,
        MenuText NVARCHAR(150),
        MenuURL NVARCHAR(250),
        ParentId INT,
        SortOrder INT,
        MenuIcon NVARCHAR(100),
        Description NVARCHAR(500)
    );

    INSERT INTO #DefinicionMenus (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description) VALUES
        (1500, 'Estructura Financiera', 'root', NULL, 2, '<i class="fa fa-line-chart"></i>', 'Estructura Financiera de la Entidad'),
        (7500, 'Evaluación Riesgos', 'root', NULL, 3, '<i class="fa fa-calculator"></i>', 'Análisis de colocación, morosidad y concentración de la cartera de crédito y liquidez.'),
        (4000, 'Cartera de crédito', 'root', 1500, 4, '<i class="fa fa-credit-card"></i>', 'Análisis de colocación, morosidad y concentración de la cartera de crédito.'),
        (4001, 'Calidad de la cartera', 'Cartera14_21/Index', 4000, 1, '<i class="fa fa-pie-chart"></i>', 'Composición de mora, morosidad agrupada y análisis según normativa SUGEF 14-21.'),
        (7510, 'Riesgo de crédito', 'root', 7500, 1, '<i class="fa fa-pie-chart"></i>', 'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.'),
        (7511, 'Indicadores de alerta', 'RiesgoCredito/Alertas', 7510, 1, '<i class="fa fa-bell-o"></i>', 'Monitoreo del deterioro del riesgo de crédito mediante indicadores de tendencia, niveles y límites internos.'),
        (7512, 'Modelo de matrices', 'Cartera14_21/Index', 7510, 2, '<i class="fa fa-th"></i>', 'Analice la composición de las fuentes de fondeo y matrices de transición de la entidad.'),
        (7513, 'Modelo de cosechas', 'Cartera14_21/Index', 7510, 3, '<i class="fa fa-line-chart"></i>', 'Analice las curvas de maduración y cosechas crediticias de la entidad.'),
        (7514, 'Simulaciones de mora', 'Cartera14_21/Index', 7510, 4, '<i class="fa fa-sliders"></i>', 'Simulaciones prospectivas de morosidad y estrés según acuerdo SUGEF 14-21.'),
        (7515, 'Evaluación de riesgo FFC', 'Cartera14_21/Index', 7510, 5, '<i class="fa fa-shield"></i>', 'Metodología y evaluación del perfil de riesgo de crédito FFC.'),
        (5000, 'Liquidez', 'Liquidez/Index', 7500, 2, '<i class="fa fa-tint"></i>', 'Submódulo de Liquidez: Liquidez Contractual y Escenarios de Estrés.'),
        (5010, 'Liquidez Contractual', 'Liquidez/Contractual', 5000, 1, '<i class="fa fa-book"></i>', 'Cálculo e insumos del Indicador de Riesgo de Liquidez: Indicadores normativos, ICL, GAP Contractual e IFNE.'),
        (5020, 'Escenarios de Estrés', 'Liquidez/Estres', 5000, 2, '<i class="fa fa-bolt"></i>', 'Modelos proyectados de liquidez: Supuestos, Razón de Cobertura a 30 días, ICL Estresado, Brechas y Horizonte.');

    -- 3.1 Insertar en [dbo].[Menu] los que falten garantizando FK posterior
    DECLARE @MenuIdentity INT = COLUMNPROPERTY(OBJECT_ID('[dbo].[Menu]'), 'Id', 'IsIdentity');

    IF EXISTS (SELECT 1 FROM #DefinicionMenus d WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Menu] m WHERE m.Id = d.Id))
    BEGIN
        IF @MenuIdentity = 1
        BEGIN
            SET IDENTITY_INSERT [dbo].[Menu] ON;
        END

        INSERT INTO [dbo].[Menu] (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        SELECT d.Id, d.MenuText, d.MenuURL, d.ParentId, d.SortOrder, d.MenuIcon, d.Description
        FROM #DefinicionMenus d
        WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Menu] m WHERE m.Id = d.Id)
        ORDER BY CASE WHEN d.ParentId IS NULL THEN 1 WHEN d.ParentId IN (1500, 7500) THEN 2 ELSE 3 END, d.Id;

        IF @MenuIdentity = 1
        BEGIN
            SET IDENTITY_INSERT [dbo].[Menu] OFF;
        END

        PRINT '   -> Menús faltantes creados en [dbo].[Menu].';
    END

    -- 3.2 Actualizar configuración de los menús existentes
    UPDATE m
    SET m.MenuText = d.MenuText,
        m.MenuURL = d.MenuURL,
        m.ParentId = d.ParentId,
        m.SortOrder = d.SortOrder,
        m.MenuIcon = d.MenuIcon,
        m.Description = d.Description
    FROM [dbo].[Menu] m
    INNER JOIN #DefinicionMenus d ON m.Id = d.Id;

    PRINT '   -> Catálogo de menús sincronizado.';

    -- 3.3 Identificar combinaciones faltantes de MenuPermission
    -- Se hace INNER JOIN con [dbo].[Menu] para GARANTIZAR que MenuId exista en dbo.Menu (evita error FK)
    IF OBJECT_ID('tempdb..#PermisosFaltantes') IS NOT NULL DROP TABLE #PermisosFaltantes;

    CREATE TABLE #PermisosFaltantes (
        Fila INT IDENTITY(1,1),
        MenuId INT,
        RoleId INT,
        SortOrder INT
    );

    INSERT INTO #PermisosFaltantes (MenuId, RoleId, SortOrder)
    SELECT 
        d.Id AS MenuId, 
        r.Id AS RoleId, 
        d.SortOrder
    FROM #DefinicionMenus d
    INNER JOIN [dbo].[Menu] m_db ON m_db.Id = d.Id
    CROSS JOIN [dbo].[Role] r
    WHERE NOT EXISTS (
        SELECT 1 
        FROM [dbo].[MenuPermission] mp 
        WHERE mp.MenuId = d.Id AND mp.RoleId = r.Id
    );

    DECLARE @CantidadFaltantes INT = 0;
    SELECT @CantidadFaltantes = COUNT(*) FROM #PermisosFaltantes;

    IF @CantidadFaltantes > 0
    BEGIN
        DECLARE @IsIdentity INT = COLUMNPROPERTY(OBJECT_ID('[dbo].[MenuPermission]'), 'Id', 'IsIdentity');

        IF @IsIdentity = 1
        BEGIN
            PRINT '   -> Columna [dbo].[MenuPermission].[Id] es IDENTITY. Insertando...';
            INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
            SELECT MenuId, RoleId, SortOrder, 1, 1, 1, 1
            FROM #PermisosFaltantes;
        END
        ELSE
        BEGIN
            PRINT '   -> Columna [dbo].[MenuPermission].[Id] NO es IDENTITY. Calculando Id autoincremental seguro...';
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

    -- 3.4 Asegurar que todos los permisos para estos menús tengan lectura y operación habilitada
    UPDATE mp
    SET mp.IsRead = 1,
        mp.IsCreate = 1,
        mp.IsUpdate = 1,
        mp.IsDelete = 1
    FROM [dbo].[MenuPermission] mp
    INNER JOIN #DefinicionMenus d ON mp.MenuId = d.Id;

    PRINT '   -> Permisos de visualización (IsRead = 1) garantizados para todos los roles.';

    DROP TABLE #PermisosFaltantes;
    DROP TABLE #DefinicionMenus;
END
GO

PRINT '=====================================================================================';
PRINT 'Asignación de permisos completada exitosamente. Todos los roles pueden ver y ejecutar.';
PRINT '=====================================================================================';
GO
