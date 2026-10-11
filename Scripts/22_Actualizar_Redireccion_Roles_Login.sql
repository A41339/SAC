-- ===============================================================================================
-- BANCO / ENTIDAD: FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)
-- PROYECTO: SISTEMA DE ADMINISTRACIÓN Y CALIFICACIÓN (SAC) - FASE I
-- SCRIPT: 22_Actualizar_Redireccion_Roles_Login.sql
-- REQUERIMIENTO: FFC-112 Punto 1 (Redirección inicial al Login)
--   - Si el usuario es de FFC (EsEntidad = 0) -> Redirigir a Perfiles (~/Cierre/Perfiles)
--   - Si el usuario es de CAC (EsEntidad = 1) -> Redirigir a Tablero (~/Home/Index)
-- ===============================================================================================

USE [FGA];
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Roles de FFC (EsEntidad = 0) redirigen a Perfiles
    UPDATE dbo.Role
    SET Url = '~/Cierre/Perfiles'
    WHERE EsEntidad = 0;

    -- 2. Roles de CAC / Cooperativa (EsEntidad = 1) redirigen a Tablero
    UPDATE dbo.Role
    SET Url = '~/Home/Index'
    WHERE EsEntidad = 1;

    COMMIT TRANSACTION;
    PRINT 'SUCCESS: Roles actualizados correctamente para redirección post-login.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;
GO

-- Verificación
SELECT Id, Nombre, EsEntidad, Url, Activo 
FROM dbo.Role
ORDER BY EsEntidad, Id;
GO
