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
        public List<string> Bandas { get; set; } = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 365 días", "Mayor a 365 días" };
        public List<decimal> Entradas { get; set; } = new List<decimal>();
        public List<decimal> Salidas { get; set; } = new List<decimal>();
        public List<decimal> BrechasBanda { get; set; } = new List<decimal>();
        public List<decimal> BrechasAcumuladas { get; set; } = new List<decimal>();
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

        public decimal EgresosContractuales_Base { get; set; }
        public decimal EgresosContractuales_Moderado { get; set; }
        public decimal EgresosContractuales_Severo { get; set; }

        public decimal DesembolsosCartera_Base { get; set; }
        public decimal DesembolsosCartera_Moderado { get; set; }
        public decimal DesembolsosCartera_Severo { get; set; }

        public decimal CorridaDepositosVista_Base { get; set; }
        public decimal CorridaDepositosVista_Moderado { get; set; }
        public decimal CorridaDepositosVista_Severo { get; set; }

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
        public List<string> Bandas { get; set; } = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 365 días" };
        public List<decimal> BrechasBase { get; set; } = new List<decimal>();
        public List<decimal> BrechasModerado { get; set; } = new List<decimal>();
        public List<decimal> BrechasSevero { get; set; } = new List<decimal>();

        public List<decimal> AcumuladoBase { get; set; } = new List<decimal>();
        public List<decimal> AcumuladoModerado { get; set; } = new List<decimal>();
        public List<decimal> AcumuladoSevero { get; set; } = new List<decimal>();
    }

    public class HorizonteSupervivenciaViewModel
    {
        public string HorizonteBase { get; set; }
        public string HorizonteModerado { get; set; }
        public string HorizonteSevero { get; set; }

        public decimal SaldoFinalBase { get; set; }
        public decimal SaldoFinalModerado { get; set; }
        public decimal SaldoFinalSevero { get; set; }
    }

    #endregion
}
