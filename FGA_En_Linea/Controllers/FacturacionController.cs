using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FGA.Model;
using FGA.Utility;
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class FacturacionController : BaseController
    {
        public ActionResult Index()
        {
            Load();
            Session["TipoReporte"] = null;
            Session["TipoReporte2"] = null;

            string idEntidad = GetSessionString(FGAConstants.Sesion.IdEntidad);
            int mesActual = DateTime.Now.Month;
            int anio = GetSessionInt("Anno", DateTime.Now.Year);
            int trimestre = GetSessionInt("Trimestre", (mesActual - 1) / 3 + 1);

            var model = CargarDatosFacturacion(idEntidad, anio, trimestre);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Buscar(string Entidades, string Anio, string Trimestre)
        {
            Session["IdEntidad"] = Entidades;
            Session["Anno"] = Anio;
            Session["Trimestre"] = Trimestre;
            Session["TipoReporte"] = enum_tipoReporte.rpt_factura;
            Session["TipoReporte2"] = enum_tipoReporte.rpt_factura_ffc;

            Load();

            int anioInt = DateTime.Now.Year;
            int.TryParse(Anio, out anioInt);

            int trimestreInt = 1;
            int.TryParse(Trimestre, out trimestreInt);

            var model = CargarDatosFacturacion(Entidades, anioInt, trimestreInt);
            return View("Index", model);
        }

        public ActionResult ExportarExcel(string Entidades, string Anio, string Trimestre, string tab = "FFC")
        {
            try
            {
                int anioInt = DateTime.Now.Year;
                int.TryParse(Anio, out anioInt);

                int trimestreInt = 1;
                int.TryParse(Trimestre, out trimestreInt);

                string idEntidad = !string.IsNullOrEmpty(Entidades) ? Entidades : (Session["IdEntidad"]?.ToString() ?? "");
                var model = CargarDatosFacturacion(idEntidad, anioInt, trimestreInt);

                var pestana = (tab?.ToUpper() == "FGD") ? model.FGD : model.FFC;
                string tabTitulo = (tab?.ToUpper() == "FGD") ? "FGD (BCCR)" : "FEE-FFC";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");

                sb.AppendLine(" <Styles>");
                sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                sb.AppendLine("   <Alignment ss:Vertical=\"Bottom\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#333333\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"HeaderMain\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"13\" ss:Bold=\"1\" ss:Color=\"#1e3a8a\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"HeaderSub\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Italic=\"1\" ss:Color=\"#64748b\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"ColHeader\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\" ss:WrapText=\"1\"/>");
                sb.AppendLine("   <Borders>");
                sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#2F5597\"/>");
                sb.AppendLine("   </Borders>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#2F5597\"/>");
                sb.AppendLine("   <Interior ss:Color=\"#F1F5F9\" ss:Pattern=\"Solid\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"CellText\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"9.5\" ss:Color=\"#1e293b\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"CellTextBold\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"9.5\" ss:Bold=\"1\" ss:Color=\"#0f172a\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"CellNum\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"9.5\" ss:Color=\"#1e293b\"/>");
                sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.00\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"CellNumBold\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"9.5\" ss:Bold=\"1\" ss:Color=\"#0f172a\"/>");
                sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.00\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"PagoTrimestral\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/>");
                sb.AppendLine("   <Interior ss:Color=\"#EA580C\" ss:Pattern=\"Solid\"/>");
                sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.00\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"PagoTrimestralLbl\">");
                sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/>");
                sb.AppendLine("   <Interior ss:Color=\"#EA580C\" ss:Pattern=\"Solid\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine(" </Styles>");

                sb.AppendLine($" <Worksheet ss:Name=\"{tabTitulo.Replace("(", "").Replace(")", "").Trim()}\">");
                sb.AppendLine("  <Table ss:DefaultRowHeight=\"20\">");
                sb.AppendLine("   <Column ss:Width=\"260\"/>");
                sb.AppendLine("   <Column ss:Width=\"140\"/>");
                sb.AppendLine("   <Column ss:Width=\"140\"/>");
                sb.AppendLine("   <Column ss:Width=\"140\"/>");
                sb.AppendLine("   <Column ss:Width=\"150\"/>");

                // Encabezados institucionales
                string headerTitulo = tab.Equals("FFC", StringComparison.OrdinalIgnoreCase)
                    ? "FONDO DE FORTALECIMIENTO COOPERATIVO (FFC)"
                    : "FONDO DE GARANTÍA DE DEPÓSITOS (FGD)";

                sb.AppendLine("   <Row ss:Height=\"24\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"4\" ss:StyleID=\"HeaderMain\"><Data ss:Type=\"String\">{headerTitulo}</Data></Cell>");
                sb.AppendLine("   </Row>");
                sb.AppendLine("   <Row ss:Height=\"20\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"4\" ss:StyleID=\"HeaderSub\"><Data ss:Type=\"String\">Facturación de Contribuciones - {tabTitulo} - {model.NomEntidad} ({model.TrimestreTexto} {model.Anio})</Data></Cell>");
                sb.AppendLine("   </Row>");
                sb.AppendLine("   <Row ss:Height=\"10\"></Row>");

                // Encabezado de columnas
                string m1 = model.MesesNombres.Count > 0 ? model.MesesNombres[0] : "Mes 1";
                string m2 = model.MesesNombres.Count > 1 ? model.MesesNombres[1] : "Mes 2";
                string m3 = model.MesesNombres.Count > 2 ? model.MesesNombres[2] : "Mes 3";

                sb.AppendLine("   <Row ss:Height=\"24\">");
                sb.AppendLine("    <Cell ss:StyleID=\"ColHeader\"><Data ss:Type=\"String\">Detalle expresado en colones</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"ColHeader\"><Data ss:Type=\"String\">{m1}</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"ColHeader\"><Data ss:Type=\"String\">{m2}</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"ColHeader\"><Data ss:Type=\"String\">{m3}</Data></Cell>");
                sb.AppendLine("    <Cell ss:StyleID=\"ColHeader\"><Data ss:Type=\"String\">Promedio</Data></Cell>");
                sb.AppendLine("   </Row>");

                // Filas de datos
                foreach (var fila in pestana.Filas)
                {
                    if (fila.EsHeaderGrupo)
                    {
                        sb.AppendLine("   <Row ss:Height=\"22\">");
                        sb.AppendLine($"    <Cell ss:MergeAcross=\"4\" ss:StyleID=\"CellTextBold\"><Data ss:Type=\"String\">{fila.Concepto}</Data></Cell>");
                        sb.AppendLine("   </Row>");
                        continue;
                    }

                    string textStyle = fila.EsTotal ? "CellTextBold" : "CellText";
                    string numStyle = fila.EsTotal ? "CellNumBold" : "CellNum";
                    string indent = fila.IndentNivel > 0 ? "    " : "";

                    sb.AppendLine("   <Row ss:Height=\"20\">");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{textStyle}\"><Data ss:Type=\"String\">{indent}{fila.Concepto}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{numStyle}\"><Data ss:Type=\"Number\">{fila.Mes1.ToString("F2", CultureInfo.InvariantCulture)}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{numStyle}\"><Data ss:Type=\"Number\">{fila.Mes2.ToString("F2", CultureInfo.InvariantCulture)}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{numStyle}\"><Data ss:Type=\"Number\">{fila.Mes3.ToString("F2", CultureInfo.InvariantCulture)}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{numStyle}\"><Data ss:Type=\"Number\">{fila.Promedio.ToString("F2", CultureInfo.InvariantCulture)}</Data></Cell>");
                    sb.AppendLine("   </Row>");
                }

                // Fila destacada Pago Trimestral
                sb.AppendLine("   <Row ss:Height=\"22\">");
                sb.AppendLine("    <Cell ss:MergeAcross=\"3\" ss:StyleID=\"PagoTrimestralLbl\"><Data ss:Type=\"String\">PAGO TRIMESTRAL</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"PagoTrimestral\"><Data ss:Type=\"Number\">{pestana.PagoTrimestral.ToString("F2", CultureInfo.InvariantCulture)}</Data></Cell>");
                sb.AppendLine("   </Row>");

                sb.AppendLine("  </Table>");
                sb.AppendLine(" </Worksheet>");
                sb.AppendLine("</Workbook>");

                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                string safeName = $"Facturacion_{tabTitulo.Replace(" ", "_").Replace("(", "").Replace(")", "")}_{model.Anio}_T{model.Trimestre}.xls";
                return File(bytes, "application/vnd.ms-excel", safeName);
            }
            catch (Exception ex)
            {
                return Content("Error al exportar: " + ex.Message);
            }
        }

        private FacturacionViewModel CargarDatosFacturacion(string idEntidad, int anio, int trimestre)
        {
            var model = new FacturacionViewModel
            {
                IdEntidad = idEntidad,
                Anio = anio,
                Trimestre = trimestre,
                TrimestreTexto = ObtenerTextoTrimestre(trimestre)
            };

            // Resolver nombre de la entidad
            try
            {
                if (!string.IsNullOrEmpty(idEntidad))
                {
                    using (var entService = new FGA_En_Linea.EntidadService.EntidadServiceClient())
                    {
                        var e = entService.Get(idEntidad);
                        if (e != null)
                        {
                            model.NomEntidad = e.Nombre;
                        }
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(model.NomEntidad) && Session["NomEntidad"] != null)
            {
                model.NomEntidad = Session["NomEntidad"].ToString();
            }

            // Resolver logotipo de la entidad
            model.LogoUrl = ResolverRutaLogo(idEntidad, model.NomEntidad);
            Session["Logo"] = model.LogoUrl;

            // Configurar nombres de los meses del trimestre
            string[] nombresMeses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Setiembre", "Octubre", "Noviembre", "Diciembre" };
            int startMonth = (trimestre - 1) * 3 + 1;
            int m1 = Math.Max(1, Math.Min(12, startMonth));
            int m2 = Math.Max(1, Math.Min(12, startMonth + 1));
            int m3 = Math.Max(1, Math.Min(12, startMonth + 2));

            model.MesesNombres = new List<string>
            {
                $"{nombresMeses[m1]} {anio}",
                $"{nombresMeses[m2]} {anio}",
                $"{nombresMeses[m3]} {anio}"
            };

            if (string.IsNullOrEmpty(idEntidad))
            {
                return model;
            }

            // Consultar datos de ambos procedimientos almacenados
            try
            {
                using (var spClient = new FGA_En_Linea.SPService.SPClient())
                {
                    // 1. FGD (BCCR)
                    try
                    {
                        var dataFGD = spClient.FGA_Consultar_FacturacionFGD(idEntidad, anio, trimestre);
                        if (dataFGD != null && dataFGD.Length > 0)
                        {
                            model.TieneDatos = true;
                            var listaFGD = dataFGD.OrderBy(d => d.ID ?? 0).ToList();
                            model.FGD = ConstruirPestanaFGD(listaFGD, model.MesesNombres);
                        }
                    }
                    catch (Exception exFGD)
                    {
                        System.Diagnostics.Debug.WriteLine("Error al consultar FGD: " + exFGD.Message);
                    }

                    // 2. FEE-FFC
                    try
                    {
                        var dataFFC = spClient.FGA_Consultar_FacturacionFFC(idEntidad, anio, trimestre);
                        if (dataFFC != null && dataFFC.Length > 0)
                        {
                            model.TieneDatos = true;
                            var listaFFC = dataFFC.OrderBy(d => d.ID ?? 0).ToList();
                            model.FFC = ConstruirPestanaFFC(listaFFC, model.MesesNombres);
                        }
                    }
                    catch (Exception exFFC)
                    {
                        System.Diagnostics.Debug.WriteLine("Error al consultar FFC: " + exFFC.Message);
                        // Fallback con datos de FGD si FFC no está disponible
                        if (model.FGD != null && model.FGD.Filas.Count > 0)
                        {
                            model.FFC = ConstruirPestanaFFCDesdeFGD(spClient.FGA_Consultar_FacturacionFGD(idEntidad, anio, trimestre).OrderBy(d => d.ID ?? 0).ToList(), model.MesesNombres);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al consultar facturación: " + ex.Message);
            }

            return model;
        }

        private FacturacionPestanaModel ConstruirPestanaFFC(List<Entities.Entities.Procedures.FGA_Consultar_FacturacionFFC_Result> data, List<string> mesesNombres)
        {
            var p = new FacturacionPestanaModel
            {
                Codigo = "FFC",
                Titulo = "FEE-FFC",
                Subtitulo = "Cálculo y detalle de facturación de contribuciones FEE-FFC"
            };

            var d1 = data.FirstOrDefault(d => d.ID == 1) ?? (data.Count > 0 ? data[0] : null);
            var d2 = data.FirstOrDefault(d => d.ID == 2) ?? (data.Count > 1 ? data[1] : null);
            var d3 = data.FirstOrDefault(d => d.ID == 3) ?? (data.Count > 2 ? data[2] : null);
            var d4 = data.FirstOrDefault(d => d.ID == 4 || (d.PERIODO != null && d.PERIODO.IndexOf("Promedio", StringComparison.OrdinalIgnoreCase) >= 0));

            int countMeses = (d1 != null ? 1 : 0) + (d2 != null ? 1 : 0) + (d3 != null ? 1 : 0);
            if (countMeses == 0) countMeses = 1;

            decimal saldo1 = d1?.SALDOTOTAL ?? 0;
            decimal saldo2 = d2?.SALDOTOTAL ?? 0;
            decimal saldo3 = d3?.SALDOTOTAL ?? 0;
            decimal promSaldo = d4?.SALDOTOTAL ?? Promedio(saldo1, saldo2, saldo3, countMeses);

            decimal cobTotal1 = d1?.COBERTURATOTAL ?? 0;
            decimal cobTotal2 = d2?.COBERTURATOTAL ?? 0;
            decimal cobTotal3 = d3?.COBERTURATOTAL ?? 0;
            decimal promCobTotal = d4?.COBERTURATOTAL ?? Promedio(cobTotal1, cobTotal2, cobTotal3, countMeses);

            decimal cobParcial1 = d1?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial2 = d2?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial3 = d3?.COBERTURAPARCIAL ?? 0;
            decimal promCobParcial = d4?.COBERTURAPARCIAL ?? Promedio(cobParcial1, cobParcial2, cobParcial3, countMeses);

            decimal base1 = d1?.TOTALBASE ?? 0;
            decimal base2 = d2?.TOTALBASE ?? 0;
            decimal base3 = d3?.TOTALBASE ?? 0;
            decimal promBase = d4?.TOTALBASE ?? Promedio(base1, base2, base3, countMeses);

            // Porcentaje Contribución FEE-FFC
            decimal porcFFC = d3?.POR_CONTRIBUCION_FFC ?? (d4?.POR_CONTRIBUCION_FFC ?? (d1?.POR_CONTRIBUCION_FFC ?? 0.045m));
            string porcTexto = FormatearPorcentaje(porcFFC, 0.045m);

            decimal aporteFFC1 = d1?.TOTALAPORTE_FFC ?? (base1 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal aporteFFC2 = d2?.TOTALAPORTE_FFC ?? (base2 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal aporteFFC3 = d3?.TOTALAPORTE_FFC ?? (base3 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal promAporteFFC = d4?.TOTALAPORTE_FFC ?? (d4?.TOTALAPORTE ?? Promedio(aporteFFC1, aporteFFC2, aporteFFC3, countMeses));

            // Pago Trimestral: Viene en PROMEDIO_FFC de la fila 4 (o fila 3), o calculado como Aporte Promedio / 4
            if (d4?.PROMEDIO_FFC.HasValue == true && d4.PROMEDIO_FFC.Value > 0)
            {
                p.PagoTrimestral = d4.PROMEDIO_FFC.Value;
            }
            else if (d3?.PROMEDIO_FFC.HasValue == true && d3.PROMEDIO_FFC.Value > 0)
            {
                p.PagoTrimestral = d3.PROMEDIO_FFC.Value;
            }
            else
            {
                p.PagoTrimestral = promAporteFFC / 4m;
            }

            p.Filas = new List<FacturaRubroItem>
            {
                new FacturaRubroItem
                {
                    Concepto = "Saldo total de obligaciones con el público",
                    Mes1 = saldo1,
                    Mes2 = saldo2,
                    Mes3 = saldo3,
                    Promedio = promSaldo,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Base de cálculo depósitos ajustados",
                    EsHeaderGrupo = true
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura total de depósitos",
                    Mes1 = cobTotal1,
                    Mes2 = cobTotal2,
                    Mes3 = cobTotal3,
                    Promedio = promCobTotal,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura parcial de depósitos",
                    Mes1 = cobParcial1,
                    Mes2 = cobParcial2,
                    Mes3 = cobParcial3,
                    Promedio = promCobParcial,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de la base de depósitos ajustados",
                    Mes1 = base1,
                    Mes2 = base2,
                    Mes3 = base3,
                    Promedio = promBase,
                    EsTotal = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = $"Contribución FEE de FFC ({porcTexto})",
                    Mes1 = aporteFFC1,
                    Mes2 = aporteFFC2,
                    Mes3 = aporteFFC3,
                    Promedio = promAporteFFC,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de aporte a la contribución anual",
                    Mes1 = aporteFFC1,
                    Mes2 = aporteFFC2,
                    Mes3 = aporteFFC3,
                    Promedio = promAporteFFC,
                    EsTotal = true,
                    EsDestacado = true,
                    IndentNivel = 0
                }
            };

            // Indicadores de Ahorrantes
            var soloMeses = data.Where(d => d.ID == null || d.ID <= 3).ToList();
            for (int i = 0; i < soloMeses.Count; i++)
            {
                var item = soloMeses[i];
                p.IndicadoresMeses.Add(new FacturaIndicadorMes
                {
                    Periodo = item.PERIODO ?? "",
                    MesNombre = (i < mesesNombres.Count) ? mesesNombres[i] : $"Mes {i + 1}",
                    CantidadTotalAhorrantes = item.CANTIDAD_TOTAL ?? 0,
                    CantidadCubierto100 = item.CANTIDAD_AH_CUBIERTO_100 ?? 0,
                    PorcCubierto100 = item.PORC_AH_CUBIERTO_100 ?? 0,
                    PorcCubiertoDepositos = item.PORC_CUBIERTO_DEPOSITOS ?? 0
                });
            }

            return p;
        }

        private FacturacionPestanaModel ConstruirPestanaFFCDesdeFGD(List<Entities.Entities.Procedures.FGA_Consultar_FacturacionFGD_Result> data, List<string> mesesNombres)
        {
            var p = new FacturacionPestanaModel
            {
                Codigo = "FFC",
                Titulo = "FEE-FFC",
                Subtitulo = "Cálculo y detalle de facturación de contribuciones FEE-FFC"
            };

            var d1 = data.FirstOrDefault(d => d.ID == 1) ?? (data.Count > 0 ? data[0] : null);
            var d2 = data.FirstOrDefault(d => d.ID == 2) ?? (data.Count > 1 ? data[1] : null);
            var d3 = data.FirstOrDefault(d => d.ID == 3) ?? (data.Count > 2 ? data[2] : null);
            var d4 = data.FirstOrDefault(d => d.ID == 4 || (d.PERIODO != null && d.PERIODO.IndexOf("Promedio", StringComparison.OrdinalIgnoreCase) >= 0));

            int countMeses = (d1 != null ? 1 : 0) + (d2 != null ? 1 : 0) + (d3 != null ? 1 : 0);
            if (countMeses == 0) countMeses = 1;

            decimal saldo1 = d1?.SALDOTOTAL ?? 0;
            decimal saldo2 = d2?.SALDOTOTAL ?? 0;
            decimal saldo3 = d3?.SALDOTOTAL ?? 0;
            decimal promSaldo = d4?.SALDOTOTAL ?? Promedio(saldo1, saldo2, saldo3, countMeses);

            decimal cobTotal1 = d1?.COBERTURATOTAL ?? 0;
            decimal cobTotal2 = d2?.COBERTURATOTAL ?? 0;
            decimal cobTotal3 = d3?.COBERTURATOTAL ?? 0;
            decimal promCobTotal = d4?.COBERTURATOTAL ?? Promedio(cobTotal1, cobTotal2, cobTotal3, countMeses);

            decimal cobParcial1 = d1?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial2 = d2?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial3 = d3?.COBERTURAPARCIAL ?? 0;
            decimal promCobParcial = d4?.COBERTURAPARCIAL ?? Promedio(cobParcial1, cobParcial2, cobParcial3, countMeses);

            decimal base1 = d1?.TOTALBASE ?? 0;
            decimal base2 = d2?.TOTALBASE ?? 0;
            decimal base3 = d3?.TOTALBASE ?? 0;
            decimal promBase = d4?.TOTALBASE ?? Promedio(base1, base2, base3, countMeses);

            decimal porcFFC = d3?.POR_CONTRIBUCION_FFC ?? (d4?.POR_CONTRIBUCION_FFC ?? (d1?.POR_CONTRIBUCION_FFC ?? 0.045m));
            string porcTexto = FormatearPorcentaje(porcFFC, 0.045m);

            decimal aporteFFC1 = d1?.TOTALAPORTE_FFC ?? (base1 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal aporteFFC2 = d2?.TOTALAPORTE_FFC ?? (base2 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal aporteFFC3 = d3?.TOTALAPORTE_FFC ?? (base3 * (porcFFC > 1 ? porcFFC / 100m : porcFFC));
            decimal promAporteFFC = d4?.TOTALAPORTE_FFC ?? (d4?.TOTALAPORTE ?? Promedio(aporteFFC1, aporteFFC2, aporteFFC3, countMeses));

            if (d4?.PROMEDIO_FFC.HasValue == true && d4.PROMEDIO_FFC.Value > 0)
            {
                p.PagoTrimestral = d4.PROMEDIO_FFC.Value;
            }
            else if (d3?.PROMEDIO_FFC.HasValue == true && d3.PROMEDIO_FFC.Value > 0)
            {
                p.PagoTrimestral = d3.PROMEDIO_FFC.Value;
            }
            else
            {
                p.PagoTrimestral = promAporteFFC / 4m;
            }

            p.Filas = new List<FacturaRubroItem>
            {
                new FacturaRubroItem
                {
                    Concepto = "Saldo total de obligaciones con el público",
                    Mes1 = saldo1,
                    Mes2 = saldo2,
                    Mes3 = saldo3,
                    Promedio = promSaldo,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Base de cálculo depósitos ajustados",
                    EsHeaderGrupo = true
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura total de depósitos",
                    Mes1 = cobTotal1,
                    Mes2 = cobTotal2,
                    Mes3 = cobTotal3,
                    Promedio = promCobTotal,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura parcial de depósitos",
                    Mes1 = cobParcial1,
                    Mes2 = cobParcial2,
                    Mes3 = cobParcial3,
                    Promedio = promCobParcial,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de la base de depósitos ajustados",
                    Mes1 = base1,
                    Mes2 = base2,
                    Mes3 = base3,
                    Promedio = promBase,
                    EsTotal = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = $"Contribución FEE de FFC ({porcTexto})",
                    Mes1 = aporteFFC1,
                    Mes2 = aporteFFC2,
                    Mes3 = aporteFFC3,
                    Promedio = promAporteFFC,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de aporte a la contribución anual",
                    Mes1 = aporteFFC1,
                    Mes2 = aporteFFC2,
                    Mes3 = aporteFFC3,
                    Promedio = promAporteFFC,
                    EsTotal = true,
                    EsDestacado = true,
                    IndentNivel = 0
                }
            };

            var soloMeses = data.Where(d => d.ID == null || d.ID <= 3).ToList();
            for (int i = 0; i < soloMeses.Count; i++)
            {
                var item = soloMeses[i];
                p.IndicadoresMeses.Add(new FacturaIndicadorMes
                {
                    Periodo = item.PERIODO ?? "",
                    MesNombre = (i < mesesNombres.Count) ? mesesNombres[i] : $"Mes {i + 1}",
                    CantidadTotalAhorrantes = item.CANTIDAD_TOTAL ?? 0,
                    CantidadCubierto100 = item.CANTIDAD_AH_CUBIERTO_100 ?? 0,
                    PorcCubierto100 = item.PORC_AH_CUBIERTO_100 ?? 0,
                    PorcCubiertoDepositos = item.PORC_CUBIERTO_DEPOSITOS ?? 0
                });
            }

            return p;
        }
        private FacturacionPestanaModel ConstruirPestanaFGD(List<Entities.Entities.Procedures.FGA_Consultar_FacturacionFGD_Result> data, List<string> mesesNombres)
        {
            var p = new FacturacionPestanaModel
            {
                Codigo = "FGD",
                Titulo = "FGD (BCCR)",
                Subtitulo = "Cálculo y detalle de facturación de contribuciones FGD BCCR"
            };

            var d1 = data.FirstOrDefault(d => d.ID == 1) ?? (data.Count > 0 ? data[0] : null);
            var d2 = data.FirstOrDefault(d => d.ID == 2) ?? (data.Count > 1 ? data[1] : null);
            var d3 = data.FirstOrDefault(d => d.ID == 3) ?? (data.Count > 2 ? data[2] : null);
            var d4 = data.FirstOrDefault(d => d.ID == 4 || (d.PERIODO != null && d.PERIODO.IndexOf("Promedio", StringComparison.OrdinalIgnoreCase) >= 0));

            int countMeses = (d1 != null ? 1 : 0) + (d2 != null ? 1 : 0) + (d3 != null ? 1 : 0);
            if (countMeses == 0) countMeses = 1;

            decimal saldo1 = d1?.SALDOTOTAL ?? 0;
            decimal saldo2 = d2?.SALDOTOTAL ?? 0;
            decimal saldo3 = d3?.SALDOTOTAL ?? 0;
            decimal promSaldo = d4?.SALDOTOTAL ?? Promedio(saldo1, saldo2, saldo3, countMeses);

            decimal cobTotal1 = d1?.COBERTURATOTAL ?? 0;
            decimal cobTotal2 = d2?.COBERTURATOTAL ?? 0;
            decimal cobTotal3 = d3?.COBERTURATOTAL ?? 0;
            decimal promCobTotal = d4?.COBERTURATOTAL ?? Promedio(cobTotal1, cobTotal2, cobTotal3, countMeses);

            decimal cobParcial1 = d1?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial2 = d2?.COBERTURAPARCIAL ?? 0;
            decimal cobParcial3 = d3?.COBERTURAPARCIAL ?? 0;
            decimal promCobParcial = d4?.COBERTURAPARCIAL ?? Promedio(cobParcial1, cobParcial2, cobParcial3, countMeses);

            decimal base1 = d1?.TOTALBASE ?? 0;
            decimal base2 = d2?.TOTALBASE ?? 0;
            decimal base3 = d3?.TOTALBASE ?? 0;
            decimal promBase = d4?.TOTALBASE ?? Promedio(base1, base2, base3, countMeses);

            decimal porcDep1 = d1?.PORC_CUBIERTO_DEPOSITOS ?? 0;
            decimal porcDep2 = d2?.PORC_CUBIERTO_DEPOSITOS ?? 0;
            decimal porcDep3 = d3?.PORC_CUBIERTO_DEPOSITOS ?? 0;
            decimal promPorcDep = d4?.PORC_CUBIERTO_DEPOSITOS ?? Promedio(porcDep1, porcDep2, porcDep3, countMeses);

            decimal ah1 = d1?.CANTIDAD_AH_CUBIERTO_100 ?? 0;
            decimal ah2 = d2?.CANTIDAD_AH_CUBIERTO_100 ?? 0;
            decimal ah3 = d3?.CANTIDAD_AH_CUBIERTO_100 ?? 0;
            decimal promAh = d4?.CANTIDAD_AH_CUBIERTO_100 ?? Promedio(ah1, ah2, ah3, countMeses);

            decimal porcAh1 = d1?.PORC_AH_CUBIERTO_100 ?? 0;
            decimal porcAh2 = d2?.PORC_AH_CUBIERTO_100 ?? 0;
            decimal porcAh3 = d3?.PORC_AH_CUBIERTO_100 ?? 0;
            decimal promPorcAh = d4?.PORC_AH_CUBIERTO_100 ?? Promedio(porcAh1, porcAh2, porcAh3, countMeses);

            decimal porcCF = d3?.POR_CONTRIBUCION_BCCR ?? (d4?.POR_CONTRIBUCION_BCCR ?? (d1?.POR_CONTRIBUCION_BCCR ?? 0.10m));
            decimal porcCAR = d3?.POR_CONTRIBUCION_CAR ?? (d4?.POR_CONTRIBUCION_CAR ?? (d1?.POR_CONTRIBUCION_CAR ?? 0.01m));

            string cfTexto = FormatearPorcentaje(porcCF, 0.10m);
            string carTexto = FormatearPorcentaje(porcCAR, 0.01m);

            decimal cf1 = d1?.CF ?? 0;
            decimal cf2 = d2?.CF ?? 0;
            decimal cf3 = d3?.CF ?? 0;
            decimal promCF = d4?.CF ?? Promedio(cf1, cf2, cf3, countMeses);

            decimal car1 = d1?.CAR ?? 0;
            decimal car2 = d2?.CAR ?? 0;
            decimal car3 = d3?.CAR ?? 0;
            decimal promCAR = d4?.CAR ?? Promedio(car1, car2, car3, countMeses);

            decimal aporte1 = d1?.TOTALAPORTE ?? (cf1 + car1);
            decimal aporte2 = d2?.TOTALAPORTE ?? (cf2 + car2);
            decimal aporte3 = d3?.TOTALAPORTE ?? (cf3 + car3);
            decimal promAporte = d4?.TOTALAPORTE ?? Promedio(aporte1, aporte2, aporte3, countMeses);

            if (d4?.PROMEDIO.HasValue == true && d4.PROMEDIO.Value > 0)
            {
                p.PagoTrimestral = d4.PROMEDIO.Value;
            }
            else if (d3?.PROMEDIO.HasValue == true && d3.PROMEDIO.Value > 0)
            {
                p.PagoTrimestral = d3.PROMEDIO.Value;
            }
            else
            {
                p.PagoTrimestral = promAporte / 4m;
            }

            p.Filas = new List<FacturaRubroItem>
            {
                new FacturaRubroItem
                {
                    Concepto = "Saldo total de obligaciones con el público",
                    Mes1 = saldo1,
                    Mes2 = saldo2,
                    Mes3 = saldo3,
                    Promedio = promSaldo,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Base de cálculo depósitos ajustados",
                    EsHeaderGrupo = true
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura total de depósitos",
                    Mes1 = cobTotal1,
                    Mes2 = cobTotal2,
                    Mes3 = cobTotal3,
                    Promedio = promCobTotal,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Cobertura parcial de depósitos",
                    Mes1 = cobParcial1,
                    Mes2 = cobParcial2,
                    Mes3 = cobParcial3,
                    Promedio = promCobParcial,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de la base de depósitos ajustados",
                    Mes1 = base1,
                    Mes2 = base2,
                    Mes3 = base3,
                    Promedio = promBase,
                    EsTotal = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "% Cubierto del total de depósitos",
                    Mes1 = porcDep1,
                    Mes2 = porcDep2,
                    Mes3 = porcDep3,
                    Promedio = promPorcDep,
                    EsPorcentaje = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Cantidad de ahorrantes cubiertos al 100%",
                    Mes1 = ah1,
                    Mes2 = ah2,
                    Mes3 = ah3,
                    Promedio = promAh,
                    EsEntero = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "% De ahorrantes cubiertos al 100%",
                    Mes1 = porcAh1,
                    Mes2 = porcAh2,
                    Mes3 = porcAh3,
                    Promedio = promPorcAh,
                    EsPorcentaje = true,
                    IndentNivel = 0
                },
                new FacturaRubroItem
                {
                    Concepto = "Contribución al FGD BCCR",
                    EsHeaderGrupo = true
                },
                new FacturaRubroItem
                {
                    Concepto = $"CF ({cfTexto})",
                    Mes1 = cf1,
                    Mes2 = cf2,
                    Mes3 = cf3,
                    Promedio = promCF,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = $"CAR ({carTexto})",
                    Mes1 = car1,
                    Mes2 = car2,
                    Mes3 = car3,
                    Promedio = promCAR,
                    IndentNivel = 1
                },
                new FacturaRubroItem
                {
                    Concepto = "Total de aporte a la contribución anual",
                    Mes1 = aporte1,
                    Mes2 = aporte2,
                    Mes3 = aporte3,
                    Promedio = promAporte,
                    EsTotal = true,
                    EsDestacado = true,
                    IndentNivel = 0
                }
            };

            var soloMeses = data.Where(d => d.ID == null || d.ID <= 3).ToList();
            for (int i = 0; i < soloMeses.Count; i++)
            {
                var item = soloMeses[i];
                p.IndicadoresMeses.Add(new FacturaIndicadorMes
                {
                    Periodo = item.PERIODO ?? "",
                    MesNombre = (i < mesesNombres.Count) ? mesesNombres[i] : $"Mes {i + 1}",
                    CantidadTotalAhorrantes = item.CANTIDAD_TOTAL ?? 0,
                    CantidadCubierto100 = item.CANTIDAD_AH_CUBIERTO_100 ?? 0,
                    PorcCubierto100 = item.PORC_AH_CUBIERTO_100 ?? 0,
                    PorcCubiertoDepositos = item.PORC_CUBIERTO_DEPOSITOS ?? 0
                });
            }

            return p;
        }

        private static decimal Promedio(decimal v1, decimal v2, decimal v3, int count)
        {
            if (count <= 0) return 0;
            return (v1 + v2 + v3) / count;
        }

        private static string FormatearPorcentaje(decimal val, decimal defaultVal)
        {
            decimal p = val > 0 ? val : defaultVal;
            // Si viene en escala 0.00045 multiplicarlo por 100, si ya viene en 0.045 usarlo directo
            if (p < 0.001m)
            {
                p *= 100m;
            }
            return p.ToString("0.000", FGAConstants.CulturaCR) + "%";
        }

        private static string ObtenerTextoTrimestre(int trimestre)
        {
            switch (trimestre)
            {
                case 1: return "I Trimestre";
                case 2: return "II Trimestre";
                case 3: return "III Trimestre";
                case 4: return "IV Trimestre";
                default: return $"{trimestre} Trimestre";
            }
        }

        private string ResolverRutaLogo(string idEnt, string nomEntidad)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomEntidad) && !string.IsNullOrWhiteSpace(idEnt))
                {
                    try
                    {
                        var entService = new FGA_En_Linea.EntidadService.EntidadServiceClient();
                        try
                        {
                            var e = entService.Get(idEnt);
                            if (e != null) nomEntidad = e.Nombre;
                        }
                        finally
                        {
                            entService.SafeClose();
                        }
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(nomEntidad)) nomEntidad = "FFC";

                string imagesPath = Server.MapPath("~/Content/images");

                // 1. Probar con el nombre tal cual
                string fileName1 = nomEntidad.Trim() + ".jpg";
                if (System.IO.File.Exists(System.IO.Path.Combine(imagesPath, fileName1)))
                {
                    return Url.Content("~/Content/images/" + fileName1);
                }

                // 2. Probar en minúsculas y sin caracteres especiales
                string cleanName = nomEntidad.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("_", "").ToLower();
                string fileName2 = cleanName + ".jpg";
                if (System.IO.File.Exists(System.IO.Path.Combine(imagesPath, fileName2)))
                {
                    return Url.Content("~/Content/images/" + fileName2);
                }

                // 3. Buscar por coincidencia parcial en los archivos existentes de Content/images
                if (System.IO.Directory.Exists(imagesPath))
                {
                    var files = System.IO.Directory.GetFiles(imagesPath, "*.jpg");
                    foreach (var f in files)
                    {
                        string fn = System.IO.Path.GetFileNameWithoutExtension(f).ToLower();
                        if (fn.Length >= 4 && (cleanName.Contains(fn) || fn.Contains(cleanName)))
                        {
                            return Url.Content("~/Content/images/" + System.IO.Path.GetFileName(f));
                        }
                    }
                }

                return Url.Content("~/Content/images/FFC.jpg");
            }
            catch
            {
                return Url.Content("~/Content/images/FFC.jpg");
            }
        }
    }
}