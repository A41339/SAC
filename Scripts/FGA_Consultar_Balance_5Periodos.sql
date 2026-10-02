/****** Objeto:  StoredProcedure [dbo].[FGA_Consultar_Balance_5Periodos]    Fecha: 26/09/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =========================================================================================
-- AUTOR:       FGA / SAC
-- FECHA:       26/09/2026
-- DESCRIPCIÓN: Consulta de alta velocidad para los 5 períodos del Balance General.
--              Sustituye las 5 llamadas secuenciales de FGA_Consultar_Balance_Comprobacion_Rango
--              por una única consulta indexada directa, sin tablas temporales ni bucles WHILE.
--
-- PARÁMETROS:
--   @IDENTIDAD: Código de entidad (ej. '13', '01')
--   @P1..@P5:   Fechas de los 5 períodos de comparación contable
-- =========================================================================================
CREATE OR ALTER PROCEDURE [dbo].[FGA_Consultar_Balance_5Periodos]
    @IDENTIDAD NVARCHAR(5),
    @P1 DATE,
    @P2 DATE,
    @P3 DATE,
    @P4 DATE,
    @P5 DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @IDENTIDAD = '99'
    BEGIN
        SET @IDENTIDAD = '13';
    END

    -- Consulta directa indexada con WITH(NOLOCK)
    -- Soporta tanto fechas de corte al primer día de mes como al último día (EOMONTH)
    SELECT 
        Periodo,
        Cuenta,
        Credito,
        Debito,
        SaldoFinal
    FROM FGA.dbo.SALIDA_BALANCE_COMPROBACION WITH(NOLOCK)
    WHERE IdEntidad = @IDENTIDAD
      AND (
          Periodo IN (@P1, @P2, @P3, @P4, @P5)
          OR Periodo IN (EOMONTH(@P1), EOMONTH(@P2), EOMONTH(@P3), EOMONTH(@P4), EOMONTH(@P5))
      )
    ORDER BY Cuenta ASC, Periodo ASC;

END
GO
