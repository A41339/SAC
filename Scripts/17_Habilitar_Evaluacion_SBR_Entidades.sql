-- ===========================================================================================
-- Script 17: Habilitar Evaluación SBR para Rol de Entidades con Pantalla de Encuesta
-- Fecha: 2026-09-30
-- Descripción:
--   1. Configura el Menú 6000 (Evaluación SBR) como raíz (ParentId = NULL, SortOrder = 7)
--      apuntando directamente a 'Evaluacion/Evaluacion' con icono fa-pie-chart.
--   2. Configura el Menú 6001 (Autoevaluación) bajo el 6000 apuntando a 'Evaluacion/Evaluacion'.
--   3. Asigna permisos completos (IsRead=1, IsCreate=1, IsUpdate=1) en dbo.MenuPermission
--      a TODOS los roles de entidad (EsEntidad = 1, RoleId 4, 5, 7, 10, 12) y roles admin (1, 2, 6).
--   4. Habilita Ind_Evaluacion = 1 en dbo.Entidad para todas las cooperativas.
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '>> [1/4] Configurando Menu 6000 (Evaluación SBR)...';
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6000)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6000, 'Evaluación SBR', 'Evaluacion/Evaluacion', NULL, 7, '<i class="fa fa-pie-chart"></i>', 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR SUGEF 24-22).');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6000, 'Evaluación SBR', 'Evaluacion/Evaluacion', NULL, 7, '<i class="fa fa-pie-chart"></i>', 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR SUGEF 24-22).');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación SBR',
            MenuURL = 'Evaluacion/Evaluacion',
            ParentId = NULL,
            SortOrder = 7,
            MenuIcon = '<i class="fa fa-pie-chart"></i>',
            Description = 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR SUGEF 24-22).'
        WHERE Id = 6000;
    END

    PRINT '>> [2/4] Configurando Menu 6001 (Autoevaluación)...';
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6001)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 6000, 1, '<i class="fa fa-pencil-square-o"></i>', 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR).');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 6000, 1, '<i class="fa fa-pencil-square-o"></i>', 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR).');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Autoevaluación',
            MenuURL = 'Evaluacion/Evaluacion',
            ParentId = 6000,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-pencil-square-o"></i>',
            Description = 'Cuestionario de autoevaluación de supervisión basada en riesgos (SBR).'
        WHERE Id = 6001;
    END

    PRINT '>> [3/4] Asignando permisos en dbo.MenuPermission para roles de entidades y admin...';
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 1 OR r.Id IN (1, 2, 4, 5, 6, 7, 10, 12))
      AND m.Id IN (6000, 6001)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1
    WHERE MenuId IN (6000, 6001) AND (RoleId IN (1, 2, 4, 5, 6, 7, 10, 12) OR RoleId IN (SELECT Id FROM dbo.Role WHERE EsEntidad = 1));

    PRINT '>> [4/4] Habilitando Ind_Evaluacion en todas las entidades...';
    UPDATE dbo.Entidad SET Ind_Evaluacion = 1 WHERE Id <> '0';

    COMMIT TRANSACTION;
    PRINT '>> EVALUACION SBR ACTUALIZADA EXITOSAMENTE EN BASE DE DATOS';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@errMsg, 16, 1);
END CATCH;
