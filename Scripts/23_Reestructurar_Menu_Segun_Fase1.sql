-- =========================================================================
-- Script: 23_Reestructurar_Menu_Segun_Fase1.sql
-- Propósito: Reestructurar el menú principal y submenús según requerimiento FFC-112 Fase I
-- 1. Raíces: 1. Tablero, 2. Estructura Financiera, 3. Cartera de Crédito, 4. Evaluación SBR, 5. Administración
-- 2. Cartera de Crédito:
--    - Ubicada después de Estructura Financiera
--    - Submenús: 1. Riesgo de crédito, 2. Desempeño, 3. Indicadores
--    - Desvincular Matrices de Mora (4110)
-- 3. Estructura Financiera:
--    - Proyecciones contiguo a Tasas implícitas
--    - Desvincular Liquidez (5000)
-- 4. Administración:
--    - Opción "Autodiagnóstico" exclusiva para roles FFC (1, 2, 6)
-- =========================================================================

BEGIN TRANSACTION;

BEGIN TRY

    -- ---------------------------------------------------------------------
    -- 1. Orden y Visibilidad de los Menús Raíz
    -- ---------------------------------------------------------------------
    UPDATE dbo.Menu SET SortOrder = 1 WHERE Id = 1000; -- Tablero
    UPDATE dbo.Menu SET SortOrder = 2 WHERE Id = 1500; -- Estructura Financiera
    UPDATE dbo.Menu SET SortOrder = 3 WHERE Id = 4000; -- Cartera de Crédito
    UPDATE dbo.Menu SET SortOrder = 4 WHERE Id = 6000; -- Evaluación SBR
    UPDATE dbo.Menu SET SortOrder = 5 WHERE Id = 9000; -- Administración

    -- Otros módulos raíz se dejan con SortOrder posterior
    UPDATE dbo.Menu SET SortOrder = 6 WHERE Id = 2250;
    UPDATE dbo.Menu SET SortOrder = 7 WHERE Id = 6500;
    UPDATE dbo.Menu SET SortOrder = 8 WHERE Id = 7000;
    UPDATE dbo.Menu SET SortOrder = 9 WHERE Id = 8000;
    UPDATE dbo.Menu SET SortOrder = 10 WHERE Id = 11000;
    UPDATE dbo.Menu SET SortOrder = 11 WHERE Id = 13000;

    -- ---------------------------------------------------------------------
    -- 2. Cartera de Crédito (Id = 4000)
    --    Orden estricto de submenús:
    --    1. Riesgo de crédito (Id 4130)
    --    2. Desempeño (Id 4100)
    --    3. Indicadores (Id 3110)
    -- ---------------------------------------------------------------------
    UPDATE dbo.Menu 
    SET MenuText = 'Riesgo de crédito', SortOrder = 1, ParentId = 4000 
    WHERE Id = 4130;

    UPDATE dbo.Menu 
    SET MenuText = 'Desempeño', SortOrder = 2, ParentId = 4000 
    WHERE Id = 4100;

    UPDATE dbo.Menu 
    SET MenuText = 'Indicadores', SortOrder = 3, ParentId = 4000 
    WHERE Id = 3110;

    -- Asegurar permisos para 4130 a todos los roles operativos/maestros como 4100
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 4130, r.Id, 1, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE r.Id IN (1, 2, 4, 5, 6, 7, 10, 12)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 4130 AND mp.RoleId = r.Id);

    -- Asegurar permisos para 3110 a todos los roles
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 3110, r.Id, 3, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE r.Id IN (1, 2, 4, 5, 6, 7, 10, 12)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 3110 AND mp.RoleId = r.Id);

    -- Desasociar permisos de Matrices de Mora (4110) para no mostrarla en menú de Cartera
    DELETE FROM dbo.MenuPermission WHERE MenuId = 4110;

    -- ---------------------------------------------------------------------
    -- 3. Estructura Financiera (Id = 1500)
    --    Submenús:
    --    - Estructura Financiera (Id 2100, SortOrder = 1)
    --    - Estructura de Fondeo (Id 13002, SortOrder = 2)
    --    - Indicadores Financieros (Id 3000, SortOrder = 3)
    --    - Márgenes Implícitos (Id 5110, SortOrder = 4)
    --    - Tasas implícitas (Id 5120, SortOrder = 5)
    --    - Proyecciones (Id 2260, SortOrder = 6) [Contiguo a Tasas]
    --    - Desasociar Liquidez (5000 / 5100 / 5130)
    -- ---------------------------------------------------------------------
    UPDATE dbo.Menu SET SortOrder = 1 WHERE Id = 2100;
    UPDATE dbo.Menu SET SortOrder = 2, ParentId = 1500 WHERE Id = 13002;
    UPDATE dbo.Menu SET SortOrder = 3 WHERE Id = 3000;
    UPDATE dbo.Menu SET SortOrder = 4 WHERE Id = 5110;
    UPDATE dbo.Menu SET MenuText = 'Tasas implícitas', SortOrder = 5, ParentId = 1500 WHERE Id = 5120;
    UPDATE dbo.Menu SET MenuText = 'Proyecciones', SortOrder = 6, ParentId = 1500 WHERE Id = 2260;

    -- Asignar permisos a Estructura de Fondeo (13002) para que los usuarios puedan verla
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 13002, r.Id, 2, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE r.Id IN (1, 2, 4, 5, 6, 7, 10, 12)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 13002 AND mp.RoleId = r.Id);

    -- Asegurar permisos para Proyecciones (2260) bajo 1500
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    SELECT 2260, r.Id, 6, 1, 1, 1, 1
    FROM dbo.Role r
    WHERE r.Id IN (1, 2, 4, 5, 6, 7, 10, 12)
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuPermission mp WHERE mp.MenuId = 2260 AND mp.RoleId = r.Id);

    -- Desasociar Liquidez de la visualización
    DELETE FROM dbo.MenuPermission WHERE MenuId IN (5000, 5100, 5130);

    -- ---------------------------------------------------------------------
    -- 4. Administración (Id = 9000): Autodiagnóstico exclusivo FFC
    --    - Si existe 9135 ("Evaluación"), renombrar a "Autodiagnóstico"
    --    - Restringir permisos únicamente a roles FFC (1, 2, 6)
    -- ---------------------------------------------------------------------
    UPDATE dbo.Menu 
    SET MenuText = 'Autodiagnóstico', MenuURL = 'Evaluacion/Index', ParentId = 9000, SortOrder = 6
    WHERE Id = 9135;

    -- Limpiar permisos previos de 9135 y otorgar solo a roles FFC (1: Admin, 2: Operativo, 6: Director)
    DELETE FROM dbo.MenuPermission WHERE MenuId = 9135;
    INSERT INTO dbo.MenuPermission (MenuId, RoleId, SortOrder, IsCreate, IsRead, IsUpdate, IsDelete)
    VALUES 
        (9135, 1, 6, 1, 1, 1, 1),
        (9135, 2, 6, 1, 1, 1, 1),
        (9135, 6, 6, 1, 1, 1, 1);

    COMMIT TRANSACTION;
    PRINT 'MENU_REESTRUCTURACION_EXITOSA';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrMsg, 16, 1);
END CATCH;
GO
