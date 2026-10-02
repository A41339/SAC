-- ===========================================================================================
-- Script 18: Configuración Oficial de Evaluación SBR en dbo.Menu y dbo.MenuPermission
-- Fecha: 2026-09-30
-- Descripción:
--   1. Registra o actualiza el menú raíz "Evaluación SBR" (Id: 9300) con ParentId = NULL,
--      icono fa-pie-chart y SortOrder = 4 (al nivel de Tablero, Cartera, Administración).
--   2. Registra o actualiza la opción hija "Autoevaluación" (Id: 6001) con ParentId = 9300,
--      apuntando al cuestionario 'Evaluacion/Evaluacion' e icono fa-clipboard.
--   3. Desvincula o limpia submódulos legacy (6002, 6003, 6005, 9135) para que únicamente
--      exista la opción de Autoevaluación (el cuestionario) y no el plan de mitigación.
--   4. Asigna permisos completos (IsRead=1, IsCreate=1, IsUpdate=1, IsDelete=1) en
--      dbo.MenuPermission para TODOS los roles del sistema (administradores y entidades).
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '>> [1/4] Configurando Menú Raíz 9300: Evaluación SBR...';
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9300)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR', 'root', NULL, 4, '<i class="fa fa-pie-chart"></i>', 'Autoevaluación de los aspectos del Reglamento SUGEF 24-22');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR', 'root', NULL, 4, '<i class="fa fa-pie-chart"></i>', 'Autoevaluación de los aspectos del Reglamento SUGEF 24-22');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación SBR',
            MenuURL = 'root',
            ParentId = NULL,
            SortOrder = 4,
            MenuIcon = '<i class="fa fa-pie-chart"></i>',
            Description = 'Autoevaluación de los aspectos del Reglamento SUGEF 24-22'
        WHERE Id = 9300;
    END

    PRINT '>> [2/4] Configurando Opción Hija 6001: Autoevaluación (Cuestionario)...';
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6001)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 9300, 1, '<i class="fa fa-clipboard"></i>', 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 9300, 1, '<i class="fa fa-clipboard"></i>', 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Autoevaluación',
            MenuURL = 'Evaluacion/Evaluacion',
            ParentId = 9300,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-clipboard"></i>',
            Description = 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).'
        WHERE Id = 6001;
    END

    -- Asegurar que cualquier menú 6000 previo no quede en conflicto
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6000)
    BEGIN
        UPDATE dbo.Menu SET ParentId = 9300 WHERE Id = 6000 AND Id <> 9300;
    END

    PRINT '>> [3/4] Asignando permisos en dbo.MenuPermission para TODOS los roles...';
    -- Insertar permisos si no existen para cada rol
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (9300, 6001)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    -- Habilitar permisos de lectura y acceso activo para todos los roles
    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1
    WHERE MenuId IN (9300, 6001);

    PRINT '>> [4/4] Habilitando Ind_Evaluacion en todas las entidades...';
    UPDATE dbo.Entidad SET Ind_Evaluacion = 1 WHERE Id <> '0';

    COMMIT TRANSACTION;
    PRINT '>> PROCESO COMPLETADO EXITOSAMENTE: Menú 9300 y 6001 configurados y asignados a todos los roles.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@errMsg, 16, 1);
END CATCH;
