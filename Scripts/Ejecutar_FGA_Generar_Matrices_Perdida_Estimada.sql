USE [FGA]
GO

SET NOCOUNT ON;

PRINT '===================================================================='
PRINT 'Iniciando ejecucion de FGA_Generar_Matrices_Perdida_Estimada'
PRINT 'Desde: Mayo 2025 en adelante'
PRINT '===================================================================='

-- 1. Tabla temporal con las entidades solicitadas
DECLARE @Entidades TABLE (
    IdEntidad NVARCHAR(5) NOT NULL
);

INSERT INTO @Entidades (IdEntidad)
VALUES 
    ('13'),
    ('16'),
    ('2'),
    ('24'),
    ('3'),
    ('4'),
    ('5'),
    ('7'),
    ('9');

-- 2. Tabla temporal de periodos a procesar
CREATE TABLE #Periodos (
    Periodo DATE NOT NULL PRIMARY KEY
);

-- Obtenemos primero los periodos reales existentes a partir de mayo 2025 en SALIDA_MATRICES
INSERT INTO #Periodos (Periodo)
SELECT DISTINCT PERIODO 
FROM [dbo].[SALIDA_MATRICES] WITH (NOLOCK)
WHERE PERIODO >= '2025-05-01';

-- Si no hubiera en SALIDA_MATRICES, buscamos en SALIDA_BALANCE_COMPROBACION
IF NOT EXISTS (SELECT 1 FROM #Periodos)
BEGIN
    INSERT INTO #Periodos (Periodo)
    SELECT DISTINCT PERIODO 
    FROM [dbo].[SALIDA_BALANCE_COMPROBACION] WITH (NOLOCK)
    WHERE PERIODO >= '2025-05-01';
END

-- Si aun no hay periodos cargados en tablas de salida, generamos los meses calendario desde 2025-05-01 hasta la fecha actual
IF NOT EXISTS (SELECT 1 FROM #Periodos)
BEGIN
    DECLARE @FechaInicio DATE = '2025-05-01';
    DECLARE @FechaFin DATE = GETDATE();

    WHILE @FechaInicio <= @FechaFin
    BEGIN
        INSERT INTO #Periodos (Periodo) VALUES (@FechaInicio);
        SET @FechaInicio = DATEADD(MONTH, 1, @FechaInicio);
    END
END

-- 3. Cursor / Bucle para ejecutar el SP por cada combinación de Entidad y Periodo
DECLARE @IDENTIDAD NVARCHAR(5);
DECLARE @PERIODO DATE;
DECLARE @TotalProcesados INT = 0;
DECLARE @TotalErrores INT = 0;

DECLARE cur_ejecucion CURSOR LOCAL FAST_FORWARD FOR
SELECT E.IdEntidad, P.Periodo
FROM @Entidades E
CROSS JOIN #Periodos P
ORDER BY E.IdEntidad, P.Periodo;

OPEN cur_ejecucion;

FETCH NEXT FROM cur_ejecucion INTO @IDENTIDAD, @PERIODO;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        -- Opcional: Eliminar registros previos para este periodo y entidad para evitar duplicidad de calculos
        DELETE FROM [dbo].[Salida_Matrices_Perdida_Estimada] 
        WHERE [IdEntidad] = @IDENTIDAD AND [Periodo] = @PERIODO;

        -- Invocacion del procedimiento almacenado
        EXEC [dbo].[FGA_Generar_Matrices_Perdida_Estimada] 
            @IDENTIDAD = @IDENTIDAD, 
            @PERIODO = @PERIODO;

        SET @TotalProcesados = @TotalProcesados + 1;
        PRINT 'OK -> Entidad: ' + @IDENTIDAD + ' | Periodo: ' + CONVERT(VARCHAR(10), @PERIODO, 120);
    END TRY
    BEGIN CATCH
        SET @TotalErrores = @TotalErrores + 1;
        PRINT 'ERROR -> Entidad: ' + @IDENTIDAD + ' | Periodo: ' + CONVERT(VARCHAR(10), @PERIODO, 120) 
              + ' | Mensaje: ' + ERROR_MESSAGE();
    END CATCH

    FETCH NEXT FROM cur_ejecucion INTO @IDENTIDAD, @PERIODO;
END

CLOSE cur_ejecucion;
DEALLOCATE cur_ejecucion;

DROP TABLE #Periodos;

PRINT '===================================================================='
PRINT 'Fin de proceso.'
PRINT 'Total procesados correctamente: ' + CAST(@TotalProcesados AS VARCHAR(10))
PRINT 'Total con errores / omisiones: ' + CAST(@TotalErrores AS VARCHAR(10))
PRINT '===================================================================='
GO
