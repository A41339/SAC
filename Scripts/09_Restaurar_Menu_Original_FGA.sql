-- ===================================================================================================
-- SCRIPT DE RESTAURACION / REGENERACION EXACTA DEL MENU Y PERMISOS DE FGA
-- Base de Datos Objetivo: [FGA]
-- Especificacion: Basado exactamente en la definicion y jerarquia oficial de FGA provista por el usuario.
-- Fecha: 2026-09-30
-- ===================================================================================================

USE [FGA];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '===============================================================================================';
PRINT 'INICIANDO SCRIPT: RESTAURACION Y REGENERACION INTEGRAL DE MENUS EN [FGA]';
PRINT '===============================================================================================';

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '>> [1/5] Deshabilitando temporalmente constraints...';
    ALTER TABLE dbo.Menu NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission NOCHECK CONSTRAINT ALL;

    PRINT '>> [2/5] Sincronizando catalogo de Menu (dbo.Menu) con la definicion exacta de FGA...';
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Menu ON;

    -- Id: 1 | Perfiles
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 1)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Perfiles', MenuURL = N'Cierre/Perfiles', ParentId = 9000, SortOrder = 8, MenuIcon = N'<i class="fa fa-shield"></i>', Description = N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
        WHERE Id = 1;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (1, N'Perfiles', N'Cierre/Perfiles', 9000, 8, N'<i class="fa fa-shield"></i>', N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.');
    END

    -- Id: 1000 | Tablero
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 1000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tablero', MenuURL = N'Home/Index', ParentId = NULL, SortOrder = 1, MenuIcon = N'<i class="fa fa-dashboard"></i>', Description = N'Acceda a la información y reportes detallados de Tablero.'
        WHERE Id = 1000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (1000, N'Tablero', N'Home/Index', NULL, 1, N'<i class="fa fa-dashboard"></i>', N'Acceda a la información y reportes detallados de Tablero.');
    END

    -- Id: 1500 | Estructura Financiera
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 1500)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estructura Financiera', MenuURL = N'filter', ParentId = NULL, SortOrder = 2, MenuIcon = N'<i class="fa fa-bar-chart"></i>', Description = NULL
        WHERE Id = 1500;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (1500, N'Estructura Financiera', N'filter', NULL, 2, N'<i class="fa fa-bar-chart"></i>', NULL);
    END

    -- Id: 2100 | Estructura Financiera
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estructura Financiera', MenuURL = N'filter', ParentId = 1500, SortOrder = 1, MenuIcon = N'<i class="fa fa-money"></i>', Description = N'Acceda a la información y reportes detallados de Estructura Financiera.'
        WHERE Id = 2100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2100, N'Estructura Financiera', N'filter', 1500, 1, N'<i class="fa fa-money"></i>', N'Acceda a la información y reportes detallados de Estructura Financiera.');
    END

    -- Id: 2110 | Estados Financieros
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estados Financieros', MenuURL = N'InformeFinanciero/Consolidado', ParentId = 2100, SortOrder = 4, MenuIcon = N'<i class="fa fa-file-text-o"></i>', Description = N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
        WHERE Id = 2110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2110, N'Estados Financieros', N'InformeFinanciero/Consolidado', 2100, 4, N'<i class="fa fa-file-text-o"></i>', N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.');
    END

    -- Id: 2120 | Estado de Resultados
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estado de Resultados', MenuURL = N'InformeFinanciero/ER', ParentId = 2100, SortOrder = 3, MenuIcon = N'<i class="fa fa-bar-chart"></i>', Description = N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
        WHERE Id = 2120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2120, N'Estado de Resultados', N'InformeFinanciero/ER', 2100, 3, N'<i class="fa fa-bar-chart"></i>', N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.');
    END

    -- Id: 2130 | Balance General
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Balance General', MenuURL = N'InformeFinanciero/Balance', ParentId = 2100, SortOrder = 1, MenuIcon = N'<i class="fa fa-balance-scale"></i>', Description = N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
        WHERE Id = 2130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2130, N'Balance General', N'InformeFinanciero/Balance', 2100, 1, N'<i class="fa fa-balance-scale"></i>', N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.');
    END

    -- Id: 2135 | Balanza Comprobación
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2135)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Balanza Comprobación', MenuURL = N'InformeFinanciero/Index', ParentId = 2100, SortOrder = 2, MenuIcon = N'<i class="fa fa-file"></i>', Description = N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
        WHERE Id = 2135;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2135, N'Balanza Comprobación', N'InformeFinanciero/Index', 2100, 2, N'<i class="fa fa-file"></i>', N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.');
    END

    -- Id: 2140 | Origen - Aplicación
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2140)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Origen - Aplicación', MenuURL = N'InformeFinanciero/Origen', ParentId = 2100, SortOrder = 5, MenuIcon = N'<i class="fa fa-circle-o"></i>', Description = N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.'
        WHERE Id = 2140;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2140, N'Origen - Aplicación', N'InformeFinanciero/Origen', 2100, 5, N'<i class="fa fa-circle-o"></i>', N'Consulte la situación financiera, los resultados y el movimiento de fondos de la entidad.');
    END

    -- Id: 2250 | Módulo de Proyecciones
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2250)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Módulo de Proyecciones', MenuURL = N'filter', ParentId = NULL, SortOrder = 3, MenuIcon = N'<i class="fa fa-eye"></i>', Description = N'Realice proyecciones financieras de la entidad.'
        WHERE Id = 2250;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2250, N'Módulo de Proyecciones', N'filter', NULL, 3, N'<i class="fa fa-eye"></i>', N'Realice proyecciones financieras de la entidad.');
    END

    -- Id: 2260 | Proyección
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2260)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Proyección', MenuURL = N'ProyeccionEEFF', ParentId = 2250, SortOrder = 1, MenuIcon = N'<i class="fa fa-file"></i>', Description = N'Realice proyecciones financieras de la entidad.'
        WHERE Id = 2260;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2260, N'Proyección', N'ProyeccionEEFF', 2250, 1, N'<i class="fa fa-file"></i>', N'Realice proyecciones financieras de la entidad.');
    END

    -- Id: 3000 | Indicadores Financieros
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Indicadores Financieros', MenuURL = N'filter', ParentId = 1500, SortOrder = 2, MenuIcon = N'<i class="fa fa-line-chart"></i>', Description = N'Analice los principales indicadores financieros de la entidad.'
        WHERE Id = 3000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3000, N'Indicadores Financieros', N'filter', 1500, 2, N'<i class="fa fa-line-chart"></i>', N'Analice los principales indicadores financieros de la entidad.');
    END

    -- Id: 3100 | SUGEF
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'SUGEF', MenuURL = N'Indicadores/Sugef', ParentId = 3000, SortOrder = 1, MenuIcon = N'<i class="fa fa-percent"></i>', Description = N'Analice los principales indicadores financieros de la entidad.'
        WHERE Id = 3100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3100, N'SUGEF', N'Indicadores/Sugef', 3000, 1, N'<i class="fa fa-percent"></i>', N'Analice los principales indicadores financieros de la entidad.');
    END

    -- Id: 3110 | FFC
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'FFC', MenuURL = N'Indicadores/FGA', ParentId = 3000, SortOrder = 2, MenuIcon = N'<i class="fa fa-check-square-o"></i>', Description = N'Analice los principales indicadores financieros de la entidad.'
        WHERE Id = 3110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3110, N'FFC', N'Indicadores/FGA', 3000, 2, N'<i class="fa fa-check-square-o"></i>', N'Analice los principales indicadores financieros de la entidad.');
    END

    -- Id: 3120 | Económicos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Económicos', MenuURL = N'IndEconomico/Index', ParentId = 3000, SortOrder = 3, MenuIcon = N'<i class="fa fa-money"></i>', Description = N'Seguimiento de tasas de interés, inflación y variables macroeconómicas.'
        WHERE Id = 3120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3120, N'Económicos', N'IndEconomico/Index', 3000, 3, N'<i class="fa fa-money"></i>', N'Seguimiento de tasas de interés, inflación y variables macroeconómicas.');
    END

    -- Id: 3130 | Personalizados
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Personalizados', MenuURL = N'Indicadores/Graficas', ParentId = 3000, SortOrder = 4, MenuIcon = N'<i class="fa fa-pie-chart"></i>', Description = N'Analice los principales indicadores financieros de la entidad.'
        WHERE Id = 3130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3130, N'Personalizados', N'Indicadores/Graficas', 3000, 4, N'<i class="fa fa-pie-chart"></i>', N'Analice los principales indicadores financieros de la entidad.');
    END

    -- Id: 4000 | Cartera de Crédito
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 4000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cartera de Crédito', MenuURL = N'filter', ParentId = NULL, SortOrder = 5, MenuIcon = N'<i class="fa fa-money"></i>', Description = N'Acceda a la información y reportes detallados de Cartera de Crédito.'
        WHERE Id = 4000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (4000, N'Cartera de Crédito', N'filter', NULL, 5, N'<i class="fa fa-money"></i>', N'Acceda a la información y reportes detallados de Cartera de Crédito.');
    END

    -- Id: 4100 | Composición
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 4100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Composición', MenuURL = N'Composicion/Index', ParentId = 4000, SortOrder = 2, MenuIcon = N'<i class="fa fa-credit-card"></i>', Description = N'Analice la composición de las fuentes de fondeo de la entidad.'
        WHERE Id = 4100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (4100, N'Composición', N'Composicion/Index', 4000, 2, N'<i class="fa fa-credit-card"></i>', N'Analice la composición de las fuentes de fondeo de la entidad.');
    END

    -- Id: 4110 | Matrices de Mora
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 4110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Matrices de Mora', MenuURL = N'Matriz/Index', ParentId = 4000, SortOrder = 3, MenuIcon = N'<i class="fa fa-minus-square-o"></i>', Description = N'Análisis de transición de mora y deterioro de créditos.'
        WHERE Id = 4110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (4110, N'Matrices de Mora', N'Matriz/Index', 4000, 3, N'<i class="fa fa-minus-square-o"></i>', N'Análisis de transición de mora y deterioro de créditos.');
    END

    -- Id: 4130 | Cálculos 14-21
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 4130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cálculos 14-21', MenuURL = N'Cartera14_21/Index', ParentId = 4000, SortOrder = 1, MenuIcon = N'<i class="fa fa-pie-chart"></i>', Description = N'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.'
        WHERE Id = 4130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (4130, N'Cálculos 14-21', N'Cartera14_21/Index', 4000, 1, N'<i class="fa fa-pie-chart"></i>', N'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.');
    END

    -- Id: 5000 | Liquidez
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Liquidez', MenuURL = N'filter', ParentId = 2250, SortOrder = 2, MenuIcon = N'<i class="fa fa-thermometer-full"></i>', Description = N'Cálculo e insumos del Indicador de Riesgo de Liquidez.'
        WHERE Id = 5000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5000, N'Liquidez', N'filter', 2250, 2, N'<i class="fa fa-thermometer-full"></i>', N'Cálculo e insumos del Indicador de Riesgo de Liquidez.');
    END

    -- Id: 5100 | IRL
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'IRL', MenuURL = N'IRL/IRL', ParentId = 5000, SortOrder = 1, MenuIcon = N'<i class="fa fa-signal"></i>', Description = N'Cálculo e insumos del Indicador de Riesgo de Liquidez.'
        WHERE Id = 5100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5100, N'IRL', N'IRL/IRL', 5000, 1, N'<i class="fa fa-signal"></i>', N'Cálculo e insumos del Indicador de Riesgo de Liquidez.');
    END

    -- Id: 5110 | Márgenes Implícitos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Márgenes Implícitos', MenuURL = N'Margen/Index', ParentId = 1500, SortOrder = 3, MenuIcon = N'<i class="fa fa-trophy"></i>', Description = N'Revise los márgenes financieros y operativos.'
        WHERE Id = 5110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5110, N'Márgenes Implícitos', N'Margen/Index', 1500, 3, N'<i class="fa fa-trophy"></i>', N'Revise los márgenes financieros y operativos.');
    END

    -- Id: 5120 | Tasas
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tasas', MenuURL = N'TasaInteres/Index', ParentId = 1500, SortOrder = 4, MenuIcon = N'<i class="fa fa-percent"></i>', Description = N'Consulte las tasas implícitas por tipo de activo y pasivo.'
        WHERE Id = 5120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5120, N'Tasas', N'TasaInteres/Index', 1500, 4, N'<i class="fa fa-percent"></i>', N'Consulte las tasas implícitas por tipo de activo y pasivo.');
    END

    -- Id: 5130 | Proyecciones
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Proyecciones', MenuURL = N'IRL/Index', ParentId = 5000, SortOrder = 2, MenuIcon = N'<i class="fa fa-paper-plane-o"></i>', Description = N'Realice proyecciones financieras de la entidad.'
        WHERE Id = 5130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5130, N'Proyecciones', N'IRL/Index', 5000, 2, N'<i class="fa fa-paper-plane-o"></i>', N'Realice proyecciones financieras de la entidad.');
    END

    -- Id: 6000 | Evaluación SBR
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Evaluación SBR', MenuURL = N'root', ParentId = NULL, SortOrder = 7, MenuIcon = N'<i class="fa fa-pie-chart"></i>', Description = NULL
        WHERE Id = 6000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6000, N'Evaluación SBR', N'root', NULL, 7, N'<i class="fa fa-pie-chart"></i>', NULL);
    END

    -- Id: 6001 | Autoevaluación
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6001)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Autoevaluación', MenuURL = N'Evaluacion/Evaluacion', ParentId = 6000, SortOrder = 1, MenuIcon = N'<i class="fa fa-file"></i>', Description = N'Formularios de autoevaluación de supervisión basada en riesgos (SBR).'
        WHERE Id = 6001;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6001, N'Autoevaluación', N'Evaluacion/Evaluacion', 6000, 1, N'<i class="fa fa-file"></i>', N'Formularios de autoevaluación de supervisión basada en riesgos (SBR).');
    END

    -- Id: 6002 | Avance
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6002)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Avance', MenuURL = N'Evaluacion/Avance', ParentId = 6000, SortOrder = 2, MenuIcon = N'<i class="fa fa-search"></i>', Description = N'Monitoreo del porcentaje de avance y nivel de respuestas completadas.'
        WHERE Id = 6002;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6002, N'Avance', N'Evaluacion/Avance', 6000, 2, N'<i class="fa fa-search"></i>', N'Monitoreo del porcentaje de avance y nivel de respuestas completadas.');
    END

    -- Id: 6003 | Historial
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6003)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Historial', MenuURL = N'Evaluacion/Historial', ParentId = 6000, SortOrder = 3, MenuIcon = N'<i class="fa fa-history"></i>', Description = N'Registro histórico de evaluaciones y calificaciones obtenidas.'
        WHERE Id = 6003;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6003, N'Historial', N'Evaluacion/Historial', 6000, 3, N'<i class="fa fa-history"></i>', N'Registro histórico de evaluaciones y calificaciones obtenidas.');
    END

    -- Id: 6005 | Resultados
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6005)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Resultados', MenuURL = N'Evaluacion/Resultados', ParentId = 6000, SortOrder = 4, MenuIcon = N'<i class="fa fa-bar-chart"></i>', Description = N'Informe de resultados consolidados y niveles de cumplimiento.'
        WHERE Id = 6005;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6005, N'Resultados', N'Evaluacion/Resultados', 6000, 4, N'<i class="fa fa-bar-chart"></i>', N'Informe de resultados consolidados y niveles de cumplimiento.');
    END

    -- Id: 6500 | Cálculos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6500)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cálculos', MenuURL = N'root', ParentId = NULL, SortOrder = 4, MenuIcon = N'<i class="fa fa-sliders"></i>', Description = NULL
        WHERE Id = 6500;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6500, N'Cálculos', N'root', NULL, 4, N'<i class="fa fa-sliders"></i>', NULL);
    END

    -- Id: 6501 | Simulación ISP
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6501)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Simulación ISP', MenuURL = N'SimulacionCapital/Index', ParentId = 6500, SortOrder = 2, MenuIcon = N'<i class="fa fa-puzzle-piece"></i>', Description = N'Simulación de suficiencia patrimonial y escenarios de estrés de capital.'
        WHERE Id = 6501;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6501, N'Simulación ISP', N'SimulacionCapital/Index', 6500, 2, N'<i class="fa fa-puzzle-piece"></i>', N'Simulación de suficiencia patrimonial y escenarios de estrés de capital.');
    END

    -- Id: 6502 | Contribuciones
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6502)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Contribuciones', MenuURL = N'Facturacion/Index', ParentId = 6500, SortOrder = 1, MenuIcon = N'<i class="fa fa-dollar"></i>', Description = N'Gestión de facturación y cuotas de mantenimiento de la plataforma.'
        WHERE Id = 6502;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6502, N'Contribuciones', N'Facturacion/Index', 6500, 1, N'<i class="fa fa-dollar"></i>', N'Gestión de facturación y cuotas de mantenimiento de la plataforma.');
    END

    -- Id: 7000 | Carga de Datos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Carga de Datos', MenuURL = N'root', ParentId = NULL, SortOrder = 6, MenuIcon = N'<i class="fa fa-cloud-upload"></i>', Description = NULL
        WHERE Id = 7000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7000, N'Carga de Datos', N'root', NULL, 6, N'<i class="fa fa-cloud-upload"></i>', NULL);
    END

    -- Id: 7100 | Estatus de la Carga
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estatus de la Carga', MenuURL = N'Cierre/Index', ParentId = 7000, SortOrder = 1, MenuIcon = N'<i class="fa fa-eye"></i>', Description = N'Monitoreo y administración de fechas de corte y cierres contables.'
        WHERE Id = 7100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7100, N'Estatus de la Carga', N'Cierre/Index', 7000, 1, N'<i class="fa fa-eye"></i>', N'Monitoreo y administración de fechas de corte y cierres contables.');
    END

    -- Id: 7110 | Carga de XML
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Carga de XML', MenuURL = N'Archivo/Index', ParentId = 7000, SortOrder = 2, MenuIcon = N'<i class="fa fa-upload"></i>', Description = N'Carga masiva, validación y procesamiento de archivos XML.'
        WHERE Id = 7110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7110, N'Carga de XML', N'Archivo/Index', 7000, 2, N'<i class="fa fa-upload"></i>', N'Carga masiva, validación y procesamiento de archivos XML.');
    END

    -- Id: 7120 | Cargas Masivas
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cargas Masivas', MenuURL = N'Explorer/Requisites', ParentId = 7000, SortOrder = 3, MenuIcon = N'<i class="fa fa-check-square-o"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 7120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7120, N'Cargas Masivas', N'Explorer/Requisites', 7000, 3, N'<i class="fa fa-check-square-o"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    -- Id: 7140 | Histórico de Cargas
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7140)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Histórico de Cargas', MenuURL = N'Explorer/Index', ParentId = 7000, SortOrder = 4, MenuIcon = N'<i class="fa fa-database"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 7140;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7140, N'Histórico de Cargas', N'Explorer/Index', 7000, 4, N'<i class="fa fa-database"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    -- Id: 7150 | Estatus
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7150)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estatus', MenuURL = N'Cierre/Monitor', ParentId = 7000, SortOrder = 5, MenuIcon = N'<i class="fa fa-laptop"></i>', Description = N'Monitoreo y administración de fechas de corte y cierres contables.'
        WHERE Id = 7150;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7150, N'Estatus', N'Cierre/Monitor', 7000, 5, N'<i class="fa fa-laptop"></i>', N'Monitoreo y administración de fechas de corte y cierres contables.');
    END

    -- Id: 7160 | Archivos Cargados
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7160)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Archivos Cargados', MenuURL = N'Cierre', ParentId = 7000, SortOrder = 6, MenuIcon = N'<i class="fa fa-search"></i>', Description = N'Monitoreo y administración de fechas de corte y cierres contables.'
        WHERE Id = 7160;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7160, N'Archivos Cargados', N'Cierre', 7000, 6, N'<i class="fa fa-search"></i>', N'Monitoreo y administración de fechas de corte y cierres contables.');
    END

    -- Id: 8000 | Control de Accesos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Control de Accesos', MenuURL = N'root', ParentId = NULL, SortOrder = 9, MenuIcon = N'<i class="fa fa-user-secret"></i>', Description = N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
        WHERE Id = 8000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (8000, N'Control de Accesos', N'root', NULL, 9, N'<i class="fa fa-user-secret"></i>', N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.');
    END

    -- Id: 8100 | Administración de Usuarios
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Administración de Usuarios', MenuURL = N'Usuario/Index', ParentId = 8000, SortOrder = 1, MenuIcon = N'<i class="fa fa-user"></i>', Description = N'Gestión de cuentas de usuario, credenciales y estados de acceso.'
        WHERE Id = 8100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (8100, N'Administración de Usuarios', N'Usuario/Index', 8000, 1, N'<i class="fa fa-user"></i>', N'Gestión de cuentas de usuario, credenciales y estados de acceso.');
    END

    -- Id: 8110 | Cambiar Contraseña
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cambiar Contraseña', MenuURL = N'Usuario/ChangePassword', ParentId = 8000, SortOrder = 2, MenuIcon = N'<i class="fa fa-key"></i>', Description = N'Gestión de cuentas de usuario, credenciales y estados de acceso.'
        WHERE Id = 8110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (8110, N'Cambiar Contraseña', N'Usuario/ChangePassword', 8000, 2, N'<i class="fa fa-key"></i>', N'Gestión de cuentas de usuario, credenciales y estados de acceso.');
    END

    -- Id: 9000 | Administración
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Administración', MenuURL = N'root', ParentId = NULL, SortOrder = 10, MenuIcon = N'<i class="fa fa-cog"></i>', Description = NULL
        WHERE Id = 9000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9000, N'Administración', N'root', NULL, 10, N'<i class="fa fa-cog"></i>', NULL);
    END

    -- Id: 9050 | Calendario
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9050)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Calendario', MenuURL = N'Calendario/Index', ParentId = 9000, SortOrder = 1, MenuIcon = N'<i class="fa fa-calendar"></i>', Description = N'Cronograma de eventos, fechas de entrega y compromisos regulatorios.'
        WHERE Id = 9050;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9050, N'Calendario', N'Calendario/Index', 9000, 1, N'<i class="fa fa-calendar"></i>', N'Cronograma de eventos, fechas de entrega y compromisos regulatorios.');
    END

    -- Id: 9100 | Catálogo SUGEF
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Catálogo SUGEF', MenuURL = N'CatalogoCuenta/Index', ParentId = 9000, SortOrder = 3, MenuIcon = N'<i class="fa fa-dollar"></i>', Description = N'Mapeo y homologación del catálogo contable según la normativa SUGEF.'
        WHERE Id = 9100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9100, N'Catálogo SUGEF', N'CatalogoCuenta/Index', 9000, 3, N'<i class="fa fa-dollar"></i>', N'Mapeo y homologación del catálogo contable según la normativa SUGEF.');
    END

    -- Id: 9110 | Catálogo de Fórmulas
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Catálogo de Fórmulas', MenuURL = N'Formula/Index', ParentId = 9000, SortOrder = 2, MenuIcon = N'<i class="fa fa-calculator"></i>', Description = N'Mapeo y homologación del catálogo contable según la normativa SUGEF.'
        WHERE Id = 9110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9110, N'Catálogo de Fórmulas', N'Formula/Index', 9000, 2, N'<i class="fa fa-calculator"></i>', N'Mapeo y homologación del catálogo contable según la normativa SUGEF.');
    END

    -- Id: 9120 | Catálogo XML
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Catálogo XML', MenuURL = N'TipoXML/Index', ParentId = 9000, SortOrder = 4, MenuIcon = N'<i class="fa fa-file-o"></i>', Description = N'Mapeo y homologación del catálogo contable según la normativa SUGEF.'
        WHERE Id = 9120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9120, N'Catálogo XML', N'TipoXML/Index', 9000, 4, N'<i class="fa fa-file-o"></i>', N'Mapeo y homologación del catálogo contable según la normativa SUGEF.');
    END

    -- Id: 9130 | Entidades Afiliadas
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Entidades Afiliadas', MenuURL = N'Entidad/Index', ParentId = 9000, SortOrder = 5, MenuIcon = N'<i class="fa fa-building"></i>', Description = N'Catálogo de entidades participantes y parámetros generales.'
        WHERE Id = 9130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9130, N'Entidades Afiliadas', N'Entidad/Index', 9000, 5, N'<i class="fa fa-building"></i>', N'Catálogo de entidades participantes y parámetros generales.');
    END

    -- Id: 9135 | Evaluación
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9135)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Evaluación', MenuURL = N'Evaluacion/Index', ParentId = 9000, SortOrder = 6, MenuIcon = N'<i class="fa fa-eye"></i>', Description = N'Acceda a la información y reportes detallados de Evaluación.'
        WHERE Id = 9135;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9135, N'Evaluación', N'Evaluacion/Index', 9000, 6, N'<i class="fa fa-eye"></i>', N'Acceda a la información y reportes detallados de Evaluación.');
    END

    -- Id: 9140 | Tipos de Informe
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9140)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tipos de Informe', MenuURL = N'TipoInforme/Index', ParentId = 9000, SortOrder = 10, MenuIcon = N'<i class="fa fa-file-pdf-o"></i>', Description = N'Acceda a la información y reportes detallados de Tipos de Informe.'
        WHERE Id = 9140;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9140, N'Tipos de Informe', N'TipoInforme/Index', 9000, 10, N'<i class="fa fa-file-pdf-o"></i>', N'Acceda a la información y reportes detallados de Tipos de Informe.');
    END

    -- Id: 9145 | Roles
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Roles', MenuURL = N'Role/Index', ParentId = 9000, SortOrder = 9, MenuIcon = N'<i class="fa fa-key"></i>', Description = N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
        WHERE Id = 9145;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9145, N'Roles', N'Role/Index', 9000, 9, N'<i class="fa fa-key"></i>', N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.');
    END

    -- Id: 9150 | Parámetros
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9150)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Parámetros', MenuURL = N'Parametros/Index', ParentId = 9000, SortOrder = 7, MenuIcon = N'<i class="fa fa-gears"></i>', Description = N'Configuración de variables operativas y parámetros globales del sistema.'
        WHERE Id = 9150;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9150, N'Parámetros', N'Parametros/Index', 9000, 7, N'<i class="fa fa-gears"></i>', N'Configuración de variables operativas y parámetros globales del sistema.');
    END

    -- Id: 11000 | Informes
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Informes', MenuURL = N'root', ParentId = NULL, SortOrder = 8, MenuIcon = N'<i class="fa fa-file-o"></i>', Description = N'Generación y visualización de informes gerenciales y financieros.'
        WHERE Id = 11000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11000, N'Informes', N'root', NULL, 8, N'<i class="fa fa-file-o"></i>', N'Generación y visualización de informes gerenciales y financieros.');
    END

    -- Id: 11100 | Carga de Informes
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Carga de Informes', MenuURL = N'Explorer/Informe', ParentId = 11000, SortOrder = 1, MenuIcon = N'<i class="fa fa-file-pdf-o"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 11100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11100, N'Carga de Informes', N'Explorer/Informe', 11000, 1, N'<i class="fa fa-file-pdf-o"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    -- Id: 11110 | Informes
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Informes', MenuURL = N'Explorer/Consulta', ParentId = 11000, SortOrder = 2, MenuIcon = N'<i class="fa fa-database"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 11110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11110, N'Informes', N'Explorer/Consulta', 11000, 2, N'<i class="fa fa-database"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    -- Id: 11120 | Notificaciones
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Notificaciones', MenuURL = N'Explorer/Notificacion', ParentId = 11000, SortOrder = 3, MenuIcon = N'<i class="fa fa-envelope"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 11120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11120, N'Notificaciones', N'Explorer/Notificacion', 11000, 3, N'<i class="fa fa-envelope"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    -- Id: 13000 | Ingresos
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Ingresos', MenuURL = N'Seguridad/Index', ParentId = NULL, SortOrder = 11, MenuIcon = N'<i class="fa fa-sign-in"></i>', Description = N'Historial de conexiones, sesiones activas y registro de auditoría.'
        WHERE Id = 13000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (13000, N'Ingresos', N'Seguridad/Index', NULL, 11, N'<i class="fa fa-sign-in"></i>', N'Historial de conexiones, sesiones activas y registro de auditoría.');
    END

    -- Id: 13001 | Menú del Sistema
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Menú del Sistema', MenuURL = N'Menu/Index', ParentId = 9000, SortOrder = 11, MenuIcon = N'<i class="fa fa-sitemap"></i>', Description = N'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.'
        WHERE Id = 13001;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (13001, N'Menú del Sistema', N'Menu/Index', 9000, 11, N'<i class="fa fa-sitemap"></i>', N'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
    END

    IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Menus procesados exitosamente (' + CAST(59 AS VARCHAR) + ' registros).';

    PRINT '>> [3/5] Eliminando contenedores temporales de FFC en FGA (Id 9200 y 9300)...';
    DELETE FROM dbo.MenuPermission WHERE MenuId IN (9200, 9300);
    DELETE FROM dbo.Menu WHERE Id IN (9200, 9300);
    PRINT '   -> Contenedores 9200 y 9300 y sus permisos eliminados de FGA.';

    PRINT '>> [4/5] Asegurando permisos en dbo.MenuPermission para los roles en FGA...';
    -- Garantizar permisos completos para los roles administrativos de FGA
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, ISNULL(m.SortOrder, 1), 1, 1, 1, 1
    FROM dbo.Menu m
    CROSS JOIN dbo.Role r
    WHERE (r.EsEntidad = 0 OR r.Id IN (1, 2, 7))
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id);

    -- Asegurar permisos de lectura para roles existentes sobre los menus restaurados
    UPDATE dbo.MenuPermission SET IsRead = 1 WHERE IsRead = 0;
    PRINT '   -> Permisos de MenuPermission verificados y sincronizados.';

    PRINT '>> [5/5] Reactivando y validando integridad de constraints...';
    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    COMMIT TRANSACTION;
    PRINT '===================================================================================================';
    PRINT '>> RESTAURACION DEL MENU EXACTO EN [FGA] COMPLETADA SATISFACTORIAMENTE.';
    PRINT '===================================================================================================';

    -- Diagnostico del arbol resultante en FGA
    PRINT '';
    PRINT '>> ARBOL COMPLETO RESTAURADO EN [FGA]:';
    SELECT 
        m.Id,
        CASE 
            WHEN m.ParentId IS NULL THEN m.MenuText
            WHEN p.ParentId IS NULL THEN '    |-- ' + m.MenuText
            ELSE '        +-- ' + m.MenuText
        END AS [Jerarquia_Menu],
        m.MenuURL,
        m.ParentId,
        m.SortOrder,
        m.MenuIcon
    FROM dbo.Menu m
    LEFT JOIN dbo.Menu p ON m.ParentId = p.Id
    ORDER BY 
        ISNULL(p.ParentId, ISNULL(m.ParentId, m.Id)),
        ISNULL(m.ParentId, 0),
        m.SortOrder;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Menu OFF;
    IF OBJECTPROPERTY(OBJECT_ID('dbo.MenuPermission'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.MenuPermission OFF;
    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;
    PRINT '>> ERROR EN RESTAURACION DE FGA: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
