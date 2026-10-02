-- ===========================================================================================
-- Script 17: Establecer MenuURL = 'root' en Módulo de Administración (Id: 9000)
-- Fecha: 2026-09-30
-- Descripción:
--   Asegura que el módulo raíz 'Administración' (Id: 9000) y sus contenedores administrativos
--   tengan MenuURL = 'root' (para no desplegar la barra de filtros de entidad/período).
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> Actualizando MenuURL a ''root'' para el módulo Administración (Id: 9000)...';
    PRINT '================================================================================';

    -- Actualizar Administración (9000) a 'root'
    UPDATE dbo.Menu
    SET MenuURL = 'root'
    WHERE Id = 9000 
       OR (ParentId IS NULL AND MenuText LIKE '%Administra%');

    -- Actualizar contenedores puramente administrativos a 'root' si estuviesen en 'filter'
    UPDATE dbo.Menu
    SET MenuURL = 'root'
    WHERE Id IN (9200, 11000)
      AND MenuURL = 'filter';

    COMMIT TRANSACTION;

    PRINT '>> MenuURL actualizado exitosamente a ''root''.';
    PRINT '';
    PRINT '>> COMPROBACIÓN: Registro de Administración en dbo.Menu:';

    SELECT Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description
    FROM dbo.Menu
    WHERE Id IN (9000, 9200, 11000)
       OR (ParentId IS NULL AND MenuText LIKE '%Administra%')
    ORDER BY Id;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO
