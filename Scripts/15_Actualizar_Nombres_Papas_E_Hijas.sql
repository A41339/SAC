-- ===========================================================================================
-- Script 15: Homologación de Nombres Oficiales: 'Gestión de Informes' y 'Estatus y Archivos Cargados'
-- Fecha: 2026-09-30
-- Descripción:
--   1. Renombra el módulo papá 11000 a 'Gestión de Informes' (bajo Administración 9000).
--   2. Renombra la opción 11100 a 'Carga y Emisión de Informes' (con sus 3 cejillas).
--   3. Mantiene 11110 como 'Consulta de Informes' (reportes emitidos).
--   4. Elimina de raíz cualquier opción huérfana de 'Tipos de Informe' (9140) y 'Notificaciones' (11120).
--   5. Elimina de raíz la opción redundante de 'Archivos Cargados' (7160).
--   6. Renombra 7100 a 'Estatus y Archivos Cargados' (con sus 2 cejillas).
--   7. Garantiza permisos de lectura y administración en dbo.MenuPermission.
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/4] Ajustando nombres y jerarquía en Módulo de Informes...';
    PRINT '================================================================================';

    -- 1.1 Renombrar papá 11000 a 'Gestión de Informes'
    UPDATE dbo.Menu
    SET MenuText = 'Gestión de Informes',
        MenuURL = 'filter',
        ParentId = 9000,
        SortOrder = 7,
        MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
        Description = 'Carga y emisión de informes, comunicados oficiales a entidades y consulta de documentos.'
    WHERE Id = 11000;

    -- 1.2 Renombrar hija 11100 a 'Carga y Emisión de Informes' (contiene las 3 cejillas)
    UPDATE dbo.Menu
    SET MenuText = 'Carga y Emisión de Informes',
        MenuURL = 'Explorer/Informe',
        ParentId = 11000,
        SortOrder = 1,
        MenuIcon = '<i class="fa fa-folder-open"></i>',
        Description = 'Carga de informes, notificaciones a entidades y catálogo de tipos de informe (unificados en cejillas).'
    WHERE Id = 11100;

    -- 1.3 Asegurar hija 11110 como 'Consulta de Informes'
    UPDATE dbo.Menu
    SET MenuText = 'Consulta de Informes',
        MenuURL = 'Explorer/Consulta',
        ParentId = 11000,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
        Description = 'Consulta y descarga de reportes oficiales emitidos por la entidad.'
    WHERE Id = 11110;

    -- 1.4 Eliminar opciones hijas huérfanas de Informes (Tipos de Informe y Notificaciones)
    DELETE FROM dbo.MenuPermission 
    WHERE MenuId IN (9140, 11120) 
       OR MenuId IN (SELECT Id FROM dbo.Menu WHERE MenuText IN ('Tipos de Informe', 'Tipos de Informes', 'Notificaciones'));

    DELETE FROM dbo.Menu 
    WHERE Id IN (9140, 11120) 
       OR MenuText IN ('Tipos de Informe', 'Tipos de Informes', 'Notificaciones');

    PRINT '   -> Informes estructurado con papá [Gestión de Informes] e hijas consolidadas.';


    PRINT '================================================================================';
    PRINT '>> [2/4] Ajustando nombres y jerarquía en Carga de Datos...';
    PRINT '================================================================================';

    -- 2.1 Asegurar papá 7000 como 'Carga de Datos' bajo Administración
    UPDATE dbo.Menu
    SET MenuText = 'Carga de Datos',
        MenuURL = 'filter',
        ParentId = 9000,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-upload"></i>',
        Description = 'Procesamiento de archivos XML, estatus de la carga y monitor de cierres.'
    WHERE Id = 7000;

    -- 2.2 Asegurar hija 7100 como 'Estatus y Archivos Cargados' (contiene las 2 cejillas)
    UPDATE dbo.Menu
    SET MenuText = 'Estatus y Archivos Cargados',
        MenuURL = 'Cierre/Index',
        ParentId = 7000,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-check-circle"></i>',
        Description = 'Estatus de la carga de cierres mensuales y detalle de archivos procesados (unificados en cejillas).'
    WHERE Id = 7100;

    -- 2.3 Eliminar la opción huérfana de 'Archivos Cargados' (7160)
    DELETE FROM dbo.MenuPermission 
    WHERE MenuId = 7160 
       OR MenuId IN (SELECT Id FROM dbo.Menu WHERE MenuText = 'Archivos Cargados' AND Id <> 7100);

    DELETE FROM dbo.Menu 
    WHERE Id = 7160 
       OR (MenuText = 'Archivos Cargados' AND Id <> 7100);

    PRINT '   -> Carga de Datos estructurado con [Estatus y Archivos Cargados] unificado.';


    PRINT '================================================================================';
    PRINT '>> [3/4] Garantizando permisos en dbo.MenuPermission...';
    PRINT '================================================================================';

    -- Insertar permisos faltantes para el Administrador (RoleId = 1)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, 1, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.MenuPermission mp 
        WHERE mp.MenuId = m.Id AND mp.RoleId = 1
    );

    -- Insertar permisos para roles internos de gestión (RoleId IN (1, 2, 7) o EsEntidad = 0)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    -- Limpiar permisos huérfanos
    DELETE FROM dbo.MenuPermission
    WHERE MenuId NOT IN (SELECT Id FROM dbo.Menu);

    PRINT '   -> Permisos sincronizados correctamente.';


    PRINT '================================================================================';
    PRINT '>> [4/4] Verificación del árbol de opciones resultante...';
    PRINT '================================================================================';

    COMMIT TRANSACTION;

    SELECT 
        m.Id,
        m.MenuText,
        m.MenuURL,
        m.ParentId,
        p.MenuText AS PapaMenuText,
        m.SortOrder
    FROM dbo.Menu m
    LEFT JOIN dbo.Menu p ON m.ParentId = p.Id
    WHERE m.Id IN (9000, 11000, 11100, 11110, 7000, 7100, 7110, 7150)
       OR m.ParentId IN (9000, 11000, 7000)
    ORDER BY ISNULL(m.ParentId, m.Id), m.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '!!! ERROR EN SCRIPT 15: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
