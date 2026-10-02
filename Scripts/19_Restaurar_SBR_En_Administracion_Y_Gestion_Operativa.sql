-- ===========================================================================================
-- Script 19: Reubicación Oficial de SBR:
--   - Menú Raíz: "Evaluación SBR" (Id: 6000) con "Autoevaluación" (Id: 6001) para todos los roles.
--   - En Administración (Id: 9000): Sub-módulo "Evaluación SBR" (Id: 9300) con:
--       * Avance SBR (Id: 6002)
--       * Historial SBR (Id: 6003)
--       * Resultados SBR (Id: 6005)
--   - En Gestión Operativa (Id: 9200):
--       * Configuración de Preguntas SBR (Id: 9135)
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> [1/5] Configurando Menú Raíz [Evaluación SBR] (Id: 6000) y Autoevaluación (6001)...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6000)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6000, 'Evaluación SBR', 'root', NULL, 4, '<i class="fa fa-pie-chart"></i>', 'Autoevaluación de los aspectos del Reglamento SUGEF 24-22');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6000, 'Evaluación SBR', 'root', NULL, 4, '<i class="fa fa-pie-chart"></i>', 'Autoevaluación de los aspectos del Reglamento SUGEF 24-22');
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
        WHERE Id = 6000;
    END

    -- 6001: Autoevaluación
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6001)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 6000, 1, '<i class="fa fa-clipboard"></i>', 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6001, 'Autoevaluación', 'Evaluacion/Evaluacion', 6000, 1, '<i class="fa fa-clipboard"></i>', 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Autoevaluación',
            MenuURL = 'Evaluacion/Evaluacion',
            ParentId = 6000,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-clipboard"></i>',
            Description = 'Evalúe los aspectos que establece el Reglamento SUGEF 24-22 para la Supervisión Basada en Riesgos (SBR).'
        WHERE Id = 6001;
    END

    PRINT '   -> Menú raíz 6000 e hija 6001 (Autoevaluación) listos.';


    PRINT '================================================================================';
    PRINT '>> [2/5] Configurando Sub-módulo [Evaluación SBR] (Id: 9300) bajo Administración (9000)...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9300)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR', 'filter', 9000, 2, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR', 'filter', 9000, 2, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación SBR',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 2,
            MenuIcon = '<i class="fa fa-tasks"></i>',
            Description = 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.'
        WHERE Id = 9300;
    END

    PRINT '   -> Sub-módulo 9300 reasignado como hijo de Administración (9000).';


    PRINT '================================================================================';
    PRINT '>> [3/5] Reubicando Avance (6002), Historial (6003) y Resultados (6005) bajo 9300...';
    PRINT '================================================================================';

    -- 6002: Avance SBR
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6002)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6002, 'Avance SBR', 'Evaluacion/Avance', 9300, 1, '<i class="fa fa-tasks"></i>', 'Monitoreo del porcentaje de avance y nivel de respuestas completadas.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6002, 'Avance SBR', 'Evaluacion/Avance', 9300, 1, '<i class="fa fa-tasks"></i>', 'Monitoreo del porcentaje de avance y nivel de respuestas completadas.');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Avance SBR',
            MenuURL = 'Evaluacion/Avance',
            ParentId = 9300,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-tasks"></i>',
            Description = 'Monitoreo del porcentaje de avance y nivel de respuestas completadas.'
        WHERE Id = 6002;
    END

    -- 6003: Historial SBR
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6003)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6003, 'Historial SBR', 'Evaluacion/Historial', 9300, 2, '<i class="fa fa-history"></i>', 'Registro histórico de evaluaciones y calificaciones obtenidas.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6003, 'Historial SBR', 'Evaluacion/Historial', 9300, 2, '<i class="fa fa-history"></i>', 'Registro histórico de evaluaciones y calificaciones obtenidas.');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Historial SBR',
            MenuURL = 'Evaluacion/Historial',
            ParentId = 9300,
            SortOrder = 2,
            MenuIcon = '<i class="fa fa-history"></i>',
            Description = 'Registro histórico de evaluaciones y calificaciones obtenidas.'
        WHERE Id = 6003;
    END

    -- 6005: Resultados SBR
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6005)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6005, 'Resultados SBR', 'Evaluacion/Resultados', 9300, 3, '<i class="fa fa-check-square-o"></i>', 'Informe de resultados consolidados y niveles de cumplimiento.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (6005, 'Resultados SBR', 'Evaluacion/Resultados', 9300, 3, '<i class="fa fa-check-square-o"></i>', 'Informe de resultados consolidados y niveles de cumplimiento.');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Resultados SBR',
            MenuURL = 'Evaluacion/Resultados',
            ParentId = 9300,
            SortOrder = 3,
            MenuIcon = '<i class="fa fa-check-square-o"></i>',
            Description = 'Informe de resultados consolidados y niveles de cumplimiento.'
        WHERE Id = 6005;
    END

    PRINT '   -> Avance, Historial y Resultados asignados como hijas de 9300 en Administración.';


    PRINT '================================================================================';
    PRINT '>> [4/5] Configurando [Configuración de Preguntas SBR] (9135) en Gestión Operativa (9200)...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9135)
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9135, 'Configuración de Preguntas SBR', 'Evaluacion/Index', 9200, 9, '<i class="fa fa-list-alt"></i>', 'Mantenimiento del banco de preguntas, pesos y categorías SBR.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9135, 'Configuración de Preguntas SBR', 'Evaluacion/Index', 9200, 9, '<i class="fa fa-list-alt"></i>', 'Mantenimiento del banco de preguntas, pesos y categorías SBR.');
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Configuración de Preguntas SBR',
            MenuURL = 'Evaluacion/Index',
            ParentId = 9200,
            SortOrder = 9,
            MenuIcon = '<i class="fa fa-list-alt"></i>',
            Description = 'Mantenimiento del banco de preguntas, pesos y categorías SBR.'
        WHERE Id = 9135;
    END

    PRINT '   -> Configuración de Preguntas SBR (9135) reubicada en Gestión Operativa (9200).';


    PRINT '================================================================================';
    PRINT '>> [5/5] Sincronizando permisos en dbo.MenuPermission...';
    PRINT '================================================================================';

    -- 1. Permiso para TODOS los roles en el menú raíz 6000 y su opción Autoevaluación 6001
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (6000, 6001)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1
    WHERE MenuId IN (6000, 6001);

    -- 2. Permiso para roles de administración / FFC en 9300, 6002, 6003, 6005 y 9135
    -- Insertar para roles de administración (ej. RoleId 1, 10, y los que tengan acceso a 9000)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, mpAdmin.RoleId, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN (
        SELECT DISTINCT RoleId FROM dbo.MenuPermission WHERE MenuId = 9000 AND IsRead = 1
    ) mpAdmin
    WHERE m.Id IN (9300, 6002, 6003, 6005, 9135)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = mpAdmin.RoleId
      );

    UPDATE dbo.MenuPermission
    SET IsRead = 1, IsCreate = 1, IsUpdate = 1
    WHERE MenuId IN (9300, 6002, 6003, 6005, 9135)
      AND RoleId IN (SELECT DISTINCT RoleId FROM dbo.MenuPermission WHERE MenuId = 9000 AND IsRead = 1);

    -- 3. Habilitar Ind_Evaluacion en todas las entidades
    UPDATE dbo.Entidad SET Ind_Evaluacion = 1 WHERE Id <> '0';

    COMMIT TRANSACTION;
    PRINT '================================================================================';
    PRINT '>> PROCESO COMPLETADO EXITOSAMENTE: SBR restaurado en Administración y Gestión Operativa.';
    PRINT '================================================================================';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@errMsg, 16, 1);
END CATCH;
