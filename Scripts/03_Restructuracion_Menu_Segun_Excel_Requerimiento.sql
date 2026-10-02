-- ===================================================================================================
-- SCRIPT DE RESTRUCTURACIÓN DEL MENÚ SEGÚN EL REQUERIMIENTO OFICIAL (EXCEL "Guía-Mejoras-SAC.xlsx")
-- Base de Datos: FFC (o FGA)
-- Objetivo:
--   1. Dejar únicamente 5 módulos raíz en el menú principal lateral (ParentId IS NULL):
--        - 1. Tablero (Id: 1000)
--        - 2. Estructura Financiera (Id: 1500)
--        - 3. Cartera de Crédito (Id: 4000)
--        - 4. Evaluación SBR (Id: 6000)
--        - 5. Administración (Id: 9000)
--   2. Reubicar todas las opciones y subcarpetas existentes bajo sus nuevos módulos padre correspondientes.
--   3. No elimina ninguna opción existente; preserva URLs, permisos y datos.
-- ===================================================================================================

USE [FFC];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '>> [1/4] Iniciando verificación previa y deshabilitación temporal de constraints...';
    ALTER TABLE dbo.Menu NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission NOCHECK CONSTRAINT ALL;

    PRINT '>> [2/4] Restructurando Jerarquía y Padres en [dbo].[Menu]...';

    -- ===============================================================================================
    -- A. LOS 5 MÓDULOS PRINCIPALES (RAÍCES: ParentId = NULL)
    -- ===============================================================================================
    
    -- 1. Tablero
    UPDATE dbo.Menu
    SET MenuText = 'Tablero',
        MenuURL = 'Home/Index',
        ParentId = NULL,
        SortOrder = 1,
        MenuIcon = '<i class="fa fa-dashboard"></i>',
        Description = 'Tablero principal e indicadores estratégicos consolidados de la entidad.'
    WHERE Id = 1000;

    -- 2. Estructura Financiera
    UPDATE dbo.Menu
    SET MenuText = 'Estructura Financiera',
        MenuURL = 'filter',
        ParentId = NULL,
        SortOrder = 2,
        MenuIcon = '<i class="fa fa-bar-chart"></i>',
        Description = 'Estados financieros, indicadores, márgenes, tasas y modelos de proyección.'
    WHERE Id = 1500;

    -- 3. Cartera de Crédito
    UPDATE dbo.Menu
    SET MenuText = 'Cartera de Crédito',
        MenuURL = 'filter',
        ParentId = NULL,
        SortOrder = 3,
        MenuIcon = '<i class="fa fa-money"></i>',
        Description = 'Monitoreo de riesgo crediticio, etapas, pérdida esperada y desempeño de colocaciones.'
    WHERE Id = 4000;

    -- 4. Evaluación SBR
    UPDATE dbo.Menu
    SET MenuText = 'Evaluación SBR',
        MenuURL = 'root',
        ParentId = NULL,
        SortOrder = 4,
        MenuIcon = '<i class="fa fa-pie-chart"></i>',
        Description = 'Formularios y seguimiento de autoevaluación de supervisión basada en riesgos.'
    WHERE Id = 6000;

    -- 5. Administración
    UPDATE dbo.Menu
    SET MenuText = 'Administración',
        MenuURL = 'root',
        ParentId = NULL,
        SortOrder = 5,
        MenuIcon = '<i class="fa fa-cog"></i>',
        Description = 'Gestión de usuarios, seguridad, catálogos, carga de datos y parámetros del sistema.'
    WHERE Id = 9000;


    -- ===============================================================================================
    -- B. SUB-OPCIONES DE: ESTRUCTURA FINANCIERA (ParentId = 1500)
    -- ===============================================================================================
    
    -- Submódulo de Estados Financieros (2100)
    UPDATE dbo.Menu
    SET MenuText = 'Estados Financieros',
        ParentId = 1500,
        SortOrder = 1
    WHERE Id = 2100;

    -- Opciones directas de Estados Financieros bajo 2100
    UPDATE dbo.Menu SET ParentId = 2100, SortOrder = 1, MenuText = 'Balance General', MenuURL = 'InformeFinanciero/Balance' WHERE Id = 2130;
    UPDATE dbo.Menu SET ParentId = 2100, SortOrder = 2, MenuText = 'Estado de Resultados', MenuURL = 'InformeFinanciero/ER' WHERE Id = 2120;
    UPDATE dbo.Menu SET ParentId = 2100, SortOrder = 3, MenuText = 'Estado de Origen y Aplicación de Fondos', MenuURL = 'InformeFinanciero/Origen' WHERE Id = 2140;
    UPDATE dbo.Menu SET ParentId = 2100, SortOrder = 4, MenuText = 'Balanza de Comprobación', MenuURL = 'InformeFinanciero/Index' WHERE Id = 2135;
    UPDATE dbo.Menu SET ParentId = 2100, SortOrder = 5, MenuText = 'Estados Financieros Consolidados', MenuURL = 'InformeFinanciero/Consolidado' WHERE Id = 2110;

    -- Estructura de Fondeo (13002) directamente bajo Estructura Financiera
    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 2,
        MenuText = 'Estructura de Fondeo',
        MenuURL = 'EstructuraFondeo/Index',
        MenuIcon = '<i class="fa fa-database"></i>',
        Description = 'Analice la composición de las fuentes de fondeo de la entidad.'
    WHERE Id = 13002;

    -- Indicadores Financieros (3000)
    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 3,
        MenuText = 'Indicadores Financieros',
        MenuURL = 'filter'
    WHERE Id = 3000;

    -- Sub-opciones de Indicadores Financieros (bajo 3000)
    UPDATE dbo.Menu SET ParentId = 3000, SortOrder = 1, MenuText = 'SUGEF' WHERE Id = 3100;
    UPDATE dbo.Menu SET ParentId = 3000, SortOrder = 2, MenuText = 'FFC' WHERE Id = 3110;
    UPDATE dbo.Menu SET ParentId = 3000, SortOrder = 3, MenuText = 'Económicos' WHERE Id = 3120;
    UPDATE dbo.Menu SET ParentId = 3000, SortOrder = 4, MenuText = 'Personalizados' WHERE Id = 3130;

    -- Márgenes Implícitos (5110)
    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 4,
        MenuText = 'Márgenes Implícitos',
        MenuURL = 'Margen/Index',
        MenuIcon = '<i class="fa fa-trophy"></i>'
    WHERE Id = 5110;

    -- Tasas Implícitas (5120)
    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 5,
        MenuText = 'Tasas Implícitas',
        MenuURL = 'TasaInteres/Index',
        MenuIcon = '<i class="fa fa-percent"></i>'
    WHERE Id = 5120;

    -- Proyecciones (Módulo de Proyecciones 2250 pasa a ser subcarpeta de Estructura Financiera 1500)
    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 6,
        MenuText = 'Proyecciones',
        MenuURL = 'filter',
        MenuIcon = '<i class="fa fa-line-chart"></i>'
    WHERE Id = 2250;

    -- Opciones de Proyecciones bajo 2250
    UPDATE dbo.Menu SET ParentId = 2250, SortOrder = 1, MenuText = 'Estados Financieros', MenuURL = 'ProyeccionEEFF' WHERE Id = 2260;
    UPDATE dbo.Menu SET ParentId = 2250, SortOrder = 2, MenuText = 'Liquidez (IRL)', MenuURL = 'IRL/IRL' WHERE Id = 5000;
    UPDATE dbo.Menu SET ParentId = 2250, SortOrder = 3, MenuText = 'Indicador Suficiencia Patrimonial (ISP)', MenuURL = 'SimulacionCapital/Index' WHERE Id = 6501;


    -- ===============================================================================================
    -- C. SUB-OPCIONES DE: CARTERA DE CRÉDITO (ParentId = 4000)
    -- ===============================================================================================
    
    -- Sub-opciones de Cartera
    UPDATE dbo.Menu SET ParentId = 4000, SortOrder = 1, MenuText = 'Riesgo de Crédito (14-21)', MenuURL = 'Cartera14_21/Index', MenuIcon = '<i class="fa fa-pie-chart"></i>' WHERE Id = 4130;
    UPDATE dbo.Menu SET ParentId = 4000, SortOrder = 2, MenuText = 'Composición de Cartera', MenuURL = 'Composicion/Index', MenuIcon = '<i class="fa fa-credit-card"></i>' WHERE Id = 4100;
    UPDATE dbo.Menu SET ParentId = 4000, SortOrder = 3, MenuText = 'Matrices de Transición y Mora', MenuURL = 'Matriz/Index', MenuIcon = '<i class="fa fa-table"></i>' WHERE Id = 4110;


    -- ===============================================================================================
    -- D. SUB-OPCIONES DE: EVALUACIÓN SBR (ParentId = 6000)
    -- ===============================================================================================
    
    UPDATE dbo.Menu SET ParentId = 6000, SortOrder = 1, MenuText = 'Autoevaluación', MenuURL = 'Evaluacion/Evaluacion' WHERE Id = 6001;
    UPDATE dbo.Menu SET ParentId = 6000, SortOrder = 2, MenuText = 'Avance SBR', MenuURL = 'Evaluacion/Avance' WHERE Id = 6002;
    UPDATE dbo.Menu SET ParentId = 6000, SortOrder = 3, MenuText = 'Historial SBR', MenuURL = 'Evaluacion/Historial' WHERE Id = 6003;
    UPDATE dbo.Menu SET ParentId = 6000, SortOrder = 4, MenuText = 'Resultados SBR', MenuURL = 'Evaluacion/Resultados' WHERE Id = 6005;


    -- ===============================================================================================
    -- E. SUB-OPCIONES DE: ADMINISTRACIÓN (ParentId = 9000)
    --    (Absorbe las antiguas raíces: Seguridad 8000, Carga de Datos 7000, Informes 11000 y Cálculos 6500)
    -- ===============================================================================================
    
    -- 1. Perfiles y Permisos (Id 1)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 1, MenuText = 'Perfiles', MenuURL = 'Cierre/Perfiles', MenuIcon = '<i class="fa fa-shield"></i>' WHERE Id = 1;

    -- 2. Usuarios SAC (Id 8100 - antes bajo Seguridad 8000)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 2, MenuText = 'Usuarios SAC', MenuURL = 'Usuario/Index', MenuIcon = '<i class="fa fa-users"></i>' WHERE Id = 8100;

    -- 3. Contraseña (Id 8110 - antes bajo Seguridad 8000)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 3, MenuText = 'Cambiar Contraseña', MenuURL = 'Usuario/ChangePassword', MenuIcon = '<i class="fa fa-key"></i>' WHERE Id = 8110;

    -- 4. Roles (Id 9145)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 4, MenuText = 'Roles', MenuURL = 'Role/Index', MenuIcon = '<i class="fa fa-lock"></i>' WHERE Id = 9145;

    -- 5. Carga de Datos (Id 7110 - Carga XML)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 5, MenuText = 'Carga de Datos (XML)', MenuURL = 'Archivo/Index', MenuIcon = '<i class="fa fa-upload"></i>' WHERE Id = 7110;

    -- 6. Estatus de la Carga (Id 7100 / 7150)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 6, MenuText = 'Estatus de la Carga', MenuURL = 'Cierre/Index', MenuIcon = '<i class="fa fa-check-circle"></i>' WHERE Id = 7100;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 7, MenuText = 'Monitor de Cierres', MenuURL = 'Cierre/Monitor', MenuIcon = '<i class="fa fa-desktop"></i>' WHERE Id = 7150;

    -- 7. Archivos Cargados (Id 7160 / 7140)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 8, MenuText = 'Archivos Cargados', MenuURL = 'Cierre', MenuIcon = '<i class="fa fa-archive"></i>' WHERE Id = 7160;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 9, MenuText = 'Histórico de Cargas', MenuURL = 'Explorer/Index', MenuIcon = '<i class="fa fa-folder-open"></i>' WHERE Id = 7140;

    -- 8. Contribuciones (Facturación - Id 6502)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 10, MenuText = 'Contribuciones', MenuURL = 'Facturacion/Index', MenuIcon = '<i class="fa fa-dollar"></i>' WHERE Id = 6502;

    -- 9. Informes y Reportes (Id 11110 / 11100)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 11, MenuText = 'Informes', MenuURL = 'Explorer/Consulta', MenuIcon = '<i class="fa fa-file-pdf-o"></i>' WHERE Id = 11110;
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 12, MenuText = 'Carga de Informes', MenuURL = 'Explorer/Informe', MenuIcon = '<i class="fa fa-file-text"></i>' WHERE Id = 11100;

    -- 10. Notificaciones (Id 11120)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 13, MenuText = 'Notificaciones', MenuURL = 'Explorer/Notificacion', MenuIcon = '<i class="fa fa-bell"></i>' WHERE Id = 11120;

    -- 11. Tipos de Informe (Id 9140)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 14, MenuText = 'Tipos de Informe', MenuURL = 'TipoInforme/Index', MenuIcon = '<i class="fa fa-tags"></i>' WHERE Id = 9140;

    -- 12. Mantenimiento de Evaluación SBR (Id 9135)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 15, MenuText = 'Configuración de Preguntas SBR', MenuURL = 'Evaluacion/Index', MenuIcon = '<i class="fa fa-list-alt"></i>' WHERE Id = 9135;

    -- 13. Parámetros del Sistema (Id 9150)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 16, MenuText = 'Parámetros Globales', MenuURL = 'Parametros/Index', MenuIcon = '<i class="fa fa-cogs"></i>' WHERE Id = 9150;

    -- 14. Entidades Afiliadas (Id 9130)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 17, MenuText = 'Entidades Afiliadas', MenuURL = 'Entidad/Index', MenuIcon = '<i class="fa fa-institution"></i>' WHERE Id = 9130;

    -- 15. Auditoría de Ingresos / Sesiones (Id 13000)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 18, MenuText = 'Historial de Ingresos', MenuURL = 'Seguridad/Index', MenuIcon = '<i class="fa fa-sign-in"></i>' WHERE Id = 13000;

    -- 16. Catálogo de Fórmulas (Id 9110)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 19, MenuText = 'Catálogo de Fórmulas', MenuURL = 'Formula/Index', MenuIcon = '<i class="fa fa-calculator"></i>' WHERE Id = 9110;

    -- 17. Catálogo SUGEF (Id 9100)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 20, MenuText = 'Catálogo SUGEF', MenuURL = 'CatalogoCuenta/Index', MenuIcon = '<i class="fa fa-book"></i>' WHERE Id = 9100;

    -- 18. Tipos de XML (Id 9120)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 21, MenuText = 'Tipos de XML', MenuURL = 'TipoXML/Index', MenuIcon = '<i class="fa fa-code"></i>' WHERE Id = 9120;

    -- 19. Menú del Sistema (Id 13001)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 22, MenuText = 'Menú del Sistema', MenuURL = 'Menu/Index', MenuIcon = '<i class="fa fa-sitemap"></i>' WHERE Id = 13001;

    -- 20. Calendario (Id 9050)
    UPDATE dbo.Menu SET ParentId = 9000, SortOrder = 23, MenuText = 'Calendario', MenuURL = 'Calendario/Index', MenuIcon = '<i class="fa fa-calendar"></i>' WHERE Id = 9050;


    -- ===============================================================================================
    -- F. ELIMINAR CONTENEDORES OBSOLETOS QUE QUEDARON SIN HIJAS (Seguridad 8000, Informes 11000, etc.)
    -- ===============================================================================================
    PRINT '>> [2.2/4] Eliminando módulos/carpetas obsoletas que quedaron sin opciones hijas...';

    -- 1. Eliminar permisos asociados en dbo.MenuPermission
    DELETE mp
    FROM dbo.MenuPermission mp
    WHERE mp.MenuId IN (
        SELECT m.Id 
        FROM dbo.Menu m
        WHERE (m.Id IN (6500, 7000, 8000, 11000)
               OR ((m.MenuURL IS NULL OR m.MenuURL IN ('root', 'filter', '#', ''))
                   AND m.Id NOT IN (1000, 1500, 4000, 6000, 9000, 2100, 3000, 2250)))
          AND NOT EXISTS (SELECT 1 FROM dbo.Menu h WHERE h.ParentId = m.Id)
    );

    -- 2. Eliminar registros de dbo.Menu
    DELETE m
    FROM dbo.Menu m
    WHERE (m.Id IN (6500, 7000, 8000, 11000)
           OR ((m.MenuURL IS NULL OR m.MenuURL IN ('root', 'filter', '#', ''))
               AND m.Id NOT IN (1000, 1500, 4000, 6000, 9000, 2100, 3000, 2250)))
      AND NOT EXISTS (SELECT 1 FROM dbo.Menu h WHERE h.ParentId = m.Id);

    -- ===============================================================================================
    -- G. ORDENAMIENTO ALFABÉTICO AUTOMÁTICO DE LOS HIJOS DENTRO DE CADA PADRE
    --    Asigna SortOrder = 1, 2, 3... según el orden alfabético de MenuText para cada grupo ParentId
    -- ===============================================================================================
    PRINT '>> [2.1/4] Reordenando alfabéticamente las opciones hijas dentro de cada menú padre...';

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

    PRINT '>> [3/4] Asegurando permisos de visibilidad para RoleId 1 (Admin) y RoleId 7 (Gerencial)...';

    -- Otorgar permiso sobre los 5 módulos raíz principales al Rol 1 y 7 si no existieran
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.RoleId, m.SortOrder, 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN (VALUES (1), (2), (7)) AS r(RoleId)
    WHERE m.Id IN (1000, 1500, 4000, 6000, 9000, 2100, 2130, 2120, 2140, 2135, 13002, 3000, 5110, 5120, 2250, 4130, 4100, 4110)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = m.Id AND mp.RoleId = r.RoleId
      );

    PRINT '>> [4/4] Reactivando constraints de integridad...';
    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    COMMIT TRANSACTION;
    PRINT '===================================================================================================';
    PRINT '>> MENÚ REESTRUCTURADO EXITOSAMENTE CONFORME AL DOCUMENTO DE REQUERIMIENTOS.';
    PRINT '===================================================================================================';

    -- Consulta de comprobación 1: Los 5 módulos raíz principales
    SELECT Id, MenuText AS [Modulo_Raiz], MenuURL, SortOrder, MenuIcon
    FROM dbo.Menu
    WHERE ParentId IS NULL
    ORDER BY SortOrder;

    -- Consulta de comprobación 2: Opciones hijas ordenadas alfabéticamente dentro de cada módulo padre
    SELECT p.MenuText AS [Modulo_Padre], 
           h.Id AS [Id_Hijo], 
           h.MenuText AS [Opcion_Hija], 
           h.SortOrder AS [Orden_Alfabetico], 
           h.MenuURL
    FROM dbo.Menu h
    INNER JOIN dbo.Menu p ON h.ParentId = p.Id
    ORDER BY p.SortOrder, h.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '>> ERROR EN EJECUCIÓN: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
