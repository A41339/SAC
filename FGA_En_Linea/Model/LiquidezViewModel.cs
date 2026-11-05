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
        public List<string> HistoricoMeses { get; set; } = new List<string>();
        public List<decimal> HistoricoValores { get; set; } = new List<decimal>();
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
        public decimal GapAcumulado30d { get; set; }
        public decimal GapAcumulado90d { get; set; }
        public decimal MaximaBrecha { get; set; }
        public string BandaCritica { get; set; }

        // Desglose de partidas detalladas (Requerimiento imagen 6 y Excel hoja Gap contractual)
        public List<decimal> Disponibilidades { get; set; } = new List<decimal>();
        public List<decimal> RecuperacionCartera { get; set; } = new List<decimal>();
        public List<decimal> VencimientoInversiones { get; set; } = new List<decimal>();

        public List<decimal> VencimientosCDP { get; set; } = new List<decimal>();
        public List<decimal> ObligacionesEntidades { get; set; } = new List<decimal>();
        public List<decimal> OtrasCuentasPagar { get; set; } = new List<decimal>();
    }

    #endregion

    #region Escenarios de Estrés - Supuestos de Sensibilización

    public class SupuestosSensibilizacionViewModel
    {
        public string Entidad { get; set; }
        public string PeriodoCorte { get; set; }

        // 1. Renovación CDP
        public string ProyectorRenovacionCDP { get; set; } = "Histórico"; // "Histórico", "Manual"
        public decimal RenovacionCDP_Base { get; set; }
        public decimal RenovacionCDP_Moderado { get; set; }
        public decimal RenovacionCDP_Severo { get; set; }

        // 2. Salida de depósitos vista (VaR Liquidez)
        public string ProyectorVaR { get; set; } = "VaR al 95%"; // "VaR al 95%", "VaR al 99%", "Máxima variación"
        public decimal SalidaVista_Base { get; set; }
        public decimal SalidaVista_Moderado { get; set; }
        public decimal SalidaVista_Severo { get; set; }

        // 3. Crecimiento Cartera de Crédito
        public string ProyectorCartera { get; set; } = "Tasa de crecimiento";
        public decimal CrecimientoCartera_Base { get; set; }
        public decimal CrecimientoCartera_Moderado { get; set; }
        public decimal CrecimientoCartera_Severo { get; set; }

        // 4. Disminución recaudo mensual cartera (Manual)
        public decimal DisminucionRecaudo_Base { get; set; }
        public decimal DisminucionRecaudo_Moderado { get; set; }
        public decimal DisminucionRecaudo_Severo { get; set; }

        // 5. Haircut inversiones líquidas
        public string ProyectorHaircut { get; set; } = "Dato Manual";
        public decimal Haircut_Base { get; set; }
        public decimal Haircut_Moderado { get; set; }
        public decimal Haircut_Severo { get; set; }

        // 6. Salida Top 20 depositantes
        public decimal SalidaTop20_Base { get; set; }
        public decimal SalidaTop20_Moderado { get; set; }
        public decimal SalidaTop20_Severo { get; set; }

        // 7. Líneas de financiamiento mensual (Manual)
        public decimal DisponibilidadLineas_Base { get; set; }
        public decimal DisponibilidadLineas_Moderado { get; set; }
        public decimal DisponibilidadLineas_Severo { get; set; }

        // 8. Pago principal de obligaciones (Manual)
        public decimal PagoObligaciones_Base { get; set; }
        public decimal PagoObligaciones_Moderado { get; set; }
        public decimal PagoObligaciones_Severo { get; set; }

        // 9. Cupones mensuales (Manual)
        public decimal Cupones_Base { get; set; }
        public decimal Cupones_Moderado { get; set; }
        public decimal Cupones_Severo { get; set; }

        // 10. Capital social anual
        public string ProyectorCapitalSocial { get; set; } = "Tasa de crecimiento";
        public decimal CapitalSocial_Base { get; set; }
        public decimal CapitalSocial_Moderado { get; set; }
        public decimal CapitalSocial_Severo { get; set; }

        // 11. Otros ingresos mensual (Promedio 6 meses)
        public string ProyectorOtrosIngresos { get; set; } = "Promedio 6 meses";
        public decimal OtrosIngresos_Base { get; set; }
        public decimal OtrosIngresos_Moderado { get; set; }
        public decimal OtrosIngresos_Severo { get; set; }

        // 12. Egresos financieros mensual (Dato Manual)
        public decimal EgresosFinancieros_Base { get; set; }
        public decimal EgresosFinancieros_Moderado { get; set; }
        public decimal EgresosFinancieros_Severo { get; set; }

        // 13. Gastos administrativos mensual (Dato Manual)
        public decimal GastosAdministrativos_Base { get; set; }
        public decimal GastosAdministrativos_Moderado { get; set; }
        public decimal GastosAdministrativos_Severo { get; set; }
    }

    #endregion

    #region Escenarios de Estrés - Razón de Cobertura

    public class RazonCoberturaViewModel
    {
        public decimal ActivosLiquidos_Base { get; set; }
        public decimal ActivosLiquidos_Moderado { get; set; }
        public decimal ActivosLiquidos_Severo { get; set; }

        public decimal IngresosContractuales_Base { get; set; }
        public decimal IngresosContractuales_Moderado { get; set; }
        public decimal IngresosContractuales_Severo { get; set; }

        // Desglose de ingresos (imagen 16)
        public decimal CarteraRecuperacion_Base { get; set; }
        public decimal CarteraRecuperacion_Moderado { get; set; }
        public decimal CarteraRecuperacion_Severo { get; set; }

        public decimal Inversiones_Base { get; set; }
        public decimal Inversiones_Moderado { get; set; }
        public decimal Inversiones_Severo { get; set; }

        public decimal Comisiones_Base { get; set; }
        public decimal Comisiones_Moderado { get; set; }
        public decimal Comisiones_Severo { get; set; }

        public decimal EgresosContractuales_Base { get; set; }
        public decimal EgresosContractuales_Moderado { get; set; }
        public decimal EgresosContractuales_Severo { get; set; }

        // Desglose de egresos contractuales (imagen 16)
        public decimal CDPContractuales_Base { get; set; }
        public decimal CDPContractuales_Moderado { get; set; }
        public decimal CDPContractuales_Severo { get; set; }

        public decimal SalidaTop20_Base { get; set; }
        public decimal SalidaTop20_Moderado { get; set; }
        public decimal SalidaTop20_Severo { get; set; }

        public decimal PagoObligaciones_Base { get; set; }
        public decimal PagoObligaciones_Moderado { get; set; }
        public decimal PagoObligaciones_Severo { get; set; }

        public decimal EgresosFinAdm_Base { get; set; }
        public decimal EgresosFinAdm_Moderado { get; set; }
        public decimal EgresosFinAdm_Severo { get; set; }

        public decimal DesembolsosCartera_Base { get; set; }
        public decimal DesembolsosCartera_Moderado { get; set; }
        public decimal DesembolsosCartera_Severo { get; set; }

        public decimal CorridaDepositosVista_Base { get; set; }
        public decimal CorridaDepositosVista_Moderado { get; set; }
        public decimal CorridaDepositosVista_Severo { get; set; }

        // Variables Proyectivas / Diferencia de escenarios (imagen 16)
        public decimal CDPDiferencia_Base { get; set; }
        public decimal CDPDiferencia_Moderado { get; set; }
        public decimal CDPDiferencia_Severo { get; set; }

        public decimal SalidaTop20Diferencia_Base { get; set; }
        public decimal SalidaTop20Diferencia_Moderado { get; set; }
        public decimal SalidaTop20Diferencia_Severo { get; set; }

        public decimal CorridaVistaDiferencia_Base { get; set; }
        public decimal CorridaVistaDiferencia_Moderado { get; set; }
        public decimal CorridaVistaDiferencia_Severo { get; set; }

        public decimal DesembolsoCarteraDiferencia_Base { get; set; }
        public decimal DesembolsoCarteraDiferencia_Moderado { get; set; }
        public decimal DesembolsoCarteraDiferencia_Severo { get; set; }

        public decimal RazonCobertura_Base { get; set; }
        public decimal RazonCobertura_Moderado { get; set; }
        public decimal RazonCobertura_Severo { get; set; }
    }

    #endregion

    #region Escenarios de Estrés - ICL Estresado

    public class ICLEstresadoViewModel
    {
        public decimal ICLContractual { get; set; }
        public decimal ICLBase { get; set; }
        public decimal ICLModerado { get; set; }
        public decimal ICLSevero { get; set; }

        public decimal ICL_Base { get { return ICLBase; } set { ICLBase = value; } }
        public decimal ICL_Moderado { get { return ICLModerado; } set { ICLModerado = value; } }
        public decimal ICL_Severo { get { return ICLSevero; } set { ICLSevero = value; } }

        public decimal FALContractual { get; set; }
        public decimal FALBase { get; set; }
        public decimal FALModerado { get; set; }
        public decimal FALSevero { get; set; }

        public decimal SalidasContractual { get; set; }
        public decimal SalidasBase { get; set; }
        public decimal SalidasModerado { get; set; }
        public decimal SalidasSevero { get; set; }
    }

    #endregion

    #region Escenarios de Estrés - Brechas y Supervivencia

    public class BrechasLiquidezViewModel
    {
        public List<string> Bandas { get; set; } = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 360 días", "(+) 360 días" };
        
        // Resumen KPIs imagen 19
        public decimal Gap30d_Base { get; set; }
        public decimal Gap90d_Base { get; set; }
        public decimal FAL_Base { get; set; }

        public decimal Gap30d_Moderado { get; set; }
        public decimal Gap90d_Moderado { get; set; }
        public decimal FAL_Moderado { get; set; }

        public decimal Gap30d_Severo { get; set; }
        public decimal Gap90d_Severo { get; set; }
        public decimal FAL_Severo { get; set; }

        public List<decimal> EntradasBase { get; set; } = new List<decimal>();
        public List<decimal> SalidasBase { get; set; } = new List<decimal>();
        public List<decimal> BrechasBase { get; set; } = new List<decimal>();
        public List<decimal> AcumuladoBase { get; set; } = new List<decimal>();

        public List<decimal> EntradasModerado { get; set; } = new List<decimal>();
        public List<decimal> SalidasModerado { get; set; } = new List<decimal>();
        public List<decimal> BrechasModerado { get; set; } = new List<decimal>();
        public List<decimal> AcumuladoModerado { get; set; } = new List<decimal>();

        public List<decimal> EntradasSevero { get; set; } = new List<decimal>();
        public List<decimal> SalidasSevero { get; set; } = new List<decimal>();
        public List<decimal> BrechasSevero { get; set; } = new List<decimal>();
        public List<decimal> AcumuladoSevero { get; set; } = new List<decimal>();
    }

    public class HorizonteSupervivenciaViewModel
    {
        public string HorizonteBase { get; set; }
        public string HorizonteModerado { get; set; }
        public string HorizonteSevero { get; set; }

        public decimal DiasSupervivenciaBase { get; set; }
        public decimal DiasSupervivenciaModerado { get; set; }
        public decimal DiasSupervivenciaSevero { get; set; }

        public decimal BufferRemanenteBase { get; set; }
        public decimal BufferRemanenteModerado { get; set; }
        public decimal BufferRemanenteSevero { get; set; }

        public decimal SaldoFinalBase { get; set; }
        public decimal SaldoFinalModerado { get; set; }
        public decimal SaldoFinalSevero { get; set; }

        public decimal ReduccionDias { get; set; }

        // Trayectoria multiescenario para Highcharts
        public List<string> TrayectoriaCategorias { get; set; } = new List<string>();
        public List<decimal> TrayectoriaBase { get; set; } = new List<decimal>();
        public List<decimal> TrayectoriaModerado { get; set; } = new List<decimal>();
        public List<decimal> TrayectoriaSevero { get; set; } = new List<decimal>();
    }

    #endregion
}
