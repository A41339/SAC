-- ===========================================================================================
-- Script 25: Configuración del Parámetro de Visibilidad del Plan de Remediación SBR
-- Fecha: 2026-10-20
-- Descripción:
--   1. Registra en dbo.Parametros la llave 'Mostrar_Plan_Remediacion' para controlar de forma
--      centralizada si el Plan de Remediación es visible o se oculta en el módulo de Evaluación.
--   2. Valor por defecto: '1' (1 = Visible / Activo, 0 = Oculto / Inactivo).
--   3. Compatible con valores booleanos: '1', '0', 'true', 'false', 'S', 'N'.
--   4. Idempotente y transaccional con control de errores.
-- ===========================================================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '================================================================================';
    PRINT '>> Configurando Parámetro [Mostrar_Plan_Remediacion] en dbo.Parametros...';
    PRINT '================================================================================';

    IF NOT EXISTS (SELECT 1 FROM dbo.Parametros WHERE Llave = 'Mostrar_Plan_Remediacion')
    BEGIN
        INSERT INTO dbo.Parametros (Llave, Valor, Descripcion)
        VALUES (
            'Mostrar_Plan_Remediacion',
            '1',
            'Controla si se muestra o se oculta el acceso y consulta al Plan de Remediación en el módulo de Evaluación SBR (1: Visible, 0: Oculto).'
        );
        PRINT '   -> Parámetro [Mostrar_Plan_Remediacion] creado exitosamente con valor ''1'' (Visible).';
    END
    ELSE
    BEGIN
        UPDATE dbo.Parametros
        SET Descripcion = 'Controla si se muestra o se oculta el acceso y consulta al Plan de Remediación en el módulo de Evaluación SBR (1: Visible, 0: Oculto).'
        WHERE Llave = 'Mostrar_Plan_Remediacion';
        PRINT '   -> Parámetro [Mostrar_Plan_Remediacion] ya existía y su descripción fue actualizada.';
    END

    COMMIT TRANSACTION;
    PRINT '================================================================================';
    PRINT '>> Configuración completada correctamente.';
    PRINT '>> Para desactivar/ocultar el Plan de Remediación en la evaluación, ejecute:';
    PRINT '>> UPDATE dbo.Parametros SET Valor = ''0'' WHERE Llave = ''Mostrar_Plan_Remediacion'';';
    PRINT '>> Para reactivar/mostrar el Plan de Remediación, ejecute:';
    PRINT '>> UPDATE dbo.Parametros SET Valor = ''1'' WHERE Llave = ''Mostrar_Plan_Remediacion'';';
    PRINT '================================================================================';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    PRINT 'Error al configurar el parámetro: ' + @ErrorMessage;
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
