USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '=====================================================================================';
PRINT 'DESHABILITACIÓN DE IRL Y CONFIGURACIÓN DEL SUBMÓDULO DE LIQUIDEZ EN EL MENÚ';
PRINT '=====================================================================================';

-- 1. DESHABILITAR OPCIONES ANTIGUAS DE IRL EN dbo.Menu
-- Desvincula las opciones antiguas de IRL del menú principal y de cualquier carpeta
PRINT '1. Deshabilitando opciones obsoletas de IRL...';

UPDATE dbo.Menu
SET ParentId = NULL,
    Description = 'OBSOLETO - Reemplazado por nuevo Submódulo de Liquidez (2026)'
WHERE (MenuURL LIKE '%IRL%' OR MenuText LIKE '%IRL%')
  AND Id NOT IN (5000, 5010, 5020);

-- 2. ELIMINAR PERMISOS ACTIVOS EN dbo.MenuPermission PARA LAS OPCIONES OBSOLETAS DE IRL
PRINT '2. Removiendo permisos activos para las opciones de IRL obsoletas...';

DELETE FROM dbo.MenuPermission
WHERE MenuId IN (
    SELECT Id FROM dbo.Menu
    WHERE (MenuURL LIKE '%IRL%' OR MenuText LIKE '%IRL%')
      AND Id NOT IN (5000, 5010, 5020)
);

-- 3. CONFIGURAR SUBMÓDULO DE LIQUIDEZ BAJO ESTRUCTURA FINANCIERA (ParentId = 1500)
PRINT '3. Configurando carpeta principal de Liquidez (Menú 5000)...';

IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5000)
BEGIN
    UPDATE dbo.Menu
    SET MenuText = 'Liquidez',
        MenuURL = 'Liquidez/Index',
        ParentId = 1500, -- Bajo Estructura Financiera
        SortOrder = 3,
        MenuIcon = '<i class="fa fa-tint"></i>',
        Description = 'Reconfiguración del submódulo: Liquidez Contractual y Escenarios de Estrés.'
    WHERE Id = 5000;
    PRINT '   -> Opción 5000 [Liquidez] actualizada bajo Estructura Financiera (1500).';
END
ELSE
BEGIN
    SET IDENTITY_INSERT dbo.Menu ON;
    INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
    VALUES (5000, 'Liquidez', 'Liquidez/Index', 1500, 3, '<i class="fa fa-tint"></i>', 'Reconfiguración del submódulo: Liquidez Contractual y Escenarios de Estrés.');
    SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Opción 5000 [Liquidez] creada exitosamente.';
END

-- 4. CREAR/ACTUALIZAR LAS DOS SUB-OPCIONES BAJO LIQUIDEZ (ParentId = 5000)
PRINT '4. Configurando sub-opciones: Liquidez Contractual (5010) y Escenarios de Estrés (5020)...';

-- 4.1 Liquidez Contractual (5010)
IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5010)
BEGIN
    UPDATE dbo.Menu
    SET MenuText = 'Liquidez Contractual',
        MenuURL = 'Liquidez/Contractual',
        ParentId = 5000,
        SortOrder = 1,
        MenuIcon = '<i class="fa fa-book"></i>',
        Description = 'Cálculo e insumos del Indicador de Riesgo de Liquidez: Indicadores normativos, ICL, GAP Contractual e IFNE.'
    WHERE Id = 5010;
    PRINT '   -> Opción 5010 [Liquidez Contractual] actualizada.';
END
ELSE
BEGIN
    SET IDENTITY_INSERT dbo.Menu ON;
    INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
    VALUES (5010, 'Liquidez Contractual', 'Liquidez/Contractual', 5000, 1, '<i class="fa fa-book"></i>', 'Cálculo e insumos del Indicador de Riesgo de Liquidez: Indicadores normativos, ICL, GAP Contractual e IFNE.');
    SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Opción 5010 [Liquidez Contractual] creada exitosamente.';
END

-- 4.2 Escenarios de Estrés (5020)
IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5020)
BEGIN
    UPDATE dbo.Menu
    SET MenuText = 'Escenarios de Estrés',
        MenuURL = 'Liquidez/Estres',
        ParentId = 5000,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-bolt"></i>',
        Description = 'Modelos proyectados de liquidez: Supuestos, Razón de Cobertura a 30 días, ICL Estresado, Brechas y Horizonte.'
    WHERE Id = 5020;
    PRINT '   -> Opción 5020 [Escenarios de Estrés] actualizada.';
END
ELSE
BEGIN
    SET IDENTITY_INSERT dbo.Menu ON;
    INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
    VALUES (5020, 'Escenarios de Estrés', 'Liquidez/Estres', 5000, 2, '<i class="fa fa-bolt"></i>', 'Modelos proyectados de liquidez: Supuestos, Razón de Cobertura a 30 días, ICL Estresado, Brechas y Horizonte.');
    SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Opción 5020 [Escenarios de Estrés] creada exitosamente.';
END

-- 5. ASIGNACIÓN DE PERMISOS EN dbo.MenuPermission PARA ROLE 1 Y ROLES CON ACCESO
PRINT '5. Asignando permisos en dbo.MenuPermission para 5000, 5010 y 5020...';

DECLARE @MenuIds TABLE (MenuId INT);
INSERT INTO @MenuIds VALUES (5000), (5010), (5020);

-- Permisos para Role 1 (Administrador)
INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
SELECT m.MenuId, 1, 1, 1, 1, 1, 1
FROM @MenuIds m
WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.MenuId AND mp.RoleId = 1);

UPDATE dbo.MenuPermission
SET IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
WHERE MenuId IN (SELECT MenuId FROM @MenuIds) AND RoleId = 1;

-- Permisos para otros roles autorizados
INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
SELECT m.MenuId, r.Id, 1, 1, 1, 1, 1
FROM @MenuIds m
CROSS JOIN dbo.Role r
WHERE r.Id IN (2, 4, 5, 6, 7, 10, 12)
  AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.MenuId AND mp.RoleId = r.Id);

PRINT '=====================================================================================';
PRINT 'CONFIGURACIÓN FINALIZADA CON ÉXITO: IRL DESHABILITADO Y SUBMÓDULO LIQUIDEZ ACTIVADO';
PRINT '=====================================================================================';
GO
