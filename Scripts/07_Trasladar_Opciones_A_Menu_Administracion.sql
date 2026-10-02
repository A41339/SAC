-- ===============================================================================================
-- BANCO / ENTIDAD: FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)
-- PROYECTO: SISTEMA DE ADMINISTRACIÓN Y CALIFICACIÓN (SAC) - FASE II
-- REQUERIMIENTO: #FFC-112 (PUNTO ADMINISTRACIÓN Y REESTRUCTURACIÓN DE MENÚS)
-- SCRIPT: 07_Trasladar_Opciones_A_Menu_Administracion.sql
-- DESCRIPCIÓN: 
--   1. Renombra el módulo raíz "Mantenimientos" (Id: 9000) a "Administración" (ParentId = NULL).
--   2. Crea y configura los nuevos "Papás" (Sub-Módulos) bajo Administración (ParentId = 9000):
--      - Gestión Operativa (Id: 9200) -> Con sus 6 opciones hijas: Calendario, Fórmulas, Catálogo SUGEF, Tipos XML, Entidades, Parámetros.
--      - Evaluación SBR (Id: 9300) -> Con sus 4 opciones hijas: Avance, Historial, Resultados, Configuración SBR.
--      - Carga de Datos (Id: 7000) -> Con sus 4 opciones hijas: Carga XML, Estatus, Archivos Cargados, Monitor de Cierres.
--      - Informes (Id: 11000) -> Con sus 4 opciones hijas: Consulta de Informes, Carga de Informes, Notificaciones, Tipos de Informes.
--   3. Mapea directamente como hijas de Administración (ParentId = 9000):
--      - Perfiles (Id: 1)
--      - Administración de Usuarios (Id: 8100)
--      - Cambiar Contraseña (Id: 8110)
--      - Contribuciones (Id: 6502)
--      - Notificaciones (Id: 11120)
--   4. Elimina definitivamente el menú obsoleto no solicitado "Seguridad" (Id: 8000) y raíces obsoletas (6500).
--   5. Configura permisos en [dbo].[MenuPermission] para todos los roles activos (FFC y Cooperativas).
--   6. Incluye sentencias de GRANT para el usuario de aplicación.
-- ===============================================================================================

-- NOTA: Este script fue consolidado en 10_Aplicar_Reestructuracion_Menu_FFC.sql.
USE [FFC];
GO

SET NOCOUNT ON;

PRINT '===============================================================================================';
PRINT 'INICIANDO SCRIPT: TRASLADO Y REESTRUCTURACIÓN DE OPCIONES HACIA EL MENÚ DE ADMINISTRACIÓN';
PRINT '===============================================================================================';

BEGIN TRANSACTION;

BEGIN TRY

    -- ===========================================================================================
    -- 1. IDENTIFICAR O CREAR EL MÓDULO RAÍZ "ADMINISTRACIÓN" (ID = 9000)
    -- ===========================================================================================
    PRINT '>> [1/6] Verificando módulo raíz [Administración] (Id: 9000)...';

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
        PRINT '   -> Menú raíz Id 9000 actualizado a [Administración].';
    END
    ELSE
    BEGIN
        DECLARE @OldAdminId INT = NULL;
        SELECT TOP 1 @OldAdminId = Id 
        FROM dbo.Menu 
        WHERE ParentId IS NULL AND (MenuText LIKE '%Mantenimiento%' OR MenuText LIKE '%Administra%');

        IF @OldAdminId IS NOT NULL
        BEGIN
            UPDATE dbo.Menu
            SET MenuText = 'Administración',
                MenuURL = 'filter',
                ParentId = NULL,
                SortOrder = 8,
                MenuIcon = '<i class="fa fa-cogs"></i>',
                Description = 'Gestión integral de usuarios, accesos, cargas de datos, contribuciones, informes y configuración operativa.'
            WHERE Id = @OldAdminId;
            PRINT '   -> Menú raíz Id ' + CAST(@OldAdminId AS VARCHAR(10)) + ' actualizado a [Administración].';
        END
        ELSE
        BEGIN
            IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
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
            PRINT '   -> Menú raíz Id 9000 [Administración] creado exitosamente.';
        END
    END


    -- ===========================================================================================
    -- 2. CREACIÓN / CONFIGURACIÓN DE LOS NUEVOS PAPÁS (SUB-MÓDULOS) BAJO ADMINISTRACIÓN (ParentId = 9000)
    -- ===========================================================================================
    PRINT '>> [2/6] Creando y asegurando los nuevos papás contenedores bajo Administración...';

    -- 2.1 Papá: Gestión Operativa (Id: 9200)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9200)
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 9, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades y parámetros.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9200, 'Gestión Operativa', 'filter', 9000, 9, '<i class="fa fa-cogs"></i>', 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades y parámetros.');
        END
        PRINT '   -> Sub-módulo Id 9200 [Gestión Operativa] creado.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Gestión Operativa',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 9,
            MenuIcon = '<i class="fa fa-cogs"></i>',
            Description = 'Calendario, catálogo de fórmulas, catálogo SUGEF, tipos de XML, entidades y parámetros.'
        WHERE Id = 9200;
        PRINT '   -> Sub-módulo Id 9200 [Gestión Operativa] actualizado.';
    END

    -- 2.2 Papá: Evaluación SBR en Administración (Id: 9300)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9300)
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR (Avance y Resultados)', 'filter', 9000, 8, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9300, 'Evaluación SBR (Avance y Resultados)', 'filter', 9000, 8, '<i class="fa fa-tasks"></i>', 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.');
        END
        PRINT '   -> Sub-módulo Id 9300 [Evaluación SBR] creado.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación SBR (Avance y Resultados)',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 8,
            MenuIcon = '<i class="fa fa-tasks"></i>',
            Description = 'Monitoreo del avance, bitácora histórica y resultados consolidados SBR 24-22.'
        WHERE Id = 9300;
        PRINT '   -> Sub-módulo Id 9300 [Evaluación SBR] actualizado.';
    END

    -- 2.3 Papá: Carga de Datos (Id: 7000)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7000)
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (7000, 'Carga de Datos', 'filter', 9000, 4, '<i class="fa fa-upload"></i>', 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (7000, 'Carga de Datos', 'filter', 9000, 4, '<i class="fa fa-upload"></i>', 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.');
        END
        PRINT '   -> Sub-módulo Id 7000 [Carga de Datos] creado bajo Administración.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Carga de Datos',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 4,
            MenuIcon = '<i class="fa fa-upload"></i>',
            Description = 'Procesamiento de archivos XML, estatus de la carga y archivos cargados.'
        WHERE Id = 7000;
        PRINT '   -> Sub-módulo Id 7000 [Carga de Datos] actualizado bajo Administración.';
    END

    -- 2.4 Papá: Informes (Id: 11000)
    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11000)
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (11000, 'Informes', 'filter', 9000, 6, '<i class="fa fa-file-pdf-o"></i>', 'Carga de informes técnicos, consulta y tipos de informes.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (11000, 'Informes', 'filter', 9000, 6, '<i class="fa fa-file-pdf-o"></i>', 'Carga de informes técnicos, consulta y tipos de informes.');
        END
        PRINT '   -> Sub-módulo Id 11000 [Informes] creado bajo Administración.';
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Informes',
            MenuURL = 'filter',
            ParentId = 9000,
            SortOrder = 6,
            MenuIcon = '<i class="fa fa-file-pdf-o"></i>',
            Description = 'Carga de informes técnicos, consulta y tipos de informes.'
        WHERE Id = 11000;
        PRINT '   -> Sub-módulo Id 11000 [Informes] actualizado bajo Administración.';
    END


    -- ===========================================================================================
    -- 3. REASIGNACIÓN DE HIJOS A SUS NUEVOS PAPÁS
    -- ===========================================================================================
    PRINT '>> [3/6] Reasignando las opciones hijas a sus respectivos papás...';

    -- 3.1 HIJOS DE GESTIÓN OPERATIVA (ParentId = 9200):
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9050)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 1, MenuText = 'Calendario', MenuURL = 'Calendario/Index', MenuIcon = '<i class="fa fa-calendar"></i>' WHERE Id = 9050;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9110)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 2, MenuText = 'Catálogo de Fórmulas', MenuURL = 'Formula/Index', MenuIcon = '<i class="fa fa-calculator"></i>' WHERE Id = 9110;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9100)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 3, MenuText = 'Catálogo SUGEF', MenuURL = 'CatalogoCuenta/Index', MenuIcon = '<i class="fa fa-book"></i>' WHERE Id = 9100;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9120)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 4, MenuText = 'Tipos de XML', MenuURL = 'TipoXML/Index', MenuIcon = '<i class="fa fa-code"></i>' WHERE Id = 9120;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9130)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 5, MenuText = 'Entidades Afiliadas', MenuURL = 'Entidad/Index', MenuIcon = '<i class="fa fa-institution"></i>' WHERE Id = 9130;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9150)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 6, MenuText = 'Parámetros Globales', MenuURL = 'Parametros/Index', MenuIcon = '<i class="fa fa-cogs"></i>' WHERE Id = 9150;

    -- Roles (Id 9145)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 7, MenuText = 'Roles', MenuURL = 'Role/Index', MenuIcon = '<i class="fa fa-key"></i>', Description = 'Gestión de roles y configuración de la matriz de permisos de menú.' WHERE Id = 9145;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Role/Index', 'Role'))
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 7, MenuText = 'Roles', MenuIcon = '<i class="fa fa-key"></i>', Description = 'Gestión de roles y configuración de la matriz de permisos de menú.' WHERE MenuURL IN ('Role/Index', 'Role');
    ELSE
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (9145, 'Roles', 'Role/Index', 9200, 7, '<i class="fa fa-key"></i>', 'Gestión de roles y configuración de la matriz de permisos de menú.');
        END
    END

    -- Menú del Sistema (Id 13001)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 8, MenuText = 'Menú del Sistema', MenuURL = 'Menu/Index', MenuIcon = '<i class="fa fa-sitemap"></i>', Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.' WHERE Id = 13001;
    ELSE IF EXISTS (SELECT 1 FROM dbo.Menu WHERE MenuURL IN ('Menu/Index', 'Menu'))
        UPDATE dbo.Menu SET ParentId = 9200, SortOrder = 8, MenuText = 'Menú del Sistema', MenuIcon = '<i class="fa fa-sitemap"></i>', Description = 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.' WHERE MenuURL IN ('Menu/Index', 'Menu');
    ELSE
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.Menu ON;
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
            SET IDENTITY_INSERT dbo.Menu OFF;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
            VALUES (13001, 'Menú del Sistema', 'Menu/Index', 9200, 8, '<i class="fa fa-sitemap"></i>', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
        END
    END

    PRINT '   -> 8 opciones asignadas como hijas de Gestión Operativa (9200).';

    -- 3.2 HIJOS DE EVALUACIÓN SBR (ParentId = 9300):
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6002)
        UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 1, MenuText = 'Avance SBR', MenuURL = 'Evaluacion/Avance', MenuIcon = '<i class="fa fa-tasks"></i>' WHERE Id = 6002;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6003)
        UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 2, MenuText = 'Historial SBR', MenuURL = 'Evaluacion/Historial', MenuIcon = '<i class="fa fa-history"></i>' WHERE Id = 6003;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6005)
        UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 3, MenuText = 'Resultados SBR', MenuURL = 'Evaluacion/Resultados', MenuIcon = '<i class="fa fa-check-square-o"></i>' WHERE Id = 6005;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9135)
        UPDATE dbo.Menu SET ParentId = 9300, SortOrder = 4, MenuText = 'Configuración SBR', MenuURL = 'Evaluacion/Index', MenuIcon = '<i class="fa fa-list-alt"></i>' WHERE Id = 9135;

    PRINT '   -> 4 opciones asignadas como hijas de Evaluación SBR (9300).';

    -- 3.3 HIJOS DE CARGA DE DATOS (ParentId = 7000):
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7110)
        UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 1, MenuText = 'Carga de Datos (XML)', MenuURL = 'Archivo/Index', MenuIcon = '<i class="fa fa-upload"></i>' WHERE Id = 7110;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7100)
        UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 2, MenuText = 'Estatus de la Carga', MenuURL = 'Cierre/Index', MenuIcon = '<i class="fa fa-check-circle"></i>' WHERE Id = 7100;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7160)
        UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 3, MenuText = 'Archivos Cargados', MenuURL = 'Cierre', MenuIcon = '<i class="fa fa-archive"></i>' WHERE Id = 7160;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7150)
        UPDATE dbo.Menu SET ParentId = 7000, SortOrder = 4, MenuText = 'Monitor de Cierres', MenuURL = 'Cierre/Monitor', MenuIcon = '<i class="fa fa-desktop"></i>' WHERE Id = 7150;

    PRINT '   -> 4 opciones asignadas como hijas de Carga de Datos (7000).';

    -- 3.4 HIJOS DE INFORMES (ParentId = 11000):
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11110)
        UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 1, MenuText = 'Consulta de Informes', MenuURL = 'Explorer/Consulta', MenuIcon = '<i class="fa fa-file-pdf-o"></i>' WHERE Id = 11110;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11100)
        UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 2, MenuText = 'Carga de Informes', MenuURL = 'Explorer/Informe', MenuIcon = '<i class="fa fa-file-text"></i>' WHERE Id = 11100;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9140)
        UPDATE dbo.Menu SET ParentId = 11000, SortOrder = 3, MenuText = 'Tipos de Informe', MenuURL = 'TipoInforme/Index', MenuIcon = '<i class="fa fa-tags"></i>' WHERE Id = 9140;

    PRINT '   -> 3 opciones asignadas como hijas de Informes (11000).';

    -- 3.5 OPCIONES DIRECTAS BAJO ADMINISTRACIÓN (ParentId = 9000):
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 1)
        UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 1, MenuText = 'Perfiles', MenuURL = 'Cierre/Perfiles', MenuIcon = '<i class="fa fa-shield"></i>' WHERE Id = 1;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8100)
        UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 2, MenuText = 'Administración de Usuarios', MenuURL = 'Usuario/Index', MenuIcon = '<i class="fa fa-users"></i>' WHERE Id = 8100;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8110)
        UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 3, MenuText = 'Cambiar Contraseña', MenuURL = 'Usuario/ChangePassword', MenuIcon = '<i class="fa fa-key"></i>' WHERE Id = 8110;

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6502)
        UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 5, MenuText = 'Contribuciones', MenuURL = 'Facturacion/Index', MenuIcon = '<i class="fa fa-dollar"></i>' WHERE Id = 6502;

    -- Notificaciones: ÚNICAMENTE directa bajo Administración (ParentId = 9000)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11120)
        UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 7, MenuText = 'Notificaciones', MenuURL = 'Explorer/Notificacion', MenuIcon = '<i class="fa fa-bell"></i>', Description = 'Emisión de comunicados oficiales y circulares para entidades afiliadas.' WHERE Id = 11120;


    -- ===========================================================================================
    -- 4. ELIMINACIÓN DE SEGURIDAD (ID: 8000) Y CONTENEDORES OBSOLETOS
    -- ===========================================================================================
    PRINT '>> [4/6] Eliminando menú no solicitado [Seguridad] (Id: 8000) y raíces obsoletas...';

    DELETE FROM dbo.MenuPermission WHERE MenuId IN (8000, 6500);

    DELETE FROM dbo.Menu 
    WHERE Id IN (8000, 6500)
      AND NOT EXISTS (SELECT 1 FROM dbo.Menu h WHERE h.ParentId = dbo.Menu.Id AND h.Id NOT IN (8000, 6500));

    PRINT '   -> Menús obsoletos (Seguridad 8000 y Cálculos 6500) removidos exitosamente.';


    -- ===========================================================================================
    -- 5. ASIGNACIÓN INTEGRAL DE PERMISOS EN [dbo].[MenuPermission]
    -- ===========================================================================================
    PRINT '>> [5/6] Configurando permisos por perfil en dbo.MenuPermission...';

    -- 5.1 Permiso sobre Administración (9000) para todos los roles
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 9000, r.Id, 8, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 9000 AND mp.RoleId = r.Id);

    -- 5.2 Permiso sobre los nuevos papás (9200, 9300, 7000, 11000) para roles administrativos FFC (1, 2, 7, etc.)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT p.MenuId, r.Id, p.SortOrder, 1, 1, 1, 1
    FROM (VALUES (9200, 9), (9300, 8), (7000, 4), (11000, 6)) AS p(MenuId, SortOrder)
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = p.MenuId AND mp.RoleId = r.Id);

    -- 5.3 Permisos para todas las opciones hijas de 9200, 9300, 7000, 11000 y 9000 para roles FFC
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (m.ParentId IN (9000, 9200, 9300, 7000, 11000) OR m.Id IN (9000, 9200, 9300, 7000, 11000))
      AND (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- 5.4 Permisos para Cooperativas (RoleId 10 o EsEntidad = 1):
    -- Requerimiento: Usuarios SAC (8100), Contraseña (8110), Carga de datos (7110), Contribuciones (6502), Informes (11110)
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 0
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE m.Id IN (8100, 8110, 7110, 6502, 11110)
      AND (r.EsEntidad = 1 OR r.Id = 10)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- Asegurar IsRead = 1
    UPDATE dbo.MenuPermission SET IsRead = 1, IsUpdate = 1;

    PRINT '   -> Permisos asignados y normalizados.';


    -- ===========================================================================================
    -- 6. ASIGNACIÓN DE PRIVILEGIOS DE BASE DE DATOS (GRANT)
    -- ===========================================================================================
    PRINT '>> [6/6] Asignando permisos de base de datos (GRANT)...';

    GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Menu TO [public];
    GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.MenuPermission TO [public];

    COMMIT TRANSACTION;

    PRINT '===============================================================================================';
    PRINT '¡PROCESO FINALIZADO EXITOSAMENTE! TODOS LOS PAPÁS E HIJOS QUEDARON MAPEADOS CORRECTAMENTE.';
    PRINT '===============================================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT '!!! ERROR EN LA EJECUCIÓN DEL SCRIPT: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
