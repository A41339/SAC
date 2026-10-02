-- ===========================================================================================
-- Script 13: Simplificación del Menú de Informes y Carga de Datos (Fase II)
-- Fecha: 2026-09-30
-- Descripción:
--   1. Unifica bajo 'Informes' (11000) la opción 'Gestión de Informes' (11100)
--      que integra en cejillas: Carga de Informes, Notificaciones y Tipos de Informes.
--   2. Retira del menú las opciones individuales ahora integradas:
--      - Id 9140 ('Tipos de Informe')
--      - Id 11120 ('Notificaciones')
--   3. Unifica en 'Carga de Datos' (7000) la opción 'Estatus y Archivos Cargados' (7100)
--      y retira el acceso redundante Id 7160 ('Archivos Cargados').
--   4. Depura los permisos huérfanos en dbo.MenuPermission.
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/4] Unificando y renombrando opciones en Menú de Informes (11000)...';
    PRINT '================================================================================';

    -- Renombrar 11100 como 'Gestión de Informes' (Carga, Notificaciones y Tipos)
    UPDATE dbo.Menu
    SET MenuText = 'Gestión de Informes',
        MenuURL = 'Explorer/Informe',
        ParentId = 11000,
        SortOrder = 1,
        MenuIcon = '<i class="fa fa-folder-open"></i>',
        Description = 'Carga de informes, comunicados/notificaciones y administración de tipos de informe.'
    WHERE Id = 11100;

    -- Mantener la Consulta/Descarga de Informes como segunda opción si existe (11110)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Consulta de Informes',
            MenuURL = 'Explorer/Consulta',
            ParentId = 11000,
            SortOrder = 2,
            MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
            Description = 'Consulta y descarga de informes generados.'
        WHERE Id = 11110;
    END

    -- Retirar del menú las opciones ahora integradas como pestañas (9140 y 11120)
    DELETE FROM dbo.MenuPermission WHERE MenuId IN (9140, 11120);
    DELETE FROM dbo.Menu WHERE Id IN (9140, 11120);

    PRINT '   -> Informes unificado: 11100 actualizado, 9140 y 11120 retirados del menú.';


    PRINT '================================================================================';
    PRINT '>> [2/4] Unificando y renombrando opciones en Carga de Datos (7000)...';
    PRINT '================================================================================';

    -- Renombrar 7100 a 'Estatus y Archivos Cargados'
    UPDATE dbo.Menu
    SET MenuText = 'Estatus y Archivos Cargados',
        MenuURL = 'Cierre/Index',
        ParentId = 7000,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-check-circle"></i>',
        Description = 'Estatus de la carga de cierres mensuales y detalle de archivos procesados.'
    WHERE Id = 7100;

    -- Retirar la opción redundante 7160 ('Archivos Cargados')
    DELETE FROM dbo.MenuPermission WHERE MenuId = 7160;
    DELETE FROM dbo.Menu WHERE Id = 7160;

    PRINT '   -> Carga de Datos unificada: 7100 actualizado, 7160 retirado del menú.';


    PRINT '================================================================================';
    PRINT '>> [3/4] Garantizar permisos del Rol Administrador en opciones unificadas...';
    PRINT '================================================================================';

    -- Garantizar que los roles que tenían acceso a 11100 o 7100 conserven su permiso
    IF NOT EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 11100 AND RoleId = 1)
    BEGIN
        INSERT INTO dbo.MenuPermission (MenuId, RoleId) VALUES (11100, 1);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE MenuId = 7100 AND RoleId = 1)
    BEGIN
        INSERT INTO dbo.MenuPermission (MenuId, RoleId) VALUES (7100, 1);
    END

    PRINT '   -> Permisos de administración verificados.';


    PRINT '================================================================================';
    PRINT '>> [4/4] Limpieza de permisos huérfanos...';
    PRINT '================================================================================';

    DELETE FROM dbo.MenuPermission
    WHERE MenuId NOT IN (SELECT Id FROM dbo.Menu);

    PRINT '   -> Permisos huérfanos eliminados correctamente.';

    COMMIT TRANSACTION;
    PRINT '================================================================================';
    PRINT '>> ÉXITO: Menú simplificado y unificado satisfactoriamente.';
    PRINT '================================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR AL APLICAR REESTRUCTURACIÓN: ' + ERROR_MESSAGE();
END CATCH;
