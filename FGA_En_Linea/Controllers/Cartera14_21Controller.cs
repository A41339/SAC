using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Entities.Entities.Procedures;
using FGA.Model;
using FGA.Models;
using FGA.Utility;
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    [System.Web.Mvc.SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class Cartera14_21Controller : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();

        public ActionResult Index(string Entidades = null, string Periodo = null, string TipoComparacion = null)
        {
            if (!string.IsNullOrWhiteSpace(Entidades))
            {
                Session["IdEntidad"] = Entidades;
            }

            Load();

            string currentEntidad = Session["IdEntidad"]?.ToString() ?? "2";
            DateTime fechaEntidad;
            try
            {
                fechaEntidad = sp.FGA_Consultar_FechaCierre(currentEntidad).AddMonths(-1);
            }
            catch
            {
                fechaEntidad = DateTime.Today.AddMonths(-1);
            }

            if (!string.IsNullOrWhiteSpace(Periodo))
            {
                try
                {
                    DateTime fechaRef = Utilitarios.ConvertirAFecha(Periodo);
                    Session["Periodo2"] = fechaRef.ToShortDateString();
                    if (string.Equals(TipoComparacion, "Interanual", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Periodo1"] = fechaRef.AddYears(-1).ToShortDateString();
                    }
                    else
                    {
                        Session["Periodo1"] = new DateTime(fechaRef.AddMonths(-4).Year, fechaRef.AddMonths(-4).Month, 1).ToShortDateString();
                    }
                }
                catch { }
            }

            if (Session["Periodo1"] is null)
                Session["Periodo1"] = new DateTime(fechaEntidad.AddMonths(-4).Year, fechaEntidad.AddMonths(-4).Month, 1).ToShortDateString();
            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.ToShortDateString();

            Session["TipoReporte"] = 101;

            var tiposGrafico = new List<Rpt_Graph>
            {
                new Rpt_Graph { Id = 101, Nombre = "Composición de mora" },
                new Rpt_Graph { Id = 102, Nombre = "Composición de mora agrupada" },
                new Rpt_Graph { Id = 2, Nombre = "Tipo de categoría riesgo" },
                new Rpt_Graph { Id = 6, Nombre = "Variación mensual pérdida esperada vrs saldo estimaciones" },
                new Rpt_Graph { Id = 3, Nombre = "Saldo por etapa del crédito" },
                new Rpt_Graph { Id = 5, Nombre = "Saldo Pérdida Esperada vrs EAD" },
                new Rpt_Graph { Id = 7, Nombre = "Pérdida esperada por tipo de segmento" },
                new Rpt_Graph { Id = 8, Nombre = "Pérdida esperada por categoría de riesgo" },
                new Rpt_Graph { Id = 4, Nombre = "Cantidad de operaciones por etapa" }
            };

            ViewBag.Grafico = new SelectList(tiposGrafico, "Id", "Nombre", Session["TipoReporte"].ToString());
            return View();
        }

        public PartialViewResult GraficaDetalle(String Entidades, int Grafico, DateTime PeriodoI, DateTime PeriodoF)
        {
            Highcharts gp = null;
            Highcharts pGp = null;

            Session["IdEntidad"] = Entidades;
            Session["Periodo1"] = PeriodoI.ToShortDateString();
            Session["Periodo2"] = PeriodoF.ToShortDateString();

            Load();
            FGA.Model.Grafico model = new Grafico();

            HighChart.ConfigChart(ref gp, "Graph", null, 420, 60);
            HighChart.ConfigChart(ref pGp, "pGraph", null);

            if (Grafico == 101)
            {
                var tak = sp.FGA_Consultar_Grafico_Mora_Cartera(Entidades, PeriodoI, PeriodoF).ToList();
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Count();
                    string[] fechas = new string[numPeriodos];
                    object[] MoraMayor90Dias = new object[numPeriodos];

                    List<Serie> listaSeries = new List<Serie>();
                    listaSeries.Add(new Serie(numPeriodos, "Al día"));
                    listaSeries.Add(new Serie(numPeriodos, "1 - 30 días"));
                    listaSeries.Add(new Serie(numPeriodos, "31 - 60 días"));
                    listaSeries.Add(new Serie(numPeriodos, "61 - 90 días"));

                    for (int j = 0; j < numPeriodos; j++)
                    {
                        fechas[j] = tak[j].PERIODO.Value.Month.ToString() + "-" + tak[j].PERIODO.Value.Year.ToString();
                        MoraMayor90Dias[j] = tak[j].HASTA180 + tak[j].MAS180 + tak[j].CJ;
                        listaSeries[0].valores[j] = tak[j].ALDIA;
                        listaSeries[1].valores[j] = tak[j].HASTA30;
                        listaSeries[2].valores[j] = tak[j].HASTA60;
                        listaSeries[3].valores[j] = tak[j].HASTA90;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    List<YAxis> yAsis = new List<YAxis>()
                    {
                        new YAxis()
                        {
                            Id = "Concentracion",
                            GridLineWidth = 0,
                            Title = new YAxisTitle()
                            {
                                Text = "Concentración",
                                Style = "fontSize: '12px', color: 'black'",
                            },
                            Labels = new YAxisLabels()
                            {
                                Formatter = "formatPercent",
                                Style = "fontSize: '12px', color: 'black'",
                            },
                            Opposite = true
                        },
                        new YAxis()
                        {
                            Id = "Mayor90Dias",
                            GridLineWidth = 0,
                            Title = new YAxisTitle()
                            {
                                Text = "Mora mayor a 90 días",
                                Style = "fontSize: '0px', color: 'black'",
                            },
                            Labels = new YAxisLabels()
                            {
                                Style = "fontSize: '0px', color: 'black'",
                            },
                        }
                    };

                    gp.SetYAxis(yAsis.ToArray());
                    pGp.SetYAxis(yAsis.ToArray());

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

                    Series[] series = new Series[listaSeries.Count() + 1];
                    Series serie;
                    int i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
                    {
                        serie = new Series
                        {
                            Type = ChartTypes.Column,
                            Name = detalle.nombre,
                            Data = new Data(detalle.valores),
                            Color = HighChart.GetColor(i),
                            YAxis = "Concentracion",
                        };

                        series[i] = serie;
                        i += 1;
                    }

                    serie = new Series
                    {
                        Type = ChartTypes.Line,
                        Name = "Mora mayor a 90 días",
                        Data = new Data(MoraMayor90Dias),
                        Color = HighChart.GetColor(1),
                        YAxis = "Mayor90Dias",
                        PlotOptionsLine = HighChart.getLinePercent()
                    };

                    series[i] = serie;
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if (Grafico == 102)
            {
                var tak = sp.FGA_Consultar_Grafico_Mora_Cartera(Entidades, PeriodoI, PeriodoF).ToList();
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Count();
                    string[] fechas = new string[numPeriodos];

                    List<Serie> listaSeries = new List<Serie>();
                    listaSeries.Add(new Serie(numPeriodos, "Al día"));
                    listaSeries.Add(new Serie(numPeriodos, "Mora Temprana"));
                    listaSeries.Add(new Serie(numPeriodos, "Mora mayor a 90 días"));

                    for (int j = 0; j < numPeriodos; j++)
                    {
                        fechas[j] = tak[j].PERIODO.Value.Month.ToString() + "-" + tak[j].PERIODO.Value.Year.ToString();
                        listaSeries[0].valores[j] = tak[j].ALDIA;
                        listaSeries[1].valores[j] = tak[j].HASTA30 + tak[j].HASTA60 + tak[j].HASTA90;
                        listaSeries[2].valores[j] = tak[j].HASTA180 + tak[j].MAS180 + tak[j].CJ;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent", AxisTypes.Logarithmic));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent", AxisTypes.Logarithmic));

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

                    Series[] series = new Series[listaSeries.Count()];
                    Series serie;
                    int i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.TipoDeSegmento)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Segmento(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];                   
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Segmento_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.PorcentajeSaldo.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.PorcentajeSaldo.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PorcentajeSaldo.Value;
                            }
                        }                       
                    }
                   
                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.CantidadDeOperacionesPorEtapa)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Etapa(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Etapa_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.Cantidad.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.Cantidad.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.Cantidad.Value;
                            }
                        }
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));

                    gp.SetPlotOptions(HighChart.getLabelStackingQuantity());
                    pGp.SetPlotOptions(HighChart.getLabelStackingQuantity());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.MontoDesembolsadoMensual)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_EAD(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = new List<string>();
                    listaTipos.Add("Monto desembolado");

                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_EAD_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual = detalle.MontoDesembolsado.Value;                        
                        listaSeries[0].total += detalle.MontoDesembolsado.Value;
                        listaSeries[0].mostrar = true;
                        listaSeries[0].valores[i] = detalle.MontoDesembolsado.Value;                          
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.PerdidaEsperadaPorCategoriaDeRiesgo)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Categoria(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Categoria_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.PorcentajeEstimacion.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.PorcentajeEstimacion.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PorcentajeEstimacion.Value;
                            }
                        }
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.PerdidaEsperadaPorTipoDeSegmento)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Segmento(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Segmento_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.PorcentajeEstimacion.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.PorcentajeEstimacion.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PorcentajeEstimacion.Value;
                            }
                        }
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.SaldoPerdidaEsperadaVrsEAD)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_EAD(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = new List<string>();
                    listaTipos.Add("EAD");
                    listaTipos.Add("Pérdida esperada");

                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_EAD_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.EAD.Value;
                        listaSeries[0].total += detalle.EAD.Value;
                        listaSeries[0].mostrar = true;
                        listaSeries[0].valores[i] = detalle.EAD.Value;

                        listaSeries[1].total += detalle.MontoEstimacion.Value;
                        listaSeries[1].mostrar = true;
                        listaSeries[1].valores[i] = detalle.MontoEstimacion.Value;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatMillion"));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.SaldoPorEtapaDelCredito)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Etapa(Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Etapa_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.PorcentajeEstimacion.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.PorcentajeEstimacion.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PorcentajeEstimacion.Value;
                            }
                        }
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelStackingNormal());
                    pGp.SetPlotOptions(HighChart.getLabelStackingNormal());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }            
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.TipoDeCategoriaRiesgo)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_Categoria (Entidades, PeriodoI, PeriodoF);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.OrderBy(j => j.CodigoCategoriaRiesgo).Select(l => l.Nombre).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_Categoria_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < porcentajeActual)
                                porcentajeMaximo = porcentajeActual;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual += detalle.PorcentajeSaldo.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.Nombre)
                            {
                                listaSeries[j].total += detalle.PorcentajeSaldo.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PorcentajeSaldo.Value;
                            }
                        }
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, porcentajeMaximo, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelStackingPercent());
                    pGp.SetPlotOptions(HighChart.getLabelStackingPercent());

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
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.VariacionMensualPerdidaEsperadaVrsSaldoEstimaciones)
            {
                var tak = sp.FGA_Consultar_Grafico_Oper_EAD(Entidades, PeriodoI, PeriodoF);               
                decimal porcentajeMaximo = 0, porcentajeActual = 0, porcentajeMinimo = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.Periodo).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = new List<string>();
                    listaTipos.Add("Pérdida esperada");
                    listaTipos.Add("Saldo estimaciones");

                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Grafico_Oper_EAD_Result detalle in tak)
                    {
                        periodoActual = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        if (string.IsNullOrEmpty(periodo))
                            periodo = fechas[i] = periodoActual;
                        else if (periodo != periodoActual)
                        {
                            if (porcentajeMaximo < detalle.VariacionEstimacion.Value)
                                porcentajeMaximo = detalle.VariacionEstimacion.Value;
                            if (porcentajeMaximo < detalle.Variacion139.Value)
                                porcentajeMaximo = detalle.Variacion139.Value;

                            if (porcentajeMinimo > detalle.VariacionEstimacion.Value)
                                porcentajeMinimo = detalle.VariacionEstimacion.Value;
                            if (porcentajeMinimo > detalle.Variacion139.Value)
                                porcentajeMinimo = detalle.Variacion139.Value;

                            i += 1;
                            porcentajeActual = 0;
                            periodo = periodoActual;
                            fechas[i] = periodo;
                        }

                        porcentajeActual = detalle.VariacionEstimacion.Value;
                        listaSeries[0].total += detalle.VariacionEstimacion.Value;
                        listaSeries[0].mostrar = true;
                        listaSeries[0].valores[i] = detalle.VariacionEstimacion.Value;

                        listaSeries[1].total += detalle.Variacion139.Value;
                        listaSeries[1].mostrar = true;
                        listaSeries[1].valores[i] = detalle.Variacion139.Value;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(fechas));

                    gp.SetYAxis(HighChart.GetYAxis(porcentajeMinimo - 1, porcentajeMaximo + 2, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(porcentajeMinimo - 1, porcentajeMaximo + 2, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelPercent());
                    pGp.SetPlotOptions(HighChart.getLabelPercent());

                    numSeries = listaSeries.Where(o => o.mostrar == true).Count();
                    Series[] series = new Series[numSeries];
                    Series serie;
                    i = 0;

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
                    {
                        if (detalle.mostrar)
                        {
                            serie = new Series
                            {
                                Type = ChartTypes.Line,
                                Name = detalle.nombre,
                                Data = new Data(detalle.valores),
                                Color = HighChart.GetColor(i),
                                PlotOptionsLine = HighChart.getLinePercent()
                            };

                            series[i] = serie;
                            i += 1;
                        }
                    }
                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }

            model.detalle = gp;
            model.pDetalle = pGp;
            return PartialView("_GraficaDetallePopUp", model);
        }
                
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.SafeClose();
                sp.SafeClose();
                usr.SafeClose();
            }
            base.Dispose(disposing);
        }
    }
}