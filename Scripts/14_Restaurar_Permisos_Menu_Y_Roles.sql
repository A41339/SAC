-- ===========================================================================================
-- Script 14: Restaurar Permisos de Menú del Sistema, Roles y Submódulos FFC
-- Fecha: 2026-09-30
-- Descripción:
--   1. Asegura que 'Roles' (9145) y 'Menú del Sistema' (13001) estén registrados correctamente
--      bajo 'Gestión Operativa' (9200).
--   2. Asigna permisos completos (IsCreate, IsRead, IsUpdate, IsDelete = 1) en dbo.MenuPermission
--      al Rol Administrador (RoleId = 1) para TODAS las opciones activas de dbo.Menu,
--      incluyendo:
--        - 9145  (Roles)
--        - 13001 (Menú del Sistema)
--        - 9200  (Gestión Operativa)
--        - 9300  (Evaluación SBR - Avance y Resultados)
--        - 11000 (Informes Hub)
--        - 13002 (Estructura de Fondeo)
--        - 6501  (Simulación ISP)
--        - 6502  (Contribuciones)
--        - 4130  (Riesgo de Crédito 14-21)
--        - 6005  (Resultados SBR)
--        - 7100, 7110, 7120 (Carga de Datos y XML)
--   3. Asigna permisos a los roles de gestión interna FFC (RoleId IN (1, 2, 7) o EsEntidad = 0).
--   4. Garantiza permisos para Cooperativas (EsEntidad = 1 o RoleId IN (10, 12)) en sus opciones.
--   5. Elimina registros huérfanos de MenuPermission que apuntan a IDs inexistentes.
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/5] Verificando y asegurando opciones en dbo.Menu...';
    PRINT '================================================================================';

    -- 1.1 Asegurar Gestión Operativa (9200)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9200)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 5, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 5, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu 
        SET MenuText = 'Gestión Operativa',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 5,
            MenuIcon = '<i class="fa fa-cogs"></i>',
            Description = 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.'
        WHERE Id = 9200;
    END

    -- 1.2 Asegurar Roles (9145) bajo Gestión Operativa (9200)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-lock"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-lock"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu 
        SET MenuText = 'Roles',
            MenuURL = 'Role/Index',
            ParentId = 9200,
            SortOrder = 7,
            MenuIcon = '<i class="fa fa-lock"></i>',
            Description = 'Gestión de roles y configuración de la matriz de permisos de menú.'
        WHERE Id = 9145;
    END

    -- 1.3 Asegurar Menú del Sistema (13001) bajo Gestión Operativa (9200)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 5, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 5, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu 
        SET MenuText = 'Menú del Sistema',
            MenuURL = 'Menu/Index',
            ParentId = 9200,
            SortOrder = 5,
            MenuIcon = '<i class="fa fa-sitemap"></i>',
            Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.'
        WHERE Id = 13001;
    END

    PRINT '   -> Menú y Roles verificados en dbo.Menu.';


    PRINT '================================================================================';
    PRINT '>> [2/5] Asignando permisos completos a RoleId = 1 (Administrador)...';
    PRINT '================================================================================';

    -- Insertar permisos faltantes para el Administrador (RoleId = 1) en TODAS las opciones activas
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 
        m.Id, 
        1, 
        ISNULL(m.SortOrder, 1), 
        1, 1, 1, 1
    FROM dbo.Menu m
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.MenuPermission mp 
        WHERE mp.MenuId = m.Id AND mp.RoleId = 1
    );

    -- Asegurar que todos los permisos de RoleId = 1 estén activos en 1 (True)
    UPDATE dbo.MenuPermission
    SET IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
    WHERE RoleId = 1;

    PRINT '   -> Permisos de RoleId = 1 (Administrador) restaurados al 100%.';


    PRINT '================================================================================';
    PRINT '>> [3/5] Asignando permisos a roles administrativos internos FFC...';
    PRINT '================================================================================';

    -- Para roles administrativos internos (RoleId IN (1, 2, 7) o EsEntidad = 0)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 
        m.Id, 
        r.Id, 
        ISNULL(m.SortOrder, 1), 
        1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    PRINT '   -> Permisos asignados a roles administrativos internos.';


    PRINT '================================================================================';
    PRINT '>> [4/5] Garantizando permisos para usuarios de Cooperativas...';
    PRINT '================================================================================';

    -- Menús permitidos para Cooperativas (RoleId 10, 12 o EsEntidad = 1)
    -- Balance, Balanza, ER, Indicadores, Cartera, IRL, Usuarios SAC, Contraseña, Carga XML, Contribuciones, Consulta Informes
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 
        m.Id, 
        r.Id, 
        ISNULL(m.SortOrder, 1), 
        1, 1, 1, 0
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 1 OR r.Id IN (10, 12))
      AND m.Id IN (
          1000, 1500, 2100, 2110, 2120, 2130, 2135, 2140, 
          2250, 2260, 3000, 3100, 3110, 3120, 3130, 4000, 
          4100, 4110, 5000, 5100, 5110, 5120, 5130, 6501, 
          6502, 7000, 7110, 8100, 8110, 9000, 11000, 11110, 13002
      )
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    PRINT '   -> Permisos de cooperativas sincronizados.';


    PRINT '================================================================================';
    PRINT '>> [5/5] Limpiando permisos huérfanos...';
    PRINT '================================================================================';

    DELETE FROM dbo.MenuPermission
    WHERE MenuId NOT IN (SELECT Id FROM dbo.Menu);

    PRINT '   -> Registros huérfanos depurados.';

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '================================================================================';
    PRINT '>> COMPROBACION: Permisos configurados para Gestion Operativa (9200) y Roles:';
    PRINT '================================================================================';

    SELECT 
        m.Id AS MenuId,
        m.MenuText,
        m.MenuURL,
        m.ParentId,
        mp.RoleId,
        r.Nombre AS RoleNombre,
        mp.IsRead,
        mp.IsCreate,
        mp.IsUpdate,
        mp.IsDelete
    FROM dbo.Menu m
    INNER JOIN dbo.MenuPermission mp ON m.Id = mp.MenuId
    INNER JOIN dbo.Role r ON mp.RoleId = r.Id
    WHERE m.Id IN (9200, 9145, 13001, 9050, 9100, 9110, 9120, 9130, 9150)
      AND mp.RoleId = 1
    ORDER BY m.ParentId, m.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '!!! ERROR EN SCRIPT 14: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
