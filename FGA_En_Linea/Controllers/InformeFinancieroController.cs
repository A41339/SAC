using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using FGA.Model;
using FGA.Models;
using System.Linq;
using Entities.Entities.Procedures;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO.Packaging;
using System.Web.UI.WebControls;
using System.Linq.Expressions;
using Microsoft.VisualStudio.OLE.Interop;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using stdole;

namespace FGA.Controllers
{
    public class InformeFinancieroController : BaseController
    {
        public void PageLoad()
        {
            Load();
            DateTime fecha = Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());

            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-6).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            Session["Periodo3"] = Session["Periodo3"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo3"];
            Session["TipoReporte"] = null;
            Session["Check"] = false;
        }

        [AllowAnonymous]
        public ActionResult LoadReportAsync(String Entidades, DateTime Periodo1, DateTime Periodo2, int? Mensual)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            Session["IdEntidad"] = Entidades;
            Load();

            return GetFullBalance(Entidades, Periodo1, Periodo2, Mensual is null ? 0 : Mensual.Value);
        }

        [AllowAnonymous]
        public PartialViewResult LoadConsolidado(String Entidades, DateTime Periodo1, DateTime Periodo2, DateTime? Periodo3)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.balanceCompleto;
            Session["IdEntidad"] = Entidades;
            Load();
            Session["Periodo3"] = Periodo3 is null ? Session["Periodo"] : Periodo3.Value.ToShortDateString();

            return PartialView("_Reporte");
        }

        [AllowAnonymous]
        public PartialViewResult LoadOrigen(String Entidades, DateTime Periodo1, DateTime Periodo2)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.origen_aplicacion;
            Session["IdEntidad"] = Entidades;
            Load();
            return PartialView("_Reporte");
        }

        [AllowAnonymous]
        public ActionResult LoadBalance(String Entidades, DateTime Periodo1, DateTime Periodo2, DateTime Periodo3)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            Session["Periodo3"] = Periodo3.ToShortDateString();
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_balance;
            Session["TipoReporte2"] = Utility.Utilitarios.enum_tipoReporte.analisis_balance;
            Session["IdEntidad"] = Entidades;
            Graficos_Financieros gp = GetGraphBalance();
            Load();
            return View("Balance", gp);
        }

        [AllowAnonymous]
        public ActionResult LoadER(String Entidades, DateTime Periodo1, DateTime Periodo2, DateTime Periodo3, int? Mensual)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            Session["Periodo3"] = Periodo3.ToShortDateString();
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_er;
            Session["TipoReporte2"] = Utility.Utilitarios.enum_tipoReporte.analisis_er;
            Session["IdEntidad"] = Entidades;
            Session["Mensual"] = Mensual;
            Graficos_Financieros gp = GetGraphER();
            Load();

            List<Entidad> lista = ent.GetAll().Where(o => o.Id != Utility.Utilitarios.entidadAdministradora && o.Activo == true).OrderBy(o => o.Nombre).ToList();
            Entidad entidad = new Entidad();
            entidad.Id = "-1";
            entidad.Nombre = "TODAS LAS COOPERATIVAS";
            lista.Add(entidad);
            ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", entidad);

            return View("ER", gp);
        }

        public ActionResult Index()
        {
            Load();
            Usuario ObjUser = usr.Get(Env.GetUserInfo("userid"));
            List<Entidad> lista;
            if (ObjUser.Entidad_Usuario_Id == Utility.Utilitarios.entidadAdministradora)
            {
                lista = ent.GetAll().Where(o => o.Id != Utility.Utilitarios.entidadAdministradora && o.Activo == true).OrderBy(o => o.Nombre).ToList();
                Entidad entidad = new Entidad();
                entidad.Id = "-1";
                entidad.Nombre = "TODAS LAS COOPERATIVAS";
                lista.Add(entidad);
                ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", Session["IdEntidad"]);
            }
            else
            {
                lista = ent.GetAll().Where(o => o.Id == ObjUser.Entidad_Usuario_Id && o.Activo == true).ToList();
                ViewBag.Entidades = new SelectList(lista, "Id", "Nombre");
            }

            DateTime fechaEntidad = Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.balanceCompleto.ToString();
            Session["Periodo1"] = Session["Periodo1"] == null ? fechaEntidad.AddMonths(-6).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fechaEntidad.AddMonths(-1) : Session["Periodo2"];
            Session["Periodo3"] = Session["Periodo3"] == null ? fechaEntidad.AddMonths(-1) : Session["Periodo3"];
            return View();
        }

        public ActionResult Origen()
        {
            PageLoad();
            DateTime fecha = Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-12).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            return View();
        }

        public ActionResult Consolidado()
        {
            PageLoad();
            return View();
        }

        public ActionResult ER()
        {
            PageLoad();
            Graficos_Financieros gp = null;
            return View(gp);
        }

        public ActionResult Balance()
        {
            PageLoad();
            Graficos_Financieros gp = null;
            return View(gp);
        }

        public Graficos_Financieros GetGraphBalance()
        {
            int i = 0;
            int num_activos = 4;
            int num_pasivos = 4;
            Graficos_Financieros m = new Graficos_Financieros();
            var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                  Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString())).Where(o => o.ACTIVO > 0).ToList();

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

                foreach (FGA_Consultar_Graficos_Financieros_Result detalle in tak)
                {
                    try
                    {
                        /*BALANCE*/
                        Pasivo[i] = Math.Round((detalle.PASIVO / detalle.ACTIVO) * 100, 2);
                        Patrimonio[i] = Math.Round((detalle.PATRIMONIO / detalle.ACTIVO) * 100, 2);
                        XAxis[i] = Utility.Utilitarios.toUpperFirstLetter(detalle.PERIODO.Value.ToString("MMMM yy"));

                        /*ACTIVO*/
                        listaActivo[i].valores[0] = Math.Round((detalle.CARTERA / detalle.ACTIVO) * 100, 2);
                        listaActivo[i].valores[1] = Math.Round((detalle.INVERSIONESFINANC / detalle.ACTIVO) * 100, 2);
                        listaActivo[i].valores[2] = Math.Round((detalle.DISPONIBILIDADES / detalle.ACTIVO) * 100, 2);
                        listaActivo[i].valores[3] = Math.Round((detalle.OTROSACTIVOS / detalle.ACTIVO) * 100, 2);

                        /*PASIVO*/
                        listaPasivo[i].valores[0] = Math.Round((detalle.OBLIGACIONESPUBLICO / detalle.PASIVO) * 100, 2);
                        listaPasivo[i].valores[1] = Math.Round((detalle.OBLIGACIONESENTIDADES / detalle.PASIVO) * 100, 2);
                        listaPasivo[i].valores[2] = Math.Round((detalle.CUENTASXPAGAR / detalle.PASIVO) * 100, 2);
                        listaPasivo[i].valores[3] = Math.Round((detalle.OTROSPASIVOS / detalle.PASIVO) * 100, 2);

                        /*ACTIVO PRODUCTIVO*/
                        listaActivoProductivo[0].valores[i] = Math.Round(detalle.INVERSIONESFINANC / 1000000, 2);
                        listaActivoProductivo[1].valores[i] = Math.Round(detalle.CARTERA / 1000000, 2);
                        listaActivoProductivo[2].valores[i] = Math.Round((decimal)listaActivoProductivo[0].valores[i] + (decimal)listaActivoProductivo[1].valores[i], 2);

                        /*PASIVO CON COSTO*/
                        listaPasivoCosto[0].valores[i] = Math.Round(detalle.OBLIGACIONESENTIDADES / 1000000, 2);
                        listaPasivoCosto[1].valores[i] = Math.Round(detalle.OBLIGACIONESPUBLICO / 1000000, 2);
                        listaPasivoCosto[2].valores[i] = Math.Round((decimal)listaPasivoCosto[0].valores[i] + (decimal)listaPasivoCosto[1].valores[i], 2);

                        /*PASIVO CON COSTO*/
                        listaPasivoCosto[0].valores[i] = Math.Round(detalle.OBLIGACIONESENTIDADES / 1000000, 2);
                        listaPasivoCosto[1].valores[i] = Math.Round(detalle.OBLIGACIONESPUBLICO / 1000000, 2);
                        listaPasivoCosto[2].valores[i] = Math.Round((decimal)listaPasivoCosto[0].valores[i] + (decimal)listaPasivoCosto[1].valores[i], 2);

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
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
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
                        Color = HighChart.GetColor(0),
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Patrimonio",
                        Data = new Data(Patrimonio),
                        Color = HighChart.GetColor(1),
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
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
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
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
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
                        Color = HighChart.GetColor(1),
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaActivoProductivo[1].nombre,
                        Data = new Data(listaActivoProductivo[1].valores),
                        Color = HighChart.GetColor(0),
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaActivoProductivo[0].nombre,
                        Data = new Data(listaActivoProductivo[0].valores),
                        Color = HighChart.GetColor(2),
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
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
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
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
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
                        Color = HighChart.GetColor(1),
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaPasivoCosto[1].nombre,
                        Data = new Data(listaPasivoCosto[1].valores),
                        Color = HighChart.GetColor(0),
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = listaPasivoCosto[0].nombre,
                        Data = new Data(listaPasivoCosto[0].valores),
                        Color = HighChart.GetColor(2),
                    }
                });

                #endregion
            }
            return m;
        }

        public Graficos_Financieros GetGraphER()
        {
            int i = 0;
            Graficos_Financieros m = new Graficos_Financieros();
            try
            {
                var tak = sp.FGA_Consultar_Graficos_Financieros(Session["IdEntidad"].ToString(),
                                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString())).Where(o => o.ACTIVO > 0).ToList();

                if (tak.Count() > 0)
                {
                    #region GetInfo
                    string[] XAxis = new string[tak.Count()];

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
                            XAxis[i] = Utility.Utilitarios.toUpperFirstLetter(detalle.PERIODO.Value.ToString("MMMM yy"));

                            /*INGRESO*/
                            listaIngreso[0].valores[i] = Math.Round(detalle.INGRESOCARTERA, 2);
                            listaIngreso[1].valores[i] = Math.Round(detalle.INGRESODIVERSOS, 2);
                            listaIngreso[2].valores[i] = Math.Round(detalle.INGRESOINST, 2);
                            listaIngreso[3].valores[i] = Math.Round(detalle.OTROSINGRESOS, 2);

                            listaIngreso[0].total += Math.Round((decimal)listaIngreso[0].valores[i], 2);
                            listaIngreso[1].total += Math.Round((decimal)listaIngreso[1].valores[i], 2);
                            listaIngreso[2].total += Math.Round((decimal)listaIngreso[2].valores[i], 2);
                            listaIngreso[3].total += Math.Round((decimal)listaIngreso[3].valores[i], 2);

                            /*GASTO*/
                            listaGasto[0].valores[i] = Math.Round(detalle.GASTOPUBLICO, 2);
                            listaGasto[1].valores[i] = Math.Round(detalle.GASTOESTIMACIONES, 2);
                            listaGasto[2].valores[i] = Math.Round(detalle.OTROSGASTOS, 2);
                            listaGasto[3].valores[i] = Math.Round(detalle.GASTOENTIDADES, 2);
                            listaGasto[4].valores[i] = Math.Round(detalle.GASTOADMIN, 2);

                            listaGasto[0].total += Math.Round((decimal)listaGasto[0].valores[i], 2);
                            listaGasto[1].total += Math.Round((decimal)listaGasto[1].valores[i], 2);
                            listaGasto[2].total += Math.Round((decimal)listaGasto[2].valores[i], 2);
                            listaGasto[3].total += Math.Round((decimal)listaGasto[3].valores[i], 2);
                            listaGasto[4].total += Math.Round((decimal)listaGasto[4].valores[i], 2);
                            i += 1;
                        }
                        catch
                        { }
                    }

                    #endregion

                    #region ComposicionIngreso
                    HighChart.ConfigChart(ref m.composicionIngreso, "Ingreso", null);
                    m.composicionIngreso.SetXAxis(new XAxis
                    {
                        Categories = XAxis,
                        Labels = new XAxisLabels()
                        {
                            Style = "fontSize: '12px', color: 'black'",
                        },
                        Title = new XAxisTitle()
                        {
                            Text = " "
                        }
                    });
                    m.composicionIngreso.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                    m.composicionIngreso.SetPlotOptions(HighChart.getLabelStackingPercent(150));
                    Series[] series = new Series[listaIngreso.Count()];
                    Series serie;
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
                        },
                        Title = new XAxisTitle()
                        {
                            Text = " "
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
            }
            catch (Exception)
            {

            }
            return m;
        }

        public ZipResult GetFullBalance(String entidad, DateTime periodoI, DateTime periodoF, int ind_mensual)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["TempExcel"].ToString();
            string fullPath = "";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FGA_Consultar_Balance_Comprobacion_Rango_Result[] tak;

            try
            {
                // Crear un archivo Excel en memoria
                using (var package = new ExcelPackage())
                {
                    // Crear una hoja de trabajo
                    var worksheet = package.Workbook.Worksheets.Add("Balance");

                    // Agregar texto como encabezado
                    worksheet.Cells["B1"].Value = Session["NomEntidad"].ToString();
                    worksheet.Cells["B1"].Style.Font.Size = 16;
                    worksheet.Cells["B1"].Style.Font.Bold = true;
                    worksheet.Cells["B1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells["B1:F1"].Merge = true;  // Merge celdas para el encabezado de texto

                    // Ajustar el alto de la fila para el encabezado de texto
                    worksheet.Row(1).Height = 30;

                    // Agregar la imagen (se coloca en la celda B2)
                    try
                    {
                        var picture = worksheet.Drawings.AddPicture("Logo", new FileInfo(Session["RutaLogo"].ToString()));
                        picture.SetPosition(0, 0, 0, 0);  // Posicionar la imagen en la fila 2, columna B
                    }
                    catch (Exception) { 
                    }

                    // Definir encabezados de la tabla
                    int row = 6; // Comenzamos la tabla después de la imagen (fila 6)
                    int col = 1;
                    int cell = 1;

                    if (entidad == "-1")
                    {
                        tak = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, periodoI, periodoI, ind_mensual == 0 ? false : true, false);

                        for (int i = 1; i <= int.Parse((Math.Round((periodoF - periodoI).TotalDays / 30, 0)).ToString()); i++)
                        {
                            var resultado = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, periodoI.AddMonths(i), periodoI.AddMonths(i), ind_mensual == 0 ? false : true, false);
                            resultado.ToList().RemoveAt(0);
                            tak = tak.Concat(resultado).ToArray();
                        }

                        var columnas = tak[0].VALORES.Split(';');
                        foreach (var header in columnas)
                        {
                            worksheet.Column(col).Width = col == 1 ? 100 : 25;
                            worksheet.Cells[row, col].Value = header;
                            worksheet.Cells[row, col].Style.Font.Bold = true; // Poner los encabezados en negrita
                            col++;
                        }
                    }
                    else
                    {
                        tak = sp.FGA_Consultar_Balance_Comprobacion_Rango(entidad, periodoI, periodoF, ind_mensual == 0 ? false : true, false);
                        string[] headers = { "Nombre", "Cuenta" };

                        // Escribir encabezados en las celdas
                        foreach (var header in headers)
                        {
                            worksheet.Column(col).Width = col == 1 ? 100 : 25;
                            worksheet.Cells[row, col].Value = header;
                            worksheet.Cells[row, col].Style.Font.Bold = true; // Poner los encabezados en negrita
                            col++;
                        }

                        for (DateTime periodo = periodoI; periodo <= periodoF; periodo = periodo.AddMonths(1))
                        {
                            worksheet.Column(col).Width = 25;
                            worksheet.Cells[row, col].Value = periodo.ToString("MMMM yy");
                            worksheet.Cells[row, col].Style.Font.Bold = true; // Poner los encabezados en negrita
                            col++;
                        }
                    }

                    // Escribir los datos debajo del encabezado
                    row++;
                    foreach (var detalle in tak)
                    {
                        if (!detalle.VALORES.Contains("Periodo"))
                        {
                            cell = 1;
                            foreach (var item in detalle.VALORES.Split(';'))
                            {
                                worksheet.Cells[row, cell].Value = item;
                                cell++;
                            }
                            row++;
                        }
                    }

                    fullPath = Path.Combine(path, "Balance " + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + Env.GetUserInfo("userid").ToString() + ".xlsx");

                    // Convertir el rango de celdas en una tabla de Excel
                    var tableRange = worksheet.Cells[6, 1, row - 1, col - 1];
                    var table = worksheet.Tables.Add(tableRange, "Table1");
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium9; // Estilo de la tabla

                    // Guardar el archivo en el sistema de archivos
                    var fileInfo = new FileInfo(fullPath);
                    package.SaveAs(fileInfo);
                }
            }
            catch (Exception e)
            {
                Log entity = new Log();
                entity.Mensaje = e.Message;
                entity.Controller = "Informe";
                entity.Fecha = DateTime.Now;
                entity.Action = "LoadInforme";
                entity.IdUsuario_Id = 11;
                log.Add(ref entity);
            }

            return DownloadResult(fullPath);
        }

        private ZipResult DownloadResult(string path)
        {
            ZipResult zip = new ZipResult();
            zip.AddFile(FileModel.Decode(path));
            return zip;
        }

        private readonly FGA_En_Linea.LogService.ServiceOf_LogClient log = new FGA_En_Linea.LogService.ServiceOf_LogClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient us = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.Rpt_Analisis_VHService.ServiceOf_Rpt_Analisis_VHClient rpt = new FGA_En_Linea.Rpt_Analisis_VHService.ServiceOf_Rpt_Analisis_VHClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                us.Close();
                ent.Close();
                sp.Close();
                usr.Close();
                rpt.Close();
                log.Close();
            }
            base.Dispose(disposing);
        }
    }
}