using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using FGA.Model;
using FGA.Utility;

namespace FGA.Controllers
{
    public class LiquidezController : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient spClient = new FGA_En_Linea.SPService.SPClient();

        // GET: Liquidez (Landing Hub Oficial con 2 Tarjetas Macro)
        public ActionResult Index(string idEntidad = null, string seccion = null, string periodo1 = null, string periodo2 = null)
        {
            if (string.Equals(seccion, "contractual", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Contractual", new { idEntidad, periodo1, periodo2 });
            if (string.Equals(seccion, "estres", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Estres", new { idEntidad, periodo1, periodo2 });

            var model = ConstruirModelo(idEntidad, "hub", periodo1, periodo2);
            return View("Index", model);
        }

        // GET: Liquidez/Contractual (Pantalla Independiente: Indicadores, ICL, GAP Contractual e IFNE)
        public ActionResult Contractual(string idEntidad = null, string periodo1 = null, string periodo2 = null)
        {
            var model = ConstruirModelo(idEntidad, "contractual", periodo1, periodo2);
            return View("Contractual", model);
        }

        // GET: Liquidez/Estres (Pantalla Independiente: Supuestos, Razón Cobertura, ICL Estresado, Brechas y Horizonte)
        public ActionResult Estres(string idEntidad = null, string periodo1 = null, string periodo2 = null)
        {
            var model = ConstruirModelo(idEntidad, "estres", periodo1, periodo2);
            return View("Estres", model);
        }

        private LiquidezIndexViewModel ConstruirModelo(string idEntidad, string seccion, string periodo1, string periodo2)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(idEntidad))
                {
                    Session["IdEntidad"] = idEntidad;
                }

                Load();

                string currentEntidad = Session["IdEntidad"]?.ToString() ?? "2";

                // Resolver fecha de cierre
                DateTime fechaReferencia;
                try
                {
                    fechaReferencia = spClient.FGA_Consultar_FechaCierre(currentEntidad);
                }
                catch
                {
                    fechaReferencia = new DateTime(2026, 8, 1);
                }

                DateTime p2Date = Utilitarios.ConvertirAFechaSegura(periodo2, fechaReferencia.AddMonths(-1));
                DateTime p1Date = Utilitarios.ConvertirAFechaSegura(periodo1, p2Date.AddYears(-1));

                string strP1 = p1Date.ToString("dd/MM/yyyy");
                string strP2 = p2Date.ToString("dd/MM/yyyy");

                string nomEntidad = Session["NomEntidad"]?.ToString();
                if (string.IsNullOrWhiteSpace(nomEntidad))
                {
                    try
                    {
                        var e = entClient.Get(currentEntidad);
                        nomEntidad = e?.Nombre ?? "Coopecar R.L.";
                    }
                    catch
                    {
                        nomEntidad = "Coopecar R.L.";
                    }
                }

                string macroSec = string.IsNullOrWhiteSpace(seccion) ? "hub" : seccion.ToLowerInvariant();

                return new LiquidezIndexViewModel
                {
                    IdEntidad = currentEntidad,
                    NombreEntidad = nomEntidad,
                    Periodo1 = strP1,
                    Periodo2 = strP2,
                    Periodo1Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p1Date.ToString("MMMM yyyy", new CultureInfo("es-ES"))),
                    Periodo2Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p2Date.ToString("MMMM yyyy", new CultureInfo("es-ES"))),
                    PeriodoCorteGap = strP2,
                    MacroSeccion = macroSec,
                    Indicadores = CalcularIndicadores(currentEntidad, p1Date, p2Date),
                    ICLDatos = ObtenerDatosICL(currentEntidad, p2Date),
                    GapDatos = ObtenerDatosGap(currentEntidad, p2Date),
                    Supuestos = ObtenerSupuestosPorDefecto(currentEntidad, p2Date),
                    RazonCobertura = CalcularRazonCoberturaDefecto(),
                    ICLEstresado = CalcularICLEstresadoDefecto(),
                    BrechasEstres = ObtenerBrechasEstresDefecto(),
                    Horizonte = ObtenerHorizonteDefecto()
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return new LiquidezIndexViewModel();
            }
        }

        #region Métodos de Negocio y Cálculo Normativo

        private List<IndicadorLiquidezItem> CalcularIndicadores(string entidad, DateTime p1, DateTime p2)
        {
            try
            {
                var spRes = spClient.FGA_Consultar_Indicadores_Liquidez(entidad, p1, p2);
                if (spRes != null && spRes.Length > 0)
                {
                    var list = new List<IndicadorLiquidezItem>();
                    foreach (var item in spRes)
                    {
                        list.Add(new IndicadorLiquidezItem
                        {
                            Orden = item.Orden ?? 0,
                            Nombre = item.Indicador,
                            Formula = item.Formula,
                            QueMide = item.QueMide,
                            ValorP1 = item.ValorP1 ?? 0m,
                            ValorP2 = item.ValorP2 ?? 0m,
                            EsPorcentaje = item.EsPorcentaje ?? true,
                            TipoGraficoAsociado = 1
                        });
                    }
                    return list;
                }
            }
            catch { }

            var result = new List<IndicadorLiquidezItem>();

            // Cargar saldos de balance
            Dictionary<string, decimal> saldosP1 = ObtenerSaldosContables(entidad, p1);
            Dictionary<string, decimal> saldosP2 = ObtenerSaldosContables(entidad, p2);

            // Valores de balance P1
            decimal disp_p1 = ObtenerSaldoCta(saldosP1, "11000000");
            decimal capVista_p1 = ObtenerSaldoCta(saldosP1, "21100000");
            decimal capPlazo_p1 = ObtenerSaldoCta(saldosP1, "21300000");
            decimal capTotal_p1 = ObtenerSaldoCta(saldosP1, "21000000");
            decimal cartNeta_p1 = ObtenerSaldoCta(saldosP1, "13000000");
            decimal actTotal_p1 = ObtenerSaldoCta(saldosP1, "10000000");
            decimal deuLP_p1 = ObtenerSaldoCta(saldosP1, "23200000");
            decimal deuTotal_p1 = ObtenerSaldoCta(saldosP1, "230000000");
            decimal pat_p1 = ObtenerSaldoCta(saldosP1, "31000000");

            // Valores de balance P2
            decimal disp_p2 = ObtenerSaldoCta(saldosP2, "11000000");
            decimal capVista_p2 = ObtenerSaldoCta(saldosP2, "21100000");
            decimal capPlazo_p2 = ObtenerSaldoCta(saldosP2, "21300000");
            decimal capTotal_p2 = ObtenerSaldoCta(saldosP2, "21000000");
            decimal cartNeta_p2 = ObtenerSaldoCta(saldosP2, "13000000");
            decimal actTotal_p2 = ObtenerSaldoCta(saldosP2, "10000000");
            decimal deuLP_p2 = ObtenerSaldoCta(saldosP2, "23200000");
            decimal deuTotal_p2 = ObtenerSaldoCta(saldosP2, "230000000");
            decimal pat_p2 = ObtenerSaldoCta(saldosP2, "31000000");

            // FAL ICL por defecto de Coopecar si no hay XML cargado
            decimal fal_p1 = 1203529297.63m;
            decimal fal_p2 = 893297120.50m;
            decimal pasCP_p1 = 2697887850.12m;
            decimal pasCP_p2 = 2697887850.12m;
            decimal top20_p1 = 2904322867.90m;
            decimal top20_p2 = 3240645735.80m;

            // 1. Liquidez inmediata: 11000000 / 21100000
            decimal v1_p1 = capVista_p1 > 0 ? (disp_p1 / capVista_p1) : 0.6973m;
            decimal v1_p2 = capVista_p2 > 0 ? (disp_p2 / capVista_p2) : 0.6845m;
            result.Add(new IndicadorLiquidezItem { Orden = 1, Nombre = "Liquidez inmediata", Formula = "Disponibilidades (11000000) / Captaciones a la vista (21100000)", QueMide = "Capacidad de atender retiros inmediatos", ValorP1 = v1_p1, ValorP2 = v1_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 2. Activos líquidos / Captaciones: FAL / 21000000
            decimal v2_p1 = capTotal_p1 > 0 ? (fal_p1 / capTotal_p1) : 0.1384m;
            decimal v2_p2 = capTotal_p2 > 0 ? (fal_p2 / capTotal_p2) : 0.0992m;
            result.Add(new IndicadorLiquidezItem { Orden = 2, Nombre = "Activos líquidos / Captaciones", Formula = "FAL ICL / Saldo captaciones totales (21000000)", QueMide = "Cobertura general de depósitos con activos líquidos", ValorP1 = v2_p1, ValorP2 = v2_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 3. Activos líquidos / Pasivos exigibles CP
            decimal v3_p1 = pasCP_p1 > 0 ? (fal_p1 / pasCP_p1) : 0.4461m;
            decimal v3_p2 = pasCP_p2 > 0 ? (fal_p2 / pasCP_p2) : 0.3311m;
            result.Add(new IndicadorLiquidezItem { Orden = 3, Nombre = "Activos líquidos / Pasivos exigibles CP", Formula = "Activos líquidos (FAL ICL) / Pasivos exigibles ≤ 30 días", QueMide = "Cobertura de obligaciones de corto plazo", ValorP1 = v3_p1, ValorP2 = v3_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 4. Cartera / Captaciones: 13000000 / (21300000 + 21100000)
            decimal den4_p1 = capPlazo_p1 + capVista_p1;
            decimal den4_p2 = capPlazo_p2 + capVista_p2;
            decimal v4_p1 = den4_p1 > 0 ? (cartNeta_p1 / den4_p1) : 0.9851m;
            decimal v4_p2 = den4_p2 > 0 ? (cartNeta_p2 / den4_p2) : 0.9776m;
            result.Add(new IndicadorLiquidezItem { Orden = 4, Nombre = "Cartera / Captaciones", Formula = "Cartera neta (13000000) / (21300000 + 21100000)", QueMide = "Qué proporción de los depósitos está colocada en cartera", ValorP1 = v4_p1, ValorP2 = v4_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 5. Cartera / Fondeo estable: 13000000 / (21300000 + 23200000 + 31000000)
            decimal den5_p1 = capPlazo_p1 + deuLP_p1 + pat_p1;
            decimal den5_p2 = capPlazo_p2 + deuLP_p2 + pat_p2;
            decimal v5_p1 = den5_p1 > 0 ? (cartNeta_p1 / den5_p1) : 0.9620m;
            decimal v5_p2 = den5_p2 > 0 ? (cartNeta_p2 / den5_p2) : 0.9176m;
            result.Add(new IndicadorLiquidezItem { Orden = 5, Nombre = "Cartera / Fondeo estable", Formula = "Cartera neta / (Captaciones LP + deuda LP + patrimonio)", QueMide = "Dependencia de fondeo estable para financiar cartera", ValorP1 = v5_p1, ValorP2 = v5_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 6. Activos líquidos / Activos: FAL / Activo Total
            decimal v6_p1 = actTotal_p1 > 0 ? (fal_p1 / actTotal_p1) : 0.1039m;
            decimal v6_p2 = actTotal_p2 > 0 ? (fal_p2 / actTotal_p2) : 0.0733m;
            result.Add(new IndicadorLiquidezItem { Orden = 6, Nombre = "Activos líquidos / Activos", Formula = "FAL ICL / Activo total", QueMide = "Buffer de liquidez respecto al tamaño de la entidad", ValorP1 = v6_p1, ValorP2 = v6_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 7. Activos líquidos / 20 mayores depositantes
            decimal v7_p1 = top20_p1 > 0 ? (fal_p1 / top20_p1) : 0.4144m;
            decimal v7_p2 = top20_p2 > 0 ? (fal_p2 / top20_p2) : 0.2757m;
            result.Add(new IndicadorLiquidezItem { Orden = 7, Nombre = "Activos líquidos / 20 mayores depositantes", Formula = "Activos líquidos (FAL ICL) / Saldo Top 20 depositantes", QueMide = "Capacidad de absorber concentración de fondeo", ValorP1 = v7_p1, ValorP2 = v7_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 8. Fondeo mayorista / Captaciones totales: 230000000 / 210000000
            decimal v8_p1 = capTotal_p1 > 0 ? (deuTotal_p1 / capTotal_p1) : 0.0106m;
            decimal v8_p2 = capTotal_p2 > 0 ? (deuTotal_p2 / capTotal_p2) : 0.0309m;
            result.Add(new IndicadorLiquidezItem { Orden = 8, Nombre = "Fondeo mayorista / Captaciones totales", Formula = "Deuda institucional (230000000) / Fondeo total (210000000)", QueMide = "Dependencia de fuentes institucionales", ValorP1 = v8_p1, ValorP2 = v8_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            return result;
        }

        private ICLViewModel ObtenerDatosICL(string entidad, DateTime fecha)
        {
            var icl = new ICLViewModel
            {
                Periodo = fecha.ToString("MM/yyyy"),
                MonedaTipo = 1,
                TieneDolares = false, // Las cooperativas ejemplo no reportan ICL en USD
                ICLRatio = 148.52m,
                FondoActivosLiquidos = 1203529297.63m,
                SalidasNetasEfectivo = 810345620.10m
            };

            icl.Items = new List<ICLDetalleItem>
            {
                new ICLDetalleItem { Codigo = "100000", Nivel = 0, Concepto = "ÍNDICE DE COBERTURA DE LIQUIDEZ (ICL)", SaldoBruto = 148.52m, Ponderador = 1m, SaldoPonderado = 148.52m, EsTotalizador = true },
                new ICLDetalleItem { Codigo = "200000", Nivel = 1, Concepto = "Fondo de Activos Líquidos (FAL)", SaldoBruto = 1203529297.63m, Ponderador = 1m, SaldoPonderado = 1203529297.63m, EsTotalizador = true },
                new ICLDetalleItem { Codigo = "210000", Nivel = 2, Concepto = "Activos Nivel 1", SaldoBruto = 1150000000.00m, Ponderador = 1m, SaldoPonderado = 1150000000.00m, EsTotalizador = false },
                new ICLDetalleItem { Codigo = "210110", Nivel = 3, Concepto = "Efectivo y disponibilidades", SaldoBruto = 550000000.00m, Ponderador = 1m, SaldoPonderado = 550000000.00m, EsTotalizador = false },
                new ICLDetalleItem { Codigo = "210120", Nivel = 3, Concepto = "Valores de deuda del BCCR o Gobierno", SaldoBruto = 600000000.00m, Ponderador = 1m, SaldoPonderado = 600000000.00m, EsTotalizador = false },
                new ICLDetalleItem { Codigo = "220000", Nivel = 2, Concepto = "Activos Nivel 2A y 2B", SaldoBruto = 62975644.27m, Ponderador = 0.85m, SaldoPonderado = 53529297.63m, EsTotalizador = false },
                new ICLDetalleItem { Codigo = "300000", Nivel = 1, Concepto = "Salidas Netas de Efectivo (30 días)", SaldoBruto = 953347788.35m, Ponderador = 1m, SaldoPonderado = 810345620.10m, EsTotalizador = true },
                new ICLDetalleItem { Codigo = "310000", Nivel = 2, Concepto = "Salidas de depósitos minoristas", SaldoBruto = 450000000.00m, Ponderador = 0.10m, SaldoPonderado = 45000000.00m, EsTotalizador = false },
                new ICLDetalleItem { Codigo = "320000", Nivel = 2, Concepto = "Salidas de depósitos no garantizados (Top 20)", SaldoBruto = 324064573.58m, Ponderador = 0.50m, SaldoPonderado = 162032286.79m, EsTotalizador = false }
            };

            return icl;
        }

        private GapContractualViewModel ObtenerDatosGap(string entidad, DateTime fecha)
        {
            try
            {
                var spRes = spClient.FGA_Consultar_GAP_Contractual(entidad, fecha);
                if (spRes != null && spRes.Length > 0)
                {
                    return new GapContractualViewModel
                    {
                        PeriodoCorte = fecha.ToString("dd/MM/yyyy"),
                        Bandas = spRes.Select(x => x.Banda).ToList(),
                        Entradas = spRes.Select(x => x.Entradas ?? 0m).ToList(),
                        Salidas = spRes.Select(x => x.Salidas ?? 0m).ToList(),
                        BrechasBanda = spRes.Select(x => x.BrechaBanda ?? 0m).ToList(),
                        BrechasAcumuladas = spRes.Select(x => x.BrechaAcumulada ?? 0m).ToList()
                    };
                }
            }
            catch { }

            return new GapContractualViewModel
            {
                PeriodoCorte = fecha.ToString("dd/MM/yyyy"),
                Bandas = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 365 días", "> 365 días" },
                Entradas = new List<decimal> { 693.81m, 120.50m, 95.30m, 210.00m, 180.20m, 115.00m, 310.50m },
                Salidas = new List<decimal> { 739.91m, 215.00m, 180.40m, 305.20m, 220.10m, 140.00m, 250.00m },
                BrechasBanda = new List<decimal> { -46.10m, -94.50m, -85.10m, -95.20m, -39.90m, -25.00m, 60.50m },
                BrechasAcumuladas = new List<decimal> { 1157.43m, 1062.93m, 977.83m, 882.63m, 842.73m, 817.73m, 878.23m }
            };
        }

        private SupuestosSensibilizacionViewModel ObtenerSupuestosPorDefecto(string entidad, DateTime fecha)
        {
            return new SupuestosSensibilizacionViewModel
            {
                Entidad = entidad,
                PeriodoCorte = fecha.ToString("MM/yyyy"),
                RenovacionCDP_Base = 0.85m,
                RenovacionCDP_Moderado = 0.68m,
                RenovacionCDP_Severo = 0.425m,
                SalidaVista_Base = 0.1009m,
                SalidaVista_Moderado = 0.1211m,
                SalidaVista_Severo = 0.1816m,
                CrecimientoCartera_Base = 0.0688m,
                CrecimientoCartera_Moderado = 0.0826m,
                CrecimientoCartera_Severo = 0.1032m,
                DisminucionRecaudo_Base = 0.035m,
                DisminucionRecaudo_Moderado = 0.05m,
                DisminucionRecaudo_Severo = 0.10m,
                Haircut_Base = 1203529297.63m,
                Haircut_Moderado = 1103529297.63m,
                Haircut_Severo = 933529297.63m,
                SalidaTop20_Base = 0.05m,
                SalidaTop20_Moderado = 0.15m,
                SalidaTop20_Severo = 0.35m,
                DisponibilidadLineas_Base = 75000000m,
                DisponibilidadLineas_Moderado = 50000000m,
                DisponibilidadLineas_Severo = 30000000m,
                PagoObligaciones_Base = 10000000m,
                PagoObligaciones_Moderado = 10000000m,
                PagoObligaciones_Severo = 10000000m,
                Cupones_Base = 2500000m,
                Cupones_Moderado = 1500000m,
                Cupones_Severo = 1000000m,
                CapitalSocial_Base = 0.035m,
                CapitalSocial_Moderado = 0.028m,
                CapitalSocial_Severo = 0.0175m
            };
        }

        private RazonCoberturaViewModel CalcularRazonCoberturaDefecto()
        {
            return new RazonCoberturaViewModel
            {
                ActivosLiquidos_Base = 1203.53m,
                ActivosLiquidos_Moderado = 1103.53m,
                ActivosLiquidos_Severo = 933.53m,

                IngresosContractuales_Base = 693.81m,
                IngresosContractuales_Moderado = 650.20m,
                IngresosContractuales_Severo = 580.40m,

                EgresosContractuales_Base = 739.91m,
                EgresosContractuales_Moderado = 790.10m,
                EgresosContractuales_Severo = 850.30m,

                DesembolsosCartera_Base = 124.96m,
                DesembolsosCartera_Moderado = 150.00m,
                DesembolsosCartera_Severo = 187.40m,

                CorridaDepositosVista_Base = 238.30m,
                CorridaDepositosVista_Moderado = 285.90m,
                CorridaDepositosVista_Severo = 428.90m,

                RazonCobertura_Base = 26.11m,
                RazonCobertura_Moderado = 18.45m,
                RazonCobertura_Severo = 9.82m
            };
        }

        private ICLEstresadoViewModel CalcularICLEstresadoDefecto()
        {
            return new ICLEstresadoViewModel
            {
                ICLContractual = 148.52m,
                ICLBase = 135.20m,
                ICLModerado = 108.40m,
                ICLSevero = 78.60m,

                FALContractual = 1203.53m,
                FALBase = 1203.53m,
                FALModerado = 962.82m,
                FALSevero = 601.76m,

                SalidasContractual = 810.35m,
                SalidasBase = 890.15m,
                SalidasModerado = 888.20m,
                SalidasSevero = 765.60m
            };
        }

        private BrechasLiquidezViewModel ObtenerBrechasEstresDefecto()
        {
            return new BrechasLiquidezViewModel
            {
                Bandas = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 365 días" },
                BrechasBase = new List<decimal> { -22.72m, -461.02m, -120.30m, -180.50m, -45.20m, 80.10m },
                BrechasModerado = new List<decimal> { -85.40m, -581.94m, -210.20m, -250.00m, -90.50m, 15.00m },
                BrechasSevero = new List<decimal> { -380.08m, -750.30m, -390.10m, -410.50m, -180.20m, -60.00m },

                AcumuladoBase = new List<decimal> { 1180.81m, 901.12m, 780.82m, -410.37m, 325.25m, 405.35m },
                AcumuladoModerado = new List<decimal> { 1018.13m, 347.87m, -581.94m, -831.94m, -922.44m, -907.44m },
                AcumuladoSevero = new List<decimal> { 1203.53m, -380.08m, -770.18m, -1180.68m, -1360.88m, -1420.88m }
            };
        }

        private HorizonteSupervivenciaViewModel ObtenerHorizonteDefecto()
        {
            return new HorizonteSupervivenciaViewModel
            {
                HorizonteBase = "91 a 180 días",
                HorizonteModerado = "61 a 90 días",
                HorizonteSevero = "1 a 30 días",
                SaldoFinalBase = -410.37m,
                SaldoFinalModerado = -581.94m,
                SaldoFinalSevero = -380.08m
            };
        }

        #endregion

        #region Helpers de Base de Datos y Saldos

        private Dictionary<string, decimal> ObtenerSaldosContables(string entidad, DateTime fecha)
        {
            var dict = new Dictionary<string, decimal>();
            try
            {
                var tak = spClient.FGA_Consultar_Balance_Comprobacion_Rango(entidad, fecha, fecha, false, false);
                if (tak != null)
                {
                    foreach (var item in tak)
                    {
                        if (string.IsNullOrWhiteSpace(item.VALORES)) continue;
                        var parts = item.VALORES.Split(';');
                        if (parts.Length >= 3)
                        {
                            string cta = parts[1].Trim();
                            decimal val;
                            if (decimal.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val) ||
                                decimal.TryParse(parts[2].Trim(), NumberStyles.Any, new CultureInfo("es-CR"), out val))
                            {
                                dict[cta] = val;
                            }
                        }
                    }
                }
            }
            catch { }
            return dict;
        }

        private decimal ObtenerSaldoCta(Dictionary<string, decimal> dict, string ctaPrefix)
        {
            if (dict == null || dict.Count == 0) return 0m;
            decimal exact;
            if (dict.TryGetValue(ctaPrefix, out exact)) return Math.Abs(exact);

            return dict.Where(k => k.Key.StartsWith(ctaPrefix)).Sum(k => Math.Abs(k.Value));
        }

        #endregion
    }
}
