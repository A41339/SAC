-- ===============================================================================================
-- Script: UpdateMenuIndicadores.sql
-- Propósito:
-- 1. Renombrar SUGEF (Id: 3100) a 'Indicadores' (permanece en Estructura Financiera / 3000).
-- 2. Renombrar FFC (Id: 3110) a 'Indicadores' y trasladarlo a Cartera de Crédito (ParentId: 4000),
--    asegurando que NO figure en los indicadores de estructura de fondeo / financiera (3000).
-- ===============================================================================================

-- 1. SUGEF -> 'Indicadores' en Indicadores Financieros (3000) de Estructura Financiera
UPDATE dbo.Menu
SET MenuText = N'Indicadores',
    MenuURL = N'Indicadores/Sugef',
    ParentId = 3000,
    SortOrder = 1
WHERE Id = 3100;

-- 2. FFC -> 'Indicadores' trasladado exclusivamente a Cartera de Crédito (4000)
UPDATE dbo.Menu
SET MenuText = N'Indicadores',
    MenuURL = N'Indicadores/FGA',
    ParentId = 4000,
    SortOrder = 2
WHERE Id = 3110;

-- 3. Actualizar URLs en la tabla de permisos
UPDATE dbo.MenuPermission
SET MenuUrl = N'Indicadores/Sugef'
WHERE MenuId = 3100;

UPDATE dbo.MenuPermission
SET MenuUrl = N'Indicadores/FGA'
WHERE MenuId = 3110;

-- 4. Asegurar que roles con permiso en Cartera de Crédito (4000) tengan permiso en Indicadores (3110)
INSERT INTO dbo.MenuPermission (MenuId, RoleId, MenuUrl)
SELECT DISTINCT 3110, p.RoleId, N'Indicadores/FGA'
FROM dbo.MenuPermission p
WHERE p.MenuId = 4000
  AND NOT EXISTS (
      SELECT 1 FROM dbo.MenuPermission p2 WHERE p2.MenuId = 3110 AND p2.RoleId = p.RoleId
  );
