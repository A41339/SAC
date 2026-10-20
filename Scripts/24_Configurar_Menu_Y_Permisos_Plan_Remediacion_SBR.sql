-- ===========================================================================================
-- Script 24: Configuración Dinámica del Menú y Permisos de Rol para Plan de Remediación SBR
-- Fecha: 2026-10-20
-- Descripción:
--   1. Registra o actualiza el parámetro configurable 'Ruta_Planes_Remediacion' en dbo.Parametros.
--   2. Registra o actualiza la opción de menú 'Planes de Remediación SBR' (Id: 6006) en dbo.Menu
--      bajo el sub-módulo de Administración / Evaluación SBR (ParentId: 9300), apuntando a
--      'Evaluacion/AdminPlanRemediacion' con icono fa-file-pdf-o y SortOrder = 4.
--   3. Asigna permisos dinámicos en dbo.MenuPermission para los roles con perfil administrativo / FFC
--      (roles con acceso a Administración Id: 9000, e.g. RoleId = 1, 6, 10).
--   4. No otorga acceso a roles exclusivos de entidades (ej. 4, 5) para preservar la exclusividad
--      de la pantalla de carga para el FFC.
--   5. Idempotente y transaccional con manejo de errores (BEGIN TRY / CATCH).
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/3] Configurando Parámetro [Ruta_Planes_Remediacion] en dbo.Parametros...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Llave = 'Ruta_Planes_Remediacion')
    BEGIN
        INSERT INTO dbo.Parametros (Llave, Valor, Descripcion)
        VALUES ('Ruta_Planes_Remediacion', '~/Uploads/PlanesRemediacion', 'Ruta física o virtual donde se almacenan los documentos PDF de Planes de Remediación SBR.');
        PRINT '   -> Parámetro [Ruta_Planes_Remediacion] creado exitosamente.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Parametros
        SET Descripcion = 'Ruta física o virtual donde se almacenan los documentos PDF de Planes de Remediación SBR.'
        WHERE Llave = 'Ruta_Planes_Remediacion';
        PRINT '   -> Parámetro [Ruta_Planes_Remediacion] ya existía y fue verificado.';
    END


    PRINT '================================================================================';
    PRINT '>> [2/3] Configurando Opción de Menú 6006: Planes de Remediación SBR...';
    PRINT '================================================================================';

    -- Determinar el ParentId adecuado (Sub-módulo 9300 de Evaluación SBR o 9000 Administración)
    DECLARE @ParentIdMenu INT = 9300;
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9300)
    BEGIN
        SET @ParentIdMenu = 9000; -- Fallback directo a Administración si 9300 no existe
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6006)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6006, 'Planes de Remediación SBR', 'Evaluacion/AdminPlanRemediacion', @ParentIdMenu, 4, '<i class="fa fa-file-pdf-o"></i>', 'Gestión y carga de planes de remediación oficiales FFC para entidades supervisadas bajo normativa SUGEF 24-22.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6006, 'Planes de Remediación SBR', 'Evaluacion/AdminPlanRemediacion', @ParentIdMenu, 4, '<i class="fa fa-file-pdf-o"></i>', 'Gestión y carga de planes de remediación oficiales FFC para entidades supervisadas bajo normativa SUGEF 24-22.');
        END
        PRINT '   -> Menú 6006 [Planes de Remediación SBR] insertado correctamente.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Planes de Remediación SBR',
            MenuURL = 'Evaluacion/AdminPlanRemediacion',
            ParentId = @ParentIdMenu,
            SortOrder = 4,
            MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
            Description = 'Gestión y carga de planes de remediación oficiales FFC para entidades supervisadas bajo normativa SUGEF 24-22.'
        WHERE Id = 6006;
        PRINT '   -> Menú 6006 [Planes de Remediación SBR] actualizado correctamente.';
    END


    PRINT '================================================================================';
    PRINT '>> [3/3] Asignando Permisos en dbo.MenuPermission para Roles de Administración FFC...';
    PRINT '================================================================================';

    -- Insertar permisos para todos los roles con acceso a Administración (MenuId = 9000 con IsRead = 1)
    -- Esto garantiza asignación 100% dinámica sin quemar IDs de rol fijos
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 6006, mpAdmin.RoleId, 4, 1, 1, 1, 1
    FROM (
        SELECT DISTINCT RoleId 
        FROM dbo.MenuPermission 
        WHERE (MenuId = 9000 OR MenuId = 9300) AND IsRead = 1
    ) mpAdmin
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 6006 AND mp.RoleId = mpAdmin.RoleId
    );

    -- Asegurar permisos activos de lectura, creación y actualización
    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1, IsDelete = 1
    WHERE MenuId = 6006
      AND RoleId IN (
          SELECT DISTINCT RoleId FROM dbo.MenuPermission WHERE (MenuId = 9000 OR MenuId = 9300) AND IsRead = 1
      );

    PRINT '   -> Permisos asignados exitosamente a los roles administrativos.';

    COMMIT TRANSACTION;
    PRINT '================================================================================';
    PRINT '>> PROCESO COMPLETADO EXITOSAMENTE: Menú, permisos y parámetro configurados.';
    PRINT '================================================================================';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@errMsg, 16, 1);
END CATCH;
