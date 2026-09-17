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
using MicrosoftHelper;

namespace FGA.Controllers
{
    public class TasaInteresController : BaseController
    {
        private Modelo_Tasas Graph
        {
            get
            {
                Modelo_Tasas m = new Modelo_Tasas();

                #region TasaPonderadaSegmento
                var takSegmento = sp.FGA_Consultar_Grafico_Oper_Segmento(Session["IdEntidad"].ToString(),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

                var takEAD = sp.FGA_Consultar_Grafico_Oper_EAD(Session["IdEntidad"].ToString(),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                int i = 0, numSeries = 0;

                if (takSegmento.Count() > 0)
                {
                    try
                    {
                        HighChart.ConfigChart(ref m.tasaPonderadaSegmento, "Segmento", null, 720);
                        HighChart.ConfigChart(ref m.pTasaPonderadaSegmento, "pSegmento", null, 720);

                        int numPeriodos = takSegmento.Select(l => l.Periodo).Distinct().Count();
                        List<string> listaTipos = takSegmento.Select(l => l.Nombre).Distinct().ToList();
                        listaTipos.Add("Tasa ponderada total");

                        List<Serie> listaSeries = new List<Serie>();
                        string[] fechas = new string[numPeriodos];
                        string periodo = string.Empty, periodoActual = string.Empty;

                        for (int j = 0; j < listaTipos.Count(); j++)
                            listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                        foreach (FGA_Consultar_Grafico_Oper_Segmento_Result detalle in takSegmento)
                        {
                            periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();

                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual + 1;


                            if (periodo != periodoActual)
                            {
                                periodo = periodoActual;
                                fechas[i] = periodo;
                                i += 1;
                            }

                            porcentajeActual = detalle.TasaPromedio.Value;
                            for (int j = 0; j < listaTipos.Count(); j++)
                            {
                                if (listaSeries[j].nombre == detalle.Nombre)
                                {
                                    listaSeries[j].total += detalle.TasaPromedio.Value;
                                    listaSeries[j].mostrar = true;
                                    listaSeries[j].valores[i - 1] = detalle.TasaPromedio.Value;
                                }
                            }
                        }

                        listaSeries[listaTipos.Count() - 1].total += takEAD[0].TasaPondera;
                        listaSeries[listaTipos.Count() - 1].mostrar = true;
                        for (int j = 0; j < takEAD.Count(); j++)
                        {
                            listaSeries[listaTipos.Count() - 1].valores[j] = takEAD[j].TasaPondera;
                            listaSeries[listaTipos.Count() - 1].total += takEAD[j].TasaPondera;
                        }

                        listaSeries = listaSeries.OrderByDescending(o => o.total).ToList();
                        m.tasaPonderadaSegmento.SetXAxis(HighChart.GetXAxis(fechas));
                        m.pTasaPonderadaSegmento.SetXAxis(HighChart.GetXAxis(fechas));

                        m.tasaPonderadaSegmento.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                        m.pTasaPonderadaSegmento.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                        m.tasaPonderadaSegmento.SetPlotOptions(HighChart.getLabelStackingNormal());
                        m.pTasaPonderadaSegmento.SetPlotOptions(HighChart.getLabelStackingNormal());

                        numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                        Series[] series = new Series[numSeries];
                        Series serie;
                        i = 0;

                        foreach (Serie detalle in listaSeries)
                        {
                            if (detalle.mostrar)
                            {
                                serie = new Series
                                {
                                    Type = ChartTypes.Column,
                                    Name = detalle.nombre,
                                    Data = new Data(detalle.valores),
                                    Color = HighChart.GetColor(i),
                                };

                                series[i] = serie;
                                i += 1;
                            }
                        }
                        m.tasaPonderadaSegmento.SetSeries(series);
                        m.pTasaPonderadaSegmento.SetSeries(series);
                    }
                    catch (Exception) { }
                }
                #endregion

                #region TasaPonderada

                var tasaPonderada = sp.FGA_Consultar_Tasa_Ponderada(Session["IdEntidad"].ToString(),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));
                if (tasaPonderada.Count() > 0)
                {
                    try
                    {
                        HighChart.ConfigChart(ref m.tasaPonderada, "Ponderada", null, 720);
                        HighChart.ConfigChart(ref m.pTasaPonderada, "pPonderada", null, 720);

                        string[] Fechas = new string[tasaPonderada.Count()];
                        object[] Variacion = new object[tasaPonderada.Count()];
                        i = 0;

                        foreach (FGA_Consultar_Tasa_Ponderada_Result detalle in tasaPonderada)
                        {
                            Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                            Variacion[i] = detalle.Total;
                            i += 1;
                        }

                        m.tasaPonderada.SetXAxis(HighChart.GetXAxis(Fechas));
                        m.pTasaPonderada.SetXAxis(HighChart.GetXAxis(Fechas));

                        m.tasaPonderada.SetPlotOptions(HighChart.getLabelPercent());
                        m.pTasaPonderada.SetPlotOptions(HighChart.getLabelPercent());

                        m.tasaPonderada.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                        m.pTasaPonderada.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                        var series = new Series[]
                        {
                            new Series{
                                Name = "Tasa Ponderada",
                                Data = new Data(Variacion),
                                Color = HighChart.GetColor(0),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplinePercent(3)
                            }
                        };
                        
                        m.tasaPonderada.SetSeries(series);
                        m.pTasaPonderada.SetSeries(series);
                    }
                    catch (Exception) { }
                }
                #endregion

                int opcion = int.Parse(TempData["Opcion"].ToString());
                var tak = sp.FGA_Consultar_Modelo_Tasas(Session["IdEntidad"].ToString(),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                        Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), opcion);

                if (tak.Count() > 0)
                {
                    try
                    {
                        i = 0;
                        decimal maxTasa = 0;
                        object[] Margen = new object[tak.Count()];
                        object[] Activo = new object[tak.Count()];
                        object[] Pasivo = new object[tak.Count()];
                        object[] MargenP = new object[tak.Count()];

                        string[] Fechas = new string[tak.Count()];
                        object[] Credito = new object[tak.Count()];
                        object[] Inversion = new object[tak.Count()];
                        object[] TasaActivo = new object[tak.Count()];
                        object[] TasaPasivo = new object[tak.Count()];
                        object[] TasaActivoP = new object[tak.Count()];
                        object[] TasaPasivoP = new object[tak.Count()];
                        object[] Captaciones = new object[tak.Count()];
                        object[] Obligaciones = new object[tak.Count()];
                        object[] MargenRelativo = new object[tak.Count()];

                        foreach (FGA_Consultar_Modelo_Tasas_Result detalle in tak)
                        {
                            /*Tasa Activa*/
                            Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                            Credito[i] = detalle.TASACARTERA;
                            Inversion[i] = detalle.TASAINVERSION;
                            TasaActivoP[i] = detalle.PTASAACTIVA;
                            TasaActivo[i] = detalle.TASAACTIVA;

                            /*Margen*/
                            Activo[i] = detalle.ACTIVOPRODUCTIVO;
                            Pasivo[i] = detalle.PASIVOCOSTO;
                            TasaPasivo[i] = detalle.TASAPASIVA;
                            TasaPasivoP[i] = detalle.PTASAPASIVA;

                            MargenRelativo[i] = ((detalle.TASAACTIVA * detalle.ACTIVOPRODUCTIVO) - (detalle.TASAPASIVA * detalle.PASIVOCOSTO)) / 100;

                            if ((decimal)TasaActivo[i] > maxTasa)
                                maxTasa = (decimal)TasaActivo[i];
                            if ((decimal)TasaPasivo[i] > maxTasa)
                                maxTasa = (decimal)TasaPasivo[i];
                            if ((decimal)MargenRelativo[i] > maxTasa)
                                maxTasa = (decimal)MargenRelativo[i];

                            /*Tasa Pasivo*/
                            Captaciones[i] = detalle.TASACAPTACION;
                            Obligaciones[i] = detalle.TASAOBLIGACIONES;
                            Margen[i] = detalle.MARGENPONDERADO;
                            MargenP[i] = detalle.PMARGENPONDERADO;

                            i += 1;
                        }

                        /*TASA ACTIVA*/
                        #region TasaActiva

                        HighChart.ConfigChart(ref m.tasaActiva, "TasaActiva", null, 720);
                        HighChart.ConfigChart(ref m.pTasaActiva, "pTasaActiva", null, 720);

                        m.tasaActiva.SetXAxis(HighChart.GetXAxis(Fechas));
                        m.pTasaActiva.SetXAxis(HighChart.GetXAxis(Fechas));

                        m.tasaActiva.SetPlotOptions(HighChart.getLabelPercent());
                        m.pTasaActiva.SetPlotOptions(HighChart.getLabelPercent());

                        m.tasaActiva.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                        m.pTasaActiva.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                        var series = new Series[]
                        {
                            new Series{
                                Name = "Tasa Crédito",
                                Data = new Data(Credito),
                                Color = HighChart.GetColor(0),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplinePercent(3)
                            },
                            new Series{
                                Name = "Tasa Inversiones",
                                Data = new Data(Inversion),
                                Color = HighChart.GetColor(1),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                            },
                            new Series{
                                Name = "Tasa Activa Implícita",
                                Data = new Data(TasaActivo),
                                Color = HighChart.GetColor(2),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                            }
                        };

                        m.tasaActiva.SetSeries(series);
                        m.pTasaActiva.SetSeries(series);

                        #endregion

                        /*MARGEN FINANCIERO TASAS*/
                        #region margenFinancieroTasas
                        HighChart.ConfigChart(ref m.margenFinancieroTasas, "MargenTasas", null, 720);
                        HighChart.ConfigChart(ref m.pMargenFinancieroTasas, "pMargenTasas", null, 720);

                        m.margenFinancieroTasas.SetXAxis(HighChart.GetXAxis(Fechas));
                        m.pMargenFinancieroTasas.SetXAxis(HighChart.GetXAxis(Fechas));

                        m.margenFinancieroTasas.SetPlotOptions(HighChart.getLabelPercent());
                        m.pMargenFinancieroTasas.SetPlotOptions(HighChart.getLabelPercent());

                        var yAxis = new List<YAxis>
                           {
                            new YAxis()
                            {
                                Id = "TotalActivos",
                                GridLineWidth = 0,
                                Title = new YAxisTitle()
                                {
                                    Text = "% sobre el total de activos",
                                    Style = "fontSize: '12px', color: 'black'",
                                },
                                Labels = new YAxisLabels()
                                {
                                    Formatter = "formatPercent",
                                    Style = "fontSize: '12px', color: 'black'",
                                },
                                Min = 0,
                                Max = 100
                            },
                            new YAxis()
                            {
                                Id = "TasasMargen",
                                GridLineWidth = 0,
                                Title = new YAxisTitle()
                                {
                                    Text = "Tasas y Margen",
                                },
                                Opposite = true,
                                Labels = new YAxisLabels()
                                {
                                    Formatter = "formatPercent",
                                    Style = "fontSize: '12px', color: 'black'",
                                },
                                Min = 0,
                                Max = (Number) maxTasa + 2
                            }
                        }.ToArray();

                        m.margenFinancieroTasas.SetYAxis(yAxis);
                        m.pMargenFinancieroTasas.SetYAxis(yAxis);

                        series = new Series[]
                            {
                            new Series{
                                Name = "Margen de intermediación %",
                                Data = new Data(MargenRelativo),
                                Color = HighChart.GetColor(0),
                                Type = ChartTypes.Line,
                                PlotOptionsLine = HighChart.getLinePercent(),
                                 YAxis = "TasasMargen"
                            },
                            new Series{
                                Name = "Tasa activa",
                                Data = new Data(TasaActivo),
                                Color = HighChart.GetColor(1),
                                Type = ChartTypes.Line,
                                PlotOptionsLine = HighChart.getLineDashPercent(),
                                 YAxis = "TasasMargen"
                            },
                            new Series{
                                Name = "Tasa pasiva",
                                Data = new Data(TasaPasivo),
                                Color = HighChart.GetColor(2),
                                Type = ChartTypes.Line,
                                PlotOptionsLine = HighChart.getLineDashPercent(),
                                 YAxis = "TasasMargen"
                            },
                            new Series{
                                Name = "Activo productivo / Activo total",
                                Data = new Data(Activo),
                                Color = HighChart.GetColor(3),
                                Type = ChartTypes.Column,
                                 YAxis = "TotalActivos"
                            },
                            new Series{
                                Name = "Pasivo con costo / Activo total",
                                Data = new Data(Pasivo),
                                Color = HighChart.GetColor(4),
                                Type = ChartTypes.Column,
                                 YAxis = "TotalActivos"
                            }
                        };

                        m.margenFinancieroTasas.SetSeries(series);
                        m.pMargenFinancieroTasas.SetSeries(series);

                        #endregion

                        /*TASA PASIVA*/
                        #region TasaPasiva

                        HighChart.ConfigChart(ref m.tasaPasiva, "TasaPasiva", null, 720);
                        HighChart.ConfigChart(ref m.pTasaPasiva, "pTasaPasiva", null, 720);

                        m.tasaPasiva.SetXAxis(HighChart.GetXAxis(Fechas));
                        m.pTasaPasiva.SetXAxis(HighChart.GetXAxis(Fechas));

                        m.tasaPasiva.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                        m.pTasaPasiva.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                        series = new Series[]
                        {
                            new Series{
                                Name = "Tasa Captaciones",
                                Data = new Data(Captaciones),
                                Color = HighChart.GetColor(0),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplinePercent(3)
                            },
                            new Series{
                                Name = "Tasa Obligaciones",
                                Data = new Data(Obligaciones),
                                Color = HighChart.GetColor(1),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                            },
                            new Series{
                                Name = "Tasa Pasiva Implícita",
                                Data = new Data(TasaPasivo),
                                Color = HighChart.GetColor(2),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                            }
                        };

                        m.tasaPasiva.SetSeries(series);
                        m.pTasaPasiva.SetSeries(series);

                        #endregion

                        /*DIFERENCIAL TASAS*/
                        #region MargenTasas

                        HighChart.ConfigChart(ref m.diferencialTasas, "Margen", null, 720);
                        HighChart.ConfigChart(ref m.pDiferencialTasas, "pMargen", null, 720);

                        m.diferencialTasas.SetXAxis(HighChart.GetXAxis(Fechas));
                        m.pDiferencialTasas.SetXAxis(HighChart.GetXAxis(Fechas));

                        m.diferencialTasas.SetPlotOptions(HighChart.getLabelPercent());
                        m.pDiferencialTasas.SetPlotOptions(HighChart.getLabelPercent());

                        m.diferencialTasas.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                        m.pDiferencialTasas.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                        series = new Series[]
                        {
                        new Series{
                            Name = "Margen Ponderado",
                            Data = new Data(Margen),
                            Color = ColorTranslator.FromHtml("#2F5597"),
                            Type = ChartTypes.Areaspline,
                            PlotOptionsAreaspline = HighChart.getAreasplinePercent(0.18, 2)
                        },
                        new Series{
                            Name = "Tasa Activa Implícita",
                            Data = new Data(TasaActivo),
                            Color = ColorTranslator.FromHtml("#0284c7"),
                            Type = ChartTypes.Spline,
                            PlotOptionsSpline = HighChart.getSplinePercent(3)
                        },
                        new Series{
                            Name = "Tasa Pasiva Implícita",
                            Data = new Data(TasaPasivo),
                            Color = ColorTranslator.FromHtml("#ea580c"),
                            Type = ChartTypes.Spline,
                            PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                        },
                        new Series{
                            Name = "Tasa Activa Implícita (Promedio)",
                            Data = new Data(TasaActivoP),
                            Color = HighChart.GetColor(3),
                            Type = ChartTypes.Spline,
                            PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                        },
                        new Series{
                            Name = "Tasa Pasiva Implícita (Promedio)",
                            Data = new Data(TasaPasivoP),
                            Color = HighChart.GetColor(4),
                            Type = ChartTypes.Spline,
                            PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                        },
                        new Series{
                            Name = "Margen Ponderado (Promedio)",
                            Data = new Data(MargenP),
                            Color = HighChart.GetColor(5),
                            Type = ChartTypes.Spline,
                            PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                        }
                        };

                        m.diferencialTasas.SetSeries(series);
                        m.pDiferencialTasas.SetSeries(series);

                        #endregion
                    }
                    catch (Exception) { }
                }
                return m;
            }
        }

        [AllowAnonymous]
        public ActionResult Buscar(String Entidades, DateTime PeriodoI, DateTime PeriodoF, int Opcion)
        {
            Modelo_Tasas m = new Modelo_Tasas();

            try
            {
                Session["IdEntidad"] = Entidades;
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
                TempData["Opcion"] = Opcion;
                Load();
                m = Graph;
            }
            catch (Exception)
            {
            }

            return View("Index", m);
        }

        public ActionResult Index()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-6).ToShortDateString();
            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            TempData["Opcion"] = uint.MinValue;
            Modelo_Tasas m = Graph;
            return View(m);
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