using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Entities.Entities.Procedures;
using FGA.Model;

namespace FGA.Controllers
{
    public class MatrizController : BaseController
    {

        public void CargaInicial()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
            
            if(Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-13).ToShortDateString();
            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            Session["PeriodoIC"] = Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()).AddMonths(-24).ToShortDateString();
            Session["PeriodoFC"] = Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()).AddMonths(-12).ToShortDateString();

        }

        public ActionResult Buscar(String Entidades, DateTime PeriodoI, DateTime PeriodoF, DateTime PeriodoIC, DateTime PeriodoFC)
        {
            Modelo_Matris m = new Modelo_Matris();

            try
            {             
                Session["IdEntidad"] = Entidades;
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
                Session["PeriodoIC"] = PeriodoIC.ToShortDateString();
                Session["PeriodoFC"] = PeriodoFC.ToShortDateString();
                Load();
                m = GetGraph();

                if (PeriodoFC < PeriodoIC.AddMonths(12))
                {
                    m.validarFechas = "(*) La fechas deben tener mínimo 12 meses de diferencia";
                    Session["PeriodoIC"] = PeriodoFC.AddMonths(-12).ToShortDateString();
                }
            }
            catch (Exception)
            {
            }

            return View("Index", m);
        }

        public ActionResult BuscarProbabilidad(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
                Load();
            }
            catch (Exception)
            {
            }

            return View("Probabilidad");
        }

        public ActionResult Index()
        {

            CargaInicial();
            Modelo_Matris m = GetGraph();
            return View(m);
        }


        public ActionResult Probabilidad()
        {
            CargaInicial();
            return View();
        }


        public ActionResult GetGrid()
        {
            try
            {
                var tak = sp.FGA_Consultar_Matrices_Desmejora_Cartera(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["PeriodoIC"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["PeriodoFC"].ToString()));

                var result = from c in tak
                             select new string[] {c.ID.ToString(), c.RIESGO,
                                 Utility.Utilitarios.ConvertirAString(c.PROBABILIDAD_DESMEJORA_COMP.Value) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.PROBABILIDAD_DESMEJORA.Value) + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridProbabilidad()
        {
            try
            {
                var tak = sp.FGA_Consultar_Matrices_Probabilidada(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

                var result = from c in tak
                             select new string[] {c.ID_INICIO.ToString(), c.FILA_INICIO, c.COLUMNA_FINAL,
                                 Utility.Utilitarios.ConvertirAString(c.PROBABILIDAD.Value) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.SALDO.Value)
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }


        private Modelo_Matris GetGraph()
        {
            Modelo_Matris m = new Modelo_Matris();

            var tak = sp.FGA_Consultar_Matrices_Variacion_Cartera(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

            if (tak.Count() > 0)
            {
                /*Concentración al día*/
                FGA_Consultar_Matrices_Variacion_Cartera_Result alDia = tak.FirstOrDefault(o => o.RIESGO == "Al dia");
                object[] Inicial = new object[2];
                Inicial[0] = alDia.INICIAL;
                Inicial[1] = alDia.FINAL;

                object[] Final = new object[2];
                Final[0] = null;
                Final[1] = alDia.VARIACION;

                HighChart.ConfigChart(ref m.concentracionAlDia, "AlDia", null);
                HighChart.ConfigChart(ref m.pConcentracionAlDia, "pAlDia", null);
                var XAxis = new XAxis
                {
                    Categories = new[] { "Concentración Inicial", "Concentración Final" },
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                };

                m.concentracionAlDia.SetXAxis(XAxis);
                m.pConcentracionAlDia.SetXAxis(XAxis);

                m.concentracionAlDia.SetYAxis(HighChart.GetYAxis(0, 100, "formatPercent"));
                m.pConcentracionAlDia.SetYAxis(HighChart.GetYAxis(0, 100, "formatPercent"));

                m.concentracionAlDia.SetPlotOptions(HighChart.getLabelStackingPercent());
                m.pConcentracionAlDia.SetPlotOptions(HighChart.getLabelStackingPercent());

                var series = new Series[]
                {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Al día",
                            Data = new Data(Inicial),
                            Color = HighChart.GetColor(0),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Variación",
                            Data = new Data(Final),
                            Color = HighChart.GetColor(1),
                        }
                };

                m.concentracionAlDia.SetSeries(series);
                m.pConcentracionAlDia.SetSeries(series);

                /*Concentración*/
                List<FGA_Consultar_Matrices_Variacion_Cartera_Result> cartera = tak.Where(o => o.RIESGO != "Al dia").ToList();

                object[] InicialCartera = new object[cartera.Count()];
                object[] FinalCartera = new object[cartera.Count()];
                string[] Riesgos = new string[cartera.Count()];
                int i = 0;

                foreach (FGA_Consultar_Matrices_Variacion_Cartera_Result detalle in cartera)
                {
                    Riesgos[i] = detalle.RIESGO;
                    InicialCartera[i] = detalle.INICIAL;
                    FinalCartera[i] = detalle.FINAL;
                    i += 1;
                }

                HighChart.ConfigChart(ref m.concentracion, "Cartera", null);
                HighChart.ConfigChart(ref m.pConcentracion, "pCartera", null);
                var xAxis = new XAxis
                {
                    Categories = Riesgos,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                };
                m.concentracion.SetXAxis(xAxis);
                m.pConcentracion.SetXAxis(xAxis);

                m.concentracion.SetPlotOptions(HighChart.getLabelPercent());
                m.pConcentracion.SetPlotOptions(HighChart.getLabelPercent());

                m.concentracion.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.pConcentracion.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));

                series = new Series[]
                {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Concentración Inicial",
                            Data = new Data(InicialCartera),
                            Color = HighChart.GetColor(0),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Concentración Final",
                            Data = new Data(FinalCartera),
                            Color = HighChart.GetColor(1),
                        }
                };

                m.concentracion.SetSeries(series);
                m.pConcentracion.SetSeries(series);
            }

            var info = sp.FGA_Consultar_Matrices_PerdidaEstimada(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

            /*Riesgo de Activo*/
            if (info.Count() > 0)
            {
                HighChart.ConfigChart(ref m.riesgoActivo, "RiesgoActivo", null);
                HighChart.ConfigChart(ref m.pRiesgoActivo, "pRiesgoActivo", null);

                string[] Fechas = new string[info.Count()];
                object[] Perdida = new object[info.Count()];
                object[] Morosidad = new object[info.Count()];
                int i = 0;

                foreach (FGA_Consultar_Matrices_PerdidaEstimada_Result detalle in info)
                {
                    Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                    Perdida[i] = detalle.PERDIDA_ESTIMADA;
                    Morosidad[i] = detalle.MOROSIDAD_90DIAS;
                    i += 1;
                }

                m.riesgoActivo.SetXAxis(HighChart.GetXAxis(Fechas));
                m.pRiesgoActivo.SetXAxis(HighChart.GetXAxis(Fechas));

                m.riesgoActivo.SetPlotOptions(HighChart.getLabelAmmount());
                m.pRiesgoActivo.SetPlotOptions(HighChart.getLabelAmmount());

                m.riesgoActivo.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.pRiesgoActivo.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));

                var series = new Series[]
                {
                    new Series{
                        Name = "Pérdida Esperada",
                        Data = new Data(Perdida),
                        Color =HighChart.GetColor(0),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Name = "Cartera con morosidad mayor a 90 días / Cartera directa",
                        Data = new Data(Morosidad),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLineDash()
                    }
                };

                m.riesgoActivo.SetSeries(series);
                m.pRiesgoActivo.SetSeries(series);

                /*Perdida calculada por saldos*/
                HighChart.ConfigChart(ref m.perdida, "Perdida", null);
                HighChart.ConfigChart(ref m.pPerdida, "pPerdida", null);

                string[] XAxisPerdidaC = new string[info.Count()];
                object[] PerdidaC = new object[info.Count()];
                object[] Estimaciones = new object[info.Count()];
                object[] Porcentajes = new object[info.Count()];
                i = 0;

                foreach (FGA_Consultar_Matrices_PerdidaEstimada_Result detalle in info)
                {
                    XAxisPerdidaC[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                    PerdidaC[i] = detalle.ESTIMACION_MATRIZ;
                    Estimaciones[i] = detalle.CUENTA139;
                    Porcentajes[i] = detalle.PORCENTAJE;
                    i += 1;
                }
                m.perdida.SetXAxis(HighChart.GetXAxis(XAxisPerdidaC));
                m.pPerdida.SetXAxis(HighChart.GetXAxis(XAxisPerdidaC));

                var yAxis = new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "Porcentaje",
                        GridLineWidth = 0,
                        Title = new YAxisTitle()
                        {
                            Text = "Porcentaje",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                        Labels = new YAxisLabels()
                        {
                            Formatter = "formatPercent",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                        Opposite = true,
                        Min = 0
                    },
                    new YAxis()
                    {
                        Id = "Millones",
                        GridLineWidth = 0,
                        Title = new YAxisTitle()
                        {
                            Text = "Millones",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                        Labels = new YAxisLabels()
                        {
                            Formatter = "formatMillion",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                    }
                }.ToArray();

                m.perdida.SetYAxis(yAxis);
                m.pPerdida.SetYAxis(yAxis);

                m.perdida.SetPlotOptions(HighChart.getLabelAmmount());
                m.pPerdida.SetPlotOptions(HighChart.getLabelAmmount());

                series = new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Porcentaje",
                        Data = new Data(Porcentajes),
                        Color = HighChart.GetColor(0),
                        YAxis = "Porcentaje",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Estimaciones (13900000)",
                        Data = new Data(Estimaciones),
                        Color = HighChart.GetColor(1),
                        YAxis = "Millones"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Pérdida esperada",
                        Data = new Data(PerdidaC),
                        Color = HighChart.GetColor(2),
                        YAxis = "Millones"
                    }
                };

                m.perdida.SetSeries(series);
                m.pPerdida.SetSeries(series);
            }
            return m;
        }

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                sp.Close();
            }
            base.Dispose(disposing);
        }
    }
}