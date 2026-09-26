-- ===================================================================================================
-- SCRIPT DE RESTRUCTURACIÓN INTEGRAL DE MENÚ Y PERMISOS DEL SISTEMA
-- Base de Datos: FGA / SAC
-- Características: Idempotente, Transaccional, No destructivo (Preserva opciones existentes sin pérdidas)
-- Fecha de Generación: 2026-09-24 20:36:53
-- ===================================================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '>> [1/4] Iniciando verificación previa y deshabilitación temporal de constraints...';

    -- Deshabilitar constraints para permitir inserción/actualización de jerarquías complejas y llaves foráneas
    ALTER TABLE dbo.Menu NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission NOCHECK CONSTRAINT ALL;

    PRINT '>> [2/4] Sincronizando catálogo de Menú (dbo.Menu)...';

    -- Habilitar IDENTITY_INSERT para permitir preservar explícitamente los IDs definidos en el catálogo
    SET IDENTITY_INSERT dbo.Menu ON;
    -- Inserción / Actualización de ítems de Menú
    -- Se utiliza MERGE / IF EXISTS para actualizar registros existentes o insertarlos con su ID exacto.

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 1500)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estructura Financiera', MenuURL = N'filter', ParentId = NULL, SortOrder = 6, MenuIcon = N'<i class="fa fa-bar-chart"></i>', Description = NULL
        WHERE Id = 1500;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (1500, N'Estructura Financiera', N'filter', NULL, 6, N'<i class="fa fa-bar-chart"></i>', NULL);
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2100)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estructura Financiera', MenuURL = N'filter', ParentId = 1500, SortOrder = 2, MenuIcon = N'<i class="fa fa-money"></i>', Description = N'Acceda a la información y reportes detallados de Estructura Financiera.'
        WHERE Id = 2100;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2100, N'Estructura Financiera', N'filter', 1500, 2, N'<i class="fa fa-money"></i>', N'Acceda a la información y reportes detallados de Estructura Financiera.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 2250)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Módulo de Proyecciones', MenuURL = N'filter', ParentId = NULL, SortOrder = 9, MenuIcon = N'<i class="fa fa-eye"></i>', Description = N'Realice proyecciones financieras de la entidad.'
        WHERE Id = 2250;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (2250, N'Módulo de Proyecciones', N'filter', NULL, 9, N'<i class="fa fa-eye"></i>', N'Realice proyecciones financieras de la entidad.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 3000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Indicadores Financieros', MenuURL = N'filter', ParentId = 1500, SortOrder = 3, MenuIcon = N'<i class="fa fa-line-chart"></i>', Description = N'Analice los principales indicadores financieros de la entidad.'
        WHERE Id = 3000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (3000, N'Indicadores Financieros', N'filter', 1500, 3, N'<i class="fa fa-line-chart"></i>', N'Analice los principales indicadores financieros de la entidad.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Márgenes Implícitos', MenuURL = N'Margen/Index', ParentId = 1500, SortOrder = 4, MenuIcon = N'<i class="fa fa-trophy"></i>', Description = N'Revise los márgenes financieros y operativos.'
        WHERE Id = 5110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5110, N'Márgenes Implícitos', N'Margen/Index', 1500, 4, N'<i class="fa fa-trophy"></i>', N'Revise los márgenes financieros y operativos.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 5120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tasas', MenuURL = N'TasaInteres/Index', ParentId = 1500, SortOrder = 5, MenuIcon = N'<i class="fa fa-percent"></i>', Description = N'Consulte las tasas implícitas por tipo de activo y pasivo.'
        WHERE Id = 5120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (5120, N'Tasas', N'TasaInteres/Index', 1500, 5, N'<i class="fa fa-percent"></i>', N'Consulte las tasas implícitas por tipo de activo y pasivo.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 6500)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Cálculos', MenuURL = N'root', ParentId = NULL, SortOrder = 3, MenuIcon = N'<i class="fa fa-sliders"></i>', Description = NULL
        WHERE Id = 6500;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (6500, N'Cálculos', N'root', NULL, 3, N'<i class="fa fa-sliders"></i>', NULL);
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Carga de Datos', MenuURL = N'root', ParentId = NULL, SortOrder = 4, MenuIcon = N'<i class="fa fa-cloud-upload"></i>', Description = NULL
        WHERE Id = 7000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7000, N'Carga de Datos', N'root', NULL, 4, N'<i class="fa fa-cloud-upload"></i>', NULL);
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 8000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Seguridad', MenuURL = N'root', ParentId = NULL, SortOrder = 10, MenuIcon = N'<i class="fa fa-key"></i>', Description = N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
        WHERE Id = 8000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (8000, N'Seguridad', N'root', NULL, 10, N'<i class="fa fa-key"></i>', N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Administración', MenuURL = N'root', ParentId = NULL, SortOrder = 2, MenuIcon = N'<i class="fa fa-cog"></i>', Description = NULL
        WHERE Id = 9000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9000, N'Administración', N'root', NULL, 2, N'<i class="fa fa-cog"></i>', NULL);
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tipos de XML', MenuURL = N'TipoXML/Index', ParentId = 9000, SortOrder = 10, MenuIcon = N'<i class="fa fa-file-o"></i>', Description = N'Mapeo y homologación del catálogo contable según la normativa SUGEF.'
        WHERE Id = 9120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9120, N'Tipos de XML', N'TipoXML/Index', 9000, 10, N'<i class="fa fa-file-o"></i>', N'Mapeo y homologación del catálogo contable según la normativa SUGEF.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9130)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Entidades Afiliadas', MenuURL = N'Entidad/Index', ParentId = 9000, SortOrder = 4, MenuIcon = N'<i class="fa fa-building"></i>', Description = N'Catálogo de entidades participantes y parámetros generales.'
        WHERE Id = 9130;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9130, N'Entidades Afiliadas', N'Entidad/Index', 9000, 4, N'<i class="fa fa-building"></i>', N'Catálogo de entidades participantes y parámetros generales.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9135)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Evaluación', MenuURL = N'Evaluacion/Index', ParentId = 9000, SortOrder = 5, MenuIcon = N'<i class="fa fa-eye"></i>', Description = N'Acceda a la información y reportes detallados de Evaluación.'
        WHERE Id = 9135;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9135, N'Evaluación', N'Evaluacion/Index', 9000, 5, N'<i class="fa fa-eye"></i>', N'Acceda a la información y reportes detallados de Evaluación.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9140)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Tipos de Informe', MenuURL = N'TipoInforme/Index', ParentId = 9000, SortOrder = 9, MenuIcon = N'<i class="fa fa-file-pdf-o"></i>', Description = N'Acceda a la información y reportes detallados de Tipos de Informe.'
        WHERE Id = 9140;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9140, N'Tipos de Informe', N'TipoInforme/Index', 9000, 9, N'<i class="fa fa-file-pdf-o"></i>', N'Acceda a la información y reportes detallados de Tipos de Informe.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 9145)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Roles', MenuURL = N'Role/Index', ParentId = 8000, SortOrder = 5, MenuIcon = N'<i class="fa fa-key"></i>', Description = N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.'
        WHERE Id = 9145;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (9145, N'Roles', N'Role/Index', 8000, 5, N'<i class="fa fa-key"></i>', N'Gestión de perfiles de usuario y configuración de la matriz de permisos de menú.');
    END

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

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11110)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Informes', MenuURL = N'Explorer/Consulta', ParentId = 11000, SortOrder = 2, MenuIcon = N'<i class="fa fa-file-pdf-o"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 11110;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11110, N'Informes', N'Explorer/Consulta', 11000, 2, N'<i class="fa fa-file-pdf-o"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 11120)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Notificaciones', MenuURL = N'Explorer/Notificacion', ParentId = 9000, SortOrder = 6, MenuIcon = N'<i class="fa fa-envelope"></i>', Description = N'Consulta, descarga y auditoría de archivos y reportes procesados.'
        WHERE Id = 11120;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (11120, N'Notificaciones', N'Explorer/Notificacion', 9000, 6, N'<i class="fa fa-envelope"></i>', N'Consulta, descarga y auditoría de archivos y reportes procesados.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13000)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Ingresos', MenuURL = N'Seguridad/Index', ParentId = 8000, SortOrder = 3, MenuIcon = N'<i class="fa fa-sign-in"></i>', Description = N'Historial de conexiones, sesiones activas y registro de auditoría.'
        WHERE Id = 13000;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (13000, N'Ingresos', N'Seguridad/Index', 8000, 3, N'<i class="fa fa-sign-in"></i>', N'Historial de conexiones, sesiones activas y registro de auditoría.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13001)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Menú del Sistema', MenuURL = N'Menu/Index', ParentId = 8000, SortOrder = 4, MenuIcon = N'<i class="fa fa-sitemap"></i>', Description = N'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.'
        WHERE Id = 13001;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (13001, N'Menú del Sistema', N'Menu/Index', 8000, 4, N'<i class="fa fa-sitemap"></i>', N'Administración de opciones de menú, íconos y descripciones para las fichas del sistema.');
    END

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 13002)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = N'Estructura de Fondeo', MenuURL = N'EstructuraFondeo/Index', ParentId = 1500, SortOrder = 1, MenuIcon = N'<i class="fa fa-database"></i>', Description = N'Analice la composición de las fuentes de fondeo de la entidad.'
        WHERE Id = 13002;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (13002, N'Estructura de Fondeo', N'EstructuraFondeo/Index', 1500, 1, N'<i class="fa fa-database"></i>', N'Analice la composición de las fuentes de fondeo de la entidad.');
    END

    SET IDENTITY_INSERT dbo.Menu OFF;
    PRINT '   -> Menús procesados exitosamente (' + CAST(60 AS VARCHAR) + ' registros).';

    PRINT '>> [3/4] Sincronizando matriz de Permisos por Rol (dbo.MenuPermission)...';

    SET IDENTITY_INSERT dbo.MenuPermission ON;
    -- Inserción / Actualización de permisos de rol

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1298)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1298;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1298, 2100, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1299)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1299;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1299, 2110, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1300)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1300;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1300, 2120, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1301)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1301;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1301, 2130, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1302)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1302;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1302, 2140, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1303)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 2, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1303;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1303, 3000, 2, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1304)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1304;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1304, 3100, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1305)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1305;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1305, 3110, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1306)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 2, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1306;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1306, 3120, 2, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1307)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 2, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1307;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1307, 4000, 2, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1308)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1308;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1308, 4100, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1309)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1309;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1309, 4110, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1310)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 2, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1310;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1310, 5000, 2, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1311)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1311;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1311, 5100, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1312)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 2, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1312;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1312, 5110, 2, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1313)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 2, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1313;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1313, 5120, 2, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1316)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 2, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1316;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1316, 7000, 2, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1321)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7140, RoleId = 2, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1321;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1321, 7140, 2, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1322)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 2, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1322;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1322, 8000, 2, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1324)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1324;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1324, 8110, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1325)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9000, RoleId = 2, SortOrder = 11, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1325;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1325, 9000, 2, 11, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1327)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9110, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1327;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1327, 9110, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1329)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9130, RoleId = 2, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1329;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1329, 9130, 2, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1334)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7150, RoleId = 2, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1334;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1334, 7150, 2, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1335)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1335;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1335, 2100, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1336)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1336;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1336, 2110, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1337)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1337;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1337, 2120, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1338)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1338;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1338, 2130, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1339)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1339;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1339, 2140, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1340)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 6, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1340;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1340, 3000, 6, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1341)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1341;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1341, 3100, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1342)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 6, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1342;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1342, 3110, 6, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1343)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 6, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1343;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1343, 3120, 6, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1344)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 6, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1344;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1344, 4000, 6, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1345)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1345;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1345, 4100, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1346)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 6, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1346;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1346, 4110, 6, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1347)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 6, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1347;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1347, 5000, 6, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1348)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1348;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1348, 5100, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1349)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 6, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1349;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1349, 5110, 6, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1350)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 6, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1350;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1350, 5120, 6, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1354)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 6, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1354;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1354, 8000, 6, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1356)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 6, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1356;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1356, 8110, 6, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1362)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1362;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1362, 2100, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1363)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1363;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1363, 2110, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1364)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1364;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1364, 2120, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1365)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1365;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1365, 2130, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1366)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1366;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1366, 2140, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1367)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 5, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1367;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1367, 3000, 5, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1368)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1368;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1368, 3100, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1369)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1369;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1369, 3110, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1370)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 5, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1370;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1370, 3120, 5, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1371)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 5, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1371;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1371, 4000, 5, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1372)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1372;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1372, 4100, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1373)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1373;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1373, 4110, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1374)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 5, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1374;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1374, 5000, 5, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1375)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1375;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1375, 5100, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1376)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 5, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1376;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1376, 5110, 5, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1377)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 5, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1377;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1377, 5120, 5, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1381)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 5, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1381;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1381, 8000, 5, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1382)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1382;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1382, 8110, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1468)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 4, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1468;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1468, 7000, 4, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1469)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7100, RoleId = 4, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1469;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1469, 7100, 4, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1471)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7110, RoleId = 4, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1471;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1471, 7110, 4, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1472)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7120, RoleId = 4, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1472;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1472, 7120, 4, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1473)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1473;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1473, 2100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1474)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1474;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1474, 2110, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1475)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1475;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1475, 2120, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1476)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1476;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1476, 2130, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1477)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1477;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1477, 2140, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1478)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 1, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1478;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1478, 3000, 1, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1479)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1479;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1479, 3100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1480)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1480;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1480, 3110, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1481)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 1, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1481;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1481, 3120, 1, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1482)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 1, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1482;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1482, 4000, 1, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1483)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1483;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1483, 4100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1484)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1484;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1484, 4110, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1485)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 1, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1485;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1485, 5000, 1, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1486)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1486;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1486, 5100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1487)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 1, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1487;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1487, 5110, 1, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1488)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 1, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1488;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1488, 5120, 1, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1491)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 1, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1491;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1491, 7000, 1, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1492)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 1, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1492;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1492, 8000, 1, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1493)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1493;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1493, 8100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1494)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1494;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1494, 8110, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1499)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7150, RoleId = 1, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1499;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1499, 7150, 1, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1501)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 2, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1501;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1501, 3130, 2, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1589)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 1, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1589;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1589, 11000, 1, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1590)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11100, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1590;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1590, 11100, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1591)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 2, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1591;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1591, 11000, 2, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1592)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11100, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1592;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1592, 11100, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1593)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 6, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1593;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1593, 11000, 6, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1595)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9140, RoleId = 2, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1595;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1595, 9140, 2, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1596)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1596;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1596, 11110, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1597)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1597;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1597, 11110, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1598)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1598;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1598, 11110, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1599)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 6, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1599;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1599, 11110, 6, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1601)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1601;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1601, 1500, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1602)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1602;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1602, 1500, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1603)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1603;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1603, 1500, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1604)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 6, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1604;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1604, 1500, 6, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1606)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9050, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1606;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1606, 9050, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1607)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9000, RoleId = 1, SortOrder = 11, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1607;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1607, 9000, 1, 11, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1608)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9100, RoleId = 1, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1608;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1608, 9100, 1, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1609)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9110, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1609;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1609, 9110, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1610)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9120, RoleId = 1, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1610;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1610, 9120, 1, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1611)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9130, RoleId = 1, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1611;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1611, 9130, 1, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1612)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9140, RoleId = 1, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1612;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1612, 9140, 1, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1614)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5130, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1614;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1614, 5130, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1615)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5130, RoleId = 2, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1615;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1615, 5130, 2, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1626)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 5, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1626;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1626, 11000, 5, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1628)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 4, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1628;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1628, 8000, 4, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1629)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 4, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1629;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1629, 8110, 4, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1631)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7140, RoleId = 1, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1631;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1631, 7140, 1, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1632)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1632;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1632, 1000, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1633)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1633;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1633, 1000, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1634)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 5, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1634;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1634, 3130, 5, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1663)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1663;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1663, 2100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1664)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1664;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1664, 2110, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1665)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1665;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1665, 2120, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1666)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1666;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1666, 2130, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1667)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1667;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1667, 2140, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1668)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 10, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1668;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1668, 3000, 10, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1669)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1669;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1669, 3100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1670)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1670;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1670, 3110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1671)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 10, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1671;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1671, 3120, 10, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1672)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 10, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1672;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1672, 4000, 10, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1673)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1673;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1673, 4100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1674)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1674;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1674, 4110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1675)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 10, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1675;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1675, 5000, 10, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1676)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1676;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1676, 5100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1677)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 10, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1677;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1677, 5110, 10, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1678)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 10, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1678;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1678, 5120, 10, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1679)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 10, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1679;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1679, 8000, 10, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1680)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1680;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1680, 8110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1681)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1681;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1681, 11110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1682)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1682;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1682, 1500, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1683)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 10, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1683;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1683, 7000, 10, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1684)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1684;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1684, 7100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1685)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1685;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1685, 7110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1686)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7120, RoleId = 10, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1686;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1686, 7120, 10, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1687)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 10, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1687;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1687, 11000, 10, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1688)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8100, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1688;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1688, 8100, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1689)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1689;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1689, 1000, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1690)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 10, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1690;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1690, 3130, 10, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1691)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 1, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1691;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1691, 3130, 1, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1692)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9110, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1692;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1692, 9110, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1693)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9000, RoleId = 5, SortOrder = 11, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1693;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1693, 9000, 5, 11, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1694)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1694;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1694, 2135, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1695)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1695;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1695, 2135, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1696)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1696;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1696, 2135, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1697)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9150, RoleId = 1, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1697;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1697, 9150, 1, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1698)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9000, RoleId = 10, SortOrder = 11, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1698;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1698, 9000, 10, 11, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1699)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9110, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1699;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1699, 9110, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1700)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13000, RoleId = 1, SortOrder = 13, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1700;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1700, 13000, 1, 13, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1701)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13000, RoleId = 2, SortOrder = 13, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1701;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1701, 13000, 2, 13, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1702)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13000, RoleId = 6, SortOrder = 13, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1702;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1702, 13000, 6, 13, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1706)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7160, RoleId = 1, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1706;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1706, 7160, 1, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1707)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1707;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1707, 1000, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1708)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1708;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1708, 1000, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1709)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11120, RoleId = 1, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1709;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1709, 11120, 1, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1710)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11120, RoleId = 2, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1710;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1710, 11120, 2, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1745)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2250, RoleId = 1, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1745;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1745, 2250, 1, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1746)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2260, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1746;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1746, 2260, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1748)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9135, RoleId = 1, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1748;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1748, 9135, 1, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1782)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 5, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1782;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1782, 2135, 5, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1783)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2250, RoleId = 5, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1783;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1783, 2250, 5, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1784)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2260, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1784;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1784, 2260, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1786)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 10, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1786;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1786, 2135, 10, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1787)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 5, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1787;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1787, 7000, 5, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1788)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7110, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1788;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1788, 7110, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1789)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6000, RoleId = 1, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1789;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1789, 6000, 1, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1790)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6001, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1790;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1790, 6001, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1791)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6002, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1791;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1791, 6002, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1792)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1792;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1792, 2100, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1793)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1793;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1793, 2110, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1794)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1794;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1794, 2120, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1795)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1795;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1795, 2130, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1796)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1796;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1796, 2140, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1797)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 12, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1797;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1797, 3000, 12, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1798)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1798;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1798, 3100, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1799)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1799;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1799, 3110, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1800)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 12, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1800;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1800, 3120, 12, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1801)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 12, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1801;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1801, 4000, 12, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1802)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1802;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1802, 4100, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1803)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1803;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1803, 4110, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1804)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 12, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1804;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1804, 5000, 12, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1805)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1805;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1805, 5100, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1806)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 12, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1806;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1806, 5110, 12, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1807)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 12, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1807;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1807, 5120, 12, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1808)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 12, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1808;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1808, 8000, 12, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1809)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1809;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1809, 8110, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1810)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1810;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1810, 11110, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1811)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1811;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1811, 1500, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1812)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 12, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1812;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1812, 11000, 12, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1813)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1813;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1813, 1000, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1814)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 12, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1814;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1814, 3130, 12, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1815)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9110, RoleId = 12, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1815;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1815, 9110, 12, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1816)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9000, RoleId = 12, SortOrder = 11, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1816;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1816, 9000, 12, 11, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1817)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 12, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1817;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1817, 2135, 12, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1818)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2250, RoleId = 12, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1818;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1818, 2250, 12, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1819)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2260, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1819;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1819, 2260, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1820)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7000, RoleId = 12, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1820;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1820, 7000, 12, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1821)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 7110, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1821;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1821, 7110, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1822)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6000, RoleId = 12, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1822;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1822, 6000, 12, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1823)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6001, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1823;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1823, 6001, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1825)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6003, RoleId = 1, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1825;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1825, 6003, 1, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1826)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1, RoleId = 1, SortOrder = 0, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1826;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1826, 1, 1, 0, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1828)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6001, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1828;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1828, 6001, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1832)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2250, RoleId = 10, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1832;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1832, 2250, 10, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1833)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2260, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1833;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1833, 2260, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1834)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6000, RoleId = 5, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1834;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1834, 6000, 5, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1835)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6001, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1835;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1835, 6001, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1838)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6000, RoleId = 10, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1838;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1838, 6000, 10, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1841)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6005, RoleId = 1, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1841;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1841, 6005, 1, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1844)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6500, RoleId = 1, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1844;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1844, 6500, 1, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1845)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6501, RoleId = 1, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1845;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1845, 6501, 1, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1846)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6502, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1846;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1846, 6502, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1847)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6500, RoleId = 10, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1847;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1847, 6500, 10, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1848)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6501, RoleId = 10, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1848;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1848, 6501, 10, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1850)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6502, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1850;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1850, 6502, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1851)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6500, RoleId = 5, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1851;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1851, 6500, 5, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1852)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6501, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1852;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1852, 6501, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1853)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6502, RoleId = 5, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1853;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1853, 6502, 5, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1854)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4130, RoleId = 1, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1854;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1854, 4130, 1, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1856)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1000, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1856;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1856, 1000, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1857)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 1500, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1857;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1857, 1500, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1858)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2100, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1858;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1858, 2100, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1859)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2110, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1859;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1859, 2110, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1860)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2120, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1860;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1860, 2120, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1861)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2130, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1861;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1861, 2130, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1862)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2135, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1862;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1862, 2135, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1863)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 2140, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1863;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1863, 2140, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1864)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3000, RoleId = 7, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1864;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1864, 3000, 7, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1865)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3100, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1865;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1865, 3100, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1866)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3110, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1866;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1866, 3110, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1867)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3120, RoleId = 7, SortOrder = 3, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1867;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1867, 3120, 7, 3, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1868)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 3130, RoleId = 7, SortOrder = 4, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1868;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1868, 3130, 7, 4, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1869)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4000, RoleId = 7, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1869;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1869, 4000, 7, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1870)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4100, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1870;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1870, 4100, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1871)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4110, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1871;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1871, 4110, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1873)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 4130, RoleId = 7, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1873;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1873, 4130, 7, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1874)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5000, RoleId = 7, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1874;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1874, 5000, 7, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1875)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5100, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1875;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1875, 5100, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1876)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5110, RoleId = 7, SortOrder = 6, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1876;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1876, 5110, 7, 6, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1877)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5120, RoleId = 7, SortOrder = 7, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1877;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1877, 5120, 7, 7, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1878)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 5130, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1878;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1878, 5130, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1879)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6000, RoleId = 7, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1879;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1879, 6000, 7, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1880)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6001, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1880;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1880, 6001, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1884)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6500, RoleId = 7, SortOrder = 5, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1884;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1884, 6500, 7, 5, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1885)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6501, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1885;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1885, 6501, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1886)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6502, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1886;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1886, 6502, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1887)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8000, RoleId = 7, SortOrder = 10, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1887;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1887, 8000, 7, 10, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1889)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 8110, RoleId = 7, SortOrder = 2, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1889;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1889, 8110, 7, 2, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1899)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11000, RoleId = 7, SortOrder = 9, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1899;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1899, 11000, 7, 9, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1900)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 11110, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1900;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1900, 11110, 7, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1904)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6500, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1904;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1904, 6500, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1905)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 6502, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1905;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1905, 6502, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1912)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 9145, RoleId = 1, SortOrder = 8, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1912;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1912, 9145, 1, 8, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1913)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13001, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1913;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1913, 13001, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1914)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 1, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1914;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1914, 13002, 1, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1915)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 2, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1915;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1915, 13002, 2, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1916)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 5, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1916;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1916, 13002, 5, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1917)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 6, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1917;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1917, 13002, 6, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1918)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 10, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1918;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1918, 13002, 10, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1919)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 12, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1919;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1919, 13002, 12, 1, 1, 1, 1, 1);
    END

    IF EXISTS (SELECT 1 FROM dbo.MenuPermission WHERE Id = 1920)
    BEGIN
        UPDATE dbo.MenuPermission
        SET MenuId = 13002, RoleId = 7, SortOrder = 1, IsCreate = 1, IsRead = 1, IsUpdate = 1, IsDelete = 1
        WHERE Id = 1920;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuPermission (Id, MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
        VALUES (1920, 13002, 7, 1, 1, 1, 1, 1);
    END

    SET IDENTITY_INSERT dbo.MenuPermission OFF;
    PRINT '   -> Permisos procesados exitosamente (' + CAST(266 AS VARCHAR) + ' registros).';

    PRINT '>> [4/4] Reactivando y validando integridad de constraints...';

    ALTER TABLE dbo.Menu WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.MenuPermission WITH CHECK CHECK CONSTRAINT ALL;

    COMMIT TRANSACTION;
    PRINT '===================================================================================================';
    PRINT '>> RESTRUCTURACIÓN COMPLETADA SATISFACTORIAMENTE EN BASE DE DATOS.';
    PRINT '===================================================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT '>> ERROR: Se ejecutó ROLLBACK TRANSACTION debido a una excepción:';
    END

    -- Asegurar apagar IDENTITY_INSERT en caso de error
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Menu'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Menu OFF;
    IF OBJECTPROPERTY(OBJECT_ID('dbo.MenuPermission'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.MenuPermission OFF;

    PRINT 'Mensaje: ' + ERROR_MESSAGE();
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    THROW;
END CATCH;
GO
