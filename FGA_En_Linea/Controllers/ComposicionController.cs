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
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class ComposicionController : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.Rpt_GraphService.ServiceOf_Rpt_GraphClient gr = new FGA_En_Linea.Rpt_GraphService.ServiceOf_Rpt_GraphClient();
        private readonly FGA_En_Linea.CategoriaRiesgoService.ServiceOf_Categoria_RiesgoClient cat = new FGA_En_Linea.CategoriaRiesgoService.ServiceOf_Categoria_RiesgoClient();
        private readonly FGA_En_Linea.TipoCarteraService.ServiceOf_Tipo_CarteraClient tipCar = new FGA_En_Linea.TipoCarteraService.ServiceOf_Tipo_CarteraClient();

        public ActionResult Index()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-6).ToShortDateString();

            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            Session["TipoReporte"] = enum_tipoGrafico.variacionCarteraMensual;
            ViewBag.Grafico = new SelectList(gr.GetAll().Where(o=>o.Ind_FGA == true).OrderByDescending(o => o.Nombre), "Id", "Nombre", Session["TipoReporte"].ToString());

            Composicion_cartera model = new Composicion_cartera();
            model.listaTiposCartera = tipCar.GetAll().OrderBy(o => o.Nombre).ToList();
            model.listaCategoriaRiesgo = cat.GetAll().Where(o => o.Analisis == true).OrderBy(o => o.CategoriaRiesgo).ToList();
            return View(model);
        }

        public PartialViewResult GraficaDetalle(String Entidades, int Grafico, DateTime PeriodoI, DateTime PeriodoF, String[] CategoriaRiesgo, String[] TipoCartera)
        {
            FGA.Model.Grafico model = new Grafico();

            Highcharts gp = null;
            Highcharts pGp = null;

            HighChart.ConfigChart(ref gp, "Graph", null, 720);
            HighChart.ConfigChart(ref pGp, "pGraph", null);

            Session["IdEntidad"] = Entidades;
            Session["Periodo1"] = PeriodoI.ToShortDateString();
            Session["Periodo2"] = PeriodoF.ToShortDateString();

            Load();
            decimal? min = 0;
            decimal? max = 0;
            if ((enum_tipoGrafico)Grafico == Utility.Utilitarios.enum_tipoGrafico.carteraTotal)
            {
                var tak = sp.FGA_Consultar_CarteraTotal(Entidades, PeriodoI, PeriodoF, 12);
                if (tak.Count() > 0)
                {
                    string[] Fechas = new string[tak.Count()];
                    object[] CarteraTotal = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_CarteraTotal_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();
                        CarteraTotal[i] = detalle.SALDONETO.Value;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetYAxis(HighChart.GetYAxis(0, null, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(0, null, "formatMillion"));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    var series =
                    new Series[]
                    {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Cartera de crédito total en millones",
                            Data = new Data(CarteraTotal),
                            Color = HighChart.GetColor(0),
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCartera || (enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCarteraMensual)
            {
                int meses = (enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCarteraMensual ? 1 : 12;
                var tak = sp.FGA_Consultar_CarteraTotal(Entidades, PeriodoI, PeriodoF, meses);
                if (tak.Count() > 0)
                {                 
                    string[] Fechas = new string[tak.Count()];
                    object[] CarteraTotalNeta = new object[tak.Count()];
                    object[] CarteraTotalBruta = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_CarteraTotal_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();
                        CarteraTotalNeta[i] = detalle.VARIACIONNETA;
                        CarteraTotalBruta[i] = detalle.VARIACIONBRUTA;
                        i += 1;
                      
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    var series =
                    new Series[]
                    {
                        new Series{
                            Name = meses == 1 ? "Variación mensual neta" : "Variación interanual neta",
                            Data = new Data(CarteraTotalNeta),
                            Color = HighChart.GetColor(0),
                            PlotOptionsLine = HighChart.getLine()
                        },
                         new Series{
                            Name = meses == 1 ? "Variación mensual bruta" : "Variación interanual bruta",
                            Data = new Data(CarteraTotalBruta),
                            Color = HighChart.GetColor(1),
                            PlotOptionsLine = HighChart.getLine()
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.calificacionCartera)
            {
                var tak = sp.FGA_Consultar_Calif_Cartera(Entidades, PeriodoI, PeriodoF, string.Empty);

                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Select(l => l.PERIODO).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.CATEGORIARIESGO).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string[] filtro = CategoriaRiesgo;
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Calific_Cartera_Result detalle in tak)
                    {
                        periodoActual = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();

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

                        if (filtro is null || filtro.Contains(detalle.CATEGORIARIESGO))
                        {
                            porcentajeActual += detalle.PORCENTAJE.Value;
                            for (int j = 0; j < listaTipos.Count(); j++)
                            {
                                if (listaSeries[j].nombre == detalle.CATEGORIARIESGO)
                                {
                                    listaSeries[j].total += detalle.PORCENTAJE.Value;
                                    listaSeries[j].mostrar = true;
                                    listaSeries[j].valores[i] = detalle.PORCENTAJE.Value;
                                }
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

                    gp.SetSeries(
                        series
                    );

                    pGp.SetSeries(
                        series
                    );
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.creditoCalificacionE)
            {
                var tak = sp.FGA_Consultar_CalificE(Entidades, PeriodoI, PeriodoF);
                string[] Fechas = new string[tak.Count()];
                object[] Monto = new object[tak.Count()];
                object[] Porcentaje = new object[tak.Count()];
                int i = 0;

                foreach (FGA_Consultar_CalificE_Result detalle in tak)
                {
                    Fechas[i] = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();
                    Monto[i] = detalle.MONTO.Value;
                    Porcentaje[i] = detalle.PORCENTAJE.Value;
                    i += 1;
                }

                gp.SetXAxis(HighChart.GetXAxis(Fechas));
                pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                var yaxis = new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "Variacion",
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
                        Id = "CarteraE",
                        GridLineWidth = 0,
                        Title = new YAxisTitle()
                        {
                            Text = "Millones",
                        },
                        Labels = new YAxisLabels()
                        {
                            Formatter = "formatMillion",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                    }
                }.ToArray();

                gp.SetYAxis(yaxis);
                pGp.SetYAxis(yaxis);

                gp.SetPlotOptions(HighChart.getLabelAmmount());
                pGp.SetPlotOptions(HighChart.getLabelAmmount());

                var series = new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Variación interanual",
                        Data = new Data(Porcentaje),
                        Color = HighChart.GetColor(1),
                        YAxis = "Variacion",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Cartera de crédito E en millones",
                        Data = new Data(Monto),
                        Color = HighChart.GetColor(0),
                        YAxis = "CarteraE"
                    }
                };

                gp.SetSeries(series);
                pGp.SetSeries(series);
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.tipoCarteraCalificacionE)
            {
                var tak = sp.FGA_Consultar_Tipo_Cartera_E(Entidades, PeriodoI, PeriodoF);

                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {

                    int numPeriodos = tak.Select(l => l.PERIODO).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.TIPOCARTERA).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Tipo_Cartera_E_Result detalle in tak)
                    {
                        periodoActual = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();

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

                        porcentajeActual += detalle.PORCENTAJE.Value;
                        for (int j = 0; j < listaTipos.Count(); j++)
                        {
                            if (listaSeries[j].nombre == detalle.TIPOCARTERA)
                            {
                                listaSeries[j].total += detalle.PORCENTAJE.Value;
                                listaSeries[j].mostrar = true;
                                listaSeries[j].valores[i] = detalle.PORCENTAJE.Value;
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
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.tipoCartera)
            {
                var tak = sp.FGA_Consultar_Tipo_Cartera(Entidades, PeriodoI, PeriodoF, 0);
                decimal porcentajeMaximo = 0, porcentajeActual = 0;
                if (tak.Count() > 0)
                {

                    int numPeriodos = tak.Select(l => l.PERIODO).Distinct().Count();
                    int i = 0, numSeries = 0;
                    List<string> listaTipos = tak.Select(l => l.TIPOCARTERA).Distinct().ToList();
                    List<Serie> listaSeries = new List<Serie>();
                    string[] fechas = new string[numPeriodos];
                    string[] filtro = TipoCartera;
                    string periodo = string.Empty, periodoActual = string.Empty;

                    for (int j = 0; j < listaTipos.Count(); j++)
                        listaSeries.Add(new Serie(numPeriodos, listaTipos[j]));

                    foreach (FGA_Consultar_Tipo_Cartera_Result detalle in tak)
                    {
                        periodoActual = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();

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

                        if (filtro is null || filtro.Contains(detalle.TIPOCARTERA))
                        {
                            porcentajeActual += detalle.PORCENTAJE.Value;
                            for (int j = 0; j < listaTipos.Count(); j++)
                            {
                                if (listaSeries[j].nombre == detalle.TIPOCARTERA)
                                {
                                    listaSeries[j].total += detalle.PORCENTAJE.Value;
                                    listaSeries[j].mostrar = true;
                                    listaSeries[j].valores[i] = detalle.PORCENTAJE.Value;
                                }
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
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.cuentasLiquidadas)
            {
                var tak = sp.FGA_Consultar_CuentasLiq(Entidades, PeriodoI, PeriodoF);
                if (tak.Count() > 0)
                {
                 
                    string[] Fechas = new string[tak.Count()];
                    object[] CuentasLiq = new object[tak.Count()];
                    object[] VariacionLiq = new object[tak.Count()];
                    int i = 0;
                    min = 0;
                    max = 0;

                    foreach (FGA_Consultar_CuentasLiq_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Value.Month.ToString() + "-" + detalle.PERIODO.Value.Year.ToString();
                        CuentasLiq[i] = detalle.SALDO;
                        VariacionLiq[i] = detalle.VARIACION;
                        i += 1;

                        if (detalle.VARIACION < min)
                            min = detalle.VARIACION.Value;

                        if (detalle.SALDO > max)
                            max = detalle.SALDO.Value;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    var yaxis = new List<YAxis>
                    {
                        new YAxis()
                        {
                            Id = "Crecimiento",
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
                            Min = (Number?) (min - 1)
                        },
                        new YAxis()
                        {
                            Id = "Montos",
                            GridLineWidth = 0,
                            Title = new YAxisTitle()
                            {
                                Text = "Millones",
                            },
                            Labels = new YAxisLabels()
                            {
                                Formatter = "formatMillion",
                                Style = "fontSize: '12px', color: 'black'",
                            },
                            Max = (Number?) max + 1
                        }
                    }.ToArray();

                    gp.SetYAxis(yaxis);
                    pGp.SetYAxis(yaxis);

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    var series = new Series[]
                    {
                        new Series{
                            Type = ChartTypes.Line,
                            Name = "Variación mensual",
                            Data = new Data(VariacionLiq),
                            Color = HighChart.GetColor(1),
                            PlotOptionsLine = HighChart.getLine(),
                            YAxis = "Crecimiento",
                            ZIndex = 2      
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Cuentas liquidadas por mes en millones",
                            Data = new Data(CuentasLiq),
                            Color = HighChart.GetColor(0),
                            YAxis = "Montos",
                            ZIndex = 1
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCapitalSocial)
            {
                var tak = sp.FGA_Consultar_Variacion_CS(Entidades, PeriodoI, PeriodoF);
                if (tak.Count() > 0)
                {
                    string[] Fechas = new string[tak.Count()];
                    object[] Variacion = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_Variacion_CS_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                        Variacion[i] = detalle.VARIACION;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetPlotOptions(HighChart.getLabelPercent());
                    pGp.SetPlotOptions(HighChart.getLabelPercent());

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                    var series = new Series[]
                    {
                        new Series{
                            Name = "Variación anual del capital social",
                            Data = new Data(Variacion),
                            Color = HighChart.GetColor(0),
                            PlotOptionsLine = HighChart.getLine()
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.tasaPonderada)
            {
                var tak = sp.FGA_Consultar_Tasa_Ponderada(Entidades, PeriodoI, PeriodoF);
                if (tak.Count() > 0)
                {
                    string[] Fechas = new string[tak.Count()];
                    object[] Variacion = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_Tasa_Ponderada_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                        Variacion[i] = detalle.Total;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetPlotOptions(HighChart.getLabelPercent());
                    pGp.SetPlotOptions(HighChart.getLabelPercent());

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                    var series = new Series[]
                    {
                        new Series{
                            Name = "Tasa Ponderada",
                            Data = new Data(Variacion),
                            Color = HighChart.GetColor(0),
                            PlotOptionsLine = HighChart.getLine()
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.comparacionRecuperacionLiquidadas)
            {
                var tak = sp.FGA_Consultar_RecuperacionActivos(Entidades, PeriodoI, PeriodoF);
                if (tak.Count() > 0)
                {
                    string[] Fechas = new string[tak.Count()];
                    object[] Recuperacion = new object[tak.Count()];
                    object[] Liquidadas = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_RecuperacionActivos_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                        Recuperacion[i] = detalle.SALDO_521;
                        Liquidadas[i] = detalle.SALDO_815;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));

                    var series = new Series[]
                    {
                        new Series{
                            Name = "Recuperaciones mensual de activos en millones",
                            Data = new Data(Recuperacion),
                            Color = HighChart.GetColor(1),
                            Type = ChartTypes.Column
                        },
                         new Series{
                            Name = "Cuentas liquidadas por mes en millones",
                            Data = new Data(Liquidadas),
                            Color = HighChart.GetColor(0),
                            Type = ChartTypes.Column
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.concentracionMora)
            {
                var tak = sp.FGA_Consultar_Grafico_Mora_Cartera(Entidades, PeriodoI, PeriodoF).ToList();
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Count();
                    object[] ListaMora = new object[tak.Count() - 1];
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
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.concentracionMoraAgrupado)
            {
                var tak = sp.FGA_Consultar_Grafico_Mora_Cartera(Entidades, PeriodoI, PeriodoF).ToList();
                if (tak.Count() > 0)
                {
                    int numPeriodos = tak.Count();
                    object[] ListaMora = new object[tak.Count() - 1];
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
            else
            {
                var tak = sp.FGA_Consultar_RecuperacionActivos(Entidades, PeriodoI, PeriodoF);
                if (tak.Count() > 0)
                {
                    string[] Fechas = new string[tak.Count()];
                    object[] Recuperacion = new object[tak.Count()];
                    int i = 0;

                    foreach (FGA_Consultar_RecuperacionActivos_Result detalle in tak)
                    {
                        Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                        Recuperacion[i] = detalle.SALDO_521;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    pGp.SetXAxis(HighChart.GetXAxis(Fechas));

                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    pGp.SetPlotOptions(HighChart.getLabelAmmount());

                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));
                    pGp.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));

                    var series = new Series[]
                    {
                        new Series{
                            Name = "Recuperación mensual de activos financieros liquidados en millones",
                            Data = new Data(Recuperacion),
                            Color = HighChart.GetColor(1),
                            PlotOptionsLine = HighChart.getLine()
                        }
                    };

                    gp.SetSeries(series);
                    pGp.SetSeries(series);
                }
            }

            model.detalle = gp;
            model.pDetalle = pGp;
            return PartialView("_GraficaDetallePopUp", model);
        }

        /*Gráficos financieros*/
        public ActionResult Financiero()
        {
            try
            {
                Load();
                DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
                Session["Periodo1"] = new DateTime(fechaEntidad.AddMonths(-13).Year, fechaEntidad.AddMonths(-13).Month, 1).ToShortDateString();
                Session["Periodo2"] = new DateTime(fechaEntidad.AddMonths(-7).Year, fechaEntidad.AddMonths(-7).Month, 1).ToShortDateString();
                Session["Periodo3"] = fechaEntidad.AddMonths(-1).ToShortDateString();
                Session["Check"] = Session["IsFGA"].ToString() == "1" ? false : true;
                Graficos_Financieros gp = GetGraphFinan();
                return View(gp);
            }
            catch (Exception)
            {
            }

            return View(new Graficos_Financieros());
        }


        public Graficos_Financieros GetGraphFinan()
        {
            int i = 0;
            int num_activos = 4;
            int num_pasivos = 4;
            Graficos_Financieros m = new Graficos_Financieros();
            var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                  bool.Parse(Session["Check"].ToString()) ? DateTime.MaxValue : Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString())).Where(o => o.ACTIVO > 0).ToList();

            if (tak.Count() > 0)
            {
                #region GetInfo

                object[] Pasivo = new object[tak.Count()];
                object[] Patrimonio = new object[tak.Count()];
                string[] XAxis = new string[tak.Count()];

                string[] XAxisActivo = new string[num_activos];
                XAxisActivo[0] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NCARTERA);
                XAxisActivo[1] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NINVERSIONESFINAN);
                XAxisActivo[2] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NDISPONIBILIDADES);
                XAxisActivo[3] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NOTROSACTIVOS);

                string[] XAxisPasivo = new string[num_pasivos];
                XAxisPasivo[0] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NOBLIGACIONESPUBLICO);
                XAxisPasivo[1] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NOBLIGACIONESENTIDADES);
                XAxisPasivo[2] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NCUENTASXPAGAR);
                XAxisPasivo[3] = Utility.Utilitarios.toUpperFirstLetter(tak[0].NOTROSPASIVOS);

                List<Serie> listaActivo = new List<Serie>();
                listaActivo.Add(new Serie(num_activos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()).ToString("MMMM yy"))));
                listaActivo.Add(new Serie(num_activos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()).ToString("MMMM yy"))));
                if (!bool.Parse(Session["Check"].ToString()))
                    listaActivo.Add(new Serie(num_activos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString()).ToString("MMMM yy"))));

                List<Serie> listaActivoProductivo = new List<Serie>();
                listaActivoProductivo.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NINVERSIONESFINAN)));
                listaActivoProductivo.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NCARTERA)));
                listaActivoProductivo.Add(new Serie(tak.Count(), "Activo productivo"));

                List<Serie> listaPasivo = new List<Serie>();
                listaPasivo.Add(new Serie(num_pasivos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()).ToString("MMMM yy"))));
                listaPasivo.Add(new Serie(num_pasivos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()).ToString("MMMM yy"))));
                if (!bool.Parse(Session["Check"].ToString()))
                    listaPasivo.Add(new Serie(num_pasivos, Utility.Utilitarios.toUpperFirstLetter(Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString()).ToString("MMMM yy"))));

                List<Serie> listaPasivoCosto = new List<Serie>();
                listaPasivoCosto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NOBLIGACIONESENTIDADES)));
                listaPasivoCosto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NOBLIGACIONESPUBLICO)));
                listaPasivoCosto.Add(new Serie(tak.Count(), "Pasivo con costo"));

                List<Serie> listaIngreso = new List<Serie>();
                listaIngreso.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NINGRESOCARTERA)));
                listaIngreso.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NINGRESODIVERSOS)));
                listaIngreso.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NINGRESOINST)));
                listaIngreso.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NOTROSINGRESOS)));

                List<Serie> listaGasto = new List<Serie>();
                listaGasto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NGASTOPUBLICO)));
                listaGasto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NGASTOESTIMACIONES)));
                listaGasto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NOTROSGASTOS)));
                listaGasto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NGASTOENTIDADES)));
                listaGasto.Add(new Serie(tak.Count(), Utility.Utilitarios.toUpperFirstLetter(tak[0].NGASTOADMIN)));

                foreach (FGA_Consultar_Graficos_Financieros_Result detalle in tak)
                {
                    try
                    {
                        /*BALANCE*/
                        Pasivo[i] = (detalle.PASIVO / detalle.ACTIVO) * 100;
                        Patrimonio[i] = (detalle.PATRIMONIO / detalle.ACTIVO) * 100;
                        XAxis[i] = Utility.Utilitarios.toUpperFirstLetter(detalle.PERIODO.Value.ToString("MMMM yy"));

                        /*ACTIVO*/
                        listaActivo[i].valores[0] = (detalle.CARTERA / detalle.ACTIVO) * 100;
                        listaActivo[i].valores[1] = (detalle.INVERSIONESFINANC / detalle.ACTIVO) * 100;
                        listaActivo[i].valores[2] = (detalle.DISPONIBILIDADES / detalle.ACTIVO) * 100;
                        listaActivo[i].valores[3] = (detalle.OTROSACTIVOS / detalle.ACTIVO) * 100;

                        /*PASIVO*/
                        listaPasivo[i].valores[0] = (detalle.OBLIGACIONESPUBLICO / detalle.PASIVO) * 100;
                        listaPasivo[i].valores[1] = (detalle.OBLIGACIONESENTIDADES / detalle.PASIVO) * 100;
                        listaPasivo[i].valores[2] = (detalle.CUENTASXPAGAR / detalle.PASIVO) * 100;
                        listaPasivo[i].valores[3] = (detalle.OTROSPASIVOS / detalle.PASIVO) * 100;

                        /*ACTIVO PRODUCTIVO*/
                        listaActivoProductivo[0].valores[i] = detalle.INVERSIONESFINANC / 1000000;
                        listaActivoProductivo[1].valores[i] = detalle.CARTERA / 1000000;
                        listaActivoProductivo[2].valores[i] = (decimal)listaActivoProductivo[0].valores[i] + (decimal)listaActivoProductivo[1].valores[i];

                        /*PASIVO CON COSTO*/
                        listaPasivoCosto[0].valores[i] = detalle.OBLIGACIONESENTIDADES / 1000000;
                        listaPasivoCosto[1].valores[i] = detalle.OBLIGACIONESPUBLICO / 1000000;
                        listaPasivoCosto[2].valores[i] = (decimal)listaPasivoCosto[0].valores[i] + (decimal)listaPasivoCosto[1].valores[i];

                        /*PASIVO CON COSTO*/
                        listaPasivoCosto[0].valores[i] = detalle.OBLIGACIONESENTIDADES / 1000000;
                        listaPasivoCosto[1].valores[i] = detalle.OBLIGACIONESPUBLICO / 1000000;
                        listaPasivoCosto[2].valores[i] = (decimal)listaPasivoCosto[0].valores[i] + (decimal)listaPasivoCosto[1].valores[i];

                        /*INGRESO*/
                        listaIngreso[0].valores[i] = detalle.INGRESOCARTERA;
                        listaIngreso[1].valores[i] = detalle.INGRESODIVERSOS;
                        listaIngreso[2].valores[i] = detalle.INGRESOINST;
                        listaIngreso[3].valores[i] = detalle.OTROSINGRESOS;

                        listaIngreso[0].total += (decimal)listaIngreso[0].valores[i];
                        listaIngreso[1].total += (decimal)listaIngreso[1].valores[i];
                        listaIngreso[2].total += (decimal)listaIngreso[2].valores[i];
                        listaIngreso[3].total += (decimal)listaIngreso[3].valores[i];

                        /*GASTO*/
                        listaGasto[0].valores[i] = detalle.GASTOPUBLICO;
                        listaGasto[1].valores[i] = detalle.GASTOESTIMACIONES;
                        listaGasto[2].valores[i] = detalle.OTROSGASTOS;
                        listaGasto[3].valores[i] = detalle.GASTOENTIDADES;
                        listaGasto[4].valores[i] = detalle.GASTOADMIN;

                        listaGasto[0].total += (decimal)listaGasto[0].valores[i];
                        listaGasto[1].total += (decimal)listaGasto[1].valores[i];
                        listaGasto[2].total += (decimal)listaGasto[2].valores[i];
                        listaGasto[3].total += (decimal)listaGasto[3].valores[i];
                        listaGasto[4].total += (decimal)listaGasto[4].valores[i];
                        i += 1;
                    }
                    catch
                    {
                    }
                }

                #endregion

                #region Balance
                HighChart.ConfigChart(ref m.composicionBalance, "Balance", null);
                m.composicionBalance.SetXAxis(new XAxis
                {
                    Categories = XAxis,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });
                m.composicionBalance.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.composicionBalance.SetPlotOptions(HighChart.getLabelStackingPercent());
                m.composicionBalance.SetSeries(new Series[]
                {
                            new Series{
                                Type = ChartTypes.Column,
                                Name = "Pasivo",
                                Data = new Data(Pasivo),
                                Color = ColorTranslator.FromHtml("#0B4E91")
                            },
                            new Series{
                                Type = ChartTypes.Column,
                                Name = "Patrimonio",
                                Data = new Data(Patrimonio),
                                Color = ColorTranslator.FromHtml("#ed7c2f")//7d7d7d
                            }
                }
                );
                #endregion

                #region Activo              
                HighChart.ConfigChart(ref m.composicionActivoPorc, "Activo", null);
                m.composicionActivoPorc.SetXAxis(new XAxis
                {
                    Categories = XAxisActivo,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });

                m.composicionActivoPorc.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.composicionActivoPorc.SetPlotOptions(HighChart.getLabelPercent());

                Series[] series = new Series[listaActivo.Count()];
                Series serie;
                i = 0;
                foreach (Serie detalle in listaActivo)
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

                m.composicionActivoPorc.SetSeries(
                    series
                );

                #endregion

                #region ActivoProductivo

                HighChart.ConfigChart(ref m.composicionActivoMonto, "ActivoP", null);
                m.composicionActivoMonto.SetXAxis(new XAxis
                {
                    Categories = XAxis,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });

                m.composicionActivoMonto.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));
                m.composicionActivoMonto.SetPlotOptions(HighChart.getLabelAmmount());
                m.composicionActivoMonto.SetSeries(new Series[]
                {
                     new Series{
                        Type = ChartTypes.Line,
                        Name = listaActivoProductivo[2].nombre,
                        Data = new Data(listaActivoProductivo[2].valores),
                        Color = ColorTranslator.FromHtml("#7d7d7d"),
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaActivoProductivo[1].nombre,
                        Data = new Data(listaActivoProductivo[1].valores),
                        Color = ColorTranslator.FromHtml("#0B4E91"),
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaActivoProductivo[0].nombre,
                        Data = new Data(listaActivoProductivo[0].valores),
                        Color = ColorTranslator.FromHtml("#ed7c2f"),
                    }
                });
                #endregion

                #region Pasivo              
                HighChart.ConfigChart(ref m.composicionPasivoPorc, "Pasivo", null);
                m.composicionPasivoPorc.SetXAxis(new XAxis
                {
                    Categories = XAxisPasivo,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });

                m.composicionPasivoPorc.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.composicionPasivoPorc.SetPlotOptions(HighChart.getLabelPercent());

                series = new Series[listaPasivo.Count];
                i = 0;
                foreach (Serie detalle in listaPasivo)
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

                m.composicionPasivoPorc.SetSeries(
                    series
                );

                #endregion

                #region PasivoCosto

                HighChart.ConfigChart(ref m.composicionPasivoMonto, "PasivoCosto", null);
                m.composicionPasivoMonto.SetXAxis(new XAxis
                {
                    Categories = XAxis,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });
                m.composicionPasivoMonto.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));
                m.composicionPasivoMonto.SetPlotOptions(HighChart.getLabelAmmount());
                m.composicionPasivoMonto.SetSeries(new Series[]
                {
                     new Series{
                        Type = ChartTypes.Line,
                        Name = listaPasivoCosto[2].nombre,
                        Data = new Data(listaPasivoCosto[2].valores),
                        Color = ColorTranslator.FromHtml("#7d7d7d"),
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaPasivoCosto[1].nombre,
                        Data = new Data(listaPasivoCosto[1].valores),
                        Color = ColorTranslator.FromHtml("#0B4E91"),
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaPasivoCosto[0].nombre,
                        Data = new Data(listaPasivoCosto[0].valores),
                        Color = ColorTranslator.FromHtml("#ed7c2f"),
                    }
                });

                #endregion

                #region ComposicionIngreso
                HighChart.ConfigChart(ref m.composicionIngreso, "Ingreso", null);
                m.composicionIngreso.SetXAxis(new XAxis
                {
                    Categories = XAxis,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });
                m.composicionIngreso.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.composicionIngreso.SetPlotOptions(HighChart.getLabelStackingPercent(150));

                series = new Series[4];
                i = 0;
                foreach (Serie detalle in listaIngreso.OrderByDescending(o => o.total))
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

                m.composicionIngreso.SetSeries(
                    series
                );
                #endregion

                #region ComposicionGasto
                HighChart.ConfigChart(ref m.composicionGasto, "Gasto", null);
                m.composicionGasto.SetXAxis(new XAxis
                {
                    Categories = XAxis,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    }
                });
                m.composicionGasto.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                m.composicionGasto.SetPlotOptions(HighChart.getLabelStackingPercent(150));

                series = new Series[5];
                i = 0;
                foreach (Serie detalle in listaGasto.OrderByDescending(o => o.total))
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

                m.composicionGasto.SetSeries(
                    series
                );

                #endregion
            }
            return m;
        }

        public ActionResult GetGridBalance()
        {
            try
            {
                var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                  bool.Parse(Session["Check"].ToString()) ? DateTime.MaxValue : Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString()))
                                  .OrderBy(o => o.PERIODO).ToList();

                /*listaBalance*/
                List<string[]> listaBalance = new List<string[]>();

                if (tak.Count() > 0)
                {
                    string[] activo = new string[tak.Count() + 1];
                    string[] pasivo = new string[tak.Count() + 1];
                    string[] patrimonio = new string[tak.Count() + 1];
                    int i = 1;
                    activo[0] = "Activo";
                    pasivo[0] = "Pasivo";
                    patrimonio[0] = "Patrimonio";

                    foreach (FGA_Consultar_Graficos_Financieros_Result detalle in tak)
                    {
                        activo[i] = Utility.Utilitarios.ConvertirAString(detalle.ACTIVO / 1000000);
                        pasivo[i] = Utility.Utilitarios.ConvertirAString(detalle.PASIVO / 1000000);
                        patrimonio[i] = Utility.Utilitarios.ConvertirAString(detalle.PATRIMONIO / 1000000);
                        i += 1;
                    }

                    listaBalance.Add(activo);
                    listaBalance.Add(pasivo);
                    listaBalance.Add(patrimonio);

                }

                return Json(new { aaData = listaBalance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridIngreso()
        {
            try
            {
                var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                    bool.Parse(Session["Check"].ToString()) ? DateTime.MaxValue : Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString()))
                                    .Where(o => o.ACTIVO > 0).OrderBy(o => o.PERIODO).ToList();

                var result = from c in tak
                             select new string[] {c.PERIODO.Value.ToString("yyyy MM"),
                                 Utility.Utilitarios.toUpperFirstLetter(c.PERIODO.Value.ToString("MMMM yyyy")),
                                 Utility.Utilitarios.ConvertirAString(c.ING_FINANCIEROS) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.ING_DIVERSOS) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.ING_RECUPERACION) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.ING_CRECIMIENTO),
                             };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridGasto()
        {
            try
            {
                var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                    bool.Parse(Session["Check"].ToString()) ? DateTime.MaxValue : Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString()))
                                    .Where(o => o.ACTIVO > 0).ToList();

                var result = from c in tak
                             select new string[] {c.PERIODO.Value.ToString("yyyy MM"),
                                 Utility.Utilitarios.toUpperFirstLetter(c.PERIODO.Value.ToString("MMMM yyyy")),
                                 Utility.Utilitarios.ConvertirAString(c.GAST_FINANCIERO) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.GAST_ESTIMACIONES) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.GAST_OPERATIVOS) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.GAST_ADMINISTRATIVOS) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.GAST_IMPUESTO) + "%",
                                 Utility.Utilitarios.ConvertirAString(c.GAST_CRECIMIENTO),
                             };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult LoadBuscarFinac(String Entidades, DateTime Periodo1, DateTime Periodo2, DateTime Periodo3, bool chkprivate)
        {
            try
            {
                DateTime[] listaPeriodos = new[] { Periodo1, Periodo2, Periodo3 };
                listaPeriodos = listaPeriodos.OrderBy(o => o).ToArray();
                Session["IdEntidad"] = Entidades;
                Session["Periodo1"] = listaPeriodos[0].ToShortDateString();
                Session["Periodo2"] = listaPeriodos[1].ToShortDateString();
                Session["Periodo3"] = listaPeriodos[2].ToShortDateString();
                Session["Check"] = chkprivate;
                Load();
            }
            catch (Exception)
            {
            }

            return View("Financiero", GetGraphFinan());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                sp.Close();
                usr.Close();
                gr.Close();
                cat.Close();
                tipCar.Close();
            }
            base.Dispose(disposing);
        }
    }
}