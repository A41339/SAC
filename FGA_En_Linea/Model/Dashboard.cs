using DotNet.Highcharts;

namespace FGA.Models
{
    public class Dashboard
    {
        public static int cantidad = 2;
        public int idCompromiso = 3;
        public int idMorosidad = 22;
        public int idRiesgo = 333;
        public int idActivo = 60;
        public int idPerdidaEsperada = 30;
        public int idPerdidaAcumulada = 127;
        public int idRiesgoTasa = 342;
        public int idRiesgoCambiario = 351;
        public int idCostoAdmin = 118;
        public int idSuficiencia = 1;
        public int idCalceMes = 192;
        public int idCalce3Mes = 258;
        public int idICL = 100000;
        public int idApalancamiento = 1100;
        public int idCN1 = -1;
        public int idCCN1 = -2;

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

    }
}

