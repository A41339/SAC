-- ===============================================================================================
-- BANCO / ENTIDAD: FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)
-- PROYECTO: SISTEMA DE ADMINISTRACIÓN Y CALIFICACIÓN (SAC) - FASE II
-- REQUERIMIENTO: LIMPIEZA DE DUPLICADOS Y RESTABLECIMIENTO DEFINITIVO DE MENÚ EN FFC
-- SCRIPT: 11_Limpiar_Duplicados_Y_Reestructurar_Menu_FFC.sql
-- BASE DE DATOS OBJETIVO: [FFC]
-- ===============================================================================================
-- MOTIVO:
--   Al haberse ejecutado por error el script de FGA en FFC, se restauraron las raíces antiguas 
--   (8000 Control de Accesos, 6500 Cálculos, 6000 Evaluación SBR, 7000 Carga como raíz, 
--   11000 Informes como raíz), provocando que las opciones aparezcan duplicadas tanto en el menú 
--   principal como dentro de Administración.
--
-- SOLUCIÓN INTEGRAL:
--   1. Desasocia y reubica a sus papás definitivos todas las opciones hijas.
--   2. Elimina en FFC las raíces obsoletas (8000, 6500, 6000, 6001).
--   3. Detecta y elimina filas duplicadas por MenuURL / Id en dbo.Menu y dbo.MenuPermission.
--   4. Establece la jerarquía única y limpia bajo Administración (9000):
--      ├── Gestión Operativa (9200) -> 8 opciones (9050, 9110, 9100, 9120, 9130, 9150, 9145, 13001)
--      ├── Evaluación SBR (9300)    -> 4 opciones (6002, 6003, 6005, 9135)
--      ├── Carga de Datos (7000)    -> 4 opciones (7110, 7100, 7160, 7150)
--      ├── Informes (11000)         -> 3 opciones (11110, 11100, 9140)
--      └── Hijas directas (9000)    -> 5 opciones (1, 8100, 8110, 6502, 11120)
--   5. Regenera y desduplica la matriz de permisos en dbo.MenuPermission.
--   6. Muestra diagnóstico visual del árbol jerárquico resultante sin duplicados.
-- ===============================================================================================

USE [FFC];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '===============================================================================================';
PRINT 'INICIANDO SCRIPT: LIMPIEZA DE DUPLICADOS Y REESTRUCTURACION DEFINITIVA DEL MENU EN [FFC]';
PRINT '===============================================================================================';

BEGIN TRANSACTION;

BEGIN TRY

    -- Deshabilitar temporalmente constraints
    ALTER TABLE dbo.Menu NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission NOCHECK CONSTRAINT ALL;

    -- ===========================================================================================
    -- PASO 1: ASEGURAR LOS MÓDULOS PADRES CONTENEDORES
    -- ===========================================================================================
    PRINT '>> [1/7] Asegurando módulos raíz y sub-módulos contenedores...';

    -- 1.1 Administración (Id: 9000, ParentId = NULL)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Administración',
            MenuURL = 'filter',
            ParentId = NULL,
            SortOrder = 8,
            MenuIcon = '<i class="fa fa-cogs"></i>',
            Description = 'Gestión integral de usuarios, accesos, cargas de datos, contribuciones, informes y configuración operativa.'
        WHERE Id = 9000;
    END
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9000, 'Administración', 'filter', NULL, 8, '<i class="fa fa-cogs"></i>', 'Gestión integral de usuarios, accesos, cargas de datos, contribuciones, informes y configuración operativa.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9000, 'Administración', 'filter', NULL, 8, '<i class="fa fa-cogs"></i>', 'Gestión integral de usuarios, accesos, cargas de datos, contribuciones, informes y configuración operativa.');
        END
    END

    -- 1.2 Gestión Operativa (Id: 9200, ParentId = 9000)
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
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 1, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades, parámetros, roles y menú del sistema.');
        END
    END

    -- 1.3 Evaluación SBR (Id: 9300, ParentId = 9000)
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
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR (Avance y Resultados)', 'filter', 9000, 2, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
        END
    END

    -- 1.4 Carga de Datos (Id: 7000, ParentId = 9000) -> Ya NO es raíz en FFC
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Carga de Datos',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 3,
            MenuIcon = '<i class="fa fa-upload"></i>',
            Description = 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.'
        WHERE Id = 7000;
    END
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (7000, 'Carga de Datos', 'filter', 9000, 3, '<i class="fa fa-upload"></i>', 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (7000, 'Carga de Datos', 'filter', 9000, 3, '<i class="fa fa-upload"></i>', 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.');
        END
    END

    -- 1.5 Informes (Id: 11000, ParentId = 9000) -> Ya NO es raíz en FFC
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Informes',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 4,
            MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
            Description = 'Carga de informes técnicos, consulta y tipos de informes.'
        WHERE Id = 11000;
    END
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (11000, 'Informes', 'filter', 9000, 4, '<i class="fa fa-file-pdf-o"></i>', 'Carga de informes técnicos, consulta y tipos de informes.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (11000, 'Informes', 'filter', 9000, 4, '<i class="fa fa-file-pdf-o"></i>', 'Carga de informes técnicos, consulta y tipos de informes.');
        END
    END

    PRINT '   -> Módulos contenedores principales configurados correctamente.';


    -- ===========================================================================================
    -- PASO 2: REASIGNAR TODAS LAS OPCIONES HIJAS A SUS PAPÁS DEFINITIVOS
    -- ===========================================================================================
    PRINT '>> [2/7] Reasignando todas las opciones hijas a sus papás definitivos...';

    -- 2.1 Hijas de Gestión Operativa (9200):
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 1, MenuText = 'Calendario', MenuURL = 'Calendario/Index', MenuIcon = '<i class="fa fa-calendar"></i>' WHERE Id = 9050;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 2, MenuText = 'Catálogo de Fórmulas', MenuURL = 'Formula/Index', MenuIcon = '<i class="fa fa-calculator"></i>' WHERE Id = 9110;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 3, MenuText = 'Catálogo SUGEF', MenuURL = 'CatalogoCuenta/Index', MenuIcon = '<i class="fa fa-book"></i>' WHERE Id = 9100;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 4, MenuText = 'Tipos de XML', MenuURL = 'TipoXML/Index', MenuIcon = '<i class="fa fa-code"></i>' WHERE Id = 9120;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 5, MenuText = 'Entidades Afiliadas', MenuURL = 'Entidad/Index', MenuIcon = '<i class="fa fa-institution"></i>' WHERE Id = 9130;
    UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 6, MenuText = 'Parámetros Globales', MenuURL = 'Parametros/Index', MenuIcon = '<i class="fa fa-cogs"></i>' WHERE Id = 9150;
    
    -- Roles (9145)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 7, MenuText = 'Roles', MenuURL = 'Role/Index', MenuIcon = '<i class="fa fa-key"></i>', Description = 'Gestión de roles y configuración de la matriz de permisos de menú.' WHERE Id = 9145;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Role/Index', 'Role'))
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 7, MenuText = 'Roles', MenuIcon = '<i class="fa fa-key"></i>', Description = 'Gestión de roles y configuración de la matriz de permisos de menú.' WHERE MenuURL IN ('Role/Index', 'Role');
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
    END

    -- Menú del Sistema (13001)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 8, MenuText = 'Menú del Sistema', MenuURL = 'Menu/Index', MenuIcon = '<i class="fa fa-sitemap"></i>', Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.' WHERE Id = 13001;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Menu/Index', 'Menu'))
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 8, MenuText = 'Menú del Sistema', MenuIcon = '<i class="fa fa-sitemap"></i>', Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.' WHERE MenuURL IN ('Menu/Index', 'Menu');
    ELSE
    BEGIN
        IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
    END

    -- 2.2 Hijas de Evaluación SBR (9300):
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 1, MenuText = 'Avance SBR', MenuURL = 'Evaluacion/Avance', MenuIcon = '<i class="fa fa-tasks"></i>' WHERE Id = 6002;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 2, MenuText = 'Historial SBR', MenuURL = 'Evaluacion/Historial', MenuIcon = '<i class="fa fa-history"></i>' WHERE Id = 6003;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 3, MenuText = 'Resultados SBR', MenuURL = 'Evaluacion/Resultados', MenuIcon = '<i class="fa fa-check-square-o"></i>' WHERE Id = 6005;
    UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 4, MenuText = 'Configuración SBR', MenuURL = 'Evaluacion/Index', MenuIcon = '<i class="fa fa-list-alt"></i>' WHERE Id = 9135;

    -- 2.3 Hijas de Carga de Datos (7000):
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 1, MenuText = 'Carga de Datos (XML)', MenuURL = 'Archivo/Index', MenuIcon = '<i class="fa fa-upload"></i>' WHERE Id = 7110;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 2, MenuText = 'Estatus de la Carga', MenuURL = 'Cierre/Index', MenuIcon = '<i class="fa fa-check-circle"></i>' WHERE Id = 7100;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 3, MenuText = 'Archivos Cargados', MenuURL = 'Cierre', MenuIcon = '<i class="fa fa-archive"></i>' WHERE Id = 7160;
    UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 4, MenuText = 'Monitor de Cierres', MenuURL = 'Cierre/Monitor', MenuIcon = '<i class="fa fa-desktop"></i>' WHERE Id = 7150;

    -- 2.4 Hijas de Informes (11000):
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 1, MenuText = 'Consulta de Informes', MenuURL = 'Explorer/Consulta', MenuIcon = '<i class="fa fa-file-pdf-o"></i>' WHERE Id = 11110;
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 2, MenuText = 'Carga de Informes', MenuURL = 'Explorer/Informe', MenuIcon = '<i class="fa fa-file-text"></i>' WHERE Id = 11100;
    UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 3, MenuText = 'Tipos de Informe', MenuURL = 'TipoInforme/Index', MenuIcon = '<i class="fa fa-tags"></i>' WHERE Id = 9140;

    -- 2.5 Hijas directas bajo Administración (9000):
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 5, MenuText = 'Perfiles', MenuURL = 'Cierre/Perfiles', MenuIcon = '<i class="fa fa-shield"></i>' WHERE Id = 1;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 6, MenuText = 'Administración de Usuarios', MenuURL = 'Usuario/Index', MenuIcon = '<i class="fa fa-users"></i>' WHERE Id = 8100;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 7, MenuText = 'Cambiar Contraseña', MenuURL = 'Usuario/ChangePassword', MenuIcon = '<i class="fa fa-key"></i>' WHERE Id = 8110;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 8, MenuText = 'Contribuciones', MenuURL = 'Facturacion/Index', MenuIcon = '<i class="fa fa-dollar"></i>' WHERE Id = 6502;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 9, MenuText = 'Notificaciones', MenuURL = 'Explorer/Notificacion', MenuIcon = '<i class="fa fa-bell"></i>', Description = 'Emisión de comunicados oficiales y circulares para entidades afiliadas.' WHERE Id = 11120;

    PRINT '   -> Hijas reasignadas a sus nuevos contenedores.';


    -- ===========================================================================================
    -- PASO 3: ELIMINAR RAÍCES OBSOLETAS QUE DUPLICAN EL MENÚ EN FFC
    -- ===========================================================================================
    PRINT '>> [3/7] Eliminando raíces obsoletas de FGA en FFC (8000, 6500, 6000, 6001)...';

    -- Si alguna opción quedó aún apuntando a estas raíces, moverlas a 9000
    UPDATE dbo.Menu SET ParentId = 9000 WHERE ParentId IN (8000, 6500);
    UPDATE dbo.Menu SET ParentId = 9300 WHERE ParentId IN (6000, 6001);

    -- Eliminar permisos asociados a estas raíces
    DELETE FROM dbo.MenuPermission WHERE MenuId IN (8000, 6500, 6000, 6001, 6501);

    -- Eliminar las raíces obsoletas de dbo.Menu
    DELETE FROM dbo.Menu WHERE Id IN (8000, 6500, 6000, 6001, 6501);

    PRINT '   -> Raíces obsoletas eliminadas.';


    -- ===========================================================================================
    -- PASO 4: DETECTAR Y ELIMINAR REGISTROS DUPLICADOS POR URL O NOMBRE
    -- ===========================================================================================
    PRINT '>> [4/7] Limpiando registros duplicados en dbo.Menu...';

    -- Encontrar si hay registros repetidos para la misma URL funcional
    ;WITH DuplicadosMenu AS (
        SELECT Id, MenuURL, MenuText, ParentId,
               ROW_NUMBER() OVER (
                   PARTITION BY LOWER(RTRIM(LTRIM(MenuURL))) 
                   ORDER BY 
                       -- Prioridad a los IDs estándar conocidos de FFC
                       CASE 
                           WHEN Id IN (9000, 9200, 9300, 7000, 11000, 9050, 9110, 9100, 9120, 9130, 
                                       9150, 9145, 13001, 6002, 6003, 6005, 9135, 7110, 7100, 7160, 
                                       7150, 11110, 11100, 9140, 1, 8100, 8110, 6502, 11120,
                                       1000, 1500, 2100, 2110, 2120, 2130, 2135, 2140, 2250, 2260,
                                       3000, 3100, 3110, 3120, 3130, 4000, 4100, 4110, 4130, 5000,
                                       5100, 5110, 5120, 5130, 13000, 13002) THEN 0
                           ELSE 1
                       END,
                       Id ASC
               ) AS NumFila
        FROM dbo.Menu
        WHERE MenuURL IS NOT NULL 
          AND LOWER(RTRIM(LTRIM(MenuURL))) NOT IN ('root', 'filter', '#', '')
    )
    DELETE FROM dbo.MenuPermission 
    WHERE MenuId IN (SELECT Id FROM DuplicadosMenu WHERE NumFila > 1);

    ;WITH DuplicadosMenu AS (
        SELECT Id, MenuURL, MenuText, ParentId,
               ROW_NUMBER() OVER (
                   PARTITION BY LOWER(RTRIM(LTRIM(MenuURL))) 
                   ORDER BY 
                       CASE 
                           WHEN Id IN (9000, 9200, 9300, 7000, 11000, 9050, 9110, 9100, 9120, 9130, 
                                       9150, 9145, 13001, 6002, 6003, 6005, 9135, 7110, 7100, 7160, 
                                       7150, 11110, 11100, 9140, 1, 8100, 8110, 6502, 11120,
                                       1000, 1500, 2100, 2110, 2120, 2130, 2135, 2140, 2250, 2260,
                                       3000, 3100, 3110, 3120, 3130, 4000, 4100, 4110, 4130, 5000,
                                       5100, 5110, 5120, 5130, 13000, 13002) THEN 0
                           ELSE 1
                       END,
                       Id ASC
               ) AS NumFila
        FROM dbo.Menu
        WHERE MenuURL IS NOT NULL 
          AND LOWER(RTRIM(LTRIM(MenuURL))) NOT IN ('root', 'filter', '#', '')
    )
    DELETE FROM dbo.Menu 
    WHERE Id IN (SELECT Id FROM DuplicadosMenu WHERE NumFila > 1);

    PRINT '   -> Duplicados por MenuURL eliminados.';


    -- ===========================================================================================
    -- PASO 5: DESDUPLICAR LA MATRIZ DE PERMISOS [dbo].[MenuPermission]
    -- ===========================================================================================
    PRINT '>> [5/7] Desduplicando y sincronizando matriz de permisos en dbo.MenuPermission...';

    ;WITH PermisosDuplicados AS (
        SELECT Id, MenuId, RoleId,
               ROW_NUMBER() OVER (PARTITION BY MenuId, RoleId ORDER BY Id ASC) AS NumFila
        FROM dbo.MenuPermission
    )
    DELETE FROM dbo.MenuPermission
    WHERE Id IN (SELECT Id FROM PermisosDuplicados WHERE NumFila > 1);

    -- Asegurar permisos sobre módulos contenedores (9000, 9200, 9300, 7000, 11000) para roles administrativos
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT p.MenuId, r.Id, p.SortOrder, 1, 1, 1, 1
    FROM (VALUES (9000, 8), (9200, 1), (9300, 2), (7000, 3), (11000, 4)) AS p(MenuId, SortOrder)
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = p.MenuId AND mp.RoleId = r.Id);

    -- Asegurar permisos sobre todas las opciones hijas de Administración
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (m.ParentId IN (9000, 9200, 9300, 7000, 11000) OR m.Id IN (9000, 9200, 9300, 7000, 11000))
      AND (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- Permisos para Cooperativas (RoleId 10 o EsEntidad = 1):
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 0
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (8100, 8110, 7110, 6502, 11110)
      AND (r.EsEntidad = 1 OR r.Id = 10)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- Normalizar lectura activa
    UPDATE dbo.MenuPermission SET IsRead = 1 WHERE IsRead = 0;

    PRINT '   -> Permisos desduplicados y sincronizados.';


    -- ===========================================================================================
    -- PASO 6: REACTIVAR CONSTRAINTS Y GRANT
    -- ===========================================================================================
    PRINT '>> [6/7] Reactivando y validando integridad referencial...';

    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Menu TO [public];
    GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.MenuPermission TO [public];

    COMMIT TRANSACTION;

    PRINT '===============================================================================================';
    PRINT '>> LIMPIEZA Y REESTRUCTURACION EN [FFC] COMPLETADA SATISFACTORIAMENTE (0 DUPLICADOS)';
    PRINT '===============================================================================================';


    -- ===========================================================================================
    -- PASO 7: DIAGNÓSTICO FINAL DEL ÁRBOL EN FFC
    -- ===========================================================================================
    PRINT '';
    PRINT '>> [7/7] DIAGNÓSTICO: MODULOS RAIZ RESULTANTES EN [FFC] (ParentId IS NULL):';
    SELECT Id, MenuText, MenuURL, SortOrder, MenuIcon
    FROM dbo.Menu
    WHERE ParentId IS NULL
    ORDER BY SortOrder, Id;

    PRINT '';
    PRINT '>> DIAGNÓSTICO: ARBOL JERARQUICO DE ADMINISTRACION EN [FFC]:';
    SELECT 
        m.Id,
        CASE 
            WHEN m.Id = 9000 THEN m.MenuText
            WHEN m.ParentId = 9000 AND m.Id IN (9200, 9300, 7000, 11000) THEN '    |-- [SUB-MODULO] ' + m.MenuText
            WHEN m.ParentId = 9000 THEN '    |-- [OPCION DIRECTA] ' + m.MenuText
            ELSE '        +-- ' + m.MenuText
        END AS [Jerarquia_Administracion],
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

    PRINT '!!! ERROR EN LA EJECUCIÓN: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
