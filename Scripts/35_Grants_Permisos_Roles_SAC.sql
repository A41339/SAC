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
PRINT '3. Sincronizando catálogo de menús en [dbo].[Menu] y asignando permisos...';

IF OBJECT_ID('[dbo].[Menu]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[MenuPermission]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL
BEGIN
    -- 3.0 Catálogo de menús a crear/garantizar en [dbo].[Menu]
    IF OBJECT_ID('tempdb..#NuevosMenus') IS NOT NULL DROP TABLE #NuevosMenus;

    CREATE TABLE #NuevosMenus (
        Id INT,
        MenuText NVARCHAR(150),
        MenuURL NVARCHAR(250),
        ParentId INT,
        SortOrder INT,
        MenuIcon NVARCHAR(100),
        Description NVARCHAR(500)
    );

    INSERT INTO #NuevosMenus (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description) VALUES
        (7500, 'Evaluación Riesgos', 'root', NULL, 3, '<i class="fa fa-calculator"></i>', 'Análisis integral de riesgo de crédito y riesgo de liquidez.'),
        (7510, 'Riesgo de crédito', 'root', 7500, 1, '<i class="fa fa-pie-chart"></i>', 'Monitoreo de indicadores de alerta temprana y calidad de cartera SUGEF 14-21.'),
        (7511, 'Indicadores de alerta', 'RiesgoCredito/Alertas', 7510, 1, '<i class="fa fa-bell-o"></i>', 'Monitoreo del deterioro del riesgo de crédito mediante indicadores de alerta temprana SUGEF 14-21.'),
        (7512, 'Modelo de matrices', 'Cartera14_21/Index?parent=7510', 7510, 2, '<i class="fa fa-th"></i>', 'Matrices de transición de mora y calificación crediticia.'),
        (7513, 'Modelo de cosechas', 'Cartera14_21/Index?parent=7510', 7510, 3, '<i class="fa fa-line-chart"></i>', 'Curvas de maduración y cosechas crediticias.'),
        (7514, 'Simulaciones de mora', 'Cartera14_21/Index?parent=7510', 7510, 4, '<i class="fa fa-sliders"></i>', 'Simulaciones prospectivas de morosidad y estrés crediticio.'),
        (7515, 'Evaluación de riesgo FFC', 'Cartera14_21/Index?parent=7510', 7510, 5, '<i class="fa fa-shield"></i>', 'Metodología y evaluación del perfil de riesgo de crédito FFC.'),
        (5010, 'Liquidez Contractual', 'Liquidez/Contractual', 5000, 1, '<i class="fa fa-book"></i>', 'Cálculo de ICL, GAP Contractual e IFNE según normativa.'),
        (5020, 'Escenarios de Estrés', 'Liquidez/Estres', 5000, 2, '<i class="fa fa-bolt"></i>', 'Modelos proyectados de liquidez: Supuestos, Cobertura, ICL Estresado, Brechas y Horizonte.');

    -- 3.1 Insertar en [dbo].[Menu] los que falten con IDENTITY_INSERT ON
    DECLARE @MenuIdentity INT = COLUMNPROPERTY(OBJECT_ID('[dbo].[Menu]'), 'Id', 'IsIdentity');

    IF EXISTS (SELECT 1 FROM #NuevosMenus n WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Menu] m WHERE m.Id = n.Id))
    BEGIN
        IF @MenuIdentity = 1
        BEGIN
            SET IDENTITY_INSERT [dbo].[Menu] ON;
        END

        -- Insertar respetando jerarquía: primero 7500, luego 7510, luego subopciones
        INSERT INTO [dbo].[Menu] (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        SELECT n.Id, n.MenuText, n.MenuURL, n.ParentId, n.SortOrder, n.MenuIcon, n.Description
        FROM #NuevosMenus n
        WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Menu] m WHERE m.Id = n.Id)
        ORDER BY CASE WHEN n.Id = 7500 THEN 1 WHEN n.Id = 7510 THEN 2 ELSE 3 END;

        IF @MenuIdentity = 1
        BEGIN
            SET IDENTITY_INSERT [dbo].[Menu] OFF;
        END

        PRINT '   -> Menús creados exitosamente en [dbo].[Menu].';
    END

    -- 3.2 Actualizar opciones existentes para estructura limpia
    -- Trasladar Liquidez (5000) bajo Evaluación Riesgos (7500)
    UPDATE [dbo].[Menu]
    SET ParentId = 7500,
        SortOrder = 2,
        MenuText = 'Liquidez',
        MenuURL = 'Liquidez/Index',
        MenuIcon = '<i class="fa fa-tint"></i>',
        Description = 'Submódulo de Liquidez: Liquidez Contractual y Escenarios de Estrés.'
    WHERE Id = 5000;

    -- Renombrar 4130 en Cartera de Crédito a 'Calidad de la cartera' para diferenciar del nuevo módulo 'Riesgo de crédito'
    UPDATE [dbo].[Menu]
    SET MenuText = 'Calidad de la cartera',
        Description = 'Composición de mora, morosidad agrupada y análisis según normativa SUGEF 14-21.'
    WHERE Id = 4130;

    -- Reordenar módulos raíz
    UPDATE [dbo].[Menu] SET SortOrder = 1, ParentId = NULL WHERE Id = 1000; -- Tablero
    UPDATE [dbo].[Menu] SET SortOrder = 2, ParentId = NULL WHERE Id = 1500; -- Estructura Financiera
    UPDATE [dbo].[Menu] SET SortOrder = 3, ParentId = NULL WHERE Id = 7500; -- Evaluación Riesgos
    UPDATE [dbo].[Menu] SET SortOrder = 4, ParentId = NULL WHERE Id = 4000; -- Cartera de Crédito
    UPDATE [dbo].[Menu] SET SortOrder = 5, ParentId = NULL WHERE Id = 6000; -- Evaluación SBR
    UPDATE [dbo].[Menu] SET SortOrder = 6, ParentId = NULL WHERE Id = 9000; -- Administración

    PRINT '   -> Jerarquía y nombres de menú sincronizados correctamente.';

    -- 3.3 Catálogo de todos los menús que deben tener permisos asignados
    IF OBJECT_ID('tempdb..#MenusConPermiso') IS NOT NULL DROP TABLE #MenusConPermiso;

    CREATE TABLE #MenusConPermiso (MenuId INT, SortOrder INT);
    INSERT INTO #MenusConPermiso (MenuId, SortOrder)
    SELECT Id, SortOrder FROM [dbo].[Menu]
    WHERE Id IN (1500, 4000, 4130, 5000, 5010, 5020, 7500, 7510, 7511, 7512, 7513, 7514, 7515);

    -- 3.4 Identificar combinaciones faltantes en dbo.MenuPermission
    -- Se enlaza estrictamente con [dbo].[Menu] para garantizar FK
    IF OBJECT_ID('tempdb..#PermisosFaltantes') IS NOT NULL DROP TABLE #PermisosFaltantes;

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
    FROM #MenusConPermiso m
    INNER JOIN [dbo].[Menu] m_db ON m_db.Id = m.MenuId
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
        DECLARE @IsIdentity INT = COLUMNPROPERTY(OBJECT_ID('[dbo].[MenuPermission]'), 'Id', 'IsIdentity');

        IF @IsIdentity = 1
        BEGIN
            PRINT '   -> Insertando permisos en [dbo].[MenuPermission] (columna Id es IDENTITY)...';
            INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
            SELECT MenuId, RoleId, SortOrder, 1, 1, 1, 1
            FROM #PermisosFaltantes;
        END
        ELSE
        BEGIN
            PRINT '   -> Insertando permisos en [dbo].[MenuPermission] (calculando Id no-identity)...';
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

        PRINT '   -> ' + CAST(@CantidadFaltantes AS NVARCHAR(10)) + ' registros de permisos insertados exitosamente.';
    END
    ELSE
    BEGIN
        PRINT '   -> Todos los roles ya tenían registros asignados en MenuPermission.';
    END

    -- 3.5 Garantizar lectura y operación completa para todos los roles
    UPDATE mp
    SET mp.IsRead = 1,
        mp.IsCreate = 1,
        mp.IsUpdate = 1,
        mp.IsDelete = 1
    FROM [dbo].[MenuPermission] mp
    INNER JOIN #MenusConPermiso m ON mp.MenuId = m.MenuId;

    PRINT '   -> Permisos de visualización (IsRead = 1) garantizados para todos los roles.';

    DROP TABLE #PermisosFaltantes;
    DROP TABLE #MenusConPermiso;
    DROP TABLE #NuevosMenus;
END
GO

PRINT '=====================================================================================';
PRINT 'Asignación de permisos completada exitosamente. Todos los roles pueden ver y ejecutar.';
PRINT '=====================================================================================';
GO
