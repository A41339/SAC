-- ===============================================================================================
-- BANCO / ENTIDAD: FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)
-- PROYECTO: SISTEMA DE ADMINISTRACIÓN Y CALIFICACIÓN (SAC) - FASE II
-- SCRIPT: 08_Mover_Roles_Y_MenuSistema_A_GestionOperativa.sql
-- DESCRIPCIÓN: 
--   1. Reubica "Roles" (Id 9145, Role/Index) y "Menú del Sistema" (Id 13001, Menu/Index)
--      bajo el sub-módulo "Gestión Operativa" (ParentId = 9200).
--   2. Asigna o asegura los permisos en dbo.MenuPermission para los roles administrativos FFC.
-- ===============================================================================================

-- NOTA: Este script fue consolidado en 10_Aplicar_Reestructuracion_Menu_FFC.sql.
USE [FFC];
GO

SET NOCOUNT ON;

PRINT '===============================================================================================';
PRINT 'INICIANDO SCRIPT: REUBICACIÓN DE ROLES Y MENÚ DEL SISTEMA A GESTIÓN OPERATIVA (9200)';
PRINT '===============================================================================================';

BEGIN TRANSACTION;

BEGIN TRY

    -- 1. Asegurar que existe el módulo padre "Gestión Operativa" (Id 9200)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9200)
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 9, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 9, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
        END
        PRINT '   -> Sub-módulo Id 9200 [Gestión Operativa] creado.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET Description = 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.'
        WHERE Id = 9200;
    END

    -- 2. Asegurar o actualizar "Roles" (Id 9145 / Role/Index) con ParentId = 9200
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
    BEGIN
        UPDATE dbo.Menu 
        SET ParentId = 9200, 
            SortOrder = 7, 
            MenuText = 'Roles', 
            MenuURL = 'Role/Index', 
            MenuIcon = '<i class="fa fa-key"></i>', 
            Description = 'Gestión de roles y configuración de la matriz de permisos de menú.'
        WHERE Id = 9145;
        PRINT '   -> Opción Id 9145 [Roles] actualizada con ParentId = 9200.';
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Role/Index', 'Role'))
    BEGIN
        UPDATE dbo.Menu 
        SET ParentId = 9200, 
            SortOrder = 7, 
            MenuText = 'Roles', 
            MenuIcon = '<i class="fa fa-key"></i>', 
            Description = 'Gestión de roles y configuración de la matriz de permisos de menú.'
        WHERE MenuURL IN ('Role/Index', 'Role');
        PRINT '   -> Opción [Roles] por URL actualizada con ParentId = 9200.';
    END
    ELSE
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
        END
        PRINT '   -> Opción Id 9145 [Roles] creada con ParentId = 9200.';
    END

    -- 3. Asegurar o actualizar "Menú del Sistema" (Id 13001 / Menu/Index) con ParentId = 9200
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
    BEGIN
        UPDATE dbo.Menu 
        SET ParentId = 9200, 
            SortOrder = 8, 
            MenuText = 'Menú del Sistema', 
            MenuURL = 'Menu/Index', 
            MenuIcon = '<i class="fa fa-sitemap"></i>', 
            Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.'
        WHERE Id = 13001;
        PRINT '   -> Opción Id 13001 [Menú del Sistema] actualizada con ParentId = 9200.';
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Menu/Index', 'Menu'))
    BEGIN
        UPDATE dbo.Menu 
        SET ParentId = 9200, 
            SortOrder = 8, 
            MenuText = 'Menú del Sistema', 
            MenuIcon = '<i class="fa fa-sitemap"></i>', 
            Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.'
        WHERE MenuURL IN ('Menu/Index', 'Menu');
        PRINT '   -> Opción [Menú del Sistema] por URL actualizada con ParentId = 9200.';
    END
    ELSE
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
        END
        PRINT '   -> Opción Id 13001 [Menú del Sistema] creada con ParentId = 9200.';
    END

    -- 4. Asignar permisos en dbo.MenuPermission para Roles (9145) y Menú del Sistema (13001) a los perfiles administrativos FFC
    DECLARE @RolId INT, @MenuRolId INT, @MenuSisId INT;
    SELECT TOP 1 @MenuRolId = Id FROM dbo.Menu WHERE MenuURL = 'Role/Index' OR Id = 9145;
    SELECT TOP 1 @MenuSisId = Id FROM dbo.Menu WHERE MenuURL = 'Menu/Index' OR Id = 13001;

    -- Roles FFC: Super Admin (1), Administrador (2), Auditor/Operador (7) o roles con EsEntidad = 0
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT @MenuRolId, r.Id, 7, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = @MenuRolId AND mp.RoleId = r.Id);

    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT @MenuSisId, r.Id, 8, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = @MenuSisId AND mp.RoleId = r.Id);

    -- Asegurar lectura y actualización activa
    UPDATE dbo.MenuPermission 
    SET IsRead = 1, IsUpdate = 1
    WHERE MenuId IN (@MenuRolId, @MenuSisId);

    PRINT '   -> Permisos asignados exitosamente a perfiles administrativos FFC.';

    COMMIT TRANSACTION;
    PRINT '===============================================================================================';
    PRINT 'SCRIPT EJECUTADO CON ÉXITO: Roles y Menú del Sistema ahora pertenecen a Gestión Operativa (9200)';
    PRINT '===============================================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '>>> ERROR AL EJECUTAR SCRIPT: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
