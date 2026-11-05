using System;
using System.Collections.Generic;

namespace FGA.Model
{
    #region ViewModels Principales del Submódulo de Liquidez

    public class LiquidezIndexViewModel
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string Periodo1 { get; set; }
        public string Periodo2 { get; set; }
        public string Periodo1Header { get; set; }
        public string Periodo2Header { get; set; }
        public string PeriodoCorteGap { get; set; }
        public string TabActiva { get; set; } = "indicadores"; // "indicadores", "icl", "gap", "ifne", "estres"
        public string MacroSeccion { get; set; } = "contractual"; // "contractual", "estres"

        // Datos de pestañas
        public List<IndicadorLiquidezItem> Indicadores { get; set; } = new List<IndicadorLiquidezItem>();
        public ICLViewModel ICLDatos { get; set; } = new ICLViewModel();
        public GapContractualViewModel GapDatos { get; set; } = new GapContractualViewModel();
        public SupuestosSensibilizacionViewModel Supuestos { get; set; } = new SupuestosSensibilizacionViewModel();
        public RazonCoberturaViewModel RazonCobertura { get; set; } = new RazonCoberturaViewModel();
        public ICLEstresadoViewModel ICLEstresado { get; set; } = new ICLEstresadoViewModel();
        public BrechasLiquidezViewModel BrechasEstres { get; set; } = new BrechasLiquidezViewModel();
        public HorizonteSupervivenciaViewModel Horizonte { get; set; } = new HorizonteSupervivenciaViewModel();
    }

    #endregion

    #region Liquidez Contractual - Indicadores

    public class IndicadorLiquidezItem
    {
        public int Orden { get; set; }
        public string Nombre { get; set; }
        public string Formula { get; set; }
        public string QueMide { get; set; }
        public decimal ValorP1 { get; set; }
        public decimal ValorP2 { get; set; }
        public decimal Variacion => ValorP2 - ValorP1;
        public bool EsPorcentaje { get; set; } = true;
        public int TipoGraficoAsociado { get; set; } = 1;

        public string ValorP1Formateado => EsPorcentaje ? string.Format("{0:N2}%", ValorP1 * 100m) : string.Format("{0:N2}", ValorP1);
        public string ValorP2Formateado => EsPorcentaje ? string.Format("{0:N2}%", ValorP2 * 100m) : string.Format("{0:N2}", ValorP2);
        public string VariacionFormateada
        {
            get
            {
                decimal varVal = EsPorcentaje ? (Variacion * 100m) : Variacion;
                string signo = varVal >= 0 ? "+" : "";
                return EsPorcentaje ? string.Format("{0}{1:N2} p.p.", signo, varVal) : string.Format("{0}{1:N2}", signo, varVal);
            }
        }
    }

    public class IndicadorLiquidezHistoricoViewModel
    {
        public int Orden { get; set; }
        public string Nombre { get; set; }
        public string Formula { get; set; }
        public string PeriodoI { get; set; }
        public string PeriodoF { get; set; }
        public bool EsPorcentaje { get; set; } = true;
        public List<string> Categorias { get; set; } = new List<string>();
        public List<decimal> Valores { get; set; } = new List<decimal>();
    }

    #endregion

    #region Liquidez Contractual - ICL

    public class ICLViewModel
    {
        public string Periodo { get; set; }
        public int MonedaTipo { get; set; } = 1; // 1 = Colones, 2 = Dólares
        public bool TieneDolares { get; set; } = false;
        public decimal ICLRatio { get; set; }
        public decimal FondoActivosLiquidos { get; set; }
        public decimal SalidasNetasEfectivo { get; set; }
        public List<ICLDetalleItem> Items { get; set; } = new List<ICLDetalleItem>();

        // Serie histórica de 12 meses para gráfico evolutivo (Requerimiento Word imagen 4)
        public List<string> HistoricoMeses { get; set; } = new List<string> { "Set 25", "Oct 25", "Nov 25", "Dic 25", "Ene 26", "Feb 26", "Mar 26", "Abr 26", "May 26", "Jun 26", "Jul 26", "Ago 26" };
        public List<decimal> HistoricoValores { get; set; } = new List<decimal> { 4.51m, 4.65m, 4.80m, 4.75m, 4.90m, 5.05m, 4.95m, 5.12m, 5.20m, 5.15m, 5.28m, 5.32m };
    }

    public class ICLDetalleItem
    {
        public string Codigo { get; set; }
        public int Nivel { get; set; }
        public string Concepto { get; set; }
        public decimal SaldoBruto { get; set; }
        public decimal Ponderador { get; set; }
        public decimal SaldoPonderado { get; set; }
        public bool EsTotalizador { get; set; }
    }

    #endregion

    #region Liquidez Contractual - GAP Contractual

    public class GapContractualViewModel
    {
        public string PeriodoCorte { get; set; }
        public List<string> Bandas { get; set; } = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 360 días", "(+) 360 días" };
        public List<decimal> Entradas { get; set; } = new List<decimal>();
        public List<decimal> Salidas { get; set; } = new List<decimal>();
        public List<decimal> BrechasBanda { get; set; } = new List<decimal>();
        public List<decimal> BrechasAcumuladas { get; set; } = new List<decimal>();

        // KPIs resumen oficiales (Requerimiento imagen 7)
        public decimal GapAcumulado30d { get; set; } = -802.84m;
        public decimal GapAcumulado90d { get; set; } = -831.42m;
        public decimal MaximaBrecha { get; set; } = -2016.97m;
        public string BandaCritica { get; set; } = "31 a 60 días";

        // Desglose de partidas detalladas (Requerimiento imagen 6 y Excel hoja Gap contractual)
        public List<decimal> Disponibilidades { get; set; } = new List<decimal> { 1214.13m, 0m, 0m, 0m, 0m, 0m, 0m };
        public List<decimal> RecuperacionCartera { get; set; } = new List<decimal> { 135.50m, 128.73m, 115.85m, 180.20m, 150.10m, 110.00m, 290.50m };
        public List<decimal> VencimientoInversiones { get; set; } = new List<decimal> { 552.50m, 551.50m, 551.00m, 30.00m, 30.10m, 5.00m, 20.00m };

        public List<decimal> VencimientosCDP { get; set; } = new List<decimal> { 127.48m, 242.78m, 388.45m, 686.38m, 1438.11m, 2888.37m, 3218.07m };
        public List<decimal> ObligacionesEntidades { get; set; } = new List<decimal> { 10.00m, 10.00m, 10.00m, 401.75m, 751.73m, 1006.70m, 278.14m };
        public List<decimal> OtrasCuentasPagar { get; set; } = new List<decimal> { 87.14m, 87.80m, 88.86m, 47.00m, 0m, 0m, 0m };
    }

    #endregion

    #region Escenarios de Estrés - Supuestos de Sensibilización

    public class SupuestosSensibilizacionViewModel
    {
        public string Entidad { get; set; }
        public string PeriodoCorte { get; set; }

        // 1. Renovación CDP
        public string ProyectorRenovacionCDP { get; set; } = "Histórico"; // "Histórico", "Manual"
        public decimal RenovacionCDP_Base { get; set; } = 0.85m;
        public decimal RenovacionCDP_Moderado { get; set; } = 0.68m;
        public decimal RenovacionCDP_Severo { get; set; } = 0.425m;

        // 2. Salida de depósitos vista (VaR Liquidez)
        public string ProyectorVaR { get; set; } = "VaR al 95%"; // "VaR al 95%", "VaR al 99%", "Máxima variación"
        public decimal SalidaVista_Base { get; set; } = 0.1008m;
        public decimal SalidaVista_Moderado { get; set; } = 0.1210m;
        public decimal SalidaVista_Severo { get; set; } = 0.1815m;

        // 3. Crecimiento Cartera de Crédito
        public string ProyectorCartera { get; set; } = "Tasa de crecimiento";
        public decimal CrecimientoCartera_Base { get; set; } = 0.0688m;
        public decimal CrecimientoCartera_Moderado { get; set; } = 0.0825m;
        public decimal CrecimientoCartera_Severo { get; set; } = 0.1032m;

        // 4. Disminución recaudo mensual cartera (Manual)
        public decimal DisminucionRecaudo_Base { get; set; } = 0.035m;
        public decimal DisminucionRecaudo_Moderado { get; set; } = 0.05m;
        public decimal DisminucionRecaudo_Severo { get; set; } = 0.10m;

        // 5. Haircut inversiones líquidas
        public string ProyectorHaircut { get; set; } = "Dato Manual";
        public decimal Haircut_Base { get; set; }
        public decimal Haircut_Moderado { get; set; }
        public decimal Haircut_Severo { get; set; }

        // 6. Salida Top 20 depositantes
        public decimal SalidaTop20_Base { get; set; } = 0.10m;
        public decimal SalidaTop20_Moderado { get; set; } = 0.25m;
        public decimal SalidaTop20_Severo { get; set; } = 0.50m;

        // 7. Líneas de financiamiento mensual (Manual)
        public decimal DisponibilidadLineas_Base { get; set; } = 75000000m;
        public decimal DisponibilidadLineas_Moderado { get; set; } = 50000000m;
        public decimal DisponibilidadLineas_Severo { get; set; } = 30000000m;

        // 8. Pago principal de obligaciones (Manual)
        public decimal PagoObligaciones_Base { get; set; } = 10000000m;
        public decimal PagoObligaciones_Moderado { get; set; } = 10000000m;
        public decimal PagoObligaciones_Severo { get; set; } = 10000000m;

        // 9. Cupones mensuales (Manual)
        public decimal Cupones_Base { get; set; } = 2500000m;
        public decimal Cupones_Moderado { get; set; } = 1500000m;
        public decimal Cupones_Severo { get; set; } = 1000000m;

        // 10. Capital social anual
        public string ProyectorCapitalSocial { get; set; } = "Tasa de crecimiento";
        public decimal CapitalSocial_Base { get; set; } = 0.035m;
        public decimal CapitalSocial_Moderado { get; set; } = 0.028m;
        public decimal CapitalSocial_Severo { get; set; } = 0.0175m;

        // 11. Otros ingresos mensual (Promedio 6 meses)
        public string ProyectorOtrosIngresos { get; set; } = "Promedio 6 meses";
        public decimal OtrosIngresos_Base { get; set; } = 0.0394m;
        public decimal OtrosIngresos_Moderado { get; set; } = 0.0315m;
        public decimal OtrosIngresos_Severo { get; set; } = 0.0197m;

        // 12. Egresos financieros mensual (Dato Manual)
        public decimal EgresosFinancieros_Base { get; set; } = 0.0100m;
        public decimal EgresosFinancieros_Moderado { get; set; } = 0.0200m;
        public decimal EgresosFinancieros_Severo { get; set; } = 0.0300m;

        // 13. Gastos administrativos mensual (Dato Manual)
        public decimal GastosAdministrativos_Base { get; set; } = 0.0150m;
        public decimal GastosAdministrativos_Moderado { get; set; } = 0.0250m;
        public decimal GastosAdministrativos_Severo { get; set; } = 0.0400m;
    }

    #endregion

    #region Escenarios de Estrés - Razón de Cobertura

    public class RazonCoberturaViewModel
    {
        public decimal ActivosLiquidos_Base { get; set; } = 1203.53m;
        public decimal ActivosLiquidos_Moderado { get; set; } = 1203.53m;
        public decimal ActivosLiquidos_Severo { get; set; } = 1203.53m;

        public decimal IngresosContractuales_Base { get; set; } = 693.81m;
        public decimal IngresosContractuales_Moderado { get; set; } = 685.99m;
        public decimal IngresosContractuales_Severo { get; set; } = 672.55m;

        // Desglose de ingresos (imagen 16)
        public decimal CarteraRecuperacion_Base { get; set; } = 135.50m;
        public decimal CarteraRecuperacion_Moderado { get; set; } = 128.73m;
        public decimal CarteraRecuperacion_Severo { get; set; } = 115.85m;

        public decimal Inversiones_Base { get; set; } = 552.50m;
        public decimal Inversiones_Moderado { get; set; } = 551.50m;
        public decimal Inversiones_Severo { get; set; } = 551.00m;

        public decimal Comisiones_Base { get; set; } = 5.81m;
        public decimal Comisiones_Moderado { get; set; } = 5.77m;
        public decimal Comisiones_Severo { get; set; } = 5.70m;

        public decimal EgresosContractuales_Base { get; set; } = 624.93m;
        public decimal EgresosContractuales_Moderado { get; set; } = 826.68m;
        public decimal EgresosContractuales_Severo { get; set; } = 1621.54m;

        // Desglose de egresos contractuales (imagen 16)
        public decimal CDPContractuales_Base { get; set; } = 127.48m;
        public decimal CDPContractuales_Moderado { get; set; } = 242.78m;
        public decimal CDPContractuales_Severo { get; set; } = 388.45m;

        public decimal SalidaTop20_Base { get; set; } = 162.03m;
        public decimal SalidaTop20_Moderado { get; set; } = 486.10m;
        public decimal SalidaTop20_Severo { get; set; } = 1134.23m;

        public decimal PagoObligaciones_Base { get; set; } = 10.00m;
        public decimal PagoObligaciones_Moderado { get; set; } = 10.00m;
        public decimal PagoObligaciones_Severo { get; set; } = 10.00m;

        public decimal EgresosFinAdm_Base { get; set; } = 87.14m;
        public decimal EgresosFinAdm_Moderado { get; set; } = 87.80m;
        public decimal EgresosFinAdm_Severo { get; set; } = 88.86m;

        public decimal DesembolsosCartera_Base { get; set; } = 124.86m;
        public decimal DesembolsosCartera_Moderado { get; set; } = 134.85m;
        public decimal DesembolsosCartera_Severo { get; set; } = 148.84m;

        public decimal CorridaDepositosVista_Base { get; set; } = 238.30m;
        public decimal CorridaDepositosVista_Moderado { get; set; } = 274.08m;
        public decimal CorridaDepositosVista_Severo { get; set; } = 381.43m;

        // Variables Proyectivas / Diferencia de escenarios (imagen 16)
        public decimal CDPDiferencia_Base { get; set; } = 0m;
        public decimal CDPDiferencia_Moderado { get; set; } = 115.28m;
        public decimal CDPDiferencia_Severo { get; set; } = 260.86m;

        public decimal SalidaTop20Diferencia_Base { get; set; } = 0m;
        public decimal SalidaTop20Diferencia_Moderado { get; set; } = 324.06m;
        public decimal SalidaTop20Diferencia_Severo { get; set; } = 972.18m;

        public decimal CorridaVistaDiferencia_Base { get; set; } = 0m;
        public decimal CorridaVistaDiferencia_Moderado { get; set; } = 35.78m;
        public decimal CorridaVistaDiferencia_Severo { get; set; } = 143.13m;

        public decimal DesembolsoCarteraDiferencia_Base { get; set; } = 0m;
        public decimal DesembolsoCarteraDiferencia_Moderado { get; set; } = 10.00m;
        public decimal DesembolsoCarteraDiferencia_Severo { get; set; } = 24.88m;

        public decimal RazonCobertura_Base { get; set; } = 2.23m;
        public decimal RazonCobertura_Moderado { get; set; } = 1.18m;
        public decimal RazonCobertura_Severo { get; set; } = 0.82m;
    }

    #endregion

    #region Escenarios de Estrés - ICL Estresado

    public class ICLEstresadoViewModel
    {
        public decimal ICLContractual { get; set; } = 1.55m;
        public decimal ICLBase { get; set; } = 1.55m;
        public decimal ICLModerado { get; set; } = 1.18m;
        public decimal ICLSevero { get; set; } = 0.82m;

        public decimal ICL_Base { get { return ICLBase; } set { ICLBase = value; } }
        public decimal ICL_Moderado { get { return ICLModerado; } set { ICLModerado = value; } }
        public decimal ICL_Severo { get { return ICLSevero; } set { ICLSevero = value; } }

        public decimal FALContractual { get; set; } = 1203.53m;
        public decimal FALBase { get; set; } = 1203.53m;
        public decimal FALModerado { get; set; } = 1103.53m;
        public decimal FALSevero { get; set; } = 933.53m;

        public decimal SalidasContractual { get; set; } = 360.12m;
        public decimal SalidasBase { get; set; } = 360.12m;
        public decimal SalidasModerado { get; set; } = 684.18m;
        public decimal SalidasSevero { get; set; } = 1332.31m;
    }

    #endregion

    #region Escenarios de Estrés - Brechas y Supervivencia

    public class BrechasLiquidezViewModel
    {
        public List<string> Bandas { get; set; } = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 360 días", "(+) 360 días" };
        
        // Resumen KPIs imagen 19
        public decimal Gap30d_Base { get; set; } = -22.72m;
        public decimal Gap90d_Base { get; set; } = -461.02m;
        public decimal FAL_Base { get; set; } = 1203.53m;

        public decimal Gap30d_Moderado { get; set; } = -560.18m;
        public decimal Gap90d_Moderado { get; set; } = -929.81m;
        public decimal FAL_Moderado { get; set; } = 1103.53m;

        public decimal Gap30d_Severo { get; set; } = -1583.61m;
        public decimal Gap90d_Severo { get; set; } = -1850.25m;
        public decimal FAL_Severo { get; set; } = 933.53m;

        public List<decimal> EntradasBase { get; set; } = new List<decimal> { 767.14m, 867.73m, 218.32m, 706.75m, 891.61m, 1096.19m, 5671.70m };
        public List<decimal> SalidasBase { get; set; } = new List<decimal> { 789.86m, 686.40m, 679.33m, 2018.24m, 1965.48m, 2032.98m, 7598.20m };
        public List<decimal> BrechasBase { get; set; } = new List<decimal> { -22.72m, 181.33m, -461.02m, -1311.49m, -1073.88m, -936.79m, -1926.50m };
        public List<decimal> AcumuladoBase { get; set; } = new List<decimal> { 1180.81m, 1362.14m, 901.12m, -410.37m, -1484.25m, -2421.04m, -4347.54m };

        public List<decimal> EntradasModerado { get; set; } = new List<decimal> { 744.77m, 845.36m, 195.95m, 639.65m, 824.25m, 1028.85m, 5604.36m };
        public List<decimal> SalidasModerado { get; set; } = new List<decimal> { 1304.95m, 1140.84m, 1125.76m, 3357.82m, 3245.27m, 3304.77m, 8870.00m };
        public List<decimal> BrechasModerado { get; set; } = new List<decimal> { -560.18m, -295.48m, -929.81m, -2718.17m, -2421.02m, -2275.92m, -3265.64m };
        public List<decimal> AcumuladoModerado { get; set; } = new List<decimal> { 643.35m, 347.87m, -581.94m, -3300.11m, -5721.13m, -7997.05m, -11262.69m };

        public List<decimal> EntradasSevero { get; set; } = new List<decimal> { 709.51m, 810.10m, 160.69m, 533.87m, 718.07m, 922.67m, 5498.19m };
        public List<decimal> SalidasSevero { get; set; } = new List<decimal> { 2293.12m, 2038.03m, 2010.95m, 6013.78m, 5811.54m, 5871.04m, 11436.27m };
        public List<decimal> BrechasSevero { get; set; } = new List<decimal> { -1583.61m, -1227.93m, -1850.25m, -5479.90m, -5093.47m, -4948.37m, -5938.08m };
        public List<decimal> AcumuladoSevero { get; set; } = new List<decimal> { -380.08m, -1608.01m, -3458.26m, -8938.16m, -14031.63m, -18980.00m, -24918.08m };
    }

    public class HorizonteSupervivenciaViewModel
    {
        public string HorizonteBase { get; set; } = "91 a 180 días";
        public string HorizonteModerado { get; set; } = "61 a 90 días";
        public string HorizonteSevero { get; set; } = "1 a 30 días";

        public decimal DiasSupervivenciaBase { get; set; } = 111m;
        public decimal DiasSupervivenciaModerado { get; set; } = 58m;
        public decimal DiasSupervivenciaSevero { get; set; } = 32m;

        public decimal BufferRemanenteBase { get; set; } = 901.12m;
        public decimal BufferRemanenteModerado { get; set; } = 347.87m;
        public decimal BufferRemanenteSevero { get; set; } = 1203.53m;

        public decimal SaldoFinalBase { get; set; } = -410.37m;
        public decimal SaldoFinalModerado { get; set; } = -581.94m;
        public decimal SaldoFinalSevero { get; set; } = -380.08m;

        public decimal ReduccionDias { get; set; } = -26m;

        // Trayectoria multiescenario para Highcharts
        public List<string> TrayectoriaCategorias { get; set; } = new List<string> { "Día 0", "Día 30", "Día 60", "Día 90", "Día 180", "Día 270", "Día 360" };
        public List<decimal> TrayectoriaBase { get; set; } = new List<decimal> { 1203.53m, 1180.81m, 1362.14m, 901.12m, -410.37m, -1484.25m, -2421.04m };
        public List<decimal> TrayectoriaModerado { get; set; } = new List<decimal> { 1103.53m, 643.35m, 347.87m, -581.94m, -3300.11m, -5721.13m, -7997.05m };
        public List<decimal> TrayectoriaSevero { get; set; } = new List<decimal> { 933.53m, -380.08m, -1608.01m, -3458.26m, -8938.16m, -14031.63m, -18980.00m };
    }

    #endregion
}
