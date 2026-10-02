-- ===============================================================================================
-- BANCO / ENTIDAD: FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)
-- PROYECTO: SISTEMA DE ADMINISTRACIÓN Y CALIFICACIÓN (SAC) - FASE II
-- SCRIPT: 12_Corregir_Y_Reorganizar_Menu_FFC.sql
-- BASE DE DATOS OBJETIVO: [FFC]
-- ===============================================================================================
-- DIAGNÓSTICO DEL ESTADO ACTUAL:
--   1. Gestión Operativa (9200) quedó incorrectamente asignado bajo Evaluación SBR (ParentId = 9300).
--   2. Todas las opciones hijas (Calendario, SUGEF, Fórmulas, XML, Entidades, Parámetros, Roles, 
--      Menú del Sistema, Cargas, Informes) quedaron sueltas como hijas directas de Administración (9000).
--   3. Evaluación SBR quedó duplicada (aparece como raíz 6000 y como sub-módulo 9300).
--   4. Carga de Datos (7000) e Informes (11000) no tienen asignadas a sus hijas.
--
-- CORRECCIÓN APLICADA EN ESTE SCRIPT:
--   1. Reubica a Gestión Operativa (9200) directamente bajo Administración (ParentId = 9000).
--   2. Asigna a Gestión Operativa (9200) sus 8 opciones:
--      - 9050 Calendario, 9110 Catálogo de Fórmulas, 9100 Catálogo SUGEF, 9120 Tipos de XML,
--        9130 Entidades Afiliadas, 9150 Parámetros Globales, 9145 Roles, 13001 Menú del Sistema.
--   3. Asigna a Evaluación SBR (9300) sus 4 opciones:
--      - 6002 Avance SBR, 6003 Historial SBR, 6005 Resultados SBR, 9135 Configuración de Preguntas SBR.
--   4. Asigna a Carga de Datos (7000) sus opciones:
--      - 7110 Carga de Datos (XML), 7100 Estatus de la Carga, 7160 Archivos Cargados,
--        7150 Monitor de Cierres, 7120 Cargas Masivas, 7140 Histórico de Cargas.
--   5. Asigna a Informes (11000) sus 3 opciones:
--      - 11110 Informes (Consulta), 11100 Carga de Informes, 9140 Tipos de Informe.
--   6. Deja únicamente como hijas directas de Administración (9000):
--      - 9200 Gestión Operativa (Sub-módulo)
--      - 9300 Evaluación SBR (Sub-módulo)
--      - 7000 Carga de Datos (Sub-módulo)
--      - 11000 Informes (Sub-módulo)
--      - 1 Perfiles
--      - 8100 Usuarios SAC
--      - 8110 Cambiar Contraseña
--      - 6502 Contribuciones
--      - 11120 Notificaciones
--      - 13000 Historial de Ingresos
--   7. Elimina la raíz obsoleta redundante 6000 y 6001 (Evaluación SBR antigua).
--   8. Mantiene ordenadas las ramas de negocio fuera de Administración (1000, 1500, 4000).
-- ===============================================================================================

USE [FFC];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '===============================================================================================';
PRINT 'INICIANDO SCRIPT: REORGANIZACION DEFINITIVA Y REAGRUPACION DE HIJAS EN [FFC]';
PRINT '===============================================================================================';

BEGIN TRANSACTION;

BEGIN TRY

    ALTER TABLE dbo.Menu NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission NOCHECK CONSTRAINT ALL;

    -- ===========================================================================================
    -- 1. CORREGIR PAPÁS CONTENEDORES BAJO ADMINISTRACIÓN (9000)
    -- ===========================================================================================
    PRINT '>> [1/6] Configurando contenedores correctos bajo Administración (9000)...';

    -- 1.1 Administración como raíz
    UPDATE dbo.Menu 
    SET MenuText = 'Administración',
        MenuURL = 'filter',
        ParentId = NULL,
        SortOrder = 4,
        MenuIcon = '<i class="fa fa-cog"></i>',
        Description = 'Gestión de usuarios, seguridad, catálogos, carga de datos y parámetros del sistema.'
    WHERE Id = 9000;

    -- 1.2 Gestión Operativa (9200) -> Deber ser hija de Administración (9000), ¡NO de 9300!
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9200)
    BEGIN
        UPDATE dbo.Menu 
        SET MenuText = 'Gestión Operativa',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-cogs"></i>',
            Description = 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.'
        WHERE Id = 9200;
    END
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 1, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 1, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
    END

    -- 1.3 Evaluación SBR (9300) -> Hija de Administración (9000)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9300)
    BEGIN
        UPDATE dbo.Menu 
        SET MenuText = 'Evaluación SBR (Avance y Resultados)',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 2,
            MenuIcon = '<i class="fa fa-tasks"></i>',
            Description = 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.'
        WHERE Id = 9300;
    END
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR (Avance y Resultados)', 'filter', 9000, 2, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR (Avance y Resultados)', 'filter', 9000, 2, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
    END

    -- 1.4 Carga de Datos (7000) -> Hija de Administración (9000)
    UPDATE dbo.Menu 
    SET MenuText = 'Carga de Datos',
        MenuURL = 'filter',
        ParentId = 9000,
        SortOrder = 3,
        MenuIcon = '<i class="fa fa-upload"></i>',
        Description = 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.'
    WHERE Id = 7000;

    -- 1.5 Informes (11000) -> Hija de Administración (9000)
    UPDATE dbo.Menu 
    SET MenuText = 'Informes',
        MenuURL = 'filter',
        ParentId = 9000,
        SortOrder = 4,
        MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
        Description = 'Carga de informes técnicos, consulta y tipos de informes.'
    WHERE Id = 11000;

    PRINT '   -> 4 sub-módulos papás configurados bajo Administración.';


    -- ===========================================================================================
    -- 2. REASIGNAR HIJAS A GESTIÓN OPERATIVA (9200)
    -- ===========================================================================================
    PRINT '>> [2/6] Agrupando opciones dentro de Gestión Operativa (9200)...';

    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 1, MenuText = 'Calendario', MenuURL = 'Calendario/Index', MenuIcon = '<i class="fa fa-calendar"></i>' WHERE Id = 9050;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 2, MenuText = 'Catálogo de Fórmulas', MenuURL = 'Formula/Index', MenuIcon = '<i class="fa fa-calculator"></i>' WHERE Id = 9110;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 3, MenuText = 'Catálogo SUGEF', MenuURL = 'CatalogoCuenta/Index', MenuIcon = '<i class="fa fa-book"></i>' WHERE Id = 9100;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 4, MenuText = 'Tipos de XML', MenuURL = 'TipoXML/Index', MenuIcon = '<i class="fa fa-code"></i>' WHERE Id = 9120;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 5, MenuText = 'Entidades Afiliadas', MenuURL = 'Entidad/Index', MenuIcon = '<i class="fa fa-institution"></i>' WHERE Id = 9130;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 6, MenuText = 'Parámetros Globales', MenuURL = 'Parametros/Index', MenuIcon = '<i class="fa fa-cogs"></i>' WHERE Id = 9150;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 7, MenuText = 'Roles', MenuURL = 'Role/Index', MenuIcon = '<i class="fa fa-lock"></i>' WHERE Id = 9145;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 8, MenuText = 'Menú del Sistema', MenuURL = 'Menu/Index', MenuIcon = '<i class="fa fa-sitemap"></i>' WHERE Id = 13001;

    PRINT '   -> 8 opciones asignadas a Gestión Operativa (9200).';


    -- ===========================================================================================
    -- 3. REASIGNAR HIJAS A EVALUACIÓN SBR (9300)
    -- ===========================================================================================
    PRINT '>> [3/6] Agrupando opciones dentro de Evaluación SBR (9300)...';

    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 1, MenuText = 'Avance SBR', MenuURL = 'Evaluacion/Avance', MenuIcon = '<i class="fa fa-tasks"></i>' WHERE Id = 6002;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 2, MenuText = 'Historial SBR', MenuURL = 'Evaluacion/Historial', MenuIcon = '<i class="fa fa-history"></i>' WHERE Id = 6003;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 3, MenuText = 'Resultados SBR', MenuURL = 'Evaluacion/Resultados', MenuIcon = '<i class="fa fa-check-square-o"></i>' WHERE Id = 6005;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 4, MenuText = 'Configuración de Preguntas SBR', MenuURL = 'Evaluacion/Index', MenuIcon = '<i class="fa fa-list-alt"></i>' WHERE Id = 9135;

    PRINT '   -> 4 opciones asignadas a Evaluación SBR (9300).';


    -- ===========================================================================================
    -- 4. REASIGNAR HIJAS A CARGA DE DATOS (7000) E INFORMES (11000)
    -- ===========================================================================================
    PRINT '>> [4/6] Agrupando opciones de Carga de Datos (7000) e Informes (11000)...';

    -- Carga de Datos (7000)
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 1, MenuText = 'Carga de Datos (XML)', MenuURL = 'Archivo/Index', MenuIcon = '<i class="fa fa-upload"></i>' WHERE Id = 7110;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 2, MenuText = 'Estatus de la Carga', MenuURL = 'Cierre/Index', MenuIcon = '<i class="fa fa-check-circle"></i>' WHERE Id = 7100;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 3, MenuText = 'Archivos Cargados', MenuURL = 'Cierre', MenuIcon = '<i class="fa fa-archive"></i>' WHERE Id = 7160;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 4, MenuText = 'Monitor de Cierres', MenuURL = 'Cierre/Monitor', MenuIcon = '<i class="fa fa-desktop"></i>' WHERE Id = 7150;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 5, MenuText = 'Cargas Masivas', MenuURL = 'Explorer/Requisites', MenuIcon = '<i class="fa fa-check-square-o"></i>' WHERE Id = 7120;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 6, MenuText = 'Histórico de Cargas', MenuURL = 'Explorer/Index', MenuIcon = '<i class="fa fa-folder-open"></i>' WHERE Id = 7140;

    -- Informes (11000)
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 1, MenuText = 'Informes', MenuURL = 'Explorer/Consulta', MenuIcon = '<i class="fa fa-file-pdf-o"></i>' WHERE Id = 11110;
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 2, MenuText = 'Carga de Informes', MenuURL = 'Explorer/Informe', MenuIcon = '<i class="fa fa-file-text"></i>' WHERE Id = 11100;
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 3, MenuText = 'Tipos de Informe', MenuURL = 'TipoInforme/Index', MenuIcon = '<i class="fa fa-tags"></i>' WHERE Id = 9140;

    PRINT '   -> Opciones de Carga de Datos e Informes reasignadas.';


    -- ===========================================================================================
    -- 5. CONFIGURAR HIJAS DIRECTAS DE ADMINISTRACIÓN (9000)
    -- ===========================================================================================
    PRINT '>> [5/6] Configurando opciones directas bajo Administración (9000)...';

    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 5, MenuText = 'Perfiles', MenuURL = 'Cierre/Perfiles', MenuIcon = '<i class="fa fa-shield"></i>' WHERE Id = 1;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 6, MenuText = 'Usuarios SAC', MenuURL = 'Usuario/Index', MenuIcon = '<i class="fa fa-users"></i>' WHERE Id = 8100;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 7, MenuText = 'Cambiar Contraseña', MenuURL = 'Usuario/ChangePassword', MenuIcon = '<i class="fa fa-key"></i>' WHERE Id = 8110;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 8, MenuText = 'Contribuciones', MenuURL = 'Facturacion/Index', MenuIcon = '<i class="fa fa-dollar"></i>' WHERE Id = 6502;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 9, MenuText = 'Historial de Ingresos', MenuURL = 'Seguridad/Index', MenuIcon = '<i class="fa fa-sign-in"></i>' WHERE Id = 13000;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 10, MenuText = 'Notificaciones', MenuURL = 'Explorer/Notificacion', MenuIcon = '<i class="fa fa-bell"></i>' WHERE Id = 11120;

    -- Eliminar la raíz duplicada obsoleta 6000 y 6001 (Evaluación SBR antigua)
    DELETE FROM dbo.MenuPermission WHERE MenuId IN (6000, 6001, 8000, 6500);
    DELETE FROM dbo.Menu WHERE Id IN (6000, 6001, 8000, 6500);

    PRINT '   -> Hijas directas configuradas y raíces duplicadas eliminadas.';


    -- ===========================================================================================
    -- 6. SINCRONIZACIÓN DE PERMISOS EN [dbo].[MenuPermission]
    -- ===========================================================================================
    PRINT '>> [6/6] Sincronizando permisos por rol en dbo.MenuPermission...';

    -- Eliminar permisos duplicados
    ;WITH PermisosDuplicados AS (
        SELECT Id, MenuId, RoleId,
               ROW_NUMBER() OVER (PARTITION BY MenuId, RoleId ORDER BY Id ASC) AS NumFila
        FROM dbo.MenuPermission
    )
    DELETE FROM dbo.MenuPermission
    WHERE Id IN (SELECT Id FROM PermisosDuplicados WHERE NumFila > 1);

    -- Permiso de Administración (9000) y sus sub-módulos (9200, 9300, 7000, 11000)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT p.MenuId, r.Id, p.SortOrder, 1, 1, 1, 1
    FROM (VALUES (9000, 4), (9200, 1), (9300, 2), (7000, 3), (11000, 4)) AS p(MenuId, SortOrder)
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = p.MenuId AND mp.RoleId = r.Id);

    -- Permisos para todas las opciones hijas
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (m.ParentId IN (9000, 9200, 9300, 7000, 11000) OR m.Id IN (9000, 9200, 9300, 7000, 11000))
      AND (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- Permisos para Cooperativas (RoleId 10 o EsEntidad = 1)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 0
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (8100, 8110, 7110, 6502, 11110)
      AND (r.EsEntidad = 1 OR r.Id = 10)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    UPDATE dbo.MenuPermission SET IsRead = 1 WHERE IsRead = 0;

    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    COMMIT TRANSACTION;

    PRINT '===============================================================================================';
    PRINT '>> REORGANIZACION FINALIZADA EXITOSAMENTE EN [FFC]. ARBOL TOTALMENTE CORREGIDO.';
    PRINT '===============================================================================================';

    -- Diagnóstico del árbol de Administración resultante
    PRINT '';
    PRINT '>> RESULTADO FINAL: ARBOL JERARQUICO DE ADMINISTRACION EN [FFC]:';
    SELECT 
        m.Id,
        CASE 
            WHEN m.Id = 9000 THEN m.MenuText
            WHEN m.ParentId = 9000 AND m.Id IN (9200, 9300, 7000, 11000) THEN '    |-- [SUB-MODULO] ' + m.MenuText
            WHEN m.ParentId = 9000 THEN '    |-- [DIRECTA] ' + m.MenuText
            ELSE '        +-- ' + m.MenuText
        END AS [Jerarquia],
        m.MenuURL,
        m.ParentId,
        m.SortOrder
    FROM dbo.Menu m
    WHERE m.Id = 9000 
       OR m.ParentId = 9000 
       OR m.ParentId IN (9200, 9300, 7000, 11000)
    ORDER BY 
        CASE WHEN m.Id = 9000 THEN 0 ELSE 1 END,
        CASE WHEN m.ParentId = 9000 AND m.Id IN (9200, 9300, 7000, 11000) THEN 1 ELSE 2 END,
        ISNULL(m.ParentId, m.Id),
        m.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    PRINT '!!! ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
