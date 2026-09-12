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
    public class MargenController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Buscar(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            ModeloMargen_Financ m = new ModeloMargen_Financ();
            try
            {
                Session["IdEntidad"] = Entidades;
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
                Load();
                m = GetGraph();
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
    
            ModeloMargen_Financ m = GetGraph();
            return View(m);
        }

        private ModeloMargen_Financ GetGraph()
        {
            ModeloMargen_Financ m = new ModeloMargen_Financ();
            var tak = sp.FGA_Consultar_Modelo_Margen(Session["IdEntidad"].ToString(),
                                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));

            if (tak.Count() > 0)
            {
                #region MargenFinaciero
                HighChart.ConfigChart(ref m.margenFinanciero, "MargenFinanciero", null, 720);
                HighChart.ConfigChart(ref m.pMargenFinanciero, "pMargenFinanciero", null, 720);

                int i = 0;
                object[] ROA = new object[tak.Count()];
                object[] ROP = new object[tak.Count()];
                object[] ROE = new object[tak.Count()];
                string[] Fechas = new string[tak.Count()];
                object[] Monto = new object[tak.Count()];
                object[] Porcentaje = new object[tak.Count()];
                object[] MargenTotal = new object[tak.Count()];
                object[] MargenOperativo = new object[tak.Count()];
                object[] MargenFinanciero = new object[tak.Count()];
                object[] MargenTotalP = new object[tak.Count()];
                object[] MargenOperativoP = new object[tak.Count()];
                object[] MargenFinancieroP = new object[tak.Count()];

                foreach (FGA_Consultar_Modelo_Margen_Result detalle in tak)
                {
                    /*Margen Financiero*/
                    Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                    Monto[i] = detalle.MARGENINTERMFINANC;
                    Porcentaje[i] = detalle.COBERTURAINTERM;

                    /*Cobertura*/
                    MargenFinanciero[i] = detalle.COBERTURAINTERM;
                    MargenOperativo[i] = detalle.MARGENOPERATIVO;
                    MargenTotal[i] = detalle.MARGENTOTAL;
                    MargenFinancieroP[i] = detalle.PCOBERTURAINTERM;
                    MargenOperativoP[i] = detalle.PMARGENOPERATIVO;
                    MargenTotalP[i] = detalle.PMARGENTOTAL;

                    /*IndicadoresRentabilidad*/
                    ROA[i] = detalle.ROA;
                    ROP[i] = detalle.ROP;
                    ROE[i] = detalle.ROE;
                    i += 1;
                }

                m.margenFinanciero.SetXAxis(HighChart.GetXAxis(Fechas));
                m.pMargenFinanciero.SetXAxis(HighChart.GetXAxis(Fechas));

                var yAxis = new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "Cobertura",
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
                        Id = "Monetario",
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

                m.margenFinanciero.SetYAxis(yAxis);
                m.pMargenFinanciero.SetYAxis(yAxis);

                m.margenFinanciero.SetPlotOptions(HighChart.getLabelAmmount());
                m.pMargenFinanciero.SetPlotOptions(HighChart.getLabelAmmount());

                var series = new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Cobertura",
                        Data = new Data(Porcentaje),
                        Color = HighChart.GetColor(1),
                        YAxis = "Cobertura",
                        PlotOptionsLine = HighChart.getLineWidth(5),
                        ZIndex = 3
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Monetario",
                        Data = new Data(Monto),
                        Color = HighChart.GetColor(0),
                        YAxis = "Monetario",
                        ZIndex = 2
                    }
                };

                m.margenFinanciero.SetSeries(series);
                m.pMargenFinanciero.SetSeries(series);

                #endregion

                #region CoberturaMargenes
                HighChart.ConfigChart(ref m.coberturaMargenes, "CoberturaMargenes", null, 720);
                HighChart.ConfigChart(ref m.pCoberturaMargenes, "pCoberturaMargenes", null, 720);

                m.coberturaMargenes.SetXAxis(HighChart.GetXAxis(Fechas));
                m.pCoberturaMargenes.SetXAxis(HighChart.GetXAxis(Fechas));

                m.coberturaMargenes.SetPlotOptions(HighChart.getLabelPercent());
                m.pCoberturaMargenes.SetPlotOptions(HighChart.getLabelPercent());

                m.coberturaMargenes.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                m.pCoberturaMargenes.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                series = new Series[]
                {
                    new Series{
                        Name = "Margen Financiero",
                        Data = new Data(MargenFinanciero),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Name = "Margen Operativo",
                        Data = new Data(MargenOperativo),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Name = "Margen Total",
                        Data = new Data(MargenTotal),
                        Color = HighChart.GetColor(2),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLine()
                    },
                     new Series{
                        Name = "Margen Financiero (Promedio)",
                        Data = new Data(MargenFinancieroP),
                        Color = HighChart.GetColor(3),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLineDash()
                    },
                    new Series{
                        Name = "Margen Operativo (Promedio)",
                        Data = new Data(MargenOperativoP),
                        Color = HighChart.GetColor(4),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLineDash()
                    },
                    new Series{
                        Name = "Margen Total (Promedio)",
                        Data = new Data(MargenTotalP),
                        Color = HighChart.GetColor(5),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLineDash()
                    }
               };

                m.coberturaMargenes.SetSeries(series);
                m.pCoberturaMargenes.SetSeries(series);

                #endregion

                #region IndicadoresRentabilidad
                HighChart.ConfigChart(ref m.indicadoresRentabilidad, "IndicadoresRentabilidad", null, 720);
                HighChart.ConfigChart(ref m.pIndicadoresRentabilidad, "pIndicadoresRentabilidad", null, 720);

                m.indicadoresRentabilidad.SetXAxis(HighChart.GetXAxis(Fechas));
                m.pIndicadoresRentabilidad.SetXAxis(HighChart.GetXAxis(Fechas));

                m.indicadoresRentabilidad.SetPlotOptions(HighChart.getLabelPercent());
                m.pIndicadoresRentabilidad.SetPlotOptions(HighChart.getLabelPercent());

                m.indicadoresRentabilidad.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                m.pIndicadoresRentabilidad.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                series = new Series[]
               {
                    new Series{
                        Name = "Rentabilidad / Activos",
                        Data = new Data(ROA),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Name = "Rentabilidad / Patrimonio",
                        Data = new Data(ROP),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLineDash()
                    },
                    new Series{
                        Name = "Rentabilidad / Capital",
                        Data = new Data(ROE),
                        Color = HighChart.GetColor(2),
                        Type = ChartTypes.Line,
                            PlotOptionsLine = HighChart.getLine()
                    }
               };

               m.indicadoresRentabilidad.SetSeries(series);
               m.pIndicadoresRentabilidad.SetSeries(series);

                #endregion
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