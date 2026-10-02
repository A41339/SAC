-- ===================================================================================================
-- SCRIPT: 04_Eliminar_Opciones_Menu_Sin_Hijas.sql
-- Base de Datos: FFC (o FGA)
-- Objetivo:
--   1. Eliminar los contenedores obsoletos que quedaron sin hijas tras la reestructuración del menú:
--        - Seguridad (Id 8000 - antiguo módulo raíz vacío)
--        - Informes (Id 11000 - antiguo módulo raíz vacío)
--        - Carga de Datos (Id 7000 - si no tiene hijas)
--        - Cálculos (Id 6500 - si no tiene hijas)
--        - Cualquier otro contenedor (MenuURL 'root', 'filter', etc.) que no tenga hijas
--   2. Eliminar previamente sus registros en dbo.MenuPermission para mantener integridad referencial.
--   3. Reordenar alfabéticamente (SortOrder = 1, 2, 3...) todas las opciones hijas dentro de cada módulo padre.
-- ===================================================================================================

USE [FFC];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '>> [1/3] Identificando y eliminando opciones de menú que quedaron sin hijas (Seguridad, Informes, etc.)...';

    -- Tabla temporal con los Ids a eliminar
    DECLARE @MenusAEliminar TABLE (
        Id INT,
        MenuText NVARCHAR(100),
        ParentId INT,
        MenuURL NVARCHAR(400)
    );

    INSERT INTO @MenusAEliminar (Id, MenuText, ParentId, MenuURL)
    SELECT m.Id, m.MenuText, m.ParentId, m.MenuURL
    FROM dbo.Menu m
    WHERE (
            -- Ids específicos de contenedores obsoletos conocidos
            m.Id IN (6500, 7000, 8000, 11000)
            -- O cualquier contenedor sin URL ejecutable que no sea un módulo raíz maestro
            OR (
                (m.MenuURL IS NULL OR m.MenuURL IN ('root', 'filter', '#', ''))
                AND m.Id NOT IN (1000, 1500, 4000, 6000, 9000) -- Proteger los 5 módulos raíz principales
                AND m.Id NOT IN (2100, 3000, 2250)             -- Proteger submódulos válidos de Estructura Financiera
            )
          )
      -- Condición indispensable: NO debe tener hijas asociadas
      AND NOT EXISTS (SELECT 1 FROM dbo.Menu h WHERE h.ParentId = m.Id);

    -- Mostrar en consola los menús detectados para eliminación
    PRINT '>> Opciones detectadas para eliminación:';
    SELECT Id, MenuText, ParentId, MenuURL FROM @MenusAEliminar;

    -- 1. Eliminar permisos en dbo.MenuPermission asociados a esas opciones
    DELETE mp
    FROM dbo.MenuPermission mp
    INNER JOIN @MenusAEliminar e ON mp.MenuId = e.Id;

    PRINT '>> Permisos de MenuPermission eliminados con éxito.';

    -- 2. Eliminar registros en dbo.Menu
    DELETE m
    FROM dbo.Menu m
    INNER JOIN @MenusAEliminar e ON m.Id = e.Id;

    PRINT '>> Registros eliminados de dbo.Menu con éxito.';

    -- ===============================================================================================
    -- 3. REORDENAR ALFABÉTICAMENTE LOS HIJOS DENTRO DE CADA PADRE (SortOrder = 1, 2, 3...)
    -- ===============================================================================================
    PRINT '>> [2/3] Reordenando alfabéticamente las opciones hijas de cada módulo padre...';

    WITH CTE_OrdenAlfabetico AS (
        SELECT Id,
               ParentId,
               MenuText,
               ROW_NUMBER() OVER (PARTITION BY ParentId ORDER BY MenuText ASC) AS NuevoOrden
        FROM dbo.Menu
        WHERE ParentId IS NOT NULL
    )
    UPDATE m
    SET m.SortOrder = c.NuevoOrden
    FROM dbo.Menu m
    INNER JOIN CTE_OrdenAlfabetico c ON m.Id = c.Id;

    PRINT '>> Orden alfabético recalculado.';

    PRINT '>> [3/3] Confirmando transacción...';
    COMMIT TRANSACTION;

    PRINT '===================================================================================================';
    PRINT '>> PROCESO COMPLETADO SATISFACTORIAMENTE.';
    PRINT '===================================================================================================';

    -- Comprobación: Opciones finales bajo Administración (ParentId = 9000)
    SELECT h.Id,
           h.MenuText AS [Opcion_Administracion],
           h.SortOrder,
           h.MenuURL,
           h.Description
    FROM dbo.Menu h
    WHERE h.ParentId = 9000
    ORDER BY h.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '>> ERROR EN EJECUCIÓN: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
