using DotNet.Highcharts;

namespace FGA.Models
{
    public class Dashboard
    {
        public static int cantidad = 2;
        // Identificadores de la Serie 8000 para el nuevo Dashboard (coexistencia con sitio viejo)
        public int idCompromiso = 8003;
        public int idMorosidad = 8022;
        public int idRiesgo = 8333;
        public int idActivo = 8060;
        public int idPerdidaEsperada = 8030;
        public int idPerdidaAcumulada = 8127;
        public int idRiesgoTasa = 8342;
        public int idRiesgoCambiario = 8351;
        public int idCostoAdmin = 8118;
        public int idSuficiencia = 8001;
        public int idCalceMes = 8192;
        public int idCalce3Mes = 8258;
        public int idICL = 8900;
        public int idApalancamiento = 9100;
        public int idCN1 = 8991;
        public int idCCN1 = 8992;

        public bool prudencial = false;

        public decimal[] Compromiso = new decimal[cantidad];
        public decimal[] Morosidad = new decimal[cantidad];
        public decimal[] Riesgo = new decimal[cantidad];
        public decimal[] Activo = new decimal[cantidad];

        public decimal[] CostoAdmin = new decimal[cantidad];
        public decimal[] Suficiencia = new decimal[cantidad];
        public decimal[] CalceMes = new decimal[cantidad];
        public decimal[] Calce3Meses = new decimal[cantidad];

        public decimal[] PerdidaEsperada = new decimal[cantidad];
        public decimal[] PerdidaAcumulada = new decimal[cantidad];
        public decimal[] RiesgoTasa = new decimal[cantidad];
        public decimal[] RiesgoCambiario = new decimal[cantidad];

        public decimal[] ICL = new decimal[cantidad];
        public decimal[] Apalancamiento = new decimal[cantidad];
        public decimal[] CN1 = new decimal[cantidad];
        public decimal[] CNN1 = new decimal[cantidad];

        public int idResultadoPeriodo = 8400;
        public int idCarteraCredito = 8130;
        public int idObligacionesPublico = 8210;

        public decimal[] ResultadoPeriodo = new decimal[cantidad];
        public decimal[] CarteraCredito = new decimal[cantidad];
        public decimal[] ObligacionesPublico = new decimal[cantidad];

        // Indicadores reales de Cartera y Otros Indicadores
        public int idDeudores100 = 8002;
        public int idInversionesTitulos = 8104;
        public int idCaptacionesPlazo = 8106;
        public int idEstimacionesMora = 8109;
        public int idRentabilidad = 8112;

        public decimal[] Deudores100 = new decimal[cantidad];
        public decimal[] InversionesTitulos = new decimal[cantidad];
        public decimal[] CaptacionesPlazo = new decimal[cantidad];
        public decimal[] EstimacionesMora = new decimal[cantidad];
        public decimal[] Rentabilidad = new decimal[cantidad];

        public Highcharts BrechaLiquidez;
        public Highcharts DiferencialTasas;
        public Highcharts IndicadoresRentabilidad;
        public Highcharts Mora;
        public Highcharts CapitalSocial;
        public Highcharts VariacionCartera;
        public Highcharts pSuficiencia;

        public Highcharts pBrechaLiquidez;
        public Highcharts pDiferencialTasas;
        public Highcharts pIndicadoresRentabilidad;
        public Highcharts pMora;
        public Highcharts pCapitalSocial;
        public Highcharts pVariacionCartera;

        public TablaPerdidaEsperada PerdidaEsperadaTabla = new TablaPerdidaEsperada();
    }

    public class TablaPerdidaEsperada
    {
        public string PeriodoAnterior { get; set; } = "";
        public string PeriodoActual { get; set; } = "";
        public string PeriodoRefMensual { get; set; } = "";
        public string PeriodoRefInteranual { get; set; } = "";
        public decimal MontoAnterior { get; set; }
        public decimal MontoActual { get; set; }
        public decimal VarMensualMonto { get; set; }
        public decimal VarMensualPct { get; set; }
        public decimal VarInteranualMonto { get; set; }
        public decimal VarInteranualPct { get; set; }
        public bool HasData { get; set; }
        public bool HasMensual { get; set; }
        public bool HasInteranual { get; set; }
    }
}

