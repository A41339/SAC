USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =====================================================================================
-- SCRIPT: 35_Grants_Permisos_Roles_SAC.sql
-- Módulo SAC: Asignación Integral de Permisos de Base de Datos y de Aplicación
-- 1. Permisos EXECUTE sobre todos los Procedimientos Almacenados de Riesgo y Liquidez
-- 2. Permisos SELECT, INSERT, UPDATE, DELETE sobre todas las Tablas del Sistema
-- 3. Permisos en [dbo].[MenuPermission] para todos los Roles activos en [dbo].[Role]
--    (Para que todos los perfiles de usuario puedan ver las opciones en la web)
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
-- Asegura que todos los roles activos (dbo.Role) puedan ver los módulos nuevos:
--   7500: Evaluación Riesgos
--   7510: Riesgo de crédito
--   7511: Indicadores de alerta
--   7512: Modelo de matrices
--   7513: Modelo de cosechas
--   7514: Simulaciones de mora
--   7515: Evaluación de riesgo FFC
--   4000: Cartera de crédito
--   4001: Calidad de la cartera
--   5000: Liquidez
--   5010: Liquidez Contractual
--   5020: Escenarios de Estrés
--   1500: Estructura Financiera
-- =====================================================================================
PRINT '3. Asignando permisos en [dbo].[MenuPermission] para todos los roles de usuario...';

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

-- 3.1 Insertar permisos completos para todos los roles existentes en dbo.Role si no los tienen
INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
SELECT 
    m.MenuId, 
    r.Id AS RoleId, 
    m.SortOrder, 
    1 AS IsCreate, 
    1 AS IsRead, 
    1 AS IsUpdate, 
    1 AS IsDelete
FROM @MenusRequeridos m
CROSS JOIN [dbo].[Role] r
WHERE NOT EXISTS (
    SELECT 1 
    FROM [dbo].[MenuPermission] mp 
    WHERE mp.MenuId = m.MenuId AND mp.RoleId = r.Id
);

PRINT '   -> Permisos insertados para roles no asignados previamente.';

-- 3.2 Asegurar que los permisos existentes tengan lectura (IsRead = 1) habilitada
UPDATE mp
SET mp.IsRead = 1,
    mp.IsCreate = 1,
    mp.IsUpdate = 1,
    mp.IsDelete = 1
FROM [dbo].[MenuPermission] mp
INNER JOIN @MenusRequeridos m ON mp.MenuId = m.MenuId;

PRINT '   -> Permisos de visualización y operación garantizados (IsRead = 1) para todos los roles.';
GO

PRINT '=====================================================================================';
PRINT 'Asignación de permisos completada exitosamente. Todos los roles pueden ver y ejecutar.';
PRINT '=====================================================================================';
GO
