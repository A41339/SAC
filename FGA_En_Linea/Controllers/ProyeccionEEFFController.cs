using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using FGA.Model;
using FGA.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class ProyeccionEEFFController : BaseController
    {
        public ActionResult Index()
        {
            Load();
            var fechaCorte = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString()).AddMonths(-1);
            Session["PeriodoI"] = fechaCorte.ToShortDateString();
            Session["TipoReporte"] = null;
            Session["TipoReporte2"] = null;
            Session["TipoReporte3"] = null;
            Session["TipoReporte4"] = null;
            Session["Plazo"] = "1";
            Session["FechaInicio"] = Session["Plazo"].ToString() == "1" ? fechaCorte : fechaCorte.Month == 12 ? fechaCorte : (new DateTime(fechaCorte.Year, 1, 1)).AddMonths(-1);
            Modelo_PEEFF model = new Modelo_PEEFF();
            return View(model);
        }

        public ActionResult Buscar(String Entidades, DateTime PeriodoI, String ddlPlazo, String ddlTasa, String tasaNuevaColocacion,
            String tasaCarteraVigente, String tasaCarteraVencida, String tasaInversiones, String tasaCaptaciones,
            String tasaObligaciones, String ReservaVoluntaria, String RecuperacionIncobrable, String ddlCrecimiento, String CarteraAnual,
            String Captaciones, String Capital, String GastoAdmin, String GastoEstimac, String command) {

            Modelo_PEEFF model = new Modelo_PEEFF();
            Proy_EEFF ent = new Proy_EEFF();
            model.AlertaProyeccion = DateTime.Now.Year != PeriodoI.Year && PeriodoI.Month != 12 ? 0 : 1;
            model.Supuestos = true;
            model.proyeccion = false;
            model.ddlTasa = ddlTasa;
            model.ddlPlazo = ddlPlazo;
            model.ddlCrecimiento = ddlCrecimiento;

            if (model.AlertaProyeccion == 0)
                model.mensaje = "Se podrán usar cortes del periodo vigente y el último cierre fiscal cargado, meses anteriores a ese no se podrán usar como base.";

            Session["IdEntidad"] = Entidades;
            Session["PeriodoI"] = PeriodoI.ToShortDateString();
            Session["TipoReporte"] = null;
            Session["TipoReporte2"] = null;
            Session["TipoReporte3"] = null;
            Session["TipoReporte4"] = null;
            Session["Plazo"] = ddlPlazo;
            Session["FechaInicio"] = Session["Plazo"].ToString() == "1" ? PeriodoI : PeriodoI.Month == 12 ? PeriodoI : (new DateTime(PeriodoI.Year, 1, 1)).AddMonths(-1);

            Load();
            var crecimiento = sp.FGA_CrecimientoSugerido(PeriodoI, Session["idEntidad"].ToString())[0];
            ent = proy.GetProy(Entidades, PeriodoI, ddlPlazo);

            model.PCapital = crecimiento.CRECIMIENTO_CAPITAL;
            model.PCaptaciones = crecimiento.CRECIMIENTO_CAPTACIONES;
            model.PCarteraAnual = crecimiento.CRECIMIENTO_CARTERA;
            model.PGastoAdmin = crecimiento.GASTO_ADMIN;
            model.PGastoEstimac = crecimiento.GASTO_ESTIMAC;
            model.PTasaCaptaciones = crecimiento.TASA_CAPTACIONES;
            model.PTasaCarteraVigente = crecimiento.TASA_CARTERAVIGENTE;
            model.PTasaCarteraVencida = crecimiento.TASA_CARTERAVENCIDA;
            model.PTasaInversiones = crecimiento.TASA_INVERSIONES;
            model.PTasaNuevaColocacion = crecimiento.TASA_CARTERANUEVA;
            model.PTasaObligaciones = crecimiento.TASA_OBLIGACIONES;

            if (command == "Consultar")
            {
                if (ent != null)
                {
                    model.Generar = true;
                    model.AlertaProyeccion = 2;
                    model.mensaje = "Se muestran los supuestos guardados con el corte " + PeriodoI.ToString("MMMM yyyy");
                }
            }
            else
            {
                model.AlertaProyeccion = 2;
                model.mensaje = "Acción realizada con éxito";

                if (command == "Guardar")
                {
                    if (ent != null)
                        proy.Delete(ent.Id.ToString());
                    else
                        ent = new Proy_EEFF();

                    ent.IdEntidad = Entidades;
                    ent.PeriodoCorte = PeriodoI;
                    ent.TipoCrecimiento = ddlCrecimiento;
                    ent.TipoTasa = ddlTasa;
                    ent.TipoPlazo = ddlPlazo;
                    ent.CrecimientoCapital = decimal.Parse(Capital.Replace(".", ",").Replace("%", ""));
                    ent.CrecimientoCaptaciones = decimal.Parse(Captaciones.Replace(".", ",").Replace("%", ""));
                    ent.CrecimientoCartera = decimal.Parse(CarteraAnual.Replace(".", ",").Replace("%", ""));
                    ent.GastoAdmin = decimal.Parse(GastoAdmin.Replace(".", ",").Replace("%", ""));
                    ent.GastoEstimac = decimal.Parse(GastoEstimac.Replace(".", ",").Replace("%", ""));
                    ent.ReservasVoluntarias = decimal.Parse(ReservaVoluntaria.Replace(".", ",").Replace("%", ""));
                    ent.RecuperacionIncobrable = decimal.Parse(RecuperacionIncobrable.Replace(".", ",").Replace("%", ""));
                    ent.TasaCaptaciones = decimal.Parse(tasaCaptaciones.Replace(".", ",").Replace("%", ""));
                    ent.TasaCarteraVencida = decimal.Parse(tasaCarteraVencida.Replace(".", ",").Replace("%", ""));
                    ent.TasaCarteraVigente = decimal.Parse(tasaCarteraVigente.Replace(".", ",").Replace("%", ""));
                    ent.TasaInversiones = decimal.Parse(tasaInversiones.Replace(".", ",").Replace("%", ""));
                    ent.TasaNuevaColocacion = decimal.Parse(tasaNuevaColocacion.Replace(".", ",").Replace("%", ""));
                    ent.TasaObligaciones = decimal.Parse(tasaObligaciones.Replace(".", ",").Replace("%", ""));
                    proy.Add(ref ent);
                    model.Generar = true;
                }
                else if (command == "Borrar")
                {
                    if (ent != null)
                        proy.Delete(ent.Id.ToString());

                    ent = null;
                }
                else
                {
                    if (ent is null)
                    {
                        model.AlertaProyeccion = 0;
                        model.mensaje = "Los supuestos no están guardados, debe guardarlo para generar las proyecciones";
                    }
                    else
                    {
                        model.proyeccion = true;
                        Session["TipoReporte"] = enum_tipoReporte.rpt_balance_proy;
                        Session["TipoReporte2"] = enum_tipoReporte.rpt_er_proy;
                        Session["TipoReporte3"] = enum_tipoReporte.rpt_an_balance_proy;
                        Session["TipoReporte4"] = enum_tipoReporte.rpt_an_er_proy;
                    }
                }
            }

            try
            {
                if (ent is null)
                {
                    if (ddlCrecimiento == "1" || ddlCrecimiento is null)
                    {
                        model.Capital = 0;
                        model.Captaciones = 0;
                        model.CarteraAnual = 0;
                        model.GastoAdmin = 0;
                        model.GastoEstimac = 0;
                    }
                    else
                    {
                        model.Capital = crecimiento.CRECIMIENTO_CAPITAL;
                        model.Captaciones = crecimiento.CRECIMIENTO_CAPTACIONES;
                        model.CarteraAnual = crecimiento.CRECIMIENTO_CARTERA;
                        model.GastoAdmin = crecimiento.GASTO_ADMIN;
                        model.GastoEstimac = crecimiento.GASTO_ESTIMAC;
                    }

                    if (ddlTasa == "1" || ddlTasa is null)
                    {
                        model.TasaCaptaciones = 0;
                        model.TasaCarteraVencida = 0;
                        model.TasaCarteraVigente = 0;
                        model.TasaInversiones = 0;
                        model.TasaNuevaColocacion = 0;
                        model.TasaObligaciones = 0;
                    }
                    else
                    {
                        model.TasaCaptaciones = crecimiento.TASA_CAPTACIONES;
                        model.TasaCarteraVencida = crecimiento.TASA_CARTERAVENCIDA;
                        model.TasaCarteraVigente = crecimiento.TASA_CARTERAVIGENTE;
                        model.TasaInversiones = crecimiento.TASA_INVERSIONES;
                        model.TasaNuevaColocacion = crecimiento.TASA_CARTERANUEVA;
                        model.TasaObligaciones = crecimiento.TASA_OBLIGACIONES;
                    }
                }
                else
                {
                    model.ddlCrecimiento = ent.TipoCrecimiento;
                    model.ddlTasa = ent.TipoTasa;
                    model.ddlPlazo = ent.TipoPlazo;
                    model.Capital = ent.CrecimientoCapital;
                    model.Captaciones = ent.CrecimientoCaptaciones;
                    model.CarteraAnual = ent.CrecimientoCartera;
                    model.GastoAdmin = ent.GastoAdmin;
                    model.GastoEstimac = ent.GastoEstimac;
                    model.ReservaVoluntaria = ent.ReservasVoluntarias;
                    model.RecuperacionIncobrable = ent.RecuperacionIncobrable;
                    model.TasaCaptaciones = ent.TasaCaptaciones;
                    model.TasaCarteraVigente = ent.TasaCarteraVigente;
                    model.TasaCarteraVencida = ent.TasaCarteraVencida;
                    model.TasaInversiones = ent.TasaInversiones;
                    model.TasaNuevaColocacion = ent.TasaNuevaColocacion;
                    model.TasaObligaciones = ent.TasaObligaciones;
                }
            }
            catch (Exception)
            {
            }

            GetGraph(ref model);
            Session["Proyeccion"] = model;
            Session["CrecimientoCartera"] = model.CarteraAnual;
            return View("Index", model);
        }

        public void getLimites(ref decimal max, ref decimal min, decimal valor) {
            if (valor > max)
                max = valor;
            else if (valor < min)
                min = valor;
        }

        public void GetGraph(ref Modelo_PEEFF modelo)
        {
            int gCartera = (int)enum_indicadoresProyeccion.cartera;
            int gTotalActivo = (int)enum_indicadoresProyeccion.totalActivo;
            int gTotalPasivo = (int)enum_indicadoresProyeccion.totalPasivo;
            int gObligacionesPub = (int)enum_indicadoresProyeccion.obligacionesPublico;
            int gCapitalSocial = (int)enum_indicadoresProyeccion.capitalSocial;
            int gTotalIngreso = (int)enum_indicadoresProyeccion.totalIngresos;
            int gTotalGasto = (int)enum_indicadoresProyeccion.totalGastos;
            int gResultadoPeriodo = (int)enum_indicadoresProyeccion.resultadoPeriodo;
            int gGastosUtilidad = (int)enum_indicadoresProyeccion.gastos_utilidadBruta;
            int gUtilidadPatrimonio = (int)enum_indicadoresProyeccion.utilidad_patrimonio;

            decimal max = 0;
            decimal min = 0;
            decimal valor = 0;

            var meses = Session["Plazo"].ToString() == "1" ? 12 : 36;
            var periodos = Session["Plazo"].ToString() == "1" ? 2 : 4;
            var fechaInicial = ConvertirAFecha(Session["FechaInicio"].ToString());
            int mesesDiciembre = 12 - fechaInicial.Month;

            var tak = sp.FGA_Generar_Proy_Matrices(24, meses, ConvertirAFecha(Session["PeriodoI"].ToString()), ConvertirAFecha(Session["PeriodoI"].ToString()), Session["IdEntidad"].ToString(), 0).ToList();
            Session["Mora"] = tak;
            
            if (tak.Count() > 0)
            {
                int numPeriodos = tak.Count();
                HighChart.ConfigChart(ref modelo.gMora, "Mora", null);
                object[] ListaMora = new object[tak.Count() - 1];
                string[] fechas = new string[numPeriodos];

                List<Serie> listaSeries = new List<Serie>();
                listaSeries.Add(new Serie(numPeriodos, "Al día"));
                listaSeries.Add(new Serie(numPeriodos, "Mora Temprana"));
                listaSeries.Add(new Serie(numPeriodos, "Mora mayor a 90 días"));

                for (int j = 0; j < numPeriodos; j++)
                {
                    fechas[j] = tak[j].PERIODO.Value.Month.ToString() + "-" + tak[j].PERIODO.Value.Year.ToString();
                    listaSeries[0].valores[j] = tak[j].PALDIA;
                    listaSeries[1].valores[j] = tak[j].PR1_30DIAS + tak[j].PR31_60DIAS + tak[j].PR61_90DIAS;
                    listaSeries[2].valores[j] = tak[j].PR91_180DIAS + tak[j].PMAS180 + tak[j].PCJ;
                }

                modelo.gMora.SetXAxis(HighChart.GetXAxis(fechas));
                modelo.gMora.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent", AxisTypes.Logarithmic));
                modelo.gMora.SetPlotOptions(HighChart.getLabelStackingNormal());
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

                modelo.gMora.SetSeries(series);
            }

            var indicadores = sp.FGA_Rpt_EEFF_Proy(Utility.Utilitarios.ConvertirAFecha(Session["PeriodoI"].ToString()), Session["IdEntidad"].ToString(), "I", int.Parse(Session["Plazo"].ToString())).ToList();
            Session["Indicadores"] = indicadores;

            if (indicadores.Count > 0) {

                string[] Periodos = new string[periodos];

                /*Cartera de crédito*/
                object[] ValoresC = new object[periodos];
                object[] PorcentajesC = new object[periodos];
                Periodos[0] = toUpperFirstLetter(fechaInicial.ToString("MMMM yyyy"));
                Periodos[1] = toUpperFirstLetter(fechaInicial.AddMonths(12).ToString("MMMM yyyy"));
                ValoresC[0] = indicadores.Where(o => o.ID == gCartera).ToList()[0].P1;
                ValoresC[1] = indicadores.Where(o => o.ID == gCartera).ToList()[0].P2;

                if (periodos == 4) {
                    ValoresC[2] = indicadores.Where(o => o.ID == gCartera).ToList()[0].P3;
                    ValoresC[3] = indicadores.Where(o => o.ID == gCartera).ToList()[0].P4;
                    Periodos[2] = toUpperFirstLetter(fechaInicial.AddMonths(24).ToString("MMMM yyyy"));
                    Periodos[3] = toUpperFirstLetter(fechaInicial.AddMonths(36).ToString("MMMM yyyy"));
                    PorcentajesC[0] = null;
                    
                    valor = (decimal)((indicadores.Where(o => o.ID == gCartera).ToList()[0].P2 / indicadores.Where(o => o.ID == gCartera).ToList()[0].P1 - 1) * 100);
                    max = valor;
                    min = valor;
                    PorcentajesC[1] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gCartera).ToList()[0].P3 / indicadores.Where(o => o.ID == gCartera).ToList()[0].P2 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesC[2] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gCartera).ToList()[0].P4 / indicadores.Where(o => o.ID == gCartera).ToList()[0].P3 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesC[3] = valor;
                }   
                

                HighChart.ConfigChart(ref modelo.gCartera, "Cartera", null);
                modelo.gCartera.SetXAxis(HighChart.GetXAxis(Periodos));
                modelo.gCartera.SetYAxis(new List<YAxis>
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
                        Min = (Number?)(min - 2),
                        Max = (Number?)(max + 2)
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
                }.ToArray());
                modelo.gCartera.SetPlotOptions(HighChart.getLabelAmmount());
                modelo.gCartera.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Variación Internanual",
                        Data = new Data(PorcentajesC),
                        Color = HighChart.GetColor(2),
                        YAxis = "Porcentaje",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Saldo",
                        Data = new Data(ValoresC),
                        Color = HighChart.GetColor(0),
                        YAxis = "Millones"
                    },
                });

                /*Activo y Pasivo*/
                object[] Activos = new object[periodos];
                object[] Pasivos = new object[periodos];

                Activos[0] = indicadores.Where(o => o.ID == gTotalActivo).ToList()[0].P1;
                Activos[1] = indicadores.Where(o => o.ID == gTotalActivo).ToList()[0].P2;
                Pasivos[0] = indicadores.Where(o => o.ID == gTotalPasivo).ToList()[0].P1;
                Pasivos[1] = indicadores.Where(o => o.ID == gTotalPasivo).ToList()[0].P2;

                if (periodos == 4)
                {
                    Activos[2] = indicadores.Where(o => o.ID == gTotalActivo).ToList()[0].P3;
                    Activos[3] = indicadores.Where(o => o.ID == gTotalActivo).ToList()[0].P3;
                    Pasivos[2] = indicadores.Where(o => o.ID == gTotalPasivo).ToList()[0].P3;
                    Pasivos[3] = indicadores.Where(o => o.ID == gTotalPasivo).ToList()[0].P4;
                }

                HighChart.ConfigChart(ref modelo.gActivo_pasivo, "ActivoPasivo", null);
                modelo.gActivo_pasivo.SetXAxis(HighChart.GetXAxis(Periodos));
                modelo.gActivo_pasivo.SetYAxis(new List<YAxis>
                {
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
                }.ToArray());
                modelo.gActivo_pasivo.SetPlotOptions(HighChart.getLabelAmmount());
                modelo.gActivo_pasivo.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Total activo productivo",
                        Data = new Data(Activos),
                        Color = HighChart.GetColor(0),
                        YAxis = "Millones",
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Total pasivo con costo",
                        Data = new Data(Pasivos),
                        Color = HighChart.GetColor(1),
                        YAxis = "Millones"
                    },
                });

                /*Obligaciones con publico*/
                object[] ValoresO = new object[periodos];
                object[] PorcentajesO = new object[periodos];
                ValoresO[0] = indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P1;
                ValoresO[1] = indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P2;

                if (periodos == 4)
                {
                    ValoresO[2] = indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P3;
                    ValoresO[3] = indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P4;                 
                    PorcentajesO[0] = null;

                    valor = (decimal)(indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P2 / indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P1 - 1) * 100;
                    max = valor;
                    min = valor;
                    PorcentajesO[1] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P3 / indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P2 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesO[2] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P4 / indicadores.Where(o => o.ID == gObligacionesPub).ToList()[0].P3 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesO[3] = valor;
                }

                HighChart.ConfigChart(ref modelo.gObligacionPublico, "ObligPublico", null);
                modelo.gObligacionPublico.SetXAxis(HighChart.GetXAxis(Periodos));
                modelo.gObligacionPublico.SetYAxis(new List<YAxis>
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
                        Min = (Number?)(min - 2),
                        Max = (Number?)(max + 2)
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
                }.ToArray());
                modelo.gObligacionPublico.SetPlotOptions(HighChart.getLabelAmmount());
                modelo.gObligacionPublico.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Variación Internanual",
                        Data = new Data(PorcentajesO),
                        Color = HighChart.GetColor(2),
                        YAxis = "Porcentaje",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Saldo",
                        Data = new Data(ValoresO),
                        Color = HighChart.GetColor(0),
                        YAxis = "Millones"
                    },
                });

                /*Gasto Utilidad*/
                object[] ValoresGU = new object[mesesDiciembre];
                string[] PeriodosGU = new string[mesesDiciembre];
                min = 0;
                max = 0;

                for (int i = 0; i < mesesDiciembre; i++)
                {
                    int propertyIndex = i % 12 + 1; // Esto va de 1 a 12 cíclicamente
                    string propertyName = "P" + propertyIndex; // Genera el nombre de la propiedad, como "P1", "P2", etc.

                    // Obtiene el valor de la propiedad correspondiente usando reflection
                    decimal valorActual = (decimal)indicadores.Where(o => o.ID == gGastosUtilidad)
                                                              .Select(o => o.GetType().GetProperty(propertyName).GetValue(o))
                                                              .FirstOrDefault();

                    if (min > valorActual)
                        min = valorActual;
                    if (max < valorActual)
                        max = valorActual;

                    PeriodosGU[i] = fechaInicial.AddMonths(i).ToShortDateString();
                    ValoresGU[i] = valorActual;
                }

                HighChart.ConfigChart(ref modelo.gGastoUtilidad, "GastoUtilidad", null);
                modelo.gGastoUtilidad.SetXAxis(HighChart.GetXAxis(PeriodosGU));
                modelo.gGastoUtilidad.SetYAxis(new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "Gasto",
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
                        Opposite = true
                    },                   
                }.ToArray());
                modelo.gGastoUtilidad.SetPlotOptions(HighChart.getLabelPercent());
                modelo.gGastoUtilidad.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Utilidad Acumulada Trimestral / Patrimonio Promedio",
                        Data = new Data(ValoresGU),
                        Color = HighChart.GetColor(0),
                        YAxis = "Gasto",
                        PlotOptionsLine = HighChart.getLine()
                    },                   
                });

                /*Utilidad Patrimonio*/
                min = 0;
                max = 0;

                for (int i = 0; i < mesesDiciembre; i++)
                {
                    int propertyIndex = i % 12 + 1; // Esto va de 1 a 12 cíclicamente
                    string propertyName = "P" + propertyIndex; // Genera el nombre de la propiedad, como "P1", "P2", etc.

                    // Obtiene el valor de la propiedad correspondiente usando reflection
                    decimal valorActual = (decimal)indicadores.Where(o => o.ID == gUtilidadPatrimonio)
                                                              .Select(o => o.GetType().GetProperty(propertyName).GetValue(o))
                                                              .FirstOrDefault();

                    if (min > valorActual)
                        min = valorActual;
                    if (max < valorActual)
                        max = valorActual;

                   // PeriodosGU[i] = fechaInicial.AddMonths(i).ToString("N2");
                    ValoresGU[i] = valorActual;
                }

                HighChart.ConfigChart(ref modelo.gUtilidadPatrimonio, "UtilidadPatrim", null);
                modelo.gUtilidadPatrimonio.SetXAxis(HighChart.GetXAxis(PeriodosGU));
                modelo.gUtilidadPatrimonio.SetYAxis(new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "Utilidad",
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
                        Opposite = true
                    },
                }.ToArray());
                modelo.gUtilidadPatrimonio.SetPlotOptions(HighChart.getLabelPercent());
                modelo.gUtilidadPatrimonio.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Gastos de administración/Utilidad operacional bruta",
                        Data = new Data(ValoresGU),
                        Color = HighChart.GetColor(1),
                        YAxis = "Utilidad",
                        PlotOptionsLine = HighChart.getLine()
                    },
                });

                /*Margen Financiero*/
                object[] MargenFinanciero = new object[periodos];
                object[] ResultadoNeto = new object[periodos];

                MargenFinanciero[0] = indicadores.Where(o => o.ID == gTotalIngreso).ToList()[0].P1 - indicadores.Where(o => o.ID == gTotalGasto).ToList()[0].P1;
                MargenFinanciero[1] = indicadores.Where(o => o.ID == gTotalIngreso).ToList()[0].P2 - indicadores.Where(o => o.ID == gTotalGasto).ToList()[0].P2;
                ResultadoNeto[0] = indicadores.Where(o => o.ID == gResultadoPeriodo).ToList()[0].P1;
                ResultadoNeto[1] = indicadores.Where(o => o.ID == gResultadoPeriodo).ToList()[0].P2;

                if (periodos == 4)
                {
                    MargenFinanciero[2] = indicadores.Where(o => o.ID == gTotalIngreso).ToList()[0].P3 - indicadores.Where(o => o.ID == gTotalGasto).ToList()[0].P3;
                    MargenFinanciero[3] = indicadores.Where(o => o.ID == gTotalIngreso).ToList()[0].P3 - indicadores.Where(o => o.ID == gTotalGasto).ToList()[0].P4;
                    ResultadoNeto[2] = indicadores.Where(o => o.ID == gResultadoPeriodo).ToList()[0].P3;
                    ResultadoNeto[3] = indicadores.Where(o => o.ID == gResultadoPeriodo).ToList()[0].P4;
                }

                HighChart.ConfigChart(ref modelo.gIngreso_gasto, "IngresoGasto", null);
                modelo.gIngreso_gasto.SetXAxis(HighChart.GetXAxis(Periodos));
                modelo.gIngreso_gasto.SetYAxis(new List<YAxis>
                {
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
                }.ToArray());
                modelo.gIngreso_gasto.SetPlotOptions(HighChart.getBarLabelAmmount());
                modelo.gIngreso_gasto.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Bar,
                        Name = "Margen Financiero",
                        Data = new Data(MargenFinanciero),
                        Color = HighChart.GetColor(0),
                        YAxis = "Millones",
                    },
                    new Series{
                        Type = ChartTypes.Bar,
                        Name = "Resultado Neto",
                        Data = new Data(ResultadoNeto),
                        Color = HighChart.GetColor(1),
                        YAxis = "Millones"
                    },
                });


                /*Capital Social*/
                object[] ValoresKS = new object[periodos];
                object[] PorcentajesKS = new object[periodos];
                ValoresKS[0] = indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P1;
                ValoresKS[1] = indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P2;

                if (periodos == 4)
                {
                    ValoresKS[2] = indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P3;
                    ValoresKS[3] = indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P4;
                    PorcentajesKS[0] = null;

                    valor = (decimal)(indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P2 / indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P1 - 1) * 100;
                    max = valor;
                    min = valor;
                    PorcentajesKS[1] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P3 / indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P2 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesKS[2] = valor;

                    valor = (decimal)(indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P4 / indicadores.Where(o => o.ID == gCapitalSocial).ToList()[0].P3 - 1) * 100;
                    getLimites(ref max, ref min, valor);
                    PorcentajesKS[3] = valor;
                }

                HighChart.ConfigChart(ref modelo.gCapitalSocial, "CapitalSocial", null);
                modelo.gCapitalSocial.SetXAxis(HighChart.GetXAxis(Periodos));
                modelo.gCapitalSocial.SetYAxis(new List<YAxis>
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
                        Min = (Number)min - 2,
                        Max = (Number)max + 2
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
                }.ToArray());
                modelo.gCapitalSocial.SetPlotOptions(HighChart.getLabelAmmount());
                modelo.gCapitalSocial.SetSeries(new Series[]
                {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Variación Internanual",
                        Data = new Data(PorcentajesKS),
                        Color = HighChart.GetColor(2),
                        YAxis = "Porcentaje",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Saldo",
                        Data = new Data(ValoresKS),
                        Color = HighChart.GetColor(0),
                        YAxis = "Millones"
                    },
                });
            }
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = (List<Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result>) Session["Mora"]; 
                var result = from c in tak
                             select new string[] {
                                 toUpperFirstLetter(c.PERIODO.Value.ToString("MMMM yyyy")),
                                 ConvertirAString(c.ALDIA.Value),
                                 ConvertirAString(c.R1_30DIAS.Value),
                                 ConvertirAString(c.R31_60DIAS.Value),
                                 ConvertirAString(c.R61_90DIAS.Value),
                                 ConvertirAString(c.R91_180DIAS.Value),
                                 ConvertirAString(c.MAS180.Value),
                                 ConvertirAString(c.CJ.Value)
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }
            return Json(new string[] { "", "", "", "", "", "", "",""}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetIndicadores()
        {
            try
            {
                var tak = (List<Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result>)Session["Mora"];
                var indicadores = new List<string[]>();

                for(int i=0; i<tak.Count; i++)
                {
                    indicadores.Add(new string[] {toUpperFirstLetter(tak[i].PERIODO.Value.ToString("MMMM yyyy")), 
                        tak[i].ESTIMACION_X_MORA.Value.ToString("N2"), ((tak[i].CARTERA_MORA.Value / tak[i].CARTERA_TOTAL.Value) * 100).ToString("N2") + "%"});
                }

                return Json(new { aaData = indicadores.ToArray() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }
            return Json(new string[] { "", "", "", "", "", "", "", "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGridMora()
        {
            try
            {
                var tak = ((List<Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result>)Session["Mora"]);
                var indicadores = new List<string[]>();
                var result = from c in tak
                             select new string[] {
                                toUpperFirstLetter(c.PERIODO.Value.ToString("MMMM yyyy")),  
                                ConvertirAString(c.PALDIA.Value) + "%",
                                 ConvertirAString(c.PR1_30DIAS.Value) + "%",
                                 ConvertirAString(c.PR31_60DIAS.Value) + "%",
                                 ConvertirAString(c.PR61_90DIAS.Value) + "%",
                                 ConvertirAString(c.PR91_180DIAS.Value) + "%",
                                 ConvertirAString(c.PMAS180.Value) + "%",
                                 ConvertirAString(c.PCJ.Value) + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return Json(new string[] { "", "", "", "", "", "", "", "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGridIndicadores()
        {
            int indicadores = (int)enum_indicadoresProyeccion.cartera;
            var tak = ((List<Entities.Entities.Procedures.FGA_Rpt_EEFF_Proy_Result>) Session["Indicadores"]).Where(o => o.ID < indicadores).ToList();
            var result = from c in tak
                            select new string[] {
                            c.NOM_CUENTA,
                            (c.P1.Value > 0 && c.P1.Value < 1 ? (c.P1.Value * 100).ToString("N2") + "%" : c.P1.Value.ToString("N2")),
                            (c.P2.Value > 0 && c.P2.Value < 1 ? (c.P2.Value * 100).ToString("N2") + "%" : c.P2.Value.ToString("N2")),
                            Session["Plazo"].ToString() == "1" ? "" : (c.P3.Value > 0 && c.P3.Value < 1 ? (c.P3.Value * 100).ToString("N2") + "%" : c.P3.Value.ToString("N2")),
                            Session["Plazo"].ToString() == "1" ? "" : (c.P4.Value > 0 && c.P4.Value < 1 ? (c.P4.Value * 100).ToString("N2") + "%" : c.P4.Value.ToString("N2"))
                        };

            if(tak is null ? true : tak.Count == 0 ? true : false)
                return Json(new string[] { "", "", "", "", "" }, JsonRequestBehavior.AllowGet);

            return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);                       
        }

        private readonly FGA_En_Linea.Proy_EEFFService.Proy_EEFFServiceClient proy = new FGA_En_Linea.Proy_EEFFService.Proy_EEFFServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                proy.Close();
                sp.Close();
            }
            base.Dispose(disposing);
        }
    }
}