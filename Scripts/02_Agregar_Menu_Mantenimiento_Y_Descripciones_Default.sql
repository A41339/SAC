-- =======================================================================================
-- SCRIPT: REGISTRO DE OPCIÓN DE MENÚ Y CARGA DE DESCRIPCIONES POR DEFECTO EN TABLA [Menu]
-- =======================================================================================
USE [FGA];
GO

SET NOCOUNT ON;

PRINT '-----------------------------------------------------------------------------------';
PRINT 'INICIANDO CONFIGURACIÓN DE OPCIÓN DE MENÚ Y OBSERVACIONES / DESCRIPCIONES DEFAULT';
PRINT '-----------------------------------------------------------------------------------';

-- =======================================================================================
-- 1. IDENTIFICAR MÓDULO PADRE DE ADMINISTRACIÓN O MANTENIMIENTO
-- =======================================================================================
DECLARE @ParentAdminId INT;

SELECT TOP 1 @ParentAdminId = Id 
FROM [dbo].[Menu] 
WHERE (MenuText LIKE '%Administra%' OR MenuText LIKE '%Mantenimiento%') 
  AND (ParentId IS NULL OR MenuURL = 'root' OR MenuURL = '#')
ORDER BY CASE 
    WHEN MenuText LIKE '%Administra%' THEN 1 
    WHEN MenuText LIKE '%Mantenimiento%' THEN 2 
    ELSE 3 
END;

IF @ParentAdminId IS NULL
BEGIN
    SELECT TOP 1 @ParentAdminId = Id FROM [dbo].[Menu] WHERE ParentId IS NULL ORDER BY Id;
END

PRINT 'Módulo Padre seleccionado Id: ' + CAST(ISNULL(@ParentAdminId, 0) AS VARCHAR(10));

-- =======================================================================================
-- 2. INSERTAR O ACTUALIZAR LA OPCIÓN "Menú del Sistema" (Menu/Index)
-- =======================================================================================
DECLARE @MenuId INT;
SELECT @MenuId = Id FROM [dbo].[Menu] WHERE MenuURL IN ('Menu/Index', 'Menu');

IF @MenuId IS NULL
BEGIN
    DECLARE @NextId INT, @NextSortOrder INT;
    SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
    SELECT @NextSortOrder = ISNULL(MAX(SortOrder), 0) + 1 FROM [dbo].[Menu] WHERE ParentId = @ParentAdminId;

    IF COLUMNPROPERTY(OBJECT_ID('dbo.Menu'), 'Id', 'IsIdentity') = 1
    BEGIN
        SET IDENTITY_INSERT [dbo].[Menu] ON;
        INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [SortOrder], [MenuIcon], [Description])
        VALUES (@NextId, 'Menú del Sistema', 'Menu/Index', @ParentAdminId, @NextSortOrder, 'fa fa-sitemap', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
        SET IDENTITY_INSERT [dbo].[Menu] OFF;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [SortOrder], [MenuIcon], [Description])
        VALUES (@NextId, 'Menú del Sistema', 'Menu/Index', @ParentAdminId, @NextSortOrder, 'fa fa-sitemap', 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
    END

    SET @MenuId = @NextId;
    PRINT '-> Opción "Menú del Sistema" creada con Id: ' + CAST(@MenuId AS VARCHAR(10));
END
ELSE
BEGIN
    UPDATE [dbo].[Menu]
    SET [MenuText] = 'Menú del Sistema',
        [MenuIcon] = 'fa fa-sitemap',
        [Description] = ISNULL([Description], 'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.')
    WHERE [Id] = @MenuId;
    PRINT '-> Opción "Menú del Sistema" ya existía (Id: ' + CAST(@MenuId AS VARCHAR(10)) + '). Actualizada.';
END

-- =======================================================================================
-- 3. ASIGNAR PERMISOS EN [MenuPermission] PARA EL ROLEID 1 (ADMINISTRADOR)
-- =======================================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuPermission] WHERE [MenuId] = @MenuId AND [RoleId] = 1)
BEGIN
    INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
    VALUES (@MenuId, 1, 1, 1, 1, 1, 1);
    PRINT '-> Permiso asignado al RoleId 1 en MenuPermission.';
END
ELSE
BEGIN
    UPDATE [dbo].[MenuPermission]
    SET [IsCreate] = 1, [IsRead] = 1, [IsUpdate] = 1, [IsDelete] = 1
    WHERE [MenuId] = @MenuId AND [RoleId] = 1;
    PRINT '-> Permiso del RoleId 1 actualizado en MenuPermission.';
END

-- =======================================================================================
-- 4. REGISTRAR OBSERVACIONES / DESCRIPCIONES DEFAULT EN LA COLUMNA [Description]
--    (Solo actualiza las que están vacías o NULL para no sobreescribir las personalizadas)
-- =======================================================================================
PRINT 'Actualizando observaciones / descripciones por defecto en la tabla Menu...';

-- Estructura Financiera
UPDATE [dbo].[Menu] 
SET [Description] = 'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Estado%Financiero%' OR [MenuURL] LIKE '%InformeFinanciero%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Analice la composición de las fuentes de fondeo de la entidad.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Estructura%Fondeo%' OR [MenuText] LIKE '%Composici%' OR [MenuURL] LIKE '%Composicion%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Analice los principales indicadores financieros de la entidad.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Indicador%Financiero%' OR [MenuURL] LIKE '%Indicadores%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Revise los márgenes financieros y operativos.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Margen%' OR [MenuText] LIKE '%Márgen%' OR [MenuURL] LIKE '%Margen%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Consulte las tasas implícitas por tipo de activo y pasivo.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Tabla%Impl%' OR [MenuText] LIKE '%Tasa%Inter%' OR [MenuURL] LIKE '%TasaInteres%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Realice proyecciones financieras de la entidad.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Proyecci%' OR [MenuURL] LIKE '%Proyeccion%');

-- Cartera de Crédito y Riesgos
UPDATE [dbo].[Menu] 
SET [Description] = 'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%14-21%' OR [MenuURL] LIKE '%Cartera14_21%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Análisis de transición de mora y deterioro de créditos.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Matriz%Mora%' OR [MenuText] LIKE '%Mora%' OR [MenuURL] LIKE '%Matriz%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Cálculo e insumos del Indicador de Riesgo de Liquidez.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%IRL%' OR [MenuText] LIKE '%Liquidez%' OR [MenuURL] LIKE '%IRL%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Simulación de suficiencia patrimonial y escenarios de estrés de capital.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Simula%Capital%' OR [MenuURL] LIKE '%SimulacionCapital%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Análisis comparativo de indicadores financieros del sector y la industria.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Industria%' OR [MenuURL] LIKE '%Industria%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Seguimiento de tasas de interés, inflación y variables macroeconómicas.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Econ%mico%' OR [MenuURL] LIKE '%IndEconomico%');

-- Mantenimientos y Administración
UPDATE [dbo].[Menu] 
SET [Description] = 'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Rol%' OR [MenuText] LIKE '%Perfil%' OR [MenuURL] LIKE '%Role%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Mapeo y homologación del catálogo contable según la normativa SUGEF.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Cat%logo%' OR [MenuURL] LIKE '%CatalogoCuenta%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Gestión de cuentas de usuario, credenciales y estados de acceso.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Usuario%' OR [MenuURL] LIKE '%Usuario%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Catálogo de entidades participantes y parámetros generales.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Entidad%' OR [MenuURL] LIKE '%Entidad%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Configuración de variables operativas y parámetros globales del sistema.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Par%metro%' OR [MenuURL] LIKE '%Parametros%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Monitoreo y administración de fechas de corte y cierres contables.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Cierre%' OR [MenuURL] LIKE '%Cierre%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Consulta, descarga y auditoría de archivos y reportes procesados.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Archivo%Cargado%' OR [MenuText] LIKE '%Consultar Archivo%' OR [MenuURL] LIKE '%Explorer%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Definición y fórmulas de cálculo de indicadores financieros y normativos.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%F%rmula%' OR [MenuURL] LIKE '%Formula%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Publicación de boletines y comunicados informativos para las entidades.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Noticia%' OR [MenuURL] LIKE '%Noticia%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Cronograma de eventos, fechas de entrega y compromisos regulatorios.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Calendario%' OR [MenuURL] LIKE '%Calendario%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Gestión de facturación y cuotas de mantenimiento de la plataforma.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Factura%' OR [MenuURL] LIKE '%Facturacion%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Carga masiva, validación y procesamiento de archivos XML.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Archivo%' OR [MenuURL] LIKE '%Archivo%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Historial de conexiones, sesiones activas y registro de auditoría.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Seguridad%' OR [MenuText] LIKE '%Sesi%n%' OR [MenuURL] LIKE '%Seguridad%');

-- Mercado de Créditos
UPDATE [dbo].[Menu] 
SET [Description] = 'Consulte las ofertas y solicitudes de créditos vigentes en el mercado interinstitucional.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Mercado%Cr%dito%' OR [MenuURL] = 'Creditos/Index');

UPDATE [dbo].[Menu] 
SET [Description] = 'Seguimiento al estado de sus solicitudes de financiamiento.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Mi%Solicitud%' OR [MenuURL] LIKE '%MisSolicitudes%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Publique nuevas ofertas de colocación de cartera en el mercado.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Publicar%Oferta%' OR [MenuURL] LIKE '%Publicar%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Administre las ofertas y solicitudes recibidas de otras entidades.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Gesti%n Recibida%' OR [MenuURL] LIKE '%Gestion%');

-- Gobierno Corporativo y Evaluación
UPDATE [dbo].[Menu] 
SET [Description] = 'Formularios de autoevaluación de supervisión basada en riesgos (SBR).'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Autoevalua%' OR [MenuURL] LIKE '%Autoevaluacion%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Monitoreo del porcentaje de avance y nivel de respuestas completadas.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Avance%' OR [MenuURL] LIKE '%Avance%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Registro histórico de evaluaciones y calificaciones obtenidas.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Historial%' OR [MenuURL] LIKE '%Historial%');

UPDATE [dbo].[Menu] 
SET [Description] = 'Informe de resultados consolidados y niveles de cumplimiento.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([MenuText] LIKE '%Resultado%' OR [MenuURL] LIKE '%Resultado%');

-- Para cualquier otra opción hija que aún no tenga descripción, asignar descripción genérica descriptiva
UPDATE [dbo].[Menu] 
SET [Description] = 'Acceda a la información y reportes detallados de ' + [MenuText] + '.'
WHERE ([Description] IS NULL OR LTRIM(RTRIM([Description])) = '')
  AND ([ParentId] IS NOT NULL)
  AND ([MenuURL] IS NOT NULL AND [MenuURL] NOT IN ('root', '#'));

PRINT '-----------------------------------------------------------------------------------';
PRINT 'PROCESO FINALIZADO EXITOSAMENTE.';
PRINT '-----------------------------------------------------------------------------------';

-- Consulta de verificación
SELECT 
    m.[Id],
    p.[MenuText] AS [ModuloPadre],
    m.[MenuText] AS [Opcion],
    m.[MenuURL],
    m.[MenuIcon],
    m.[Description] AS [DescripcionFicha]
FROM [dbo].[Menu] m
LEFT JOIN [dbo].[Menu] p ON m.[ParentId] = p.[Id]
WHERE m.[ParentId] IS NOT NULL
ORDER BY ISNULL(p.[MenuText], 'Z'), m.[SortOrder], m.[MenuText];

GO
