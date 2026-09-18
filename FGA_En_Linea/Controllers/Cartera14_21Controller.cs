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
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class Cartera14_21Controller : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();

        public ActionResult Index()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString()).AddMonths(-1);

            if (Session["Periodo1"] is null)
                Session["Periodo1"] = new DateTime(fechaEntidad.AddMonths(-4).Year, fechaEntidad.AddMonths(-4).Month, 1).ToShortDateString();
            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.ToShortDateString();

            Session["TipoReporte"] = enum_Grafico14_21.TipoDeSegmento;

            var tiposGrafico = new List<Rpt_Graph>
            {
                new Rpt_Graph { Id = 1, Nombre = "Tipo de segmento" },
                new Rpt_Graph { Id = 2, Nombre = "Tipo de categoría riesgo" },
                new Rpt_Graph { Id = 3, Nombre = "Saldo por etapa del crédito" },
                new Rpt_Graph { Id = 4, Nombre = "Cantidad de operaciones por etapa" },
                new Rpt_Graph { Id = 5, Nombre = "Saldo Pérdida Esperada vrs EAD" },
                new Rpt_Graph { Id = 6, Nombre = "Variación mensual pérdida esperada vrs saldo estimaciones" },
                new Rpt_Graph { Id = 7, Nombre = "Pérdida esperada por tipo de segmento" },
                new Rpt_Graph { Id = 8, Nombre = "Pérdida esperada por categoría de riesgo" }
            };

            ViewBag.Grafico = new SelectList(tiposGrafico.OrderByDescending(o => o.Nombre), "Id", "Nombre", Session["TipoReporte"].ToString());
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

            HighChart.ConfigChart(ref gp, "Graph", null, 720);
            HighChart.ConfigChart(ref pGp, "pGraph", null);

            if ((enum_Grafico14_21)Grafico == Utility.Utilitarios.enum_Grafico14_21.TipoDeSegmento)
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
                ent.Close();
                sp.Close();
                usr.Close();
            }
            base.Dispose(disposing);
        }
    }
}