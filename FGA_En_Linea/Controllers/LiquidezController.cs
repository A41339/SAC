using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FGA.Model;
using FGA.Utility;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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

        [HttpGet]
        public JsonResult GetHistoricoIndicador(int orden, string entidad = null, string periodo1 = null, string periodo2 = null)
        {
            try
            {
                string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
                DateTime p2 = Utilitarios.ConvertirAFechaSegura(periodo2, DateTime.Now);
                DateTime p1 = Utilitarios.ConvertirAFechaSegura(periodo1, p2.AddYears(-1));

                // Construir serie mensual dinámica de los últimos 7 periodos
                var categorias = new List<string>();
                var valores = new List<decimal>();
                string nombreIndicador = "Indicador de Liquidez";
                bool esPorc = true;

                for (int m = 6; m >= 0; m--)
                {
                    DateTime fechaMes = p2.AddMonths(-m);
                    categorias.Add(fechaMes.ToString("MMM yyyy", new CultureInfo("es-CR")));

                    try
                    {
                        var saldosMes = ObtenerSaldosContables(idEnt, fechaMes);
                        decimal val = 0m;
                        switch (orden)
                        {
                            case 1: // Liquidez inmediata
                                nombreIndicador = "Liquidez Inmediata";
                                decimal disp = ObtenerSaldoCta(saldosMes, "11000000");
                                decimal vista = ObtenerSaldoCta(saldosMes, "21100000");
                                val = vista > 0 ? (disp / vista) * 100m : 0m;
                                break;
                            case 2: // Activos líquidos / Captaciones
                                nombreIndicador = "Activos Líquidos / Captaciones";
                                decimal capt = ObtenerSaldoCta(saldosMes, "21000000");
                                decimal fal = (ObtenerSaldoCta(saldosMes, "11000000") + ObtenerSaldoCta(saldosMes, "12000000"));
                                val = capt > 0 ? (fal / capt) * 100m : 0m;
                                break;
                            case 3: // Activos líquidos / Pasivos exigibles CP
                                nombreIndicador = "Activos Líquidos / Pasivos Exigibles CP";
                                decimal pasCP = ObtenerSaldoCta(saldosMes, "21100000") + (ObtenerSaldoCta(saldosMes, "21300000") * 0.35m);
                                decimal fal3 = (ObtenerSaldoCta(saldosMes, "11000000") + ObtenerSaldoCta(saldosMes, "12000000"));
                                val = pasCP > 0 ? (fal3 / pasCP) * 100m : 0m;
                                break;
                            case 4: // Cartera / Captaciones
                                nombreIndicador = "Cartera / Captaciones";
                                decimal cta130 = ObtenerSaldoCta(saldosMes, "13000000");
                                decimal cta210 = ObtenerSaldoCta(saldosMes, "21000000");
                                val = cta210 > 0 ? (cta130 / cta210) * 100m : 0m;
                                break;
                            case 5: // Cartera / Fondeo estable
                                nombreIndicador = "Cartera / Fondeo Estable";
                                decimal cta130_5 = ObtenerSaldoCta(saldosMes, "13000000");
                                decimal fondeoEst = ObtenerSaldoCta(saldosMes, "21300000") + ObtenerSaldoCta(saldosMes, "23200000") + ObtenerSaldoCta(saldosMes, "31000000");
                                val = fondeoEst > 0 ? (cta130_5 / fondeoEst) * 100m : 0m;
                                break;
                            case 6: // Activos líquidos / Activos
                                nombreIndicador = "Activos Líquidos / Activo Total";
                                decimal actTot = ObtenerSaldoCta(saldosMes, "10000000");
                                decimal fal6 = (ObtenerSaldoCta(saldosMes, "11000000") + ObtenerSaldoCta(saldosMes, "12000000"));
                                val = actTot > 0 ? (fal6 / actTot) * 100m : 0m;
                                break;
                            case 7: // Activos líquidos / Top 20
                                nombreIndicador = "Activos Líquidos / 20 Mayores Depositantes";
                                decimal top20 = ObtenerSaldoCta(saldosMes, "21000000") * 0.25m;
                                decimal fal7 = (ObtenerSaldoCta(saldosMes, "11000000") + ObtenerSaldoCta(saldosMes, "12000000"));
                                val = top20 > 0 ? (fal7 / top20) * 100m : 0m;
                                break;
                            case 8: // Fondeo mayorista / Captaciones
                                nombreIndicador = "Fondeo Mayorista / Captaciones Totales";
                                decimal deuTot = ObtenerSaldoCta(saldosMes, "23000000");
                                decimal captTot = ObtenerSaldoCta(saldosMes, "21000000");
                                val = captTot > 0 ? (deuTot / captTot) * 100m : 0m;
                                break;
                            default:
                                val = 0m;
                                break;
                        }
                        valores.Add(Math.Round(val, 2));
                    }
                    catch
                    {
                        valores.Add(0m);
                    }
                }

                return Json(new
                {
                    success = true,
                    nombre = nombreIndicador,
                    esPorcentaje = esPorc,
                    categorias = categorias,
                    valores = valores
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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

                var gapDatos = ObtenerDatosGap(currentEntidad, p2Date);
                var saldos = ObtenerSaldosContables(currentEntidad, p2Date);
                var supuestos = ObtenerSupuestosPorDefecto(currentEntidad, p2Date);
                var icl = ObtenerDatosICL(currentEntidad, p2Date);

                var razonCobertura = CalcularRazonCobertura(saldos, gapDatos, supuestos);
                var iclEstresado = CalcularICLEstresado(icl, saldos, supuestos, razonCobertura);
                var brechasEstres = CalcularBrechasEstres(gapDatos, saldos, supuestos, razonCobertura);
                var horizonte = CalcularHorizonteSupervivencia(brechasEstres, razonCobertura.ActivosLiquidos_Base);

                // Calcular horizonte del mes anterior para comparativa y gobernanza de tendencia
                try
                {
                    DateTime pAntDate = p2Date.AddMonths(-1);
                    var saldosAnt = ObtenerSaldosContables(currentEntidad, pAntDate);
                    var gapAnt = ObtenerDatosGap(currentEntidad, pAntDate);
                    var supAnt = ObtenerSupuestosPorDefecto(currentEntidad, pAntDate);
                    var rcAnt = CalcularRazonCobertura(saldosAnt, gapAnt, supAnt);
                    var beAnt = CalcularBrechasEstres(gapAnt, saldosAnt, supAnt, rcAnt);
                    var hAnt = CalcularHorizonteSupervivencia(beAnt, rcAnt.ActivosLiquidos_Base);

                    horizonte.DiasSupervivenciaMesAnteriorBase = hAnt.DiasSupervivenciaBase;
                    horizonte.DiasSupervivenciaMesAnteriorModerado = hAnt.DiasSupervivenciaModerado;
                    horizonte.DiasSupervivenciaMesAnteriorSevero = hAnt.DiasSupervivenciaSevero;
                }
                catch { }

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
                    ICLDatos = icl,
                    GapDatos = gapDatos,
                    Supuestos = supuestos,
                    RazonCobertura = razonCobertura,
                    ICLEstresado = iclEstresado,
                    BrechasEstres = brechasEstres,
                    Horizonte = horizonte,
                    FuenteDatos = "Balanza de Comprobación SUGEF y Calce de Plazos XML",
                    FechaCalculo = DateTime.Now,
                    UsuarioActual = User?.Identity?.Name ?? Session["Usuario"]?.ToString() ?? "Comité de Riesgos"
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return new LiquidezIndexViewModel();
            }
        }

        #region Interactividad, Simulación What-If y Gobernanza

        [HttpPost]
        public JsonResult SimularEstres(string idEntidad, string periodo2, SupuestosSensibilizacionViewModel supuestos)
        {
            try
            {
                string currentEntidad = string.IsNullOrWhiteSpace(idEntidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : idEntidad;
                DateTime fechaReferencia = spClient.FGA_Consultar_FechaCierre(currentEntidad);
                DateTime fechaCorte = Utilitarios.ConvertirAFechaSegura(periodo2, fechaReferencia.AddMonths(-1));

                var saldos = ObtenerSaldosContables(currentEntidad, fechaCorte);
                var gapDatos = ObtenerDatosGap(currentEntidad, fechaCorte);
                var icl = ObtenerDatosICL(currentEntidad, fechaCorte);

                if (supuestos == null)
                    supuestos = ObtenerSupuestosPorDefecto(currentEntidad, fechaCorte);

                var razonCobertura = CalcularRazonCobertura(saldos, gapDatos, supuestos);
                var iclEstresado = CalcularICLEstresado(icl, saldos, supuestos, razonCobertura);
                var brechasEstres = CalcularBrechasEstres(gapDatos, saldos, supuestos, razonCobertura);
                var horizonte = CalcularHorizonteSupervivencia(brechasEstres, razonCobertura.ActivosLiquidos_Base);

                return Json(new
                {
                    success = true,
                    razonCobertura = razonCobertura,
                    iclEstresado = iclEstresado,
                    brechasEstres = brechasEstres,
                    horizonte = horizonte
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GuardarParametrosLiquidez(string idEntidad, SupuestosSensibilizacionViewModel supuestos)
        {
            try
            {
                string currentEntidad = string.IsNullOrWhiteSpace(idEntidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : idEntidad;
                string usuario = User?.Identity?.Name ?? Session["Usuario"]?.ToString() ?? "Comité de Riesgos";

                int res = spClient.FGA_Guardar_Parametros_Liquidez(
                    currentEntidad,
                    supuestos.RenovacionCDP_Base,
                    supuestos.DisminucionRecaudo_Base,
                    supuestos.DisponibilidadLineas_Base,
                    supuestos.PagoObligaciones_Base,
                    supuestos.Cupones_Base,
                    usuario
                );

                return Json(new { success = true, message = "Supuestos institucionales guardados correctamente en la entidad." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar parámetros: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ExportarExcelContractual(string idEntidad = null, string periodo1 = null, string periodo2 = null)
        {
            try
            {
                var model = ConstruirModelo(idEntidad, "contractual", periodo1, periodo2);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // 1. Hoja Indicadores
                    var wsInd = package.Workbook.Worksheets.Add("Indicadores de Liquidez");
                    wsInd.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsInd, "INDICADORES NORMATIVOS Y DE GESTIÓN DE LIQUIDEZ", model.NombreEntidad, model.Periodo2);

                    wsInd.Cells[5, 1].Value = "#";
                    wsInd.Cells[5, 2].Value = "Indicador";
                    wsInd.Cells[5, 3].Value = "Fórmula / Cuentas";
                    wsInd.Cells[5, 4].Value = "Objetivo / Qué Mide";
                    wsInd.Cells[5, 5].Value = model.Periodo1Header;
                    wsInd.Cells[5, 6].Value = model.Periodo2Header;
                    wsInd.Cells[5, 7].Value = "Variación";
                    FormatearRangoEncabezado(wsInd.Cells[5, 1, 5, 7]);

                    int r = 6;
                    foreach (var item in model.Indicadores)
                    {
                        wsInd.Cells[r, 1].Value = item.Orden;
                        wsInd.Cells[r, 2].Value = item.Nombre;
                        wsInd.Cells[r, 3].Value = item.Formula;
                        wsInd.Cells[r, 4].Value = item.QueMide;
                        wsInd.Cells[r, 5].Value = item.ValorP1;
                        wsInd.Cells[r, 6].Value = item.ValorP2;
                        wsInd.Cells[r, 7].Value = item.Variacion;

                        if (item.EsPorcentaje)
                        {
                            wsInd.Cells[r, 5].Style.Numberformat.Format = "0.00%";
                            wsInd.Cells[r, 6].Style.Numberformat.Format = "0.00%";
                            wsInd.Cells[r, 7].Style.Numberformat.Format = "+0.00%;-0.00%;0.00%";
                        }
                        else
                        {
                            wsInd.Cells[r, 5].Style.Numberformat.Format = "#,##0.00";
                            wsInd.Cells[r, 6].Style.Numberformat.Format = "#,##0.00";
                            wsInd.Cells[r, 7].Style.Numberformat.Format = "+#,##0.00;-#,##0.00;0.00";
                        }
                        r++;
                    }
                    wsInd.Cells.AutoFitColumns();

                    // 2. Hoja GAP Contractual
                    var wsGap = package.Workbook.Worksheets.Add("GAP Contractual");
                    wsGap.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsGap, "CALCE DE PLAZOS Y GAP CONTRACTUAL", model.NombreEntidad, model.Periodo2);

                    wsGap.Cells[5, 1].Value = "Banda Temporal";
                    wsGap.Cells[5, 2].Value = "Entradas (₡ MM)";
                    wsGap.Cells[5, 3].Value = "Salidas (₡ MM)";
                    wsGap.Cells[5, 4].Value = "Brecha por Banda (₡ MM)";
                    wsGap.Cells[5, 5].Value = "Brecha Acumulada (₡ MM)";
                    FormatearRangoEncabezado(wsGap.Cells[5, 1, 5, 5]);

                    r = 6;
                    for (int i = 0; i < model.GapDatos.Bandas.Count; i++)
                    {
                        wsGap.Cells[r, 1].Value = model.GapDatos.Bandas[i];
                        wsGap.Cells[r, 2].Value = model.GapDatos.Entradas.Count > i ? model.GapDatos.Entradas[i] : 0m;
                        wsGap.Cells[r, 3].Value = model.GapDatos.Salidas.Count > i ? model.GapDatos.Salidas[i] : 0m;
                        wsGap.Cells[r, 4].Value = model.GapDatos.BrechasBanda.Count > i ? model.GapDatos.BrechasBanda[i] : 0m;
                        wsGap.Cells[r, 5].Value = model.GapDatos.BrechasAcumuladas.Count > i ? model.GapDatos.BrechasAcumuladas[i] : 0m;

                        wsGap.Cells[r, 2, r, 5].Style.Numberformat.Format = "#,##0.00";
                        r++;
                    }
                    wsGap.Cells.AutoFitColumns();

                    // 3. Hoja ICL Normativo
                    var wsIcl = package.Workbook.Worksheets.Add("ICL Normativo");
                    wsIcl.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsIcl, "ÍNDICE DE COBERTURA DE LIQUIDEZ (ICL)", model.NombreEntidad, model.Periodo2);

                    wsIcl.Cells[5, 1].Value = "Código";
                    wsIcl.Cells[5, 2].Value = "Concepto Normativo";
                    wsIcl.Cells[5, 3].Value = "Saldo Bruto (₡)";
                    wsIcl.Cells[5, 4].Value = "Ponderador";
                    wsIcl.Cells[5, 5].Value = "Saldo Ponderado (₡)";
                    FormatearRangoEncabezado(wsIcl.Cells[5, 1, 5, 5]);

                    r = 6;
                    foreach (var item in model.ICLDatos.Items)
                    {
                        wsIcl.Cells[r, 1].Value = item.Codigo;
                        wsIcl.Cells[r, 2].Value = item.Concepto;
                        wsIcl.Cells[r, 3].Value = item.SaldoBruto;
                        wsIcl.Cells[r, 4].Value = item.Ponderador;
                        wsIcl.Cells[r, 5].Value = item.SaldoPonderado;

                        wsIcl.Cells[r, 3].Style.Numberformat.Format = "#,##0.00";
                        wsIcl.Cells[r, 4].Style.Numberformat.Format = "0.00%";
                        wsIcl.Cells[r, 5].Style.Numberformat.Format = "#,##0.00";

                        if (item.EsTotalizador)
                        {
                            wsIcl.Cells[r, 1, r, 5].Style.Font.Bold = true;
                            wsIcl.Cells[r, 1, r, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            wsIcl.Cells[r, 1, r, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 249, 255));
                        }
                        r++;
                    }
                    wsIcl.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    string fileName = $"Liquidez_Contractual_{model.IdEntidad}_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Content("Error al exportar a Excel: " + ex.Message);
            }
        }

        [HttpGet]
        public ActionResult ExportarExcelEstres(string idEntidad = null, string periodo1 = null, string periodo2 = null)
        {
            try
            {
                var model = ConstruirModelo(idEntidad, "estres", periodo1, periodo2);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // 1. Hoja Horizonte de Supervivencia
                    var wsH = package.Workbook.Worksheets.Add("Horizonte Supervivencia");
                    wsH.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsH, "HORIZONTE DE SUPERVIVENCIA MULTIESCENARIO", model.NombreEntidad, model.Periodo2);

                    wsH.Cells[5, 1].Value = "Escenario";
                    wsH.Cells[5, 2].Value = "Días de Supervivencia";
                    wsH.Cells[5, 3].Value = "Buffer Remanente (₡ MM)";
                    wsH.Cells[5, 4].Value = "Banda de Inflexión";
                    wsH.Cells[5, 5].Value = "Saldo Final Inflexión (₡ MM)";
                    wsH.Cells[5, 6].Value = "Variación vs Mes Ant. (días)";
                    FormatearRangoEncabezado(wsH.Cells[5, 1, 5, 6]);

                    wsH.Cells[6, 1].Value = "Base";
                    wsH.Cells[6, 2].Value = model.Horizonte.DiasSupervivenciaBase;
                    wsH.Cells[6, 3].Value = model.Horizonte.BufferRemanenteBase;
                    wsH.Cells[6, 4].Value = model.Horizonte.HorizonteBase;
                    wsH.Cells[6, 5].Value = model.Horizonte.SaldoFinalBase;
                    wsH.Cells[6, 6].Value = model.Horizonte.VariacionDiasBase;

                    wsH.Cells[7, 1].Value = "Moderado";
                    wsH.Cells[7, 2].Value = model.Horizonte.DiasSupervivenciaModerado;
                    wsH.Cells[7, 3].Value = model.Horizonte.BufferRemanenteModerado;
                    wsH.Cells[7, 4].Value = model.Horizonte.HorizonteModerado;
                    wsH.Cells[7, 5].Value = model.Horizonte.SaldoFinalModerado;
                    wsH.Cells[7, 6].Value = model.Horizonte.VariacionDiasModerado;

                    wsH.Cells[8, 1].Value = "Severo";
                    wsH.Cells[8, 2].Value = model.Horizonte.DiasSupervivenciaSevero;
                    wsH.Cells[8, 3].Value = model.Horizonte.BufferRemanenteSevero;
                    wsH.Cells[8, 4].Value = model.Horizonte.HorizonteSevero;
                    wsH.Cells[8, 5].Value = model.Horizonte.SaldoFinalSevero;
                    wsH.Cells[8, 6].Value = model.Horizonte.VariacionDiasSevero;

                    wsH.Cells[6, 2, 8, 2].Style.Numberformat.Format = "#,##0";
                    wsH.Cells[6, 3, 8, 3].Style.Numberformat.Format = "#,##0.00";
                    wsH.Cells[6, 5, 8, 5].Style.Numberformat.Format = "#,##0.00";
                    wsH.Cells[6, 6, 8, 6].Style.Numberformat.Format = "+#,##0;-#,##0;0";
                    wsH.Cells.AutoFitColumns();

                    // 2. Hoja Razón de Cobertura
                    var wsRc = package.Workbook.Worksheets.Add("Razón de Cobertura");
                    wsRc.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsRc, "RAZÓN DE COBERTURA DE LIQUIDEZ BAJO ESTRÉS", model.NombreEntidad, model.Periodo2);

                    wsRc.Cells[5, 1].Value = "Componente (₡ MM)";
                    wsRc.Cells[5, 2].Value = "Escenario Base";
                    wsRc.Cells[5, 3].Value = "Escenario Moderado";
                    wsRc.Cells[5, 4].Value = "Escenario Severo";
                    FormatearRangoEncabezado(wsRc.Cells[5, 1, 5, 4]);

                    int r = 6;
                    Action<string, decimal, decimal, decimal, bool> agregarFilaRc = (titulo, vB, vM, vS, bold) =>
                    {
                        wsRc.Cells[r, 1].Value = titulo;
                        wsRc.Cells[r, 2].Value = vB;
                        wsRc.Cells[r, 3].Value = vM;
                        wsRc.Cells[r, 4].Value = vS;
                        wsRc.Cells[r, 2, r, 4].Style.Numberformat.Format = "#,##0.00";
                        if (bold)
                        {
                            wsRc.Cells[r, 1, r, 4].Style.Font.Bold = true;
                            wsRc.Cells[r, 1, r, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            wsRc.Cells[r, 1, r, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(241, 245, 249));
                        }
                        r++;
                    };

                    agregarFilaRc("Activos Líquidos (FAL)", model.RazonCobertura.ActivosLiquidos_Base, model.RazonCobertura.ActivosLiquidos_Moderado, model.RazonCobertura.ActivosLiquidos_Severo, true);
                    agregarFilaRc("Total Ingresos Contractuales", model.RazonCobertura.IngresosContractuales_Base, model.RazonCobertura.IngresosContractuales_Moderado, model.RazonCobertura.IngresosContractuales_Severo, true);
                    agregarFilaRc("Total Egresos Contractuales", model.RazonCobertura.EgresosContractuales_Base, model.RazonCobertura.EgresosContractuales_Moderado, model.RazonCobertura.EgresosContractuales_Severo, true);
                    agregarFilaRc("Razón de Cobertura (veces)", model.RazonCobertura.RazonCobertura_Base, model.RazonCobertura.RazonCobertura_Moderado, model.RazonCobertura.RazonCobertura_Severo, true);
                    wsRc.Cells.AutoFitColumns();

                    // 3. Hoja ICL Estresado
                    var wsIcl = package.Workbook.Worksheets.Add("ICL Estresado");
                    wsIcl.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsIcl, "ÍNDICE DE COBERTURA DE LIQUIDEZ ESTRESADO", model.NombreEntidad, model.Periodo2);

                    wsIcl.Cells[5, 1].Value = "Concepto (₡ MM)";
                    wsIcl.Cells[5, 2].Value = "Contractual";
                    wsIcl.Cells[5, 3].Value = "Base";
                    wsIcl.Cells[5, 4].Value = "Moderado";
                    wsIcl.Cells[5, 5].Value = "Severo";
                    FormatearRangoEncabezado(wsIcl.Cells[5, 1, 5, 5]);

                    wsIcl.Cells[6, 1].Value = "Fondo de Activos Líquidos (FAL)";
                    wsIcl.Cells[6, 2].Value = model.ICLEstresado.FALContractual;
                    wsIcl.Cells[6, 3].Value = model.ICLEstresado.FALBase;
                    wsIcl.Cells[6, 4].Value = model.ICLEstresado.FALModerado;
                    wsIcl.Cells[6, 5].Value = model.ICLEstresado.FALSevero;

                    wsIcl.Cells[7, 1].Value = "Salidas Netas de Efectivo (30d)";
                    wsIcl.Cells[7, 2].Value = model.ICLEstresado.SalidasContractual;
                    wsIcl.Cells[7, 3].Value = model.ICLEstresado.SalidasBase;
                    wsIcl.Cells[7, 4].Value = model.ICLEstresado.SalidasModerado;
                    wsIcl.Cells[7, 5].Value = model.ICLEstresado.SalidasSevero;

                    wsIcl.Cells[8, 1].Value = "ICL Estresado (veces)";
                    wsIcl.Cells[8, 2].Value = model.ICLEstresado.ICLContractual;
                    wsIcl.Cells[8, 3].Value = model.ICLEstresado.ICLBase;
                    wsIcl.Cells[8, 4].Value = model.ICLEstresado.ICLModerado;
                    wsIcl.Cells[8, 5].Value = model.ICLEstresado.ICLSevero;

                    wsIcl.Cells[6, 2, 8, 5].Style.Numberformat.Format = "#,##0.00";
                    wsIcl.Cells[8, 1, 8, 5].Style.Font.Bold = true;
                    wsIcl.Cells.AutoFitColumns();

                    // 4. Hoja Brechas Estresadas
                    var wsB = package.Workbook.Worksheets.Add("Brechas Estresadas");
                    wsB.View.ShowGridLines = true;
                    AplicarCabeceraReporte(wsB, "BRECHAS DE LIQUIDEZ Y ACUMULADO POR ESCENARIO", model.NombreEntidad, model.Periodo2);

                    wsB.Cells[5, 1].Value = "Banda Temporal";
                    wsB.Cells[5, 2].Value = "Entradas Base";
                    wsB.Cells[5, 3].Value = "Salidas Base";
                    wsB.Cells[5, 4].Value = "Acum. Base";
                    wsB.Cells[5, 5].Value = "Entradas Mod.";
                    wsB.Cells[5, 6].Value = "Salidas Mod.";
                    wsB.Cells[5, 7].Value = "Acum. Mod.";
                    wsB.Cells[5, 8].Value = "Entradas Sev.";
                    wsB.Cells[5, 9].Value = "Salidas Sev.";
                    wsB.Cells[5, 10].Value = "Acum. Sev.";
                    FormatearRangoEncabezado(wsB.Cells[5, 1, 5, 10]);

                    r = 6;
                    for (int i = 0; i < model.BrechasEstres.Bandas.Count; i++)
                    {
                        wsB.Cells[r, 1].Value = model.BrechasEstres.Bandas[i];
                        wsB.Cells[r, 2].Value = model.BrechasEstres.EntradasBase.Count > i ? model.BrechasEstres.EntradasBase[i] : 0m;
                        wsB.Cells[r, 3].Value = model.BrechasEstres.SalidasBase.Count > i ? model.BrechasEstres.SalidasBase[i] : 0m;
                        wsB.Cells[r, 4].Value = model.BrechasEstres.AcumuladoBase.Count > i ? model.BrechasEstres.AcumuladoBase[i] : 0m;

                        wsB.Cells[r, 5].Value = model.BrechasEstres.EntradasModerado.Count > i ? model.BrechasEstres.EntradasModerado[i] : 0m;
                        wsB.Cells[r, 6].Value = model.BrechasEstres.SalidasModerado.Count > i ? model.BrechasEstres.SalidasModerado[i] : 0m;
                        wsB.Cells[r, 7].Value = model.BrechasEstres.AcumuladoModerado.Count > i ? model.BrechasEstres.AcumuladoModerado[i] : 0m;

                        wsB.Cells[r, 8].Value = model.BrechasEstres.EntradasSevero.Count > i ? model.BrechasEstres.EntradasSevero[i] : 0m;
                        wsB.Cells[r, 9].Value = model.BrechasEstres.SalidasSevero.Count > i ? model.BrechasEstres.SalidasSevero[i] : 0m;
                        wsB.Cells[r, 10].Value = model.BrechasEstres.AcumuladoSevero.Count > i ? model.BrechasEstres.AcumuladoSevero[i] : 0m;

                        wsB.Cells[r, 2, r, 10].Style.Numberformat.Format = "#,##0.00";
                        r++;
                    }
                    wsB.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    string fileName = $"Escenarios_Estres_Liquidez_{model.IdEntidad}_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Content("Error al exportar a Excel: " + ex.Message);
            }
        }

        private void AplicarCabeceraReporte(ExcelWorksheet ws, string modulo, string entidad, string periodo)
        {
            ws.Cells[1, 1].Value = "SISTEMA DE ALERTAS Y CALIFICACIÓN - SAC";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 13;
            ws.Cells[1, 1].Style.Font.Color.SetColor(Color.FromArgb(20, 55, 80));

            ws.Cells[2, 1].Value = modulo;
            ws.Cells[2, 1].Style.Font.Bold = true;
            ws.Cells[2, 1].Style.Font.Size = 11;
            ws.Cells[2, 1].Style.Font.Color.SetColor(Color.FromArgb(2, 132, 199));

            ws.Cells[3, 1].Value = $"Entidad: {entidad} | Corte: {periodo} | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Cells[3, 1].Style.Font.Italic = true;
            ws.Cells[3, 1].Style.Font.Size = 10;
            ws.Cells[3, 1].Style.Font.Color.SetColor(Color.FromArgb(100, 116, 139));
        }

        private void FormatearRangoEncabezado(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(43, 87, 154)); // #2b579a
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        #endregion

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
            decimal inv_p1 = ObtenerSaldoCta(saldosP1, "12000000");
            decimal capVista_p1 = ObtenerSaldoCta(saldosP1, "21100000");
            decimal capPlazo_p1 = ObtenerSaldoCta(saldosP1, "21300000");
            decimal capTotal_p1 = ObtenerSaldoCta(saldosP1, "21000000");
            decimal cartNeta_p1 = ObtenerSaldoCta(saldosP1, "13000000");
            decimal actTotal_p1 = ObtenerSaldoCta(saldosP1, "10000000");
            decimal deuLP_p1 = ObtenerSaldoCta(saldosP1, "23200000");
            decimal deuTotal_p1 = ObtenerSaldoCta(saldosP1, "23000000");
            decimal pat_p1 = ObtenerSaldoCta(saldosP1, "31000000");

            // Valores de balance P2
            decimal disp_p2 = ObtenerSaldoCta(saldosP2, "11000000");
            decimal inv_p2 = ObtenerSaldoCta(saldosP2, "12000000");
            decimal capVista_p2 = ObtenerSaldoCta(saldosP2, "21100000");
            decimal capPlazo_p2 = ObtenerSaldoCta(saldosP2, "21300000");
            decimal capTotal_p2 = ObtenerSaldoCta(saldosP2, "21000000");
            decimal cartNeta_p2 = ObtenerSaldoCta(saldosP2, "13000000");
            decimal actTotal_p2 = ObtenerSaldoCta(saldosP2, "10000000");
            decimal deuLP_p2 = ObtenerSaldoCta(saldosP2, "23200000");
            decimal deuTotal_p2 = ObtenerSaldoCta(saldosP2, "23000000");
            decimal pat_p2 = ObtenerSaldoCta(saldosP2, "31000000");

            // FAL ICL directamente de saldos contables
            decimal fal_p1 = disp_p1 + inv_p1;
            decimal fal_p2 = disp_p2 + inv_p2;
            decimal pasCP_p1 = capVista_p1 + (capPlazo_p1 * 0.35m);
            decimal pasCP_p2 = capVista_p2 + (capPlazo_p2 * 0.35m);
            decimal top20_p1 = capTotal_p1 * 0.25m;
            decimal top20_p2 = capTotal_p2 * 0.25m;

            // 1. Liquidez inmediata: 11000000 / 21100000
            decimal v1_p1 = capVista_p1 > 0 ? (disp_p1 / capVista_p1) : 0m;
            decimal v1_p2 = capVista_p2 > 0 ? (disp_p2 / capVista_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 1, Nombre = "Liquidez inmediata", Formula = "Disponibilidades (11000000) / Captaciones a la vista (21100000)", QueMide = "Capacidad de atender retiros inmediatos", ValorP1 = v1_p1, ValorP2 = v1_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 2. Activos líquidos / Captaciones: FAL / 21000000
            decimal v2_p1 = capTotal_p1 > 0 ? (fal_p1 / capTotal_p1) : 0m;
            decimal v2_p2 = capTotal_p2 > 0 ? (fal_p2 / capTotal_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 2, Nombre = "Activos líquidos / Captaciones", Formula = "FAL ICL / Saldo captaciones totales (21000000)", QueMide = "Cobertura general de depósitos con activos líquidos", ValorP1 = v2_p1, ValorP2 = v2_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 3. Activos líquidos / Pasivos exigibles CP
            decimal v3_p1 = pasCP_p1 > 0 ? (fal_p1 / pasCP_p1) : 0m;
            decimal v3_p2 = pasCP_p2 > 0 ? (fal_p2 / pasCP_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 3, Nombre = "Activos líquidos / Pasivos exigibles CP", Formula = "Activos líquidos (FAL ICL) / Pasivos exigibles ≤ 30 días", QueMide = "Cobertura de obligaciones de corto plazo", ValorP1 = v3_p1, ValorP2 = v3_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 4. Cartera / Captaciones: 13000000 / (21300000 + 21100000)
            decimal den4_p1 = capPlazo_p1 + capVista_p1;
            decimal den4_p2 = capPlazo_p2 + capVista_p2;
            decimal v4_p1 = den4_p1 > 0 ? (cartNeta_p1 / den4_p1) : 0m;
            decimal v4_p2 = den4_p2 > 0 ? (cartNeta_p2 / den4_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 4, Nombre = "Cartera / Captaciones", Formula = "Cartera neta (13000000) / (21300000 + 21100000)", QueMide = "Qué proporción de los depósitos está colocada en cartera", ValorP1 = v4_p1, ValorP2 = v4_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 5. Cartera / Fondeo estable: 13000000 / (21300000 + 23200000 + 31000000)
            decimal den5_p1 = capPlazo_p1 + deuLP_p1 + pat_p1;
            decimal den5_p2 = capPlazo_p2 + deuLP_p2 + pat_p2;
            decimal v5_p1 = den5_p1 > 0 ? (cartNeta_p1 / den5_p1) : 0m;
            decimal v5_p2 = den5_p2 > 0 ? (cartNeta_p2 / den5_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 5, Nombre = "Cartera / Fondeo estable", Formula = "Cartera neta / (Captaciones LP + deuda LP + patrimonio)", QueMide = "Dependencia de fondeo estable para financiar cartera", ValorP1 = v5_p1, ValorP2 = v5_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 6. Activos líquidos / Activos: FAL / Activo Total
            decimal v6_p1 = actTotal_p1 > 0 ? (fal_p1 / actTotal_p1) : 0m;
            decimal v6_p2 = actTotal_p2 > 0 ? (fal_p2 / actTotal_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 6, Nombre = "Activos líquidos / Activos", Formula = "FAL ICL / Activo total", QueMide = "Buffer de liquidez respecto al tamaño de la entidad", ValorP1 = v6_p1, ValorP2 = v6_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 7. Activos líquidos / 20 mayores depositantes
            decimal v7_p1 = top20_p1 > 0 ? (fal_p1 / top20_p1) : 0m;
            decimal v7_p2 = top20_p2 > 0 ? (fal_p2 / top20_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 7, Nombre = "Activos líquidos / 20 mayores depositantes", Formula = "Activos líquidos (FAL ICL) / Saldo Top 20 depositantes", QueMide = "Capacidad de absorber concentración de fondeo", ValorP1 = v7_p1, ValorP2 = v7_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 8. Fondeo mayorista / Captaciones totales: 23000000 / 21000000
            decimal v8_p1 = capTotal_p1 > 0 ? (deuTotal_p1 / capTotal_p1) : 0m;
            decimal v8_p2 = capTotal_p2 > 0 ? (deuTotal_p2 / capTotal_p2) : 0m;
            result.Add(new IndicadorLiquidezItem { Orden = 8, Nombre = "Fondeo mayorista / Captaciones totales", Formula = "Deuda institucional (23000000) / Fondeo total (21000000)", QueMide = "Dependencia de fuentes institucionales", ValorP1 = v8_p1, ValorP2 = v8_p2, EsPorcentaje = true, TipoGraficoAsociado = 1 });

            return result;
        }

        private ICLViewModel ObtenerDatosICL(string entidad, DateTime fecha)
        {
            var saldos = ObtenerSaldosContables(entidad, fecha);
            decimal disp = ObtenerSaldoCta(saldos, "11000000");
            decimal inv = ObtenerSaldoCta(saldos, "12000000");
            decimal fal = disp + inv;

            decimal depVista = ObtenerSaldoCta(saldos, "21100000");
            decimal depPlazo = ObtenerSaldoCta(saldos, "21300000");
            decimal depTotal = depVista + depPlazo;
            decimal top20 = depTotal * 0.20m;

            decimal salidaVistaPond = depVista * 0.10m;
            decimal salidaTop20Pond = top20 * 0.50m;
            decimal salidasNetas = salidaVistaPond + salidaTop20Pond;

            decimal iclRatio = salidasNetas > 0 ? Math.Round((fal / salidasNetas), 2) : 0m;

            var icl = new ICLViewModel
            {
                Periodo = fecha.ToString("MM/yyyy"),
                MonedaTipo = 1,
                TieneDolares = false,
                ICLRatio = iclRatio,
                FondoActivosLiquidos = fal,
                SalidasNetasEfectivo = salidasNetas,
                Items = new List<ICLDetalleItem>
                {
                    new ICLDetalleItem { Codigo = "100000", Nivel = 0, Concepto = "ÍNDICE DE COBERTURA DE LIQUIDEZ (ICL)", SaldoBruto = iclRatio, Ponderador = 1m, SaldoPonderado = iclRatio, EsTotalizador = true },
                    new ICLDetalleItem { Codigo = "200000", Nivel = 1, Concepto = "Fondo de Activos Líquidos (FAL)", SaldoBruto = fal, Ponderador = 1m, SaldoPonderado = fal, EsTotalizador = true },
                    new ICLDetalleItem { Codigo = "210000", Nivel = 2, Concepto = "Activos Nivel 1", SaldoBruto = fal, Ponderador = 1m, SaldoPonderado = fal, EsTotalizador = false },
                    new ICLDetalleItem { Codigo = "210110", Nivel = 3, Concepto = "Efectivo y disponibilidades", SaldoBruto = disp, Ponderador = 1m, SaldoPonderado = disp, EsTotalizador = false },
                    new ICLDetalleItem { Codigo = "210120", Nivel = 3, Concepto = "Valores de deuda del BCCR o Gobierno", SaldoBruto = inv, Ponderador = 1m, SaldoPonderado = inv, EsTotalizador = false },
                    new ICLDetalleItem { Codigo = "220000", Nivel = 2, Concepto = "Activos Nivel 2A y 2B", SaldoBruto = 0m, Ponderador = 0.85m, SaldoPonderado = 0m, EsTotalizador = false },
                    new ICLDetalleItem { Codigo = "300000", Nivel = 1, Concepto = "Salidas Netas de Efectivo (30 días)", SaldoBruto = depVista + top20, Ponderador = 1m, SaldoPonderado = salidasNetas, EsTotalizador = true },
                    new ICLDetalleItem { Codigo = "310000", Nivel = 2, Concepto = "Salidas de depósitos minoristas", SaldoBruto = depVista, Ponderador = 0.10m, SaldoPonderado = salidaVistaPond, EsTotalizador = false },
                    new ICLDetalleItem { Codigo = "320000", Nivel = 2, Concepto = "Salidas de depósitos no garantizados (Top 20)", SaldoBruto = top20, Ponderador = 0.50m, SaldoPonderado = salidaTop20Pond, EsTotalizador = false }
                },
                HistoricoMeses = new List<string>(),
                HistoricoValores = new List<decimal>()
            };

            for (int m = 11; m >= 0; m--)
            {
                DateTime fM = fecha.AddMonths(-m);
                icl.HistoricoMeses.Add(fM.ToString("MMM yy", new CultureInfo("es-CR")));
                try
                {
                    var sM = ObtenerSaldosContables(entidad, fM);
                    decimal dM = ObtenerSaldoCta(sM, "11000000");
                    decimal iM = ObtenerSaldoCta(sM, "12000000");
                    decimal fM_fal = dM + iM;
                    decimal depVM = ObtenerSaldoCta(sM, "21100000");
                    decimal topM = (depVM + ObtenerSaldoCta(sM, "21300000")) * 0.20m;
                    decimal salM = (depVM * 0.10m) + (topM * 0.50m);
                    decimal ratM = salM > 0 ? Math.Round((fM_fal / salM), 2) : 0m;
                    icl.HistoricoValores.Add(ratM);
                }
                catch
                {
                    icl.HistoricoValores.Add(0m);
                }
            }

            return icl;
        }

        private GapContractualViewModel ObtenerDatosGap(string entidad, DateTime fecha)
        {
            var vm = new GapContractualViewModel
            {
                PeriodoCorte = fecha.ToString("dd/MM/yyyy"),
                Bandas = new List<string> { "1 a 30 días", "31 a 60 días", "61 a 90 días", "91 a 180 días", "181 a 270 días", "271 a 365 días", "(+) 360 días" },
                Entradas = new List<decimal>(),
                Salidas = new List<decimal>(),
                BrechasBanda = new List<decimal>(),
                BrechasAcumuladas = new List<decimal>(),
                Disponibilidades = new List<decimal>(),
                RecuperacionCartera = new List<decimal>(),
                VencimientoInversiones = new List<decimal>(),
                VencimientosCDP = new List<decimal>(),
                ObligacionesEntidades = new List<decimal>(),
                OtrasCuentasPagar = new List<decimal>()
            };

            try
            {
                var spRes = spClient.FGA_Consultar_GAP_Contractual(entidad, fecha);
                if (spRes != null && spRes.Length > 0)
                {
                    vm.Bandas = spRes.Select(x => x.Banda).ToList();
                    vm.Entradas = spRes.Select(x => x.Entradas ?? 0m).ToList();
                    vm.Salidas = spRes.Select(x => x.Salidas ?? 0m).ToList();
                    vm.BrechasBanda = spRes.Select(x => x.BrechaBanda ?? 0m).ToList();
                    vm.BrechasAcumuladas = spRes.Select(x => x.BrechaAcumulada ?? 0m).ToList();
                }
            }
            catch { }

            // Si no hay datos en base de datos de GAP, calcular a partir de saldos de balance
            var saldos = ObtenerSaldosContables(entidad, fecha);
            decimal disp = ObtenerSaldoCta(saldos, "11000000") / 1000000m;
            decimal inv = ObtenerSaldoCta(saldos, "12000000") / 1000000m;
            decimal cart = ObtenerSaldoCta(saldos, "13000000") / 1000000m;
            decimal cdp = ObtenerSaldoCta(saldos, "21300000") / 1000000m;
            decimal oblig = ObtenerSaldoCta(saldos, "23000000") / 1000000m;
            decimal otrasPagar = ObtenerSaldoCta(saldos, "24000000") / 1000000m;

            // Desglose de partidas por banda (7 bandas)
            vm.Disponibilidades = new List<decimal> { disp, 0m, 0m, 0m, 0m, 0m, 0m };
            decimal invMes = inv > 0 ? inv / 6m : 0m;
            vm.VencimientoInversiones = new List<decimal> { invMes, invMes, invMes, invMes, invMes, invMes, 0m };
            decimal cartMes = cart > 0 ? (cart * 0.025m) : 0m;
            vm.RecuperacionCartera = new List<decimal> { cartMes, cartMes, cartMes, cartMes * 3m, cartMes * 3m, cartMes * 3m, cart > 0 ? cart * 0.3m : 0m };
            decimal cdpMes = cdp > 0 ? (cdp / 12m) : 0m;
            vm.VencimientosCDP = new List<decimal> { cdpMes, cdpMes, cdpMes, cdpMes * 3m, cdpMes * 3m, cdpMes * 3m, cdp > 0 ? cdp * 0.25m : 0m };
            decimal obligMes = oblig > 0 ? (oblig / 12m) : 0m;
            vm.ObligacionesEntidades = new List<decimal> { obligMes, obligMes, obligMes, obligMes * 3m, obligMes * 3m, obligMes * 3m, oblig > 0 ? oblig * 0.25m : 0m };
            decimal opMes = otrasPagar > 0 ? (otrasPagar / 6m) : 0m;
            vm.OtrasCuentasPagar = new List<decimal> { opMes, opMes, opMes, opMes, opMes, opMes, 0m };

            if (vm.Entradas.Count == 0 || vm.Entradas.All(x => x == 0m))
            {
                vm.Entradas.Clear();
                vm.Salidas.Clear();
                vm.BrechasBanda.Clear();
                vm.BrechasAcumuladas.Clear();
                decimal acum = disp + inv;
                for (int i = 0; i < 7; i++)
                {
                    decimal ent = vm.Disponibilidades[i] + vm.VencimientoInversiones[i] + vm.RecuperacionCartera[i];
                    decimal sal = vm.VencimientosCDP[i] + vm.ObligacionesEntidades[i] + vm.OtrasCuentasPagar[i];
                    decimal brecha = ent - sal;
                    acum += brecha;

                    vm.Entradas.Add(Math.Round(ent, 2));
                    vm.Salidas.Add(Math.Round(sal, 2));
                    vm.BrechasBanda.Add(Math.Round(brecha, 2));
                    vm.BrechasAcumuladas.Add(Math.Round(acum, 2));
                }
            }

            if (vm.BrechasAcumuladas.Count > 0) vm.GapAcumulado30d = vm.BrechasAcumuladas[0];
            if (vm.BrechasAcumuladas.Count > 2) vm.GapAcumulado90d = vm.BrechasAcumuladas[2];
            if (vm.BrechasBanda.Count > 0)
            {
                vm.MaximaBrecha = vm.BrechasBanda.Min();
                int idxMin = vm.BrechasBanda.IndexOf(vm.MaximaBrecha);
                vm.BandaCritica = (idxMin >= 0 && idxMin < vm.Bandas.Count) ? vm.Bandas[idxMin] : "";
            }

            return vm;
        }

        private SupuestosSensibilizacionViewModel ObtenerSupuestosPorDefecto(string entidad, DateTime fecha)
        {
            var saldos = ObtenerSaldosContables(entidad, fecha);
            decimal disp = ObtenerSaldoCta(saldos, "11000000");
            decimal inv = ObtenerSaldoCta(saldos, "12000000");
            decimal fal = disp + inv;

            decimal obligTotal = ObtenerSaldoCta(saldos, "23000000");
            decimal pagoDeudaMes = obligTotal > 0 ? Math.Round(obligTotal / 12m, 2) : 0m;
            decimal cuponesMes = inv > 0 ? Math.Round((inv * 0.05m) / 12m, 2) : 0m;

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
                Haircut_Base = fal,
                Haircut_Moderado = Math.Round(fal * 0.9169m, 2),
                Haircut_Severo = Math.Round(fal * 0.7757m, 2),
                SalidaTop20_Base = 0.05m,
                SalidaTop20_Moderado = 0.15m,
                SalidaTop20_Severo = 0.35m,
                DisponibilidadLineas_Base = 0m,
                DisponibilidadLineas_Moderado = 0m,
                DisponibilidadLineas_Severo = 0m,
                PagoObligaciones_Base = pagoDeudaMes,
                PagoObligaciones_Moderado = pagoDeudaMes,
                PagoObligaciones_Severo = pagoDeudaMes,
                Cupones_Base = cuponesMes,
                Cupones_Moderado = Math.Round(cuponesMes * 0.60m, 2),
                Cupones_Severo = Math.Round(cuponesMes * 0.40m, 2),
                CapitalSocial_Base = 0.035m,
                CapitalSocial_Moderado = 0.028m,
                CapitalSocial_Severo = 0.0175m,
                OtrosIngresos_Base = 0.0394m,
                OtrosIngresos_Moderado = 0.0315m,
                OtrosIngresos_Severo = 0.0197m,
                EgresosFinancieros_Base = 0.0100m,
                EgresosFinancieros_Moderado = 0.0200m,
                EgresosFinancieros_Severo = 0.0300m,
                GastosAdministrativos_Base = 0.0150m,
                GastosAdministrativos_Moderado = 0.0250m,
                GastosAdministrativos_Severo = 0.0400m
            };
        }

        private RazonCoberturaViewModel CalcularRazonCobertura(Dictionary<string, decimal> saldos, GapContractualViewModel gap, SupuestosSensibilizacionViewModel supuestos)
        {
            var rc = new RazonCoberturaViewModel();

            decimal disp = ObtenerSaldoCta(saldos, "11000000") / 1000000m;
            decimal invLiq = ObtenerSaldoCta(saldos, "12000000") / 1000000m;
            decimal fal = (disp + invLiq);

            rc.ActivosLiquidos_Base = fal;
            rc.ActivosLiquidos_Moderado = supuestos.Haircut_Moderado > 0 ? Math.Round(supuestos.Haircut_Moderado / 1000000m, 2) : Math.Round(fal * 0.9169m, 2);
            rc.ActivosLiquidos_Severo = supuestos.Haircut_Severo > 0 ? Math.Round(supuestos.Haircut_Severo / 1000000m, 2) : Math.Round(fal * 0.7757m, 2);

            // Ingresos Contractuales (Requerimiento imagen 16 y Anexo Excel Hoja Razón de cobertura)
            decimal cartNeta = ObtenerSaldoCta(saldos, "13000000") / 1000000m;
            decimal recCarteraBase = (gap.RecuperacionCartera != null && gap.RecuperacionCartera.Count > 0 && gap.RecuperacionCartera[0] > 0)
                ? gap.RecuperacionCartera[0]
                : (cartNeta > 0 ? (cartNeta * 0.025m) : 0m);

            rc.CarteraRecuperacion_Base = Math.Round(recCarteraBase, 2);
            rc.CarteraRecuperacion_Moderado = Math.Round(recCarteraBase * (1m - supuestos.DisminucionRecaudo_Moderado), 2);
            rc.CarteraRecuperacion_Severo = Math.Round(rc.CarteraRecuperacion_Moderado * (1m - supuestos.DisminucionRecaudo_Severo), 2);

            decimal invBase = (gap.VencimientoInversiones != null && gap.VencimientoInversiones.Count > 0 && gap.VencimientoInversiones[0] > 0)
                ? gap.VencimientoInversiones[0]
                : (invLiq > 0 ? invLiq / 6m : 0m);

            rc.Inversiones_Base = Math.Round(invBase + (supuestos.Cupones_Base / 1000000m), 2);
            rc.Inversiones_Moderado = Math.Round(invBase + (supuestos.Cupones_Moderado / 1000000m), 2);
            rc.Inversiones_Severo = Math.Round(invBase + (supuestos.Cupones_Severo / 1000000m), 2);

            decimal comisiones = ObtenerSaldoCta(saldos, "52000000") / 1000000m;
            if (comisiones <= 0m) comisiones = ObtenerSaldoCta(saldos, "51000000") / 1000000m * 0.05m;
            rc.Comisiones_Base = Math.Round(comisiones * (1m + supuestos.OtrosIngresos_Base), 2);
            rc.Comisiones_Moderado = Math.Round(comisiones * (1m + supuestos.OtrosIngresos_Moderado), 2);
            rc.Comisiones_Severo = Math.Round(comisiones * (1m + supuestos.OtrosIngresos_Severo), 2);

            rc.IngresosContractuales_Base = Math.Round(rc.CarteraRecuperacion_Base + rc.Inversiones_Base + rc.Comisiones_Base, 2);
            rc.IngresosContractuales_Moderado = Math.Round(rc.CarteraRecuperacion_Moderado + rc.Inversiones_Moderado + rc.Comisiones_Moderado, 2);
            rc.IngresosContractuales_Severo = Math.Round(rc.CarteraRecuperacion_Severo + rc.Inversiones_Severo + rc.Comisiones_Severo, 2);

            // Egresos Contractuales
            decimal cdpTotal = ObtenerSaldoCta(saldos, "21300000") / 1000000m;
            decimal cdp1Mes = (gap.VencimientosCDP != null && gap.VencimientosCDP.Count > 0 && gap.VencimientosCDP[0] > 0)
                ? gap.VencimientosCDP[0]
                : (cdpTotal > 0 ? (cdpTotal / 12m) : 0m);

            rc.CDPContractuales_Base = Math.Round(cdp1Mes * (1m - supuestos.RenovacionCDP_Base), 2);
            rc.CDPContractuales_Moderado = Math.Round(cdp1Mes * (1m - supuestos.RenovacionCDP_Moderado), 2);
            rc.CDPContractuales_Severo = Math.Round(cdp1Mes * (1m - supuestos.RenovacionCDP_Severo), 2);

            decimal capTotal = ObtenerSaldoCta(saldos, "21000000") / 1000000m;
            decimal top20Total = capTotal * 0.25m;
            rc.SalidaTop20_Base = Math.Round(top20Total * supuestos.SalidaTop20_Base, 2);
            rc.SalidaTop20_Moderado = Math.Round(top20Total * supuestos.SalidaTop20_Moderado, 2);
            rc.SalidaTop20_Severo = Math.Round(top20Total * supuestos.SalidaTop20_Severo, 2);

            rc.PagoObligaciones_Base = Math.Round(supuestos.PagoObligaciones_Base / 1000000m, 2);
            rc.PagoObligaciones_Moderado = Math.Round(supuestos.PagoObligaciones_Moderado / 1000000m, 2);
            rc.PagoObligaciones_Severo = Math.Round(supuestos.PagoObligaciones_Severo / 1000000m, 2);

            decimal egresosFinAdm = (ObtenerSaldoCta(saldos, "41000000") + ObtenerSaldoCta(saldos, "42000000")) / 1000000m;
            rc.EgresosFinAdm_Base = Math.Round(egresosFinAdm * (1m + supuestos.EgresosFinancieros_Base + supuestos.GastosAdministrativos_Base), 2);
            rc.EgresosFinAdm_Moderado = Math.Round(egresosFinAdm * (1m + supuestos.EgresosFinancieros_Moderado + supuestos.GastosAdministrativos_Moderado), 2);
            rc.EgresosFinAdm_Severo = Math.Round(egresosFinAdm * (1m + supuestos.EgresosFinancieros_Severo + supuestos.GastosAdministrativos_Severo), 2);

            decimal lineas = (supuestos.DisponibilidadLineas_Base / 1000000m);
            rc.DesembolsosCartera_Base = Math.Round((cartNeta * (supuestos.CrecimientoCartera_Base / 12m)) + lineas, 2);
            rc.DesembolsosCartera_Moderado = Math.Round((cartNeta * (supuestos.CrecimientoCartera_Moderado / 12m)) + (supuestos.DisponibilidadLineas_Moderado / 1000000m), 2);
            rc.DesembolsosCartera_Severo = Math.Round((cartNeta * (supuestos.CrecimientoCartera_Severo / 12m)) + (supuestos.DisponibilidadLineas_Severo / 1000000m), 2);

            decimal vistaTotal = ObtenerSaldoCta(saldos, "21100000") / 1000000m;
            rc.CorridaDepositosVista_Base = Math.Round(vistaTotal * supuestos.SalidaVista_Base, 2);
            rc.CorridaDepositosVista_Moderado = Math.Round(vistaTotal * supuestos.SalidaVista_Moderado, 2);
            rc.CorridaDepositosVista_Severo = Math.Round(vistaTotal * supuestos.SalidaVista_Severo, 2);

            rc.EgresosContractuales_Base = Math.Round(rc.CDPContractuales_Base + rc.SalidaTop20_Base + rc.PagoObligaciones_Base + rc.EgresosFinAdm_Base + rc.DesembolsosCartera_Base + rc.CorridaDepositosVista_Base, 2);
            rc.EgresosContractuales_Moderado = Math.Round(rc.CDPContractuales_Moderado + rc.SalidaTop20_Moderado + rc.PagoObligaciones_Moderado + rc.EgresosFinAdm_Moderado + rc.DesembolsosCartera_Moderado + rc.CorridaDepositosVista_Moderado, 2);
            rc.EgresosContractuales_Severo = Math.Round(rc.CDPContractuales_Severo + rc.SalidaTop20_Severo + rc.PagoObligaciones_Severo + rc.EgresosFinAdm_Severo + rc.DesembolsosCartera_Severo + rc.CorridaDepositosVista_Severo, 2);

            // Razón de Cobertura oficial Excel F16 / (F22 - F18)
            decimal netoBase = rc.EgresosContractuales_Base - rc.IngresosContractuales_Base;
            rc.RazonCobertura_Base = netoBase > 0 ? Math.Round(rc.ActivosLiquidos_Base / netoBase, 2) : 0m;

            decimal netoMod = rc.EgresosContractuales_Moderado - rc.IngresosContractuales_Moderado;
            rc.RazonCobertura_Moderado = netoMod > 0 ? Math.Round(rc.ActivosLiquidos_Moderado / netoMod, 2) : 0m;

            decimal netoSev = rc.EgresosContractuales_Severo - rc.IngresosContractuales_Severo;
            rc.RazonCobertura_Severo = netoSev > 0 ? Math.Round(rc.ActivosLiquidos_Severo / netoSev, 2) : 0m;

            // Diferencia de Escenarios
            rc.CDPDiferencia_Base = 0m;
            rc.CDPDiferencia_Moderado = Math.Round(rc.CDPContractuales_Moderado - rc.CDPContractuales_Base, 2);
            rc.CDPDiferencia_Severo = Math.Round(rc.CDPContractuales_Severo - rc.CDPContractuales_Base, 2);

            rc.SalidaTop20Diferencia_Base = 0m;
            rc.SalidaTop20Diferencia_Moderado = Math.Round(rc.SalidaTop20_Moderado - rc.SalidaTop20_Base, 2);
            rc.SalidaTop20Diferencia_Severo = Math.Round(rc.SalidaTop20_Severo - rc.SalidaTop20_Base, 2);

            rc.CorridaVistaDiferencia_Base = 0m;
            rc.CorridaVistaDiferencia_Moderado = Math.Round(rc.CorridaDepositosVista_Moderado - rc.CorridaDepositosVista_Base, 2);
            rc.CorridaVistaDiferencia_Severo = Math.Round(rc.CorridaDepositosVista_Severo - rc.CorridaDepositosVista_Base, 2);

            rc.DesembolsoCarteraDiferencia_Base = 0m;
            rc.DesembolsoCarteraDiferencia_Moderado = Math.Round(rc.DesembolsosCartera_Moderado - rc.DesembolsosCartera_Base, 2);
            rc.DesembolsoCarteraDiferencia_Severo = Math.Round(rc.DesembolsosCartera_Severo - rc.DesembolsosCartera_Base, 2);

            return rc;
        }

        private ICLEstresadoViewModel CalcularICLEstresado(ICLViewModel icl, Dictionary<string, decimal> saldos, SupuestosSensibilizacionViewModel supuestos, RazonCoberturaViewModel rc)
        {
            var vm = new ICLEstresadoViewModel();
            decimal disp = ObtenerSaldoCta(saldos, "11000000") / 1000000m;
            decimal invLiq = ObtenerSaldoCta(saldos, "12000000") / 1000000m;
            decimal fal = (icl != null && icl.FondoActivosLiquidos > 0) ? (icl.FondoActivosLiquidos / 1000000m) : (disp + invLiq);

            vm.FALContractual = Math.Round(fal, 2);
            vm.FALBase = Math.Round(supuestos.Haircut_Base > 0 ? supuestos.Haircut_Base / 1000000m : fal, 2);
            vm.FALModerado = Math.Round(supuestos.Haircut_Moderado > 0 ? supuestos.Haircut_Moderado / 1000000m : fal * 0.9169m, 2);
            vm.FALSevero = Math.Round(supuestos.Haircut_Severo > 0 ? supuestos.Haircut_Severo / 1000000m : fal * 0.7757m, 2);

            decimal salContractual = (icl != null && icl.SalidasNetasEfectivo > 0) ? (icl.SalidasNetasEfectivo / 1000000m) : rc.EgresosContractuales_Base;
            vm.SalidasContractual = Math.Round(salContractual, 2);
            vm.SalidasBase = Math.Round(salContractual + rc.SalidaTop20_Base, 2);
            vm.SalidasModerado = Math.Round(salContractual + rc.SalidaTop20_Moderado, 2);
            vm.SalidasSevero = Math.Round(salContractual + rc.SalidaTop20_Severo, 2);

            vm.ICLContractual = vm.SalidasContractual > 0 ? Math.Round(vm.FALContractual / vm.SalidasContractual, 2) : 0m;
            vm.ICLBase = vm.SalidasBase > 0 ? Math.Round(vm.FALBase / vm.SalidasBase, 2) : 0m;
            vm.ICLModerado = vm.SalidasModerado > 0 ? Math.Round(vm.FALModerado / vm.SalidasModerado, 2) : 0m;
            vm.ICLSevero = vm.SalidasSevero > 0 ? Math.Round(vm.FALSevero / vm.SalidasSevero, 2) : 0m;

            return vm;
        }

        private BrechasLiquidezViewModel CalcularBrechasEstres(GapContractualViewModel gap, Dictionary<string, decimal> saldos, SupuestosSensibilizacionViewModel supuestos, RazonCoberturaViewModel rc)
        {
            var vm = new BrechasLiquidezViewModel();
            decimal fal = rc.ActivosLiquidos_Base;
            decimal falMod = rc.ActivosLiquidos_Moderado;
            decimal falSev = rc.ActivosLiquidos_Severo;

            vm.FAL_Base = fal;
            vm.FAL_Moderado = falMod;
            vm.FAL_Severo = falSev;

            vm.EntradasBase = new List<decimal>();
            vm.SalidasBase = new List<decimal>();
            vm.BrechasBase = new List<decimal>();
            vm.AcumuladoBase = new List<decimal>();

            vm.EntradasModerado = new List<decimal>();
            vm.SalidasModerado = new List<decimal>();
            vm.BrechasModerado = new List<decimal>();
            vm.AcumuladoModerado = new List<decimal>();

            vm.EntradasSevero = new List<decimal>();
            vm.SalidasSevero = new List<decimal>();
            vm.BrechasSevero = new List<decimal>();
            vm.AcumuladoSevero = new List<decimal>();

            decimal capSocialSaldo = ObtenerSaldoCta(saldos, "31000000") / 1000000m;
            decimal cuponBase = supuestos.Cupones_Base / 1000000m;
            decimal cuponMod = supuestos.Cupones_Moderado / 1000000m;
            decimal cuponSev = supuestos.Cupones_Severo / 1000000m;

            // Factores mensuales por banda (7 bandas: 1m, 1m, 1m, 3m, 3m, 3m, 12m)
            decimal[] factores = { 1m, 1m, 1m, 3m, 3m, 3m, 12m };

            decimal acumB = fal;
            decimal acumM = falMod;
            decimal acumS = falSev;

            for (int i = 0; i < 7; i++)
            {
                decimal factor = factores[i];

                // 1. Entradas por banda
                decimal recCarteraB = (i == 0) ? rc.CarteraRecuperacion_Base : ((gap.RecuperacionCartera != null && gap.RecuperacionCartera.Count > i && gap.RecuperacionCartera[i] > 0) ? gap.RecuperacionCartera[i] : rc.CarteraRecuperacion_Base * factor);
                decimal recCarteraM = recCarteraB * (1m - supuestos.DisminucionRecaudo_Moderado);
                decimal recCarteraS = recCarteraM * (1m - supuestos.DisminucionRecaudo_Severo);

                decimal invBanda = (gap.VencimientoInversiones != null && gap.VencimientoInversiones.Count > i) ? gap.VencimientoInversiones[i] : 0m;
                decimal comisionesB = rc.Comisiones_Base * factor;
                decimal comisionesM = rc.Comisiones_Moderado * factor;
                decimal comisionesS = rc.Comisiones_Severo * factor;

                decimal capSocialB = (capSocialSaldo * (supuestos.CapitalSocial_Base / 12m)) * factor;
                decimal capSocialM = (capSocialSaldo * (supuestos.CapitalSocial_Moderado / 12m)) * factor;
                decimal capSocialS = (capSocialSaldo * (supuestos.CapitalSocial_Severo / 12m)) * factor;

                decimal entB = Math.Round(recCarteraB + invBanda + comisionesB + capSocialB + (cuponBase * factor), 2);
                decimal entM = Math.Round(recCarteraM + invBanda + comisionesM + capSocialM + (cuponMod * factor), 2);
                decimal entS = Math.Round(recCarteraS + invBanda + comisionesS + capSocialS + (cuponSev * factor), 2);

                vm.EntradasBase.Add(entB);
                vm.EntradasModerado.Add(entM);
                vm.EntradasSevero.Add(entS);

                // 2. Salidas por banda
                decimal cdpBanda = (gap.VencimientosCDP != null && gap.VencimientosCDP.Count > i && gap.VencimientosCDP[i] > 0)
                    ? gap.VencimientosCDP[i]
                    : ((i == 0) ? (rc.CDPContractuales_Base / (1m - supuestos.RenovacionCDP_Base > 0 ? (1m - supuestos.RenovacionCDP_Base) : 1m)) : (rc.CDPContractuales_Base * factor));

                decimal cdpSalidaB = cdpBanda * (1m - supuestos.RenovacionCDP_Base);
                decimal cdpSalidaM = cdpBanda * (1m - supuestos.RenovacionCDP_Moderado);
                decimal cdpSalidaS = cdpBanda * (1m - supuestos.RenovacionCDP_Severo);

                decimal vistaSalidaB = rc.CorridaDepositosVista_Base * factor;
                decimal vistaSalidaM = rc.CorridaDepositosVista_Moderado * factor;
                decimal vistaSalidaS = rc.CorridaDepositosVista_Severo * factor;

                decimal top20SalidaB = rc.SalidaTop20_Base * factor;
                decimal top20SalidaM = rc.SalidaTop20_Moderado * factor;
                decimal top20SalidaS = rc.SalidaTop20_Severo * factor;

                decimal finAdmSalidaB = rc.EgresosFinAdm_Base * factor;
                decimal finAdmSalidaM = rc.EgresosFinAdm_Moderado * factor;
                decimal finAdmSalidaS = rc.EgresosFinAdm_Severo * factor;

                decimal pagoObligB = rc.PagoObligaciones_Base * factor;
                decimal pagoObligM = rc.PagoObligaciones_Moderado * factor;
                decimal pagoObligS = rc.PagoObligaciones_Severo * factor;

                decimal desembCartB = rc.DesembolsosCartera_Base * factor;
                decimal desembCartM = rc.DesembolsosCartera_Moderado * factor;
                decimal desembCartS = rc.DesembolsosCartera_Severo * factor;

                decimal salB = Math.Round(cdpSalidaB + vistaSalidaB + top20SalidaB + finAdmSalidaB + pagoObligB + desembCartB, 2);
                decimal salM = Math.Round(cdpSalidaM + vistaSalidaM + top20SalidaM + finAdmSalidaM + pagoObligM + desembCartM, 2);
                decimal salS = Math.Round(cdpSalidaS + vistaSalidaS + top20SalidaS + finAdmSalidaS + pagoObligS + desembCartS, 2);

                vm.SalidasBase.Add(salB);
                vm.SalidasModerado.Add(salM);
                vm.SalidasSevero.Add(salS);

                // 3. Brechas y Acumulado
                decimal gB = entB - salB;
                acumB += gB;
                vm.BrechasBase.Add(Math.Round(gB, 2));
                vm.AcumuladoBase.Add(Math.Round(acumB, 2));

                decimal gM = entM - salM;
                acumM += gM;
                vm.BrechasModerado.Add(Math.Round(gM, 2));
                vm.AcumuladoModerado.Add(Math.Round(acumM, 2));

                decimal gS = entS - salS;
                acumS += gS;
                vm.BrechasSevero.Add(Math.Round(gS, 2));
                vm.AcumuladoSevero.Add(Math.Round(acumS, 2));
            }

            vm.Gap30d_Base = vm.BrechasBase[0];
            vm.Gap90d_Base = vm.BrechasBase[2];

            vm.Gap30d_Moderado = vm.BrechasModerado[0];
            vm.Gap90d_Moderado = vm.BrechasModerado[2];

            vm.Gap30d_Severo = vm.BrechasSevero[0];
            vm.Gap90d_Severo = vm.BrechasSevero[2];

            return vm;
        }

        private HorizonteSupervivenciaViewModel CalcularHorizonteSupervivencia(BrechasLiquidezViewModel brechas, decimal falInicial)
        {
            var vm = new HorizonteSupervivenciaViewModel();
            int[] diasInicio = { 1, 30, 60, 90, 180, 270, 360 };
            int[] diasAncho = { 30, 30, 30, 90, 90, 90, 90 };

            // Escenario Base
            CalcularPuntoCruce(brechas.AcumuladoBase, falInicial, diasInicio, diasAncho,
                out string bandaB, out decimal bufferB, out decimal saldoB, out decimal diasB);
            vm.HorizonteBase = bandaB;
            vm.BufferRemanenteBase = bufferB;
            vm.SaldoFinalBase = saldoB;
            vm.DiasSupervivenciaBase = diasB;

            // Escenario Moderado
            CalcularPuntoCruce(brechas.AcumuladoModerado, brechas.FAL_Moderado, diasInicio, diasAncho,
                out string bandaM, out decimal bufferM, out decimal saldoM, out decimal diasM);
            vm.HorizonteModerado = bandaM;
            vm.BufferRemanenteModerado = bufferM;
            vm.SaldoFinalModerado = saldoM;
            vm.DiasSupervivenciaModerado = diasM;

            // Escenario Severo
            CalcularPuntoCruce(brechas.AcumuladoSevero, brechas.FAL_Severo, diasInicio, diasAncho,
                out string bandaS, out decimal bufferS, out decimal saldoS, out decimal diasS);
            vm.HorizonteSevero = bandaS;
            vm.BufferRemanenteSevero = bufferS;
            vm.SaldoFinalSevero = saldoS;
            vm.DiasSupervivenciaSevero = diasS;

            vm.ReduccionDias = diasS - diasM;

            // Trayectoria dinámica multiescenario para Highcharts
            vm.TrayectoriaCategorias = new List<string> { "Día 0", "Día 30", "Día 60", "Día 90", "Día 180", "Día 270", "Día 360" };
            vm.TrayectoriaBase = new List<decimal> { falInicial };
            vm.TrayectoriaBase.AddRange(brechas.AcumuladoBase.Take(6));

            vm.TrayectoriaModerado = new List<decimal> { brechas.FAL_Moderado };
            vm.TrayectoriaModerado.AddRange(brechas.AcumuladoModerado.Take(6));

            vm.TrayectoriaSevero = new List<decimal> { brechas.FAL_Severo };
            vm.TrayectoriaSevero.AddRange(brechas.AcumuladoSevero.Take(6));

            return vm;
        }

        private void CalcularPuntoCruce(List<decimal> acumulado, decimal fal, int[] diasInicio, int[] diasAncho,
            out string banda, out decimal buffer, out decimal saldoFinal, out decimal dias)
        {
            banda = "> 360 días";
            buffer = fal;
            saldoFinal = 0m;
            dias = 360m;

            for (int i = 0; i < acumulado.Count; i++)
            {
                if (acumulado[i] <= 0m)
                {
                    banda = (i == 0) ? "1 a 30 días" : (i == 1) ? "31 a 60 días" : (i == 2) ? "61 a 90 días" : (i == 3) ? "91 a 180 días" : (i == 4) ? "181 a 270 días" : (i == 5) ? "271 a 360 días" : "(+) 360 días";
                    buffer = (i == 0) ? fal : acumulado[i - 1];
                    saldoFinal = acumulado[i];
                    int start = diasInicio[i];
                    int width = diasAncho[i];
                    decimal denom = buffer - saldoFinal;
                    dias = denom > 0m ? Math.Round(start + (buffer / denom) * width, 0) : start;
                    return;
                }
            }
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
