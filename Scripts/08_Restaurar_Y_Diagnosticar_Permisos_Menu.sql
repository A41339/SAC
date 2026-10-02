-- ===============================================================================================
-- SCRIPT DE DIAGNÓSTICO Y RESTAURACIÓN DE PERMISOS / MENÚS
-- FGA - Fondo de Fortalecimiento Cooperativo (SAC)
-- Archivo: Scripts/08_Restaurar_Y_Diagnosticar_Permisos_Menu.sql
-- ===============================================================================================

USE [FGA];
GO

SET NOCOUNT ON;

PRINT '===============================================================================================';
PRINT '1. DIAGNÓSTICO ACTUAL: MENÚS RAÍZ Y PERMISOS POR ROL';
PRINT '===============================================================================================';

-- 1.1 Menús Raíz existentes
PRINT '--> Menús Raíz (ParentId IS NULL):';
SELECT Id, MenuText, MenuURL, SortOrder, MenuIcon 
FROM dbo.Menu 
WHERE ParentId IS NULL 
ORDER BY SortOrder, Id;

-- 1.2 Menús asignados por rol
PRINT '--> Conteo de menús permitidos por cada rol:';
SELECT mp.RoleId, ISNULL(r.Nombre, 'Sin Nombre') AS NombreRol, COUNT(*) AS TotalOpcionesPermitidas
FROM dbo.MenuPermission mp
LEFT JOIN dbo.Role r ON mp.RoleId = r.Id
GROUP BY mp.RoleId, r.Nombre
ORDER BY mp.RoleId;

PRINT '===============================================================================================';
PRINT '2. REPARACIÓN Y HABILITACIÓN DE PERMISOS PARA TODOS LOS ROLES ACTIVOS';
PRINT '===============================================================================================';

BEGIN TRANSACTION;
BEGIN TRY

    -- 2.1 Asegurar que el menú raíz Administración (Id 9000) esté visible para TODOS los roles existentes
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 9000, r.Id, 8, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 9000 AND mp.RoleId = r.Id
    );

    -- 2.2 Asignar a todos los roles administrativos (FFC / Administradores) acceso a TODAS las opciones hijas de Administración
    -- (Aplica para todos los roles que no sean entidades/cooperativas)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (m.ParentId = 9000 OR m.Id = 9000)
      AND (r.EsEntidad = 0 OR r.Id IN (1, 2, 7)) -- Roles FFC / Administradores
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    -- 2.3 Asegurar que los roles de Cooperativas (EsEntidad = 1 o RoleId = 10) tengan permiso a sus 5 opciones clave:
    -- 8100 (Usuarios SAC), 8110 (Contraseña), 7110 (Carga de datos), 6502 (Contribuciones), 11110 (Informes)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 0
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (8100, 8110, 7110, 6502, 11110)
      AND (r.EsEntidad = 1 OR r.Id = 10)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    -- Asegurar que IsRead = 1 para que sean visibles
    UPDATE dbo.MenuPermission
    SET IsRead = 1
    WHERE MenuId IN (9000, 8100, 8110, 7110, 6502, 11110);

    COMMIT TRANSACTION;
    PRINT '-> Permisos normalizados con éxito para todos los roles.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;
GO
