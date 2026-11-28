USE [FGA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT '=====================================================================================';
PRINT 'RECONFIGURACIÓN DEL MENÚ: EVALUACIÓN DE RIESGOS, TRASLADO DE CARTERA Y CRÉDITO';
PRINT '=====================================================================================';

BEGIN TRANSACTION;

BEGIN TRY

    -- ---------------------------------------------------------------------------------
    -- 1. MENÚS RAÍZ (SIDEBAR PRINCIPAL)
    --    1. Tablero (1000)
    --    2. Estructura Financiera (1500)
    --    3. Evaluación Riesgos (7500) - [NUEVO MÓDULO RAÍZ]
    --    4. Evaluación SBR (6000)
    --    5. Administración (9000)
    -- ---------------------------------------------------------------------------------
    PRINT '1. Configurando el nuevo módulo raíz [Evaluación Riesgos] (7500)...';

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7500)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación Riesgos',
            MenuURL = 'root',
            ParentId = NULL,
            SortOrder = 3,
            MenuIcon = '<i class="fa fa-calculator"></i>',
            Description = 'Análisis de colocación, morosidad y concentración de la cartera de crédito y liquidez.'
        WHERE Id = 7500;
        PRINT '   -> Opción 7500 [Evaluación Riesgos] actualizada.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7500, 'Evaluación Riesgos', 'root', NULL, 3, '<i class="fa fa-calculator"></i>', 'Análisis de colocación, morosidad y concentración de la cartera de crédito y liquidez.');
        PRINT '   -> Opción 7500 [Evaluación Riesgos] creada exitosamente.';
    END

    -- Orden estricto de los módulos raíz
    UPDATE dbo.Menu SET SortOrder = 1, ParentId = NULL WHERE Id = 1000; -- Tablero
    UPDATE dbo.Menu SET SortOrder = 2, ParentId = NULL WHERE Id = 1500; -- Estructura Financiera
    UPDATE dbo.Menu SET SortOrder = 3, ParentId = NULL WHERE Id = 7500; -- Evaluación Riesgos
    UPDATE dbo.Menu SET SortOrder = 4, ParentId = NULL WHERE Id = 6000; -- Evaluación SBR
    UPDATE dbo.Menu SET SortOrder = 5, ParentId = NULL WHERE Id = 9000; -- Administración

    -- Otros módulos raíz secundarios (si existen) se colocan posteriormente
    UPDATE dbo.Menu SET SortOrder = 6 WHERE Id = 2250;
    UPDATE dbo.Menu SET SortOrder = 7 WHERE Id = 6500;
    UPDATE dbo.Menu SET SortOrder = 8 WHERE Id = 7000;
    UPDATE dbo.Menu SET SortOrder = 9 WHERE Id = 8000;
    UPDATE dbo.Menu SET SortOrder = 10 WHERE Id = 11000;
    UPDATE dbo.Menu SET SortOrder = 11 WHERE Id = 13000;

    -- ---------------------------------------------------------------------------------
    -- 2. TRASLADAR [Cartera de Crédito] (4000) DENTRO DE [Estructura Financiera] (1500)
    -- ---------------------------------------------------------------------------------
    PRINT '2. Trasladando [Cartera de Crédito] (4000) bajo Estructura Financiera (1500)...';

    UPDATE dbo.Menu
    SET ParentId = 1500,
        SortOrder = 4,
        MenuText = 'Cartera de crédito',
        MenuURL = 'root',
        MenuIcon = '<i class="fa fa-credit-card"></i>',
        Description = 'Análisis de colocación, morosidad y concentración de la cartera de crédito.'
    WHERE Id = 4000;

    -- Reordenar las 7 tarjetas dentro de Estructura Financiera (1500)
    UPDATE dbo.Menu SET SortOrder = 1 WHERE Id = 2100;  -- Estados Financieros
    UPDATE dbo.Menu SET SortOrder = 2 WHERE Id = 13002; -- Estructura de Fondeo
    UPDATE dbo.Menu SET SortOrder = 3 WHERE Id = 3000;  -- Indicadores Financieros
    UPDATE dbo.Menu SET SortOrder = 4 WHERE Id = 4000;  -- Cartera de crédito
    UPDATE dbo.Menu SET SortOrder = 5 WHERE Id = 5110;  -- Márgenes Implícitos
    UPDATE dbo.Menu SET SortOrder = 6 WHERE Id = 2260;  -- Proyecciones
    UPDATE dbo.Menu SET SortOrder = 7 WHERE Id = 5120;  -- Tasas implícitas

    -- ---------------------------------------------------------------------------------
    -- 3. TRASLADAR [Liquidez] (5000) DENTRO DE [Evaluación Riesgos] (7500)
    -- ---------------------------------------------------------------------------------
    PRINT '3. Trasladando [Liquidez] (5000) bajo Evaluación Riesgos (7500)...';

    UPDATE dbo.Menu
    SET ParentId = 7500,
        SortOrder = 2,
        MenuText = 'Liquidez',
        MenuURL = 'Liquidez/Index',
        MenuIcon = '<i class="fa fa-tint"></i>',
        Description = 'Submódulo de Liquidez: Liquidez Contractual y Escenarios de Estrés.'
    WHERE Id = 5000;

    -- Sub-opciones de Liquidez se mantienen bajo 5000
    UPDATE dbo.Menu SET ParentId = 5000, SortOrder = 1 WHERE Id = 5010; -- Liquidez Contractual
    UPDATE dbo.Menu SET ParentId = 5000, SortOrder = 2 WHERE Id = 5020; -- Escenarios de Estrés

    -- ---------------------------------------------------------------------------------
    -- 4. CREAR/CONFIGURAR [Riesgo de crédito] (7510) BAJO [Evaluación Riesgos] (7500)
    -- ---------------------------------------------------------------------------------
    PRINT '4. Configurando [Riesgo de crédito] (7510) bajo Evaluación Riesgos (7500)...';

    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7510)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Riesgo de crédito',
            MenuURL = 'root',
            ParentId = 7500,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-pie-chart"></i>',
            Description = 'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.'
        WHERE Id = 7510;
        PRINT '   -> Opción 7510 [Riesgo de crédito] actualizada.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7510, 'Riesgo de crédito', 'root', 7500, 1, '<i class="fa fa-pie-chart"></i>', 'Monitoreo de saldos de cartera según acuerdo SUGEF 14-21.');
        PRINT '   -> Opción 7510 [Riesgo de crédito] creada exitosamente.';
    END

    -- ---------------------------------------------------------------------------------
    -- 5. CONFIGURAR LAS 5 SUB-OPCIONES BAJO [Riesgo de crédito] (ParentId = 7510)
    -- ---------------------------------------------------------------------------------
    PRINT '5. Configurando las 5 opciones de Riesgo de Crédito (7511 - 7515)...';

    -- 5.1 Indicadores de alerta (7511)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7511)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Indicadores de alerta',
            MenuURL = 'RiesgoCredito/Alertas',
            ParentId = 7510,
            SortOrder = 1,
            MenuIcon = '<i class="fa fa-bell-o"></i>',
            Description = 'Monitoreo del deterioro del riesgo de crédito mediante indicadores de tendencia, niveles y límites internos.'
        WHERE Id = 7511;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7511, 'Indicadores de alerta', 'RiesgoCredito/Alertas', 7510, 1, '<i class="fa fa-bell-o"></i>', 'Monitoreo del deterioro del riesgo de crédito mediante indicadores de tendencia, niveles y límites internos.');
    END

    -- 5.2 Modelo de matrices (7512)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7512)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Modelo de matrices',
            MenuURL = 'Cartera14_21/Index',
            ParentId = 7510,
            SortOrder = 2,
            MenuIcon = '<i class="fa fa-th"></i>',
            Description = 'Analice la composición de las fuentes de fondeo y matrices de transición de la entidad.'
        WHERE Id = 7512;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7512, 'Modelo de matrices', 'Cartera14_21/Index', 7510, 2, '<i class="fa fa-th"></i>', 'Analice la composición de las fuentes de fondeo y matrices de transición de la entidad.');
    END

    -- 5.3 Modelo de cosechas (7513)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7513)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Modelo de cosechas',
            MenuURL = 'Cartera14_21/Index',
            ParentId = 7510,
            SortOrder = 3,
            MenuIcon = '<i class="fa fa-line-chart"></i>',
            Description = 'Analice las curvas de maduración y cosechas crediticias de la entidad.'
        WHERE Id = 7513;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7513, 'Modelo de cosechas', 'Cartera14_21/Index', 7510, 3, '<i class="fa fa-line-chart"></i>', 'Analice las curvas de maduración y cosechas crediticias de la entidad.');
    END

    -- 5.4 Simulaciones de mora (7514)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7514)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Simulaciones de mora',
            MenuURL = 'Cartera14_21/Index',
            ParentId = 7510,
            SortOrder = 4,
            MenuIcon = '<i class="fa fa-sliders"></i>',
            Description = 'Simulaciones prospectivas de morosidad y estrés según acuerdo SUGEF 14-21.'
        WHERE Id = 7514;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7514, 'Simulaciones de mora', 'Cartera14_21/Index', 7510, 4, '<i class="fa fa-sliders"></i>', 'Simulaciones prospectivas de morosidad y estrés según acuerdo SUGEF 14-21.');
    END

    -- 5.5 Evaluación de riesgo FFC (7515)
    IF EXISTS (SELECT 1 FROM dbo.Menu WHERE Id = 7515)
    BEGIN
        UPDATE dbo.Menu
        SET MenuText = 'Evaluación de riesgo FFC',
            MenuURL = 'Cartera14_21/Index',
            ParentId = 7510,
            SortOrder = 5,
            MenuIcon = '<i class="fa fa-shield"></i>',
            Description = 'Metodología y evaluación del perfil de riesgo de crédito FFC.'
        WHERE Id = 7515;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Menu (Id, MenuText, MenuURL, ParentId, SortOrder, MenuIcon, Description)
        VALUES (7515, 'Evaluación de riesgo FFC', 'Cartera14_21/Index', 7510, 5, '<i class="fa fa-shield"></i>', 'Metodología y evaluación del perfil de riesgo de crédito FFC.');
    END

    -- ---------------------------------------------------------------------------------
    -- 6. ASIGNACIÓN DE PERMISOS EN dbo.MenuPermission
    -- ---------------------------------------------------------------------------------
    PRINT '6. Asignando permisos en dbo.MenuPermission para roles activos...';

    DECLARE @MenuIds TABLE (Id INT);
    INSERT INTO @MenuIds (Id) VALUES (7500), (7510), (7511), (7512), (7513), (7514), (7515), (4000), (5000), (5010), (5020);

    -- Insertar permisos completos para roles 1, 2, 4, 5, 6, 7, 10, 12
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT m.Id, r.Id, 1, 1, 1, 1, 1
    FROM @MenuIds m
    CROSS JOIN dbo.Role r
    WHERE r.Id IN (1, 2, 4, 5, 6, 7, 10, 12)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.MenuPermission mp 
          WHERE mp.MenuId = m.Id AND mp.RoleId = r.Id
      );

    PRINT '=====================================================================================';
    PRINT 'RECONFIGURACIÓN DE MENÚ COMPLETADA EXITOSAMENTE';
    PRINT '=====================================================================================';

    COMMIT TRANSACTION;

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO
