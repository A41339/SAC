-- ===========================================================================================
-- Script 16: Asignación de Permisos de Gestión e Informes para Rol 4 (Usuario Carga)
-- Fecha: 2026-09-30
-- Descripción:
--   Garantiza que el Rol 4 (Usuario Carga / Entidad) tenga permisos de acceso y consulta
--   a la opción 'Consulta de Informes' (MenuId 11110), 'Carga de Informes' (MenuId 11100)
--   y su menú contenedor (MenuId 11000).
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> Asignando permisos de Gestión e Informes al Rol 4 (Usuario Carga)...';
    PRINT '================================================================================';

    -- 1. Asegurar permiso para el módulo padre 11000 (Gestión de Informes / Informes)
    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 11000 AND RoleId = 4)
    BEGIN
        UPDATE dbo.MenuPermission 
        SET IsRead = 1, IsCreate = 1, IsUpdate = 1, IsDelete = 1 
        WHERE MenuId = 11000 AND RoleId = 4;
        PRINT '   -> Permiso actualizado en módulo padre 11000 para Rol 4.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (11000, 4, 7, 1, 1, 1, 1);
        PRINT '   -> Permiso insertado en módulo padre 11000 para Rol 4.';
    END

    -- 2. Asegurar permiso para la opción hija 11100 (Explorer/Informe - Carga y Emisión de Informes)
    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 11100 AND RoleId = 4)
    BEGIN
        UPDATE dbo.MenuPermission 
        SET IsRead = 1, IsCreate = 1, IsUpdate = 1, IsDelete = 1 
        WHERE MenuId = 11100 AND RoleId = 4;
        PRINT '   -> Permiso actualizado en opción hija 11100 (Explorer/Informe) para Rol 4.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (11100, 4, 1, 1, 1, 1, 1);
        PRINT '   -> Permiso insertado en opción hija 11100 (Explorer/Informe) para Rol 4.';
    END

    -- 3. Asegurar permiso para la opción hija 11110 (Explorer/Consulta - Consulta de Informes)
    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 11110 AND RoleId = 4)
    BEGIN
        UPDATE dbo.MenuPermission 
        SET IsRead = 1, IsCreate = 1, IsUpdate = 1, IsDelete = 1 
        WHERE MenuId = 11110 AND RoleId = 4;
        PRINT '   -> Permiso actualizado en opción hija 11110 (Explorer/Consulta) para Rol 4.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (11110, 4, 2, 1, 1, 1, 1);
        PRINT '   -> Permiso insertado en opción hija 11110 (Explorer/Consulta) para Rol 4.';
    END

    -- 4. También garantizar para otros roles de entidad si los hubiera (RoleId 10, 12 o EsEntidad = 1)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT mId, r.Id, 2, 1, 1, 1, 1
    FROM (VALUES (11000), (11100), (11110)) AS M(mId)
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 1 OR r.Id IN (4, 10, 12))
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = M.mId AND mp.RoleId = r.Id
      );

    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1, IsDelete = 1
    WHERE MenuId IN (11000, 11100, 11110) AND RoleId IN (4, 10, 12);

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '================================================================================';
    PRINT '>> COMPROBACIÓN: Permisos configurados para Módulo de Informes (11000, 11100, 11110):';
    PRINT '================================================================================';

    SELECT 
        mp.Id,
        mp.MenuId,
        m.MenuText,
        m.MenuURL,
        mp.RoleId,
        r.Nombre AS RolNombre,
        mp.IsRead,
        mp.IsCreate,
        mp.IsUpdate,
        mp.IsDelete
    FROM dbo.MenuPermission mp
    JOIN dbo.Menu m ON mp.MenuId = m.Id
    JOIN dbo.Role r ON mp.RoleId = r.Id
    WHERE mp.MenuId IN (11000, 11100, 11110)
    ORDER BY mp.MenuId, mp.RoleId;

    PRINT '>> Operación completada con éxito.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO
