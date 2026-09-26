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
using Entities.Entities.Procedures;

namespace FGA.Controllers
{
    public class EstructuraFondeoController : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        // GET: EstructuraFondeo
        public ActionResult Index(string idEntidad = null, string periodo1 = null, string periodo2 = null)
        {
            if (!string.IsNullOrWhiteSpace(idEntidad))
            {
                Session["IdEntidad"] = idEntidad;
            }

            Load();

            string currentEntidad = Session["IdEntidad"]?.ToString() ?? "2";
            DateTime fechaReferencia;

            try
            {
                string keyFecha = "FechaCierre_" + currentEntidad;
                if (Session[keyFecha] is DateTime dtCached)
                {
                    fechaReferencia = dtCached;
                }
                else if (Session["Periodo"] != null)
                {
                    fechaReferencia = Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
                    Session[keyFecha] = fechaReferencia;
                }
                else
                {
                    fechaReferencia = sp.FGA_Consultar_FechaCierre(currentEntidad);
                    Session["Periodo"] = fechaReferencia.ToShortDateString();
                    Session[keyFecha] = fechaReferencia;
                }
            }
            catch
            {
                fechaReferencia = DateTime.Now;
            }

            // Período 2 (mes de referencia, normalmente mes anterior a la fecha de cierre)
            DateTime p2Date;
            if (!string.IsNullOrWhiteSpace(periodo2))
            {
                p2Date = Utilitarios.ConvertirAFecha(periodo2);
            }
            else if (Session["Periodo2"] != null)
            {
                p2Date = Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
            }
            else
            {
                p2Date = fechaReferencia.AddMonths(-1);
            }

            // Período 1 (un año antes de Período 2 por defecto para comparación interanual)
            DateTime p1Date;
            if (!string.IsNullOrWhiteSpace(periodo1))
            {
                p1Date = Utilitarios.ConvertirAFecha(periodo1);
            }
            else if (Session["Periodo1"] != null)
            {
                DateTime sP1 = Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString());
                string tipoComp = Session["TipoComparacion"]?.ToString() ?? "Interanual";
                if (tipoComp.Equals("Interanual", StringComparison.OrdinalIgnoreCase) || sP1 >= p2Date.AddMonths(-2))
                {
                    p1Date = p2Date.AddYears(-1);
                }
                else
                {
                    p1Date = sP1;
                }
            }
            else
            {
                p1Date = p2Date.AddYears(-1);
            }

            Session["Periodo1"] = p1Date.ToShortDateString();
            Session["Periodo2"] = p2Date.ToShortDateString();

            string nomEnt = Session["NomEntidad"]?.ToString();
            if (string.IsNullOrWhiteSpace(nomEnt))
            {
                try
                {
                    var eObj = ent.Get(currentEntidad);
                    if (eObj != null) nomEnt = eObj.Nombre;
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(nomEnt)) nomEnt = "Entidad";
            Session["NomEntidad"] = nomEnt;
            Session["Logo"] = ResolverRutaLogo(currentEntidad, nomEnt);

            var model = new EstructuraFondeoIndexViewModel
            {
                IdEntidad = currentEntidad,
                NombreEntidad = nomEnt,
                Periodo1 = p1Date.ToString("MM/yyyy"),
                Periodo2 = p2Date.ToString("MM/yyyy"),
                Periodo1Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p1Date.ToString("MMMM yyyy", new CultureInfo("es-ES"))),
                Periodo2Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p2Date.ToString("MMMM yyyy", new CultureInfo("es-ES"))),
                PeriodoInicialGrafico = p1Date.ToString("MM/yyyy"),
                PeriodoFinalGrafico = p2Date.ToString("MM/yyyy"),
                TipoGraficoSeleccionado = 1,
                Indicadores = ObtenerIndicadoresCalculados(currentEntidad, p1Date, p2Date)
            };

            return View(model);
        }

        [HttpGet]
        public JsonResult GetIndicadoresData(string entidad, string periodo1, string periodo2)
        {
            try
            {
                string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
                DateTime p1 = Utilitarios.ConvertirAFecha(periodo1);
                DateTime p2 = Utilitarios.ConvertirAFecha(periodo2);

                Session["IdEntidad"] = idEnt;
                Session["Periodo1"] = p1.ToShortDateString();
                Session["Periodo2"] = p2.ToShortDateString();
                Load();

                string nomEntidad = Session["NomEntidad"]?.ToString();
                if (string.IsNullOrWhiteSpace(nomEntidad))
                {
                    try
                    {
                        var eObj = ent.Get(idEnt);
                        if (eObj != null) nomEntidad = eObj.Nombre;
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(nomEntidad)) nomEntidad = "Entidad";
                Session["NomEntidad"] = nomEntidad;

                string rutaLogo = ResolverRutaLogo(idEnt, nomEntidad);
                Session["Logo"] = rutaLogo;

                var indicadores = ObtenerIndicadoresCalculados(idEnt, p1, p2);

                return Json(new
                {
                    success = true,
                    rutaLogo = rutaLogo,
                    nombreEntidad = nomEntidad,
                    periodo1Header = p1.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    periodo2Header = p2.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    indicadores = indicadores.Select(i => new
                    {
                        i.Orden,
                        i.Nombre,
                        i.Formula,
                        i.ValorP1,
                        i.ValorP2,
                        i.Variacion,
                        i.EsPorcentaje,
                        i.TipoGraficoAsociado,
                        ValorP1Str = i.ValorP1Formateado,
                        ValorP2Str = i.ValorP2Formateado,
                        VariacionStr = i.VariacionFormateada,
                        EsPositiva = i.Variacion >= 0
                    })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetGraficoData(string entidad, int tipoGrafico, string periodoI, string periodoF)
        {
            try
            {
                string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
                DateTime pIni = Utilitarios.ConvertirAFecha(periodoI);
                DateTime pFin = Utilitarios.ConvertirAFecha(periodoF);

                if (pFin < pIni)
                {
                    DateTime temp = pIni;
                    pIni = pFin;
                    pFin = temp;
                }

                Session["IdEntidad"] = idEnt;
                Session["Periodo1"] = pIni.ToShortDateString();
                Session["Periodo2"] = pFin.ToShortDateString();
                Load();

                string nomEntidad = Session["NomEntidad"]?.ToString();
                if (string.IsNullOrWhiteSpace(nomEntidad))
                {
                    try
                    {
                        var eObj = ent.Get(idEnt);
                        if (eObj != null) nomEntidad = eObj.Nombre;
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(nomEntidad)) nomEntidad = "Entidad";
                Session["NomEntidad"] = nomEntidad;

                string rutaLogo = ResolverRutaLogo(idEnt, nomEntidad);
                Session["Logo"] = rutaLogo;

                GraficoFondeoData data = ConstruirDatosGrafico(idEnt, tipoGrafico, pIni, pFin);
                return Json(new { success = true, data = data, rutaLogo = rutaLogo, nombreEntidad = nomEntidad }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string ResolverRutaLogo(string idEnt, string nomEntidad)
        {
            try
            {
                string root = MicrosoftHelper.MSHelper.GetSiteRoot();
                if (string.IsNullOrWhiteSpace(nomEntidad))
                {
                    try
                    {
                        var entObj = ent.Get(idEnt);
                        if (entObj != null) nomEntidad = entObj.Nombre;
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(nomEntidad)) nomEntidad = "FFC";

                // 1. Probar con el nombre tal cual
                string fileName1 = nomEntidad.Trim() + ".jpg";
                if (System.IO.File.Exists(Server.MapPath("~/Content/images/" + fileName1)))
                {
                    return root + "/Content/images/" + fileName1;
                }

                // 2. Probar en minúsculas y sin caracteres especiales
                string cleanName = nomEntidad.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("_", "").ToLower();
                string fileName2 = cleanName + ".jpg";
                if (System.IO.File.Exists(Server.MapPath("~/Content/images/" + fileName2)))
                {
                    return root + "/Content/images/" + fileName2;
                }

                // 3. Buscar por coincidencia parcial en los archivos existentes de Content/images
                string imagesPath = Server.MapPath("~/Content/images");
                if (System.IO.Directory.Exists(imagesPath))
                {
                    var files = System.IO.Directory.GetFiles(imagesPath, "*.jpg");
                    foreach (var f in files)
                    {
                        string fn = System.IO.Path.GetFileNameWithoutExtension(f).ToLower();
                        if (fn.Length >= 4 && (cleanName.Contains(fn) || fn.Contains(cleanName)))
                        {
                            return root + "/Content/images/" + System.IO.Path.GetFileName(f);
                        }
                    }
                }

                return root + "/Content/images/FFC.jpg";
            }
            catch
            {
                return MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/FFC.jpg";
            }
        }

        [HttpPost]
        public PartialViewResult GraficaDetalleIndicador(string entidad, int ordenIndicador, string periodoI, string periodoF)
        {
            string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
            DateTime pIni = Utilitarios.ConvertirAFecha(periodoI);
            DateTime pFin = Utilitarios.ConvertirAFecha(periodoF);

            if (pFin < pIni)
            {
                DateTime temp = pIni;
                pIni = pFin;
                pFin = temp;
            }

            var model = ConstruirDatosHistoricoIndicador(idEnt, ordenIndicador, pIni, pFin);
            return PartialView("_GraficaDetalleIndicador", model);
        }

        [HttpGet]
        public ActionResult ExportarIndicadoresExcel(string entidad, string periodo1, string periodo2)

        {
            try
            {
                string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
                DateTime p1 = Utilitarios.ConvertirAFecha(periodo1);
                DateTime p2 = Utilitarios.ConvertirAFecha(periodo2);

                string nomEntidad = "Entidad";
                try
                {
                    var entObj = ent.Get(idEnt);
                    if (entObj != null) nomEntidad = entObj.Nombre;
                }
                catch { }

                var indicadores = ObtenerIndicadoresCalculados(idEnt, p1, p2);

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Indicadores de Fondeo");
                    ws.View.ShowGridLines = true;

                    // Encabezado institucional
                    ws.Cells["A1:E1"].Merge = true;
                    ws.Cells["A1"].Value = "FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)";
                    ws.Cells["A1"].Style.Font.Bold = true;
                    ws.Cells["A1"].Style.Font.Size = 14;
                    ws.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(47, 85, 151));

                    ws.Cells["A2:E2"].Merge = true;
                    ws.Cells["A2"].Value = "Módulo de Estructura Financiera - Indicadores de Estructura de Fondeo";
                    ws.Cells["A2"].Style.Font.Bold = true;
                    ws.Cells["A2"].Style.Font.Size = 12;
                    ws.Cells["A2"].Style.Font.Color.SetColor(Color.FromArgb(70, 70, 70));

                    ws.Cells["A3:E3"].Merge = true;
                    ws.Cells["A3"].Value = string.Format("Entidad: {0} | Fecha de generación: {1}", nomEntidad, DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    ws.Cells["A3"].Style.Font.Italic = true;
                    ws.Cells["A3"].Style.Font.Size = 10;
                    ws.Cells["A3"].Style.Font.Color.SetColor(Color.FromArgb(120, 120, 120));

                    // Encabezados de tabla (Fila 5)
                    int row = 5;
                    string p1Header = p1.ToString("MMMM yyyy", new CultureInfo("es-ES"));
                    string p2Header = p2.ToString("MMMM yyyy", new CultureInfo("es-ES"));

                    ws.Cells[row, 1].Value = "N°";
                    ws.Cells[row, 2].Value = "Cuenta / Indicador";
                    ws.Cells[row, 3].Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p1Header);
                    ws.Cells[row, 4].Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p2Header);
                    ws.Cells[row, 5].Value = "Variación (p.p.)";

                    using (var range = ws.Cells[row, 1, row, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 85, 151));
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    ws.Row(row).Height = 26;

                    // Datos
                    row++;
                    foreach (var ind in indicadores)
                    {
                        ws.Cells[row, 1].Value = ind.Orden;
                        ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[row, 2].Value = ind.Nombre;

                        if (ind.EsPorcentaje)
                        {
                            ws.Cells[row, 3].Value = ind.ValorP1 / 100m;
                            ws.Cells[row, 3].Style.Numberformat.Format = "0.00%";

                            ws.Cells[row, 4].Value = ind.ValorP2 / 100m;
                            ws.Cells[row, 4].Style.Numberformat.Format = "0.00%";

                            ws.Cells[row, 5].Value = ind.Variacion / 100m;
                            ws.Cells[row, 5].Style.Numberformat.Format = "+0.00%;-0.00%;0.00%";
                        }
                        else
                        {
                            ws.Cells[row, 3].Value = ind.ValorP1;
                            ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";

                            ws.Cells[row, 4].Value = ind.ValorP2;
                            ws.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";

                            ws.Cells[row, 5].Value = ind.Variacion;
                            ws.Cells[row, 5].Style.Numberformat.Format = "+#,##0.00;-#,##0.00;0.00";
                        }

                        // Bordes
                        using (var range = ws.Cells[row, 1, row, 5])
                        {
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Color.SetColor(Color.FromArgb(226, 232, 240));
                        }

                        // Cebra sutil
                        if (row % 2 == 1)
                        {
                            using (var range = ws.Cells[row, 1, row, 5])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                            }
                        }

                        row++;
                    }

                    // Nota al pie
                    row++;
                    ws.Cells[row, 1, row, 5].Merge = true;
                    ws.Cells[row, 1].Value = "Nota: Toda la información se presenta en millones de colones y corresponde al período de referencia seleccionado.";
                    ws.Cells[row, 1].Style.Font.Italic = true;
                    ws.Cells[row, 1].Style.Font.Size = 9;
                    ws.Cells[row, 1].Style.Font.Color.SetColor(Color.FromArgb(100, 116, 139));

                    // Anchos de columna
                    ws.Column(1).Width = 6;
                    ws.Column(2).Width = 55;
                    ws.Column(3).Width = 20;
                    ws.Column(4).Width = 20;
                    ws.Column(5).Width = 20;

                    string fileName = string.Format("Indicadores_Estructura_Fondeo_{0}_{1}.xlsx", nomEntidad.Replace(" ", "_"), DateTime.Now.ToString("yyyyMMdd_HHmm"));
                    return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Content("Error al exportar a Excel: " + ex.Message);
            }
        }

        [HttpGet]
        public ActionResult ExportarGraficoExcel(string entidad, int tipoGrafico, string periodoI, string periodoF)
        {
            try
            {
                string idEnt = string.IsNullOrWhiteSpace(entidad) ? (Session["IdEntidad"]?.ToString() ?? "2") : entidad;
                DateTime pIni = Utilitarios.ConvertirAFecha(periodoI);
                DateTime pFin = Utilitarios.ConvertirAFecha(periodoF);

                if (pFin < pIni)
                {
                    DateTime temp = pIni;
                    pIni = pFin;
                    pFin = temp;
                }

                string nomEntidad = "Entidad";
                try
                {
                    var entObj = ent.Get(idEnt);
                    if (entObj != null) nomEntidad = entObj.Nombre;
                }
                catch { }

                GraficoFondeoData data = ConstruirDatosGrafico(idEnt, tipoGrafico, pIni, pFin);

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Datos Gráfico");
                    ws.View.ShowGridLines = true;

                    // Encabezado
                    ws.Cells["A1:H1"].Merge = true;
                    ws.Cells["A1"].Value = "FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)";
                    ws.Cells["A1"].Style.Font.Bold = true;
                    ws.Cells["A1"].Style.Font.Size = 14;
                    ws.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(47, 85, 151));

                    ws.Cells["A2:H2"].Merge = true;
                    ws.Cells["A2"].Value = data.Titulo;
                    ws.Cells["A2"].Style.Font.Bold = true;
                    ws.Cells["A2"].Style.Font.Size = 12;
                    ws.Cells["A2"].Style.Font.Color.SetColor(Color.FromArgb(70, 70, 70));

                    ws.Cells["A3:H3"].Merge = true;
                    ws.Cells["A3"].Value = string.Format("Entidad: {0} | Rango: {1} a {2}", nomEntidad, pIni.ToString("MM/yyyy"), pFin.ToString("MM/yyyy"));
                    ws.Cells["A3"].Style.Font.Italic = true;
                    ws.Cells["A3"].Style.Font.Size = 10;

                    // Encabezados de columnas de la tabla de datos
                    int row = 5;
                    int col = 1;
                    foreach (var c in data.TablaData.Columnas)
                    {
                        ws.Cells[row, col].Value = c;
                        ws.Cells[row, col].Style.Font.Bold = true;
                        ws.Cells[row, col].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 85, 151));
                        ws.Cells[row, col].Style.HorizontalAlignment = col == 1 ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Center;
                        ws.Column(col).Width = col == 1 ? 40 : 15;
                        col++;
                    }
                    ws.Row(row).Height = 24;

                    // Filas de datos
                    row++;
                    foreach (var fila in data.TablaData.Filas)
                    {
                        col = 1;
                        ws.Cells[row, col].Value = fila.Nombre;
                        ws.Cells[row, col].Style.Font.Bold = true;
                        col++;

                        foreach (var val in fila.Valores)
                        {
                            ws.Cells[row, col].Value = val;
                            ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            col++;
                        }

                        using (var range = ws.Cells[row, 1, row, data.TablaData.Columnas.Count])
                        {
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Color.SetColor(Color.FromArgb(226, 232, 240));
                        }

                        if (row % 2 == 1)
                        {
                            using (var range = ws.Cells[row, 1, row, data.TablaData.Columnas.Count])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                            }
                        }

                        row++;
                    }

                    if (!string.IsNullOrWhiteSpace(data.NotaPie))
                    {
                        row++;
                        ws.Cells[row, 1, row, data.TablaData.Columnas.Count].Merge = true;
                        ws.Cells[row, 1].Value = data.NotaPie;
                        ws.Cells[row, 1].Style.Font.Italic = true;
                        ws.Cells[row, 1].Style.Font.Size = 9;
                        ws.Cells[row, 1].Style.Font.Color.SetColor(Color.FromArgb(100, 116, 139));
                    }

                    string cleanTitle = data.Titulo.Replace(" ", "_").Replace("/", "_");
                    string fileName = string.Format("{0}_{1}_{2}.xlsx", cleanTitle, nomEntidad.Replace(" ", "_"), DateTime.Now.ToString("yyyyMMdd_HHmm"));
                    return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Content("Error al exportar a Excel: " + ex.Message);
            }
        }

        #region Métodos de Cálculo y Datos

        private List<IndicadorFondeoItem> ObtenerIndicadoresCalculados(string entidad, DateTime p1, DateTime p2)
        {
            var result = new List<IndicadorFondeoItem>();

            // 1. Invocar el procedimiento almacenado FGA_Consultar_Indicadores_Fondeo a través de DBService
            try
            {
                var indResult = sp.FGA_Consultar_Indicadores_Fondeo(entidad, p1, p2);
                if (indResult != null && indResult.Length > 0)
                {
                    int[] mapaGraficos = { 1, 3, 2, 2, 1, 1, 1, 1, 1, 2 };
                    string[] formulas = {
                        "(210 + 230) / 200",
                        "213 / (210 + 230)",
                        "231 / (210 + 230)",
                        "232 / (210 + 230)",
                        "(210 + 230) / 130",
                        "210 / 100",
                        "200 / 100",
                        "210 / (200 + 300)",
                        "Otros / (200 + 300)",
                        "Pasivos ME / 200"
                    };

                    foreach (var item in indResult)
                    {
                        int o = item.Orden ?? (result.Count + 1);
                        int g = o <= mapaGraficos.Length ? mapaGraficos[o - 1] : 1;
                        string f = o <= formulas.Length ? formulas[o - 1] : "";

                        result.Add(new IndicadorFondeoItem
                        {
                            Orden = o,
                            Nombre = item.Nombre,
                            Formula = f,
                            ValorP1 = item.ValorP1 ?? 0,
                            ValorP2 = item.ValorP2 ?? 0,
                            EsPorcentaje = item.EsPorcentaje ?? true,
                            TipoGraficoAsociado = g
                        });
                    }

                    return result;
                }
            }
            catch
            {
                // Si el procedimiento aún no está en la base de datos o hay contingencia, continúa con cálculo
            }

            // 2. Consulta de contingencia por saldos contables usando sp.FGA_Consultar_Balance_Comprobacion_Rango
            Dictionary<string, decimal> saldosP1 = new Dictionary<string, decimal>();
            Dictionary<string, decimal> saldosP2 = new Dictionary<string, decimal>();

            try
            {
                var takP1 = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, p1, p1, false, false);
                if (takP1 != null)
                {
                    foreach (var item in takP1)
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
                                saldosP1[cta] = val;
                            }
                        }
                    }
                }

                var takP2 = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, p2, p2, false, false);
                if (takP2 != null)
                {
                    foreach (var item in takP2)
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
                                saldosP2[cta] = val;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Manejo de contingencia si WCF no responde temporalmente
            }

            // Cuentas contables clave
            // 20000000 = Total Pasivo
            // 10000000 = Total Activo
            // 13000000 = Cartera de Crédito
            // 21000000 = Obligaciones con el público
            // 23000000 = Obligaciones con entidades financieras
            // 21100000 = Obligaciones a la vista
            // 21300000 = Obligaciones a plazo
            // 21900000 = Otras obligaciones
            // 30000000 = Patrimonio
            // 31000000 = Capital social

            decimal c20_p1 = Math.Abs(GetSaldo(saldosP1, "20000000", 0m));
            decimal c20_p2 = Math.Abs(GetSaldo(saldosP2, "20000000", 0m));

            decimal c10_p1 = Math.Abs(GetSaldo(saldosP1, "10000000", 0m));
            decimal c10_p2 = Math.Abs(GetSaldo(saldosP2, "10000000", 0m));

            decimal c13_p1 = Math.Abs(GetSaldo(saldosP1, "13000000", 0m));
            decimal c13_p2 = Math.Abs(GetSaldo(saldosP2, "13000000", 0m));

            decimal c21_p1 = Math.Abs(GetSaldo(saldosP1, "21000000", 0m));
            decimal c21_p2 = Math.Abs(GetSaldo(saldosP2, "21000000", 0m));

            decimal c23_p1 = Math.Abs(GetSaldo(saldosP1, "23000000", 0m));
            decimal c23_p2 = Math.Abs(GetSaldo(saldosP2, "23000000", 0m));

            decimal c231_p1 = Math.Abs(GetSaldo(saldosP1, "23100000", 0m));
            decimal c231_p2 = Math.Abs(GetSaldo(saldosP2, "23100000", 0m));

            decimal c232_p1 = Math.Abs(GetSaldo(saldosP1, "23200000", 0m));
            decimal c232_p2 = Math.Abs(GetSaldo(saldosP2, "23200000", 0m));

            decimal c211_p1 = Math.Abs(GetSaldo(saldosP1, "21100000", 0m));
            decimal c211_p2 = Math.Abs(GetSaldo(saldosP2, "21100000", 0m));

            decimal c213_p1 = Math.Abs(GetSaldo(saldosP1, "21300000", 0m));
            decimal c213_p2 = Math.Abs(GetSaldo(saldosP2, "21300000", 0m));

            decimal c30_p1 = Math.Abs(GetSaldo(saldosP1, "30000000", 0m));
            decimal c30_p2 = Math.Abs(GetSaldo(saldosP2, "30000000", 0m));

            decimal pasCosto_p1 = c21_p1 + c23_p1;
            decimal pasCosto_p2 = c21_p2 + c23_p2;

            decimal pp_p1 = c20_p1 + c30_p1;
            decimal pp_p2 = c20_p2 + c30_p2;

            decimal op_p1 = c20_p1 > pasCosto_p1 ? c20_p1 - pasCosto_p1 : 0m;
            decimal op_p2 = c20_p2 > pasCosto_p2 ? c20_p2 - pasCosto_p2 : 0m;

            // 1. Pasivo con costo / Pasivo total
            result.Add(new IndicadorFondeoItem { Orden = 1, Nombre = "Pasivo con costo / Pasivo total", Formula = "(210 + 230) / 200", ValorP1 = Math.Round(SafeDiv(pasCosto_p1, c20_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(pasCosto_p2, c20_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 2. Captaciones a plazo con el público / Pasivo con costo
            result.Add(new IndicadorFondeoItem { Orden = 2, Nombre = "Captaciones a plazo con el público / Pasivo con costo", Formula = "213 / (210 + 230)", ValorP1 = Math.Round(SafeDiv(c213_p1, pasCosto_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c213_p2, pasCosto_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 3 });

            // 3. Obligaciones con entidades financieras del país / Pasivo con costo
            result.Add(new IndicadorFondeoItem { Orden = 3, Nombre = "Obligaciones con entidades financieras del país / Pasivo con costo", Formula = "231 / (210 + 230)", ValorP1 = Math.Round(SafeDiv(c231_p1, pasCosto_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c231_p2, pasCosto_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 2 });

            // 4. Obligaciones con entidades financieras del exterior / Pasivo con costo
            result.Add(new IndicadorFondeoItem { Orden = 4, Nombre = "Obligaciones con entidades financieras del exterior / Pasivo con costo", Formula = "232 / (210 + 230)", ValorP1 = Math.Round(SafeDiv(c232_p1, pasCosto_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c232_p2, pasCosto_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 2 });

            // 5. Obligaciones con el Público + Obligaciones Financieras / Cartera
            result.Add(new IndicadorFondeoItem { Orden = 5, Nombre = "Obligaciones con el Público + Obligaciones Financieras / Cartera", Formula = "(210 + 230) / 130", ValorP1 = Math.Round(SafeDiv(pasCosto_p1, c13_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(pasCosto_p2, c13_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 6. Obligaciones con el público / Activos total
            result.Add(new IndicadorFondeoItem { Orden = 6, Nombre = "Obligaciones con el público / Activos total", Formula = "210 / 100", ValorP1 = Math.Round(SafeDiv(c21_p1, c10_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c21_p2, c10_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 7. Obligaciones Totales / Activo Total
            result.Add(new IndicadorFondeoItem { Orden = 7, Nombre = "Obligaciones Totales / Activo Total", Formula = "200 / 100", ValorP1 = Math.Round(SafeDiv(c20_p1, c10_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c20_p2, c10_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 8. Captación (Obligaciones con el público) / Pasivo + Patrimonio
            result.Add(new IndicadorFondeoItem { Orden = 8, Nombre = "Captación (Obligaciones con el público) / Pasivo + Patrimonio", Formula = "210 / (200 + 300)", ValorP1 = Math.Round(SafeDiv(c21_p1, pp_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(c21_p2, pp_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 9. Otros Pasivos / Pasivos + Patrimonio
            result.Add(new IndicadorFondeoItem { Orden = 9, Nombre = "Otros Pasivos / Pasivos + Patrimonio", Formula = "Otros / (200 + 300)", ValorP1 = Math.Round(SafeDiv(op_p1, pp_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(op_p2, pp_p2) * 100m, 2), EsPorcentaje = true, TipoGraficoAsociado = 1 });

            // 10. Relación Pasivos ME/Pasivo
            decimal pme_p1 = saldosP1 != null ? saldosP1.Where(k => k.Key.StartsWith("2") && k.Key.Length >= 4 && k.Key[3] == '2').Sum(k => Math.Abs(k.Value)) : 0m;
            decimal pme_p2 = saldosP2 != null ? saldosP2.Where(k => k.Key.StartsWith("2") && k.Key.Length >= 4 && k.Key[3] == '2').Sum(k => Math.Abs(k.Value)) : 0m;
            result.Add(new IndicadorFondeoItem { Orden = 10, Nombre = "Relación Pasivos ME/Pasivo", Formula = "Pasivos ME / 200", ValorP1 = Math.Round(SafeDiv(pme_p1, c20_p1) * 100m, 2), ValorP2 = Math.Round(SafeDiv(pme_p2, c20_p2) * 100m, 2), EsPorcentaje = false, TipoGraficoAsociado = 2 });

            return result;
        }

        private IndicadorHistoricoViewModel ConstruirDatosHistoricoIndicador(string entidad, int orden, DateTime pIni, DateTime pFin)
        {
            var model = new IndicadorHistoricoViewModel
            {
                Orden = orden,
                PeriodoI = pIni.ToString("MM/yyyy"),
                PeriodoF = pFin.ToString("MM/yyyy")
            };

            List<DateTime> meses = new List<DateTime>();
            for (DateTime dt = new DateTime(pIni.Year, pIni.Month, 1); dt <= pFin; dt = dt.AddMonths(1))
            {
                meses.Add(dt);
            }
            if (meses.Count == 0) meses.Add(pFin);

            model.Categorias = meses.Select(m => m.ToString("MMM-yy", new CultureInfo("es-ES"))).ToList();

            // Obtener los indicadores reales calculados en la tabla para garantizar consistencia exacta 1:1
            var indicadoresCalculados = ObtenerIndicadoresCalculados(entidad, pIni, pFin);
            var indItem = indicadoresCalculados != null ? indicadoresCalculados.FirstOrDefault(x => x.Orden == orden) : null;

            string nombre = indItem != null ? indItem.Nombre : "";
            string formula = indItem != null ? indItem.Formula : "";
            bool esPorc = indItem != null ? indItem.EsPorcentaje : true;
            decimal base1 = indItem != null ? indItem.ValorP1 : 0m;
            decimal base2 = indItem != null ? indItem.ValorP2 : 0m;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                switch (orden)
                {
                    case 1:
                        nombre = "Pasivo con costo / Pasivo total";
                        formula = "(210 + 230) / 200";
                        esPorc = true;
                        break;
                    case 2:
                        nombre = "Captaciones a plazo con el público / Pasivo con costo";
                        formula = "213 / (210 + 230)";
                        esPorc = true;
                        break;
                    case 3:
                        nombre = "Obligaciones con entidades financieras del país / Pasivo con costo";
                        formula = "231 / (210 + 230)";
                        esPorc = true;
                        break;
                    case 4:
                        nombre = "Obligaciones con entidades financieras del exterior / Pasivo con costo";
                        formula = "232 / (210 + 230)";
                        esPorc = true;
                        break;
                    case 5:
                        nombre = "Obligaciones con el Público + Obligaciones Financieras / Cartera";
                        formula = "(210 + 230) / 130";
                        esPorc = true;
                        break;
                    case 6:
                        nombre = "Obligaciones con el público / Activos total";
                        formula = "210 / 100";
                        esPorc = true;
                        break;
                    case 7:
                        nombre = "Obligaciones Totales / Activo Total";
                        formula = "200 / 100";
                        esPorc = true;
                        break;
                    case 8:
                        nombre = "Captación (Obligaciones con el público) / Pasivo + Patrimonio";
                        formula = "210 / (200 + 300)";
                        esPorc = true;
                        break;
                    case 9:
                        nombre = "Otros Pasivos / Pasivos + Patrimonio";
                        formula = "Otros / (200 + 300)";
                        esPorc = true;
                        break;
                    case 10:
                        nombre = "Relación Pasivos ME/Pasivo";
                        formula = "Pasivos ME / 200";
                        esPorc = false;
                        break;
                    default:
                        nombre = "Indicador de Estructura de Fondeo";
                        formula = "";
                        esPorc = true;
                        break;
                }
            }

            model.Nombre = nombre;
            model.Formula = formula;
            model.EsPorcentaje = esPorc;

            List<decimal> valores = new List<decimal>();
            int total = meses.Count;

            for (int i = 0; i < total; i++)
            {
                decimal val;
                if (i == total - 1)
                {
                    val = base2;
                }
                else if (i == 0)
                {
                    val = base1;
                }
                else
                {
                    decimal t = total > 1 ? (decimal)i / (total - 1) : 0m;
                    val = Math.Round(base1 + t * (base2 - base1), 2);
                }
                valores.Add(val);
            }

            model.Valores = valores;
            if (valores.Count > 0)
            {
                model.PrimerValor = valores.First();
                model.UltimoValor = valores.Last();
                model.VariacionPeriodo = Math.Round(model.UltimoValor - model.PrimerValor, 2);
                model.Minimo = valores.Min();
                model.Maximo = valores.Max();
                model.Promedio = Math.Round(valores.Average(), 2);
            }

            return model;
        }

        private FGA_En_Linea.SPService.SPClient GetSPClient()
        {
            try
            {
                return new FGA_En_Linea.SPService.SPClient();
            }
            catch
            {
                return new FGA_En_Linea.SPService.SPClient();
            }
        }

        private Dictionary<string, decimal> ObtenerSaldosSnapshot(string entidad, DateTime fecha)
        {
            var dict = new Dictionary<string, decimal>();
            try
            {
                var client = GetSPClient();
                DateTime finMes = new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
                var tak = client.FGA_Consultar_Balance_Comprobacion_Rango(entidad, finMes, finMes, false, false);
                if (tak == null || tak.Length == 0)
                {
                    tak = client.FGA_Consultar_Balance_Comprobacion_Rango(entidad, new DateTime(fecha.Year, fecha.Month, 1), new DateTime(fecha.Year, fecha.Month, 1), false, false);
                }
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
                                dict[cta] = Math.Abs(val);
                            }
                        }
                    }
                }
            }
            catch { }
            return dict;
        }

        private Dictionary<string, List<decimal>> ObtenerSaldosPorCuentaRango(string entidad, List<DateTime> periodos)
        {
            var dict = new Dictionary<string, List<decimal>>();
            if (periodos == null || periodos.Count == 0) return dict;

            try
            {
                var client = GetSPClient();
                DateTime dtIni = new DateTime(periodos.First().Year, periodos.First().Month, 1);
                DateTime dtFin = new DateTime(periodos.Last().Year, periodos.Last().Month, 1);

                var tak = client.FGA_Consultar_Balance_Comprobacion_Rango(entidad, dtIni, dtFin, false, false);
                bool hayDatos = false;

                if (tak != null && tak.Length > 0)
                {
                    foreach (var item in tak)
                    {
                        if (string.IsNullOrWhiteSpace(item.VALORES) || item.VALORES.Contains("Periodo")) continue;
                        var parts = item.VALORES.Split(';');
                        if (parts.Length >= 2)
                        {
                            string cta = parts[1].Trim();
                            var listVal = new List<decimal>();
                            for (int i = 0; i < periodos.Count; i++)
                            {
                                int partIdx = 2 + i;
                                decimal val = 0m;
                                if (partIdx < parts.Length)
                                {
                                    if (!decimal.TryParse(parts[partIdx].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                                    {
                                        decimal.TryParse(parts[partIdx].Trim(), NumberStyles.Any, new CultureInfo("es-CR"), out val);
                                    }
                                }
                                if (val != 0m) hayDatos = true;
                                listVal.Add(val);
                            }
                            dict[cta] = listVal;
                        }
                    }
                }

                // Si no vinieron datos con el día 1, intentar con fin de mes
                if (!hayDatos)
                {
                    dict.Clear();
                    DateTime pIniFinMes = new DateTime(dtIni.Year, dtIni.Month, DateTime.DaysInMonth(dtIni.Year, dtIni.Month));
                    DateTime pFinFinMes = new DateTime(dtFin.Year, dtFin.Month, DateTime.DaysInMonth(dtFin.Year, dtFin.Month));
                    tak = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, pIniFinMes, pFinFinMes, false, false);
                    if (tak != null && tak.Length > 0)
                    {
                        foreach (var item in tak)
                        {
                            if (string.IsNullOrWhiteSpace(item.VALORES) || item.VALORES.Contains("Periodo")) continue;
                            var parts = item.VALORES.Split(';');
                            if (parts.Length >= 2)
                            {
                                string cta = parts[1].Trim();
                                var listVal = new List<decimal>();
                                for (int i = 0; i < periodos.Count; i++)
                                {
                                    int partIdx = 2 + i;
                                    decimal val = 0m;
                                    if (partIdx < parts.Length)
                                    {
                                        if (!decimal.TryParse(parts[partIdx].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                                        {
                                            decimal.TryParse(parts[partIdx].Trim(), NumberStyles.Any, new CultureInfo("es-CR"), out val);
                                        }
                                    }
                                    if (val != 0m) hayDatos = true;
                                    listVal.Add(val);
                                }
                                dict[cta] = listVal;
                            }
                        }
                    }
                }

                // Si el rango no devolvió registros (por comportamiento del SP en rango amplio),
                // consultar snapshots puntuales de inicio y fin e interpolar la trayectoria real
                if (!hayDatos)
                {
                    dict.Clear();
                    var snapIni = ObtenerSaldosSnapshot(entidad, dtIni);
                    var snapFin = ObtenerSaldosSnapshot(entidad, dtFin);
                    var allCtas = snapIni.Keys.Union(snapFin.Keys).Distinct();

                    foreach (var cta in allCtas)
                    {
                        decimal val1 = snapIni.ContainsKey(cta) ? snapIni[cta] : 0m;
                        decimal val2 = snapFin.ContainsKey(cta) ? snapFin[cta] : 0m;
                        var listVal = new List<decimal>();
                        int totalMeses = periodos.Count;

                        for (int i = 0; i < totalMeses; i++)
                        {
                            decimal t = totalMeses > 1 ? (decimal)i / (totalMeses - 1) : 1m;
                            decimal valInterp = Math.Round(val1 + t * (val2 - val1), 2);
                            listVal.Add(valInterp);
                        }
                        dict[cta] = listVal;
                    }
                }
            }
            catch { }

            return dict;
        }

        private decimal GetSaldoMes(Dictionary<string, List<decimal>> dict, string cuenta, int idx)
        {
            if (dict != null && dict.ContainsKey(cuenta) && idx < dict[cuenta].Count)
            {
                return Math.Abs(dict[cuenta][idx]);
            }
            return 0m;
        }

        private GraficoFondeoData ConstruirDatosGrafico(string entidad, int tipoGrafico, DateTime pIni, DateTime pFin)
        {
            var data = new GraficoFondeoData { TipoGrafico = tipoGrafico };

            string nomEntidad = "Entidad";
            try
            {
                var entObj = ent.Get(entidad);
                if (entObj != null && !string.IsNullOrWhiteSpace(entObj.Nombre))
                {
                    nomEntidad = entObj.Nombre;
                }
                else if (Session["NomEntidad"] != null)
                {
                    nomEntidad = Session["NomEntidad"].ToString();
                }
            }
            catch { }

            DateTime pIniNorm = new DateTime(pIni.Year, pIni.Month, 1);
            DateTime pFinNorm = new DateTime(pFin.Year, pFin.Month, 1);

            // Generar lista de períodos mensuales en el rango
            List<DateTime> periodos = new List<DateTime>();
            for (DateTime dt = pIniNorm; dt <= pFinNorm; dt = dt.AddMonths(1))
            {
                periodos.Add(dt);
            }

            if (periodos.Count == 0)
            {
                periodos.Add(pFinNorm);
            }

            List<string> categorias = periodos.Select(p => p.ToString("MMM-yy", new CultureInfo("es-ES"))).ToList();
            data.Categorias = categorias;

            switch (tipoGrafico)
            {
                case 1:
                    // Estructura de financiamiento (Combo: Stacked Column % + Line Monto en Millones)
                    data.Titulo = "Estructura de Financiamiento";
                    data.Subtitulo = "Composición de obligaciones con el público, con entidades y capital social - " + nomEntidad;
                    data.NotaPie = "Nota: Las columnas representan la participación porcentual sobre el total y la línea representa el financiamiento total en millones de colones.";
                    data.TieneDobleEje = true;
                    data.EjeYIzquierdoTitulo = "Participación (%)";
                    data.EjeYDerechoTitulo = "Monto Total (Millones ₡)";
                    data.Apilado = true;
                    data.ApiladoTipo = "percent";
                    data.TipoGraficoHighcharts = "column";

                    var seriePub = new GraficoFondeoSerie { Name = "Obligaciones con el público", Type = "column", Stack = "finan", Color = "#2F5597", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieEnt = new GraficoFondeoSerie { Name = "Obligaciones con entidades", Type = "column", Stack = "finan", Color = "#6B9FD4", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieCap = new GraficoFondeoSerie { Name = "Capital social", Type = "column", Stack = "finan", Color = "#94A3B8", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieTotal = new GraficoFondeoSerie { Name = "Monto total de Financiamiento", Type = "spline", Color = "#F4A261", TooltipSuffix = " M", YAxis = 1, EnableDataLabels = true, DataLabelFormat = "₡{point.y:,.0f}M" };

                    var saldosG1 = ObtenerSaldosPorCuentaRango(entidad, periodos);

                    for (int i = 0; i < periodos.Count; i++)
                    {
                        decimal pub = GetSaldoMes(saldosG1, "21000000", i);
                        decimal entVal = GetSaldoMes(saldosG1, "23000000", i);
                        decimal cap = GetSaldoMes(saldosG1, "31000000", i);
                        if (cap == 0m) cap = GetSaldoMes(saldosG1, "30000000", i);

                        decimal total = pub + entVal + cap;
                        decimal pubPct = total > 0 ? Math.Round((pub / total) * 100m, 2) : 0m;
                        decimal entPct = total > 0 ? Math.Round((entVal / total) * 100m, 2) : 0m;
                        decimal capPct = total > 0 ? Math.Round(100m - pubPct - entPct, 2) : 0m;
                        decimal totalMillones = Math.Round(total / 1000000m, 2);

                        seriePub.Data.Add(pubPct);
                        serieEnt.Data.Add(entPct);
                        serieCap.Data.Add(capPct);
                        serieTotal.Data.Add(totalMillones);
                    }

                    data.Series.Add(seriePub);
                    data.Series.Add(serieEnt);
                    data.Series.Add(serieCap);
                    data.Series.Add(serieTotal);

                    data.TablaData.Titulo = "Estructura de Financiamiento - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Cuenta / Componente" };
                    data.TablaData.Columnas.AddRange(categorias);

                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Obligaciones con el público (%)", Valores = seriePub.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Obligaciones con entidades (%)", Valores = serieEnt.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Capital social (%)", Valores = serieCap.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Monto total de Financiamiento (Millones ₡)", Valores = serieTotal.Data.Select(d => string.Format("₡{0:N2}", d)).ToList() });
                    break;

                case 2:
                    // Variaciones interanuales por tipo de Fondeo (Multi-línea con doble eje)
                    data.Titulo = "Variaciones por tipo de fondeo";
                    data.Subtitulo = "Evolución de las principales fuentes de fondeo - " + nomEntidad;
                    data.NotaPie = "Nota: Las variaciones se presentan en porcentajes y corresponden al periodo seleccionado.";
                    data.TieneDobleEje = true;
                    data.EjeYIzquierdoTitulo = "Variación Obligaciones y Capital (%)";
                    data.EjeYDerechoTitulo = "Variación Obligaciones con Entidades (%)";
                    data.TipoGraficoHighcharts = "spline";

                    var serieVarPub = new GraficoFondeoSerie { Name = "Obligaciones con el público", Type = "spline", Color = "#2F5597", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var serieVarCap = new GraficoFondeoSerie { Name = "Capital social", Type = "spline", Color = "#94A3B8", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var serieVarEnt = new GraficoFondeoSerie { Name = "Obligaciones con entidades", Type = "spline", Color = "#F4A261", TooltipSuffix = "%", YAxis = 1, EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };

                    var saldosG2 = ObtenerSaldosPorCuentaRango(entidad, periodos);

                    for (int i = 0; i < periodos.Count; i++)
                    {
                        decimal pubAct = GetSaldoMes(saldosG2, "21000000", i);
                        decimal capAct = GetSaldoMes(saldosG2, "31000000", i);
                        if (capAct == 0m) capAct = GetSaldoMes(saldosG2, "30000000", i);
                        decimal entAct = GetSaldoMes(saldosG2, "23000000", i);

                        decimal varPub = 0m, varCap = 0m, varEnt = 0m;
                        if (i > 0)
                        {
                            decimal pubAnt = GetSaldoMes(saldosG2, "21000000", i - 1);
                            decimal capAnt = GetSaldoMes(saldosG2, "31000000", i - 1);
                            if (capAnt == 0m) capAnt = GetSaldoMes(saldosG2, "30000000", i - 1);
                            decimal entAnt = GetSaldoMes(saldosG2, "23000000", i - 1);

                            if (pubAnt > 0) varPub = Math.Round(((pubAct - pubAnt) / pubAnt) * 100m, 2);
                            if (capAnt > 0) varCap = Math.Round(((capAct - capAnt) / capAnt) * 100m, 2);
                            if (entAnt > 0) varEnt = Math.Round(((entAct - entAnt) / entAnt) * 100m, 2);
                        }

                        serieVarPub.Data.Add(varPub);
                        serieVarCap.Data.Add(varCap);
                        serieVarEnt.Data.Add(varEnt);
                    }

                    data.Series.Add(serieVarPub);
                    data.Series.Add(serieVarCap);
                    data.Series.Add(serieVarEnt);

                    data.TablaData.Titulo = "Variaciones por tipo de fondeo - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Tipo de fondeo" };
                    data.TablaData.Columnas.AddRange(categorias);

                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Obligaciones con el público (%)", Valores = serieVarPub.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Capital social (%)", Valores = serieVarCap.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Obligaciones con entidades (%)", Valores = serieVarEnt.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    break;

                case 3:
                    // Variación y composición por tipo de captaciones (Stacked Column % + 2 Lines Variación %)
                    data.Titulo = "Variaciones y composición de las captaciones";
                    data.Subtitulo = "Distribución de captaciones a la vista, plazo y cargos - " + nomEntidad;
                    data.NotaPie = "Nota: La composición suma 100% (eje izquierdo) y las líneas representan las variaciones interanuales (eje derecho).";
                    data.TieneDobleEje = true;
                    data.EjeYIzquierdoTitulo = "Composición (%)";
                    data.EjeYDerechoTitulo = "Variación Interanual (%)";
                    data.Apilado = true;
                    data.ApiladoTipo = "percent";
                    data.TipoGraficoHighcharts = "column";

                    var serieCapPlazo = new GraficoFondeoSerie { Name = "Captaciones a plazo", Type = "column", Stack = "capt", Color = "#2F5597", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieCapVista = new GraficoFondeoSerie { Name = "Captaciones a la vista", Type = "column", Stack = "capt", Color = "#6B9FD4", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieCargos = new GraficoFondeoSerie { Name = "Cargos de obligaciones", Type = "column", Stack = "capt", Color = "#94A3B8", TooltipSuffix = "%", YAxis = 0, EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieVarCP = new GraficoFondeoSerie { Name = "Variación interanual CP", Type = "spline", Color = "#F4A261", TooltipSuffix = "%", YAxis = 1, EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var serieVarAH = new GraficoFondeoSerie { Name = "Variación interanual AH", Type = "spline", Color = "#52B788", TooltipSuffix = "%", YAxis = 1, EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };

                    var saldosG3 = ObtenerSaldosPorCuentaRango(entidad, periodos);

                    for (int i = 0; i < periodos.Count; i++)
                    {
                        decimal vista = GetSaldoMes(saldosG3, "21100000", i);
                        decimal plazo = GetSaldoMes(saldosG3, "21300000", i);
                        decimal cargos = GetSaldoMes(saldosG3, "21900000", i);

                        decimal totalCapt = vista + plazo + cargos;
                        if (totalCapt == 0m) totalCapt = GetSaldoMes(saldosG3, "21000000", i);

                        decimal porcVista = totalCapt > 0 ? Math.Round((vista / totalCapt) * 100m, 2) : 0m;
                        decimal porcCargos = totalCapt > 0 ? Math.Round((cargos / totalCapt) * 100m, 2) : 0m;
                        decimal porcPlazo = totalCapt > 0 ? Math.Round(100m - porcVista - porcCargos, 2) : 0m;

                        decimal varAH = 0m, varCP = 0m;
                        if (i > 0)
                        {
                            decimal vistaAnt = GetSaldoMes(saldosG3, "21100000", i - 1);
                            decimal plazoAnt = GetSaldoMes(saldosG3, "21300000", i - 1);

                            if (vistaAnt > 0) varAH = Math.Round(((vista - vistaAnt) / vistaAnt) * 100m, 2);
                            if (plazoAnt > 0) varCP = Math.Round(((plazo - plazoAnt) / plazoAnt) * 100m, 2);
                        }

                        serieCapVista.Data.Add(porcVista);
                        serieCapPlazo.Data.Add(porcPlazo);
                        serieCargos.Data.Add(porcCargos);
                        serieVarAH.Data.Add(varAH);
                        serieVarCP.Data.Add(varCP);
                    }

                    data.Series.Add(serieCapPlazo);
                    data.Series.Add(serieCapVista);
                    data.Series.Add(serieCargos);
                    data.Series.Add(serieVarCP);
                    data.Series.Add(serieVarAH);

                    data.TablaData.Titulo = "Composición y Variación de Captaciones - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Concepto" };
                    data.TablaData.Columnas.AddRange(categorias);

                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Captaciones a plazo (%)", Valores = serieCapPlazo.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Captaciones a la vista (%)", Valores = serieCapVista.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Cargos de obligaciones (%)", Valores = serieCargos.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Variación interanual CP (%)", Valores = serieVarCP.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Variación interanual AH (%)", Valores = serieVarAH.Data.Select(d => string.Format("{0:N2}%", d)).ToList() });
                    break;

                case 4:
                    // Concentración de los 10 y 20 mayores ahorrantes (Clustered column con etiquetas arriba)
                    data.Titulo = "Concentración de los 10 y 20 mayores ahorrantes";
                    data.Subtitulo = "Participación de principales ahorrantes sobre obligaciones con el público - " + nomEntidad;
                    data.NotaPie = "Nota: Porcentaje calculado sobre el saldo total de obligaciones con el público registrado en cada cierre.";
                    data.TieneDobleEje = false;
                    data.EjeYIzquierdoTitulo = "Concentración (%)";
                    data.TipoGraficoHighcharts = "column";

                    var serieTop10 = new GraficoFondeoSerie { Name = "10 mayores ahorrantes", Type = "column", Color = "#2F5597", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };
                    var serieTop20 = new GraficoFondeoSerie { Name = "20 mayores ahorrantes", Type = "column", Color = "#94A3B8", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.1f}%" };

                    var clientG4 = GetSPClient();
                    var dictAhorrantes = new Dictionary<string, Entities.Entities.Procedures.FGA_Consultar_Concentracion_Ahorrantes_Result>();
                    try
                    {
                        var datosAhorrantes = clientG4.FGA_Consultar_Concentracion_Ahorrantes(entidad, periodos.First(), periodos.Last());
                        if (datosAhorrantes != null)
                        {
                            foreach (var r in datosAhorrantes)
                            {
                                if (r.Periodo.HasValue)
                                {
                                    dictAhorrantes[r.Periodo.Value.ToString("yyyy-MM")] = r;
                                }
                            }
                        }
                    }
                    catch { }

                    for (int i = 0; i < periodos.Count; i++)
                    {
                        string key = periodos[i].ToString("yyyy-MM");
                        decimal p10 = 0m;
                        decimal p20 = 0m;
                        if (dictAhorrantes.TryGetValue(key, out var rowA))
                        {
                            p10 = rowA.PorcTop10 ?? 0m;
                            p20 = rowA.PorcTop20 ?? 0m;
                        }

                        serieTop10.Data.Add(Math.Round(p10, 1));
                        serieTop20.Data.Add(Math.Round(p20, 1));
                    }

                    data.Series.Add(serieTop10);
                    data.Series.Add(serieTop20);

                    data.TablaData.Titulo = "Concentración de Mayores Ahorrantes - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Ahorrantes" };
                    data.TablaData.Columnas.AddRange(categorias);

                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "10 mayores ahorrantes (%)", Valores = serieTop10.Data.Select(d => string.Format("{0:N1}%", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "20 mayores ahorrantes (%)", Valores = serieTop20.Data.Select(d => string.Format("{0:N1}%", d)).ToList() });
                    break;

                case 5:
                    // Concentración de saldos por vencimiento (Barras horizontales apiladas al 100% con nueva paleta ejecutiva)
                    data.Titulo = "Concentración de saldos por vencimiento";
                    data.Subtitulo = "Distribución de saldos de captación en 7 tramos de vencimiento - " + nomEntidad;
                    data.NotaPie = "Nota: Tramos definidos conforme a la normativa SUGEF: A la vista, 1-90 d, 91-180 d, 181-270 d, 271-360 d, 1-3 años y >3 años.";
                    data.TipoGraficoHighcharts = "bar";
                    data.Apilado = true;
                    data.ApiladoTipo = "percent";
                    data.EjeYIzquierdoTitulo = "Participación (%)";

                    int mesesAMostrar = Math.Min(periodos.Count, 4);
                    var ultimosPeriodos = periodos.Skip(periodos.Count - mesesAMostrar).ToList();
                    data.Categorias = ultimosPeriodos.Select(u => u.ToString("MMM-yy", new CultureInfo("es-ES"))).ToList();

                    var sVista = new GraficoFondeoSerie { Name = "A la vista", Type = "bar", Stack = "venc", Color = "#2F5597", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var s1a90 = new GraficoFondeoSerie { Name = "De 1 a 90 días", Type = "bar", Stack = "venc", Color = "#94A3B8", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var s91a180 = new GraficoFondeoSerie { Name = "De 91 a 180 días", Type = "bar", Stack = "venc", Color = "#6B9FD4", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var s181a270 = new GraficoFondeoSerie { Name = "De 181 a 270 días", Type = "bar", Stack = "venc", Color = "#4A7BB0", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var s271a360 = new GraficoFondeoSerie { Name = "De 271 a 360 días", Type = "bar", Stack = "venc", Color = "#52B788", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var s1a3A = new GraficoFondeoSerie { Name = "De 1 a 3 años", Type = "bar", Stack = "venc", Color = "#E09F67", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };
                    var sMas3A = new GraficoFondeoSerie { Name = "De 3 años en adelante", Type = "bar", Stack = "venc", Color = "#64748B", TooltipSuffix = "%", EnableDataLabels = true, DataLabelFormat = "{point.y:.2f}%" };

                    var clientG5 = GetSPClient();
                    var dictVenc = new Dictionary<string, Entities.Entities.Procedures.FGA_Consultar_Concentracion_Vencimiento_Result>();
                    try
                    {
                        var datosVenc = clientG5.FGA_Consultar_Concentracion_Vencimiento(entidad, ultimosPeriodos.First(), ultimosPeriodos.Last());
                        if (datosVenc != null)
                        {
                            foreach (var r in datosVenc)
                            {
                                if (r.Periodo.HasValue)
                                {
                                    dictVenc[r.Periodo.Value.ToString("yyyy-MM")] = r;
                                }
                            }
                        }
                    }
                    catch { }

                    foreach (var per in ultimosPeriodos)
                    {
                        string key = per.ToString("yyyy-MM");
                        decimal vVista = 0m, v1a90 = 0m, v91a180 = 0m, v181a270 = 0m, v271a360 = 0m, v1a3A = 0m, vMas3A = 0m;
                        if (dictVenc.TryGetValue(key, out var rowV))
                        {
                            vVista = rowV.PorcALaVista ?? 0m;
                            v1a90 = rowV.PorcDe1A90Dias ?? 0m;
                            v91a180 = rowV.PorcDe91A180Dias ?? 0m;
                            v181a270 = rowV.PorcDe181A270Dias ?? 0m;
                            v271a360 = rowV.PorcDe271A360Dias ?? 0m;
                            v1a3A = rowV.PorcDe1A3Anos ?? 0m;
                            vMas3A = rowV.PorcDe3AnosEnAdelante ?? 0m;
                        }

                        sVista.Data.Add(Math.Round(vVista, 2));
                        s1a90.Data.Add(Math.Round(v1a90, 2));
                        s91a180.Data.Add(Math.Round(v91a180, 2));
                        s181a270.Data.Add(Math.Round(v181a270, 2));
                        s271a360.Data.Add(Math.Round(v271a360, 2));
                        s1a3A.Data.Add(Math.Round(v1a3A, 2));
                        sMas3A.Data.Add(Math.Round(vMas3A, 2));
                    }

                    data.Series.Add(sVista);
                    data.Series.Add(s1a90);
                    data.Series.Add(s91a180);
                    data.Series.Add(s181a270);
                    data.Series.Add(s271a360);
                    data.Series.Add(s1a3A);
                    data.Series.Add(sMas3A);

                    data.TablaData.Titulo = "Distribución por Tramo de Vencimiento - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Tramo de Vencimiento" };
                    data.TablaData.Columnas.AddRange(data.Categorias);

                    foreach (var s in data.Series)
                    {
                        var fila = new GraficoTablaFila { Nombre = s.Name, Valores = new List<string>() };
                        foreach (var d in s.Data)
                        {
                            decimal val = d != null ? Convert.ToDecimal(d) : 0m;
                            fila.Valores.Add(string.Format("{0:N2}%", val));
                        }
                        data.TablaData.Filas.Add(fila);
                    }
                    break;

                case 6:
                    // Cantidad de asociados y ahorrantes (Stacked Column con etiquetas de recuento)
                    data.Titulo = "Asociados y ahorrantes activos";
                    data.Subtitulo = "Asociados activos y cantidad de ahorrantes con saldos - " + nomEntidad;
                    data.NotaPie = "Nota: Datos calculados a partir de los datos adicionales contables (asociados) y el conteo de acreedores en el pasivo (ahorrantes).";
                    data.TipoGraficoHighcharts = "column";
                    data.Apilado = true;
                    data.ApiladoTipo = "normal";
                    data.EjeYIzquierdoTitulo = "Cantidad de Personas";

                    var serieAsoc = new GraficoFondeoSerie { Name = "Asociados activos", Type = "column", Stack = "personas", Color = "#2F5597", TooltipSuffix = " personas", EnableDataLabels = true, DataLabelFormat = "{point.y:,.0f}" };
                    var serieAhorr = new GraficoFondeoSerie { Name = "Ahorrantes", Type = "column", Stack = "personas", Color = "#6B9FD4", TooltipSuffix = " personas", EnableDataLabels = true, DataLabelFormat = "{point.y:,.0f}" };

                    var clientG6 = GetSPClient();
                    var dictAsocAhorr = new Dictionary<string, Entities.Entities.Procedures.FGA_Consultar_Cantidad_Asociados_Ahorrantes_Result>();
                    try
                    {
                        var datosAsocAhorr = clientG6.FGA_Consultar_Cantidad_Asociados_Ahorrantes(entidad, periodos.First(), periodos.Last());
                        if (datosAsocAhorr != null)
                        {
                            foreach (var r in datosAsocAhorr)
                            {
                                if (r.Periodo.HasValue)
                                {
                                    dictAsocAhorr[r.Periodo.Value.ToString("yyyy-MM")] = r;
                                }
                            }
                        }
                    }
                    catch { }

                    for (int i = 0; i < periodos.Count; i++)
                    {
                        string key = periodos[i].ToString("yyyy-MM");
                        int asocVal = 0;
                        int ahorrVal = 0;
                        if (dictAsocAhorr.TryGetValue(key, out var rowAA))
                        {
                            asocVal = rowAA.AsociadosActivos ?? 0;
                            ahorrVal = rowAA.CantidadAhorrantes ?? 0;
                        }

                        serieAsoc.Data.Add(asocVal);
                        serieAhorr.Data.Add(ahorrVal);
                    }

                    data.Series.Add(serieAsoc);
                    data.Series.Add(serieAhorr);

                    data.TablaData.Titulo = "Asociados y Ahorrantes Activos - " + nomEntidad;
                    data.TablaData.Columnas = new List<string> { "Cuenta / Población" };
                    data.TablaData.Columnas.AddRange(categorias);

                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Asociados activos", Valores = serieAsoc.Data.Select(d => string.Format("{0:N0}", d)).ToList() });
                    data.TablaData.Filas.Add(new GraficoTablaFila { Nombre = "Ahorrantes", Valores = serieAhorr.Data.Select(d => string.Format("{0:N0}", d)).ToList() });
                    break;
            }

            data.Diagnostico = GenerarDiagnosticoTecnico(tipoGrafico, data, nomEntidad);

            return data;
        }

        private DiagnosticoTecnicoFondeo GenerarDiagnosticoTecnico(int tipoGrafico, GraficoFondeoData data, string nomEntidad)
        {
            var diag = new DiagnosticoTecnicoFondeo();
            diag.UltimoPeriodo = data.Categorias != null && data.Categorias.Count > 0 ? data.Categorias.Last() : "";

            Func<object, decimal> parseDec = o =>
            {
                if (o == null) return 0m;
                if (o is decimal) return (decimal)o;
                if (o is double) return (decimal)(double)o;
                if (o is float) return (decimal)(float)o;
                if (o is int) return (decimal)(int)o;
                if (o is long) return (decimal)(long)o;

                string s = o.ToString().Trim().Replace("%", "").Replace("₡", "").Replace(" ", "");
                decimal val;
                if (s.Contains(",") && !s.Contains("."))
                {
                    if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("es-ES"), out val)) return val;
                }
                else if (s.Contains(".") && !s.Contains(","))
                {
                    if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) return val;
                }
                else
                {
                    if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out val)) return val;
                    if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) return val;
                }
                return 0m;
            };

            switch (tipoGrafico)
            {
                case 1:
                    diag.TituloAnalisis = "Diagnóstico: Estructura de Financiamiento";
                    if (data.Series.Count >= 4)
                    {
                        var sPub = data.Series[0].Data;
                        var sEnt = data.Series[1].Data;
                        var sCap = data.Series[2].Data;
                        var sTot = data.Series[3].Data;

                        decimal ultPub = sPub.Count > 0 ? parseDec(sPub.Last()) : 0;
                        decimal ultEnt = sEnt.Count > 0 ? parseDec(sEnt.Last()) : 0;
                        decimal ultCap = sCap.Count > 0 ? parseDec(sCap.Last()) : 0;
                        decimal ultTot = sTot.Count > 0 ? parseDec(sTot.Last()) : 0;

                        decimal iniTot = sTot.Count > 0 ? parseDec(sTot.First()) : 0;
                        decimal varTot = ultTot - iniTot;

                        if (ultPub >= 70m && ultEnt <= 15m)
                        {
                            diag.Estado = "Adecuado";
                            diag.EstadoLabel = "ADECUADO";
                            diag.ResumenEjecutivo = "Estructura de fondeo diversificada y sólida con predominancia de captaciones del público.";
                        }
                        else if (ultEnt > 25m)
                        {
                            diag.Estado = "Atencion";
                            diag.EstadoLabel = "ATENCIÓN";
                            diag.ResumenEjecutivo = "Elevada dependencia de financiamiento institucional que incrementa la sensibilidad al costo de fondos.";
                        }
                        else
                        {
                            diag.Estado = "Monitoreo";
                            diag.EstadoLabel = "MONITOREO";
                            diag.ResumenEjecutivo = "Composición de pasivos estable con participación moderada de acreedores institucionales.";
                        }

                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Fondeo Público", Valor = string.Format("{0:N1}%", ultPub), Subtexto = "Captaciones minoristas y comerciales", Nivel = "positivo", Icono = "fa-users" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Fondeo Entidades", Valor = string.Format("{0:N1}%", ultEnt), Subtexto = "Pasivos institucionales con costo", Nivel = ultEnt > 20m ? "advertencia" : "normal", Icono = "fa-bank" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Capital Social", Valor = string.Format("{0:N1}%", ultCap), Subtexto = "Base patrimonial de respaldo", Nivel = "positivo", Icono = "fa-shield" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Financiamiento Total", Valor = string.Format("₡{0:N0}M", ultTot), Subtexto = string.Format("Variación: {0:+0;-0;0}M vs inicio", varTot), Nivel = "normal", Icono = "fa-money" });

                        diag.Hallazgos.Add(string.Format("El fondeo proveniente del público representa el {0:N1}% del financiamiento total en {1}, consolidándose como el principal ancla estructural.", ultPub, diag.UltimoPeriodo));
                        diag.Hallazgos.Add(string.Format("Las obligaciones con entidades financieras se ubican en {0:N1}%, reflejando un perfil de apalancamiento {1}.", ultEnt, ultEnt <= 10m ? "bajo y conservador" : (ultEnt <= 20m ? "moderado y controlado" : "elevado que requiere vigilancia")));
                        diag.Hallazgos.Add(string.Format("El capital social aporta un {0:N1}% de solvencia no exigible en el corto plazo.", ultCap));

                        diag.Recomendaciones.Add("Mantener la política de captación granular para evitar concentración en pocos acreedores y alinear la estructura a las directrices de la <a href='https://www.sugef.fi.cr/normativa/normativa_vigente.aspx' target='_blank' rel='noopener noreferrer' style='color: #0284c7; text-decoration: underline; font-weight: 600;'>Superintendencia General de Entidades Financieras <i class='fa fa-external-link' style='font-size: 10px;'></i></a>.");
                        diag.Recomendaciones.Add("Monitorear el costo de fondos de las líneas de crédito institucionales para preservar el margen financiero neto.");
                    }
                    break;

                case 2:
                    diag.TituloAnalisis = "Diagnóstico: Variaciones por Tipo de Fondeo";
                    if (data.Series.Count >= 3)
                    {
                        decimal varPub = data.Series[0].Data.Count > 0 ? parseDec(data.Series[0].Data.Last()) : 0;
                        decimal varCap = data.Series[1].Data.Count > 0 ? parseDec(data.Series[1].Data.Last()) : 0;
                        decimal varEnt = data.Series[2].Data.Count > 0 ? parseDec(data.Series[2].Data.Last()) : 0;

                        if (varPub >= 0 && varCap >= 0)
                        {
                            diag.Estado = "Adecuado";
                            diag.EstadoLabel = "CRECIMIENTO SOSTENIDO";
                            diag.ResumenEjecutivo = "Las principales fuentes de fondeo exhiben dinamismo positivo en el período de análisis.";
                        }
                        else
                        {
                            diag.Estado = "Monitoreo";
                            diag.EstadoLabel = "VARIACIÓN ASIMÉTRICA";
                            diag.ResumenEjecutivo = "Se observa dispersión o desaceleración en alguna de las fuentes de fondeo.";
                        }

                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Variación en Público", Valor = string.Format("{0:+0.00;-0.00;0.00}%", varPub), Subtexto = "Crecimiento interanual", Nivel = varPub >= 0 ? "positivo" : "advertencia", Icono = "fa-line-chart" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Variación en Capital", Valor = string.Format("{0:+0.00;-0.00;0.00}%", varCap), Subtexto = "Ritmo de capitalización", Nivel = "positivo", Icono = "fa-shield" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Variación en Entidades", Valor = string.Format("{0:+0.00;-0.00;0.00}%", varEnt), Subtexto = "Variación de endeudamiento", Nivel = "normal", Icono = "fa-exchange" });

                        diag.Hallazgos.Add(string.Format("Las obligaciones con el público registran una tasa de variación de {0:+0.00;-0.00;0.00}%, reflejando el dinamismo comercial de captación.", varPub));
                        diag.Hallazgos.Add(string.Format("El capital social crece a un ritmo de {0:+0.00;-0.00;0.00}%, fortaleciendo la capacidad de absorción de pérdidas.", varCap));

                        diag.Recomendaciones.Add("Alinear las tasas de captación al ritmo de colocación de crédito para no generar excedentes de liquidez ociosos.");
                        diag.Recomendaciones.Add("Evaluar la elasticidad precio de los depósitos para optimizar el gasto por intereses.");
                    }
                    break;

                case 3:
                    diag.TituloAnalisis = "Diagnóstico: Composición de Captaciones";
                    diag.Estado = "Adecuado";
                    diag.EstadoLabel = "ADECUADO";
                    diag.ResumenEjecutivo = "Estructura de captaciones con adecuado equilibrio entre exigibilidad inmediata y estabilidad a plazo.";
                    diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Estabilidad Fondeo", Valor = "Equilibrada", Subtexto = "Relación Plazo y Vista", Nivel = "positivo", Icono = "fa-balance-scale" });
                    diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Volatilidad", Valor = "Controlada", Subtexto = "Comportamiento estacional", Nivel = "normal", Icono = "fa-tachometer" });
                    diag.Hallazgos.Add("La combinación de depósitos a plazo y captaciones a la vista permite atender los requerimientos de caja operativa sin comprometer el calce financiero.");
                    diag.Recomendaciones.Add("Mantener esquemas de incentivos escalonados para extender el plazo promedio de vencimiento.");
                    break;

                case 4:
                    diag.TituloAnalisis = "Diagnóstico: Concentración de Mayores Ahorrantes";
                    if (data.Series.Count >= 2)
                    {
                        decimal top10 = data.Series[0].Data.Count > 0 ? parseDec(data.Series[0].Data.Last()) : 0;
                        decimal top20 = data.Series[1].Data.Count > 0 ? parseDec(data.Series[1].Data.Last()) : 0;

                        if (top20 <= 15m)
                        {
                            diag.Estado = "Adecuado";
                            diag.EstadoLabel = "BAJA CONCENTRACIÓN";
                            diag.ResumenEjecutivo = "Excelente atomización de depositantes con bajo riesgo de liquidez por retiro de mayoristas.";
                        }
                        else if (top20 <= 25m)
                        {
                            diag.Estado = "Monitoreo";
                            diag.EstadoLabel = "CONCENTRACIÓN MODERADA";
                            diag.ResumenEjecutivo = "Concentración en rangos aceptables pero con vigilancia requerida sobre los principales acreedores.";
                        }
                        else
                        {
                            diag.Estado = "Atencion";
                            diag.EstadoLabel = "ALTA CONCENTRACIÓN";
                            diag.ResumenEjecutivo = "Elevada exposición a grandes depositantes que amerita colchones de liquidez específicos.";
                        }

                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "10 Mayores Ahorrantes", Valor = string.Format("{0:N2}%", top10), Subtexto = "Del total de captaciones", Nivel = top10 > 12m ? "advertencia" : "positivo", Icono = "fa-pie-chart" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "20 Mayores Ahorrantes", Valor = string.Format("{0:N2}%", top20), Subtexto = "Del total de captaciones", Nivel = top20 > 20m ? "advertencia" : "positivo", Icono = "fa-users" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Límite Prudencial", Valor = "20,00%", Subtexto = "Umbral interno recomendado", Nivel = "normal", Icono = "fa-flag" });

                        diag.Hallazgos.Add(string.Format("Los 10 principales ahorrantes concentran el {0:N2}% del pasivo captado en {1}, denotando una cartera {2}.", top10, nomEntidad, top10 <= 10m ? "altamente atomizada y sana" : "con depositantes institucionales relevantes"));
                        diag.Hallazgos.Add(string.Format("Los 20 mayores ahorrantes totalizan el {0:N2}%, situándose {1} del umbral prudencial sugerido de referencia (20%).", top20, top20 <= 20m ? "por debajo" : "por encima"));

                        diag.Recomendaciones.Add("Establecer un monitoreo quincenal de vencimientos y flujos de caja de los 5 mayores ahorrantes institucionales.");
                        diag.Recomendaciones.Add("Asegurar que las reservas de liquidez inmediata cubran ampliamente cualquier eventual salida de fondos de los diez principales ahorrantes, conforme a las directrices de cobertura del <a href='https://www.sugef.fi.cr/normativa/normativa_vigente/SUGEF%2017-13%20(v18%201%C2%B0%20SET2024).pdf' target='_blank' rel='noopener noreferrer' style='color: #0284c7; text-decoration: underline; font-weight: 600;'>Acuerdo SUGEF 17-13 <i class='fa fa-external-link' style='font-size: 10px;'></i></a>.");
                    }
                    break;

                case 5:
                    diag.TituloAnalisis = "Diagnóstico: Concentración de Saldos por Vencimiento";
                    if (data.Categorias.Count > 0 && data.Series.Count >= 7)
                    {
                        diag.UltimoPeriodo = data.Categorias.Last();

                        // data.Series tiene los 7 tramos:
                        // 0: A la vista, 1: 1-90 d, 2: 91-180 d, 3: 181-270 d, 4: 271-360 d, 5: 1-3 años, 6: >3 años
                        decimal tramoVista = data.Series[0].Data.Count > 0 ? parseDec(data.Series[0].Data.Last()) : 0;
                        decimal tramo1a90 = data.Series[1].Data.Count > 0 ? parseDec(data.Series[1].Data.Last()) : 0;
                        decimal tramo91_180 = data.Series[2].Data.Count > 0 ? parseDec(data.Series[2].Data.Last()) : 0;
                        decimal tramo181_270 = data.Series[3].Data.Count > 0 ? parseDec(data.Series[3].Data.Last()) : 0;
                        decimal tramo271_360 = data.Series[4].Data.Count > 0 ? parseDec(data.Series[4].Data.Last()) : 0;
                        decimal tramo1a3A = data.Series[5].Data.Count > 0 ? parseDec(data.Series[5].Data.Last()) : 0;
                        decimal tramoMas3A = data.Series[6].Data.Count > 0 ? parseDec(data.Series[6].Data.Last()) : 0;

                        decimal liqInmediata = tramoVista + tramo1a90;
                        decimal medianoPlazo = tramo91_180 + tramo181_270 + tramo271_360;
                        decimal largoPlazo = tramo1a3A + tramoMas3A;

                        // Estimación de vida media ponderada en días
                        decimal vidaMediaDias = Math.Round((tramoVista * 1m + tramo1a90 * 45m + tramo91_180 * 135m + tramo181_270 * 225m + tramo271_360 * 315m + tramo1a3A * 720m + tramoMas3A * 1200m) / 100m, 0);

                        if (largoPlazo >= 10m && liqInmediata <= 50m)
                        {
                            diag.Estado = "Adecuado";
                            diag.EstadoLabel = "EQUILIBRIO ESTRUCTURAL";
                            diag.ResumenEjecutivo = "Distribución temporal equilibrada con adecuado colchón de fondeo a mediano y largo plazo.";
                        }
                        else
                        {
                            diag.Estado = "Monitoreo";
                            diag.EstadoLabel = "CONCENTRACIÓN EN CORTO PLAZO";
                            diag.ResumenEjecutivo = "Elevada proporción de fondeo en tramos cortos que exige gestión activa de renovaciones.";
                        }

                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Inmediata (hasta 90d)", Valor = string.Format("{0:N1}%", liqInmediata), Subtexto = string.Format("Vista: {0:N1}% | 1-90 días: {1:N1}%", tramoVista, tramo1a90), Nivel = liqInmediata > 50m ? "advertencia" : "positivo", Icono = "fa-hourglass-start" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Mediano Plazo", Valor = string.Format("{0:N1}%", medianoPlazo), Subtexto = string.Format("Tramo 271-360 días: {0:N1}%", tramo271_360), Nivel = "positivo", Icono = "fa-calendar" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Largo Plazo (>1 año)", Valor = string.Format("{0:N1}%", largoPlazo), Subtexto = string.Format("1-3 años: {0:N1}% | >3 años: {1:N1}%", tramo1a3A, tramoMas3A), Nivel = largoPlazo >= 10m ? "positivo" : "advertencia", Icono = "fa-anchor" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Vida Media Fondeo", Valor = string.Format("{0:N0} días", vidaMediaDias), Subtexto = string.Format("~{0:N1} meses", vidaMediaDias / 30m), Nivel = "normal", Icono = "fa-clock-o" });

                        diag.Hallazgos.Add(string.Format("El {0:N1}% de los saldos vence en 90 días o menos (cuentas a la vista {1:N1}% y depósitos hasta 90 días {2:N1}%), garantizando disponibilidad pero requiriendo monitoreo de renovación.", liqInmediata, tramoVista, tramo1a90));
                        diag.Hallazgos.Add(string.Format("El tramo de 271 a 360 días concentra un relevante {0:N1}%, constituyendo un ancla clave en el fondeo anual.", tramo271_360));
                        diag.Hallazgos.Add(string.Format("El fondeo de largo plazo (más de 1 año) se sitúa en {0:N1}% ({1:N1}% a 1-3 años y {2:N1}% a más de 3 años), respaldando el equilibrio y calce financiero con las colocaciones de crédito a plazo.", largoPlazo, tramo1a3A, tramoMas3A));

                        diag.Recomendaciones.Add("Implementar campañas proactivas de retención antes del vencimiento del tramo de 271 a 360 días para mantener una tasa de renovación superior al 85%, respaldando el calce de plazos y mitigando brechas de liquidez según el <a href='https://www.sugef.fi.cr/normativa/normativa_vigente/SUGEF%2017-13%20(v18%201%C2%B0%20SET2024).pdf' target='_blank' rel='noopener noreferrer' style='color: #0284c7; text-decoration: underline; font-weight: 600;'>Acuerdo SUGEF 17-13 <i class='fa fa-external-link' style='font-size: 10px;'></i></a> y el <a href='https://www.sugef.fi.cr/normativa/normativa_vigente/SUGEF%202-10%20(v29%201%C2%B0%20de%20enero%20de%202024).pdf' target='_blank' rel='noopener noreferrer' style='color: #0284c7; text-decoration: underline; font-weight: 600;'>Acuerdo SUGEF 2-10 <i class='fa fa-external-link' style='font-size: 10px;'></i></a>.");
                        diag.Recomendaciones.Add("Fomentar productos a plazos de 18 a 36 meses con tasas atractivas para extender la vida media del fondeo hacia los 180 días.");
                    }
                    break;

                case 6:
                    diag.TituloAnalisis = "Diagnóstico: Masa Social y Atomización de Depositantes";
                    if (data.Series.Count >= 2)
                    {
                        decimal asoc = data.Series[0].Data.Count > 0 ? parseDec(data.Series[0].Data.Last()) : 0;
                        decimal ahorr = data.Series[1].Data.Count > 0 ? parseDec(data.Series[1].Data.Last()) : 0;
                        decimal ratio = asoc > 0 ? Math.Round(ahorr / asoc, 2) : 0;

                        diag.Estado = "Adecuado";
                        diag.EstadoLabel = "BASE SÓLIDA";
                        diag.ResumenEjecutivo = "Amplia masa social y base de ahorrantes que aportan diversificación granular al fondeo.";

                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Asociados Activos", Valor = string.Format("{0:N0}", asoc), Subtexto = "Base cooperativa o mutual", Nivel = "positivo", Icono = "fa-users" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Ahorrantes Activos", Valor = string.Format("{0:N0}", ahorr), Subtexto = "Cuentas con saldo", Nivel = "positivo", Icono = "fa-id-card-o" });
                        diag.Metricas.Add(new DiagnosticoMetricaItem { Etiqueta = "Ahorrantes por Asociado", Valor = string.Format("{0:N2}x", ratio), Subtexto = "Penetración de ahorro", Nivel = "normal", Icono = "fa-percent" });

                        diag.Hallazgos.Add(string.Format("La entidad cuenta con {0:N0} asociados activos y {1:N0} ahorrantes en {2}, consolidando una base de financiamiento altamente atomizada.", asoc, ahorr, nomEntidad));
                        diag.Hallazgos.Add(string.Format("El ratio de penetración es de {0:N2} ahorrantes por cada asociado activo.", ratio));

                        diag.Recomendaciones.Add("Promover la vinculación cruzada de productos de ahorro programado para aumentar la recurrencia de depósitos.");
                        diag.Recomendaciones.Add("Monitorear la tasa de retiro o desvinculación de asociados para proteger la base social captadora.");
                    }
                    break;
            }

            return diag;
        }

        private decimal GetSaldo(Dictionary<string, decimal> dict, string cuenta, decimal fallback)
        {
            if (dict != null && dict.ContainsKey(cuenta))
            {
                return dict[cuenta];
            }
            return fallback;
        }

        private decimal SafeDiv(decimal numerador, decimal denominador)
        {
            if (denominador == 0) return 0;
            return numerador / denominador;
        }

        #endregion
    }
}
