using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Entities.Entities.Procedures;
using FGA.Model;
using FGA.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static FGA.Utility.Utilitarios;

namespace FGA.Controllers
{
    public class IndustriaController : BaseController
    {
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.SectorService.SectorServiceClient sec = new FGA_En_Linea.SectorService.SectorServiceClient();
        private readonly FGA_En_Linea.Rpt_GraphService.ServiceOf_Rpt_GraphClient gr = new FGA_En_Linea.Rpt_GraphService.ServiceOf_Rpt_GraphClient();

        [HttpPost]
        public JsonResult GetGraphOptions(int sector)
        {
            SelectList itemList;

            if (sector == Utility.Utilitarios.SF)
                itemList = new SelectList(gr.GetAll().Where(o => o.Ind_SF == true).OrderByDescending(o => o.Nombre), "Id", "Nombre", Session["TipoReporte"].ToString());
            else
                itemList = new SelectList(gr.GetAll().Where(o => o.Ind_Sector == true).OrderByDescending(o => o.Nombre), "Id", "Nombre", Session["TipoReporte"].ToString());

            return Json(itemList);
        }

        public void PageLoad()
        {
            DateTime fecha = Session["Periodo"] == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-4).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            Session["Periodo3"] = Session["Periodo3"] == null ? fecha.ToShortDateString() : Session["Periodo3"];
            Session["TipoReporte"] = null;
        }

        public ActionResult Index()
        {
            DateTime fecha = Session["Periodo"] == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-4).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            ViewBag.Sectores = new SelectList(sec.GetAll(), "Id", "Nombre");
            Session["TipoReporte"] = enum_tipoGrafico.variacionCarteraMensual;
            ViewBag.Grafico = new SelectList(gr.GetAll().Where(o => o.Ind_Sector == true).OrderByDescending(o => o.Nombre), "Id", "Nombre", Session["TipoReporte"].ToString());

            return View();
        }

        public ActionResult ER()
        {
            DateTime fecha = Session["Periodo"] == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-4).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_er_sf;
            Session["TipoReporte2"] = Utility.Utilitarios.enum_tipoReporte.rpt_er_sf;
            return View();
        }

        public ActionResult Balance()
        {
            DateTime fecha = Session["Periodo"] == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-6).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-3).ToShortDateString() : Session["Periodo2"];
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_balance_sf;
            Session["TipoReporte2"] = Utility.Utilitarios.enum_tipoReporte.rpt_balance_sf;
            return View();
        }

        [AllowAnonymous]
        public PartialViewResult LoadOrigen(DateTime Periodo1, DateTime Periodo2)
        {
            Session["Periodo1"] = Periodo1.ToShortDateString();
            Session["Periodo2"] = Periodo2.ToShortDateString();
            return PartialView("_Reporte");
        }

        [AllowAnonymous]
        public PartialViewResult LoadER_AN(DateTime Periodo)
        {
            Session["Periodo1"] = Periodo.ToShortDateString();
            return PartialView("_Reporte");
        }

        [AllowAnonymous]
        public PartialViewResult LoadBalance_AN(DateTime Periodo)
        {
            Session["Periodo1"] = Periodo.ToShortDateString();
            return PartialView("_Reporte");
        }

        public ActionResult Origen()
        {
            DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-4).ToShortDateString() : Session["Periodo1"];
            Session["Periodo2"] = Session["Periodo2"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            Session["Periodo3"] = Session["Periodo3"] == null ? fecha.ToShortDateString() : Session["Periodo3"];
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.origen_aplicacion_sf;
            return View(Graficos());
        }

        public ActionResult ER_AN()
        {
            DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_er_an_sf;
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo1"];

            Modelo_SF md = new Modelo_SF();
            md.lIngresos = sp.FGA_Consultar_Ingreso_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString())).ToList();

            if (md.lIngresos.Count() > 0)
            {
                #region Ingresos
                List<Serie> listaAnalisis = new List<Serie>();
                string[] XAxis = new string[3];
                XAxis[0] = md.lIngresos[0].periodo.Value.ToString("MMMM yyyy");
                XAxis[1] = md.lIngresos[1].periodo.Value.ToString("MMMM yyyy");
                XAxis[2] = md.lIngresos[2].periodo.Value.ToString("MMMM yyyy");
                int i = 0;

                listaAnalisis.Add(new Serie(3, "Bancos comerciales del estado"));
                listaAnalisis[0].valores[0] = md.lIngresos[0].sector1;
                listaAnalisis[0].valores[1] = md.lIngresos[1].sector1;
                listaAnalisis[0].valores[2] = md.lIngresos[2].sector1;

                listaAnalisis.Add(new Serie(3, "Bancos creados por leyes especiales"));
                listaAnalisis[1].valores[0] = md.lIngresos[0].sector2;
                listaAnalisis[1].valores[1] = md.lIngresos[1].sector2;
                listaAnalisis[1].valores[2] = md.lIngresos[2].sector2;

                listaAnalisis.Add(new Serie(3, "Bancos privados y cooperativos"));
                listaAnalisis[2].valores[0] = md.lIngresos[0].sector3;
                listaAnalisis[2].valores[1] = md.lIngresos[1].sector3;
                listaAnalisis[2].valores[2] = md.lIngresos[2].sector3;

                listaAnalisis.Add(new Serie(3, "Empresas financieras no bancarias"));
                listaAnalisis[3].valores[0] = md.lIngresos[0].sector4;
                listaAnalisis[3].valores[1] = md.lIngresos[1].sector4;
                listaAnalisis[3].valores[2] = md.lIngresos[2].sector4;

                listaAnalisis.Add(new Serie(3, "Otras entidades financieras"));
                listaAnalisis[4].valores[0] = md.lIngresos[0].sector5;
                listaAnalisis[4].valores[1] = md.lIngresos[1].sector5;
                listaAnalisis[4].valores[2] = md.lIngresos[2].sector5;

                listaAnalisis.Add(new Serie(3, "Organizaciones cooperativas"));
                listaAnalisis[5].valores[0] = md.lIngresos[0].sector6;
                listaAnalisis[5].valores[1] = md.lIngresos[1].sector6;
                listaAnalisis[5].valores[2] = md.lIngresos[2].sector6;

                listaAnalisis.Add(new Serie(3, "Entidades autorizadas nacional vivienda"));
                listaAnalisis[6].valores[0] = md.lIngresos[0].sector7;
                listaAnalisis[6].valores[1] = md.lIngresos[1].sector7;
                listaAnalisis[6].valores[2] = md.lIngresos[2].sector7;

                HighChart.ConfigChart(ref md.gIngresos, "Ingresos", null);
                md.gIngresos.SetXAxis(new XAxis
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
                md.gIngresos.SetYAxis(HighChart.GetYAxis(1, null, "formatPercent"));
                md.gIngresos.SetPlotOptions(HighChart.getLabelStackingPercent(80));
                Series[] series = new Series[listaAnalisis.Count()];
                Series serie;
                i = 0;
                foreach (Serie detalle in listaAnalisis.OrderByDescending(o => o.porcentual))
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

                md.gIngresos.SetSeries(
                    series
                );
                #endregion
            }

            md.lGastos = sp.FGA_Consultar_Gasto_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString())).ToList();

            if (md.lGastos.Count() > 0)
            {
                #region Gastos
                List<Serie> listaAnalisis = new List<Serie>();
                string[] XAxis = new string[5];
                XAxis[0] = md.lGastos[0].nombre;
                XAxis[1] = md.lGastos[1].nombre;
                XAxis[2] = md.lGastos[2].nombre;
                XAxis[3] = md.lGastos[3].nombre;
                XAxis[4] = md.lGastos[4].nombre;

                int i = 0;

                listaAnalisis.Add(new Serie(5, "Bancos comerciales del estado"));
                listaAnalisis[0].valores[0] = md.lGastos[0].sector1;
                listaAnalisis[0].valores[1] = md.lGastos[1].sector1;
                listaAnalisis[0].valores[2] = md.lGastos[2].sector1;
                listaAnalisis[0].valores[3] = md.lGastos[3].sector1;
                listaAnalisis[0].valores[4] = md.lGastos[4].sector1;

                listaAnalisis.Add(new Serie(5, "Bancos creados por leyes especiales"));
                listaAnalisis[1].valores[0] = md.lGastos[0].sector2;
                listaAnalisis[1].valores[1] = md.lGastos[1].sector2;
                listaAnalisis[1].valores[2] = md.lGastos[2].sector2;
                listaAnalisis[1].valores[3] = md.lGastos[3].sector2;
                listaAnalisis[1].valores[4] = md.lGastos[4].sector2;

                listaAnalisis.Add(new Serie(5, "Bancos privados y cooperativos"));
                listaAnalisis[2].valores[0] = md.lGastos[0].sector3;
                listaAnalisis[2].valores[1] = md.lGastos[1].sector3;
                listaAnalisis[2].valores[2] = md.lGastos[2].sector3;
                listaAnalisis[2].valores[3] = md.lGastos[3].sector3;
                listaAnalisis[2].valores[4] = md.lGastos[4].sector3;

                listaAnalisis.Add(new Serie(5, "Empresas financieras no bancarias"));
                listaAnalisis[3].valores[0] = md.lGastos[0].sector4;
                listaAnalisis[3].valores[1] = md.lGastos[1].sector4;
                listaAnalisis[3].valores[2] = md.lGastos[2].sector4;
                listaAnalisis[3].valores[3] = md.lGastos[3].sector4;
                listaAnalisis[3].valores[4] = md.lGastos[4].sector4;

                listaAnalisis.Add(new Serie(5, "Otras entidades financieras"));
                listaAnalisis[4].valores[0] = md.lGastos[0].sector5;
                listaAnalisis[4].valores[1] = md.lGastos[1].sector5;
                listaAnalisis[4].valores[2] = md.lGastos[2].sector5;
                listaAnalisis[4].valores[3] = md.lGastos[3].sector5;
                listaAnalisis[4].valores[4] = md.lGastos[4].sector5;

                listaAnalisis.Add(new Serie(5, "Organizaciones cooperativas"));
                listaAnalisis[5].valores[0] = md.lGastos[0].sector6;
                listaAnalisis[5].valores[1] = md.lGastos[1].sector6;
                listaAnalisis[5].valores[2] = md.lGastos[2].sector6;
                listaAnalisis[5].valores[3] = md.lGastos[3].sector6;
                listaAnalisis[5].valores[4] = md.lGastos[4].sector6;

                listaAnalisis.Add(new Serie(5, "Entidades autorizadas nacional vivienda"));
                listaAnalisis[6].valores[0] = md.lGastos[0].sector7;
                listaAnalisis[6].valores[1] = md.lGastos[1].sector7;
                listaAnalisis[6].valores[2] = md.lGastos[2].sector7;
                listaAnalisis[6].valores[3] = md.lGastos[3].sector7;
                listaAnalisis[6].valores[4] = md.lGastos[4].sector7;

                HighChart.ConfigChart(ref md.gGastos, "Gastos", null);
                md.gGastos.SetXAxis(new XAxis
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
                md.gGastos.SetYAxis(HighChart.GetYAxis(1, null, "formatPercent"));
                md.gGastos.SetPlotOptions(HighChart.getLabelStackingPercent(80));
                Series[] series = new Series[listaAnalisis.Count()];
                Series serie;
                i = 0;
                foreach (Serie detalle in listaAnalisis.OrderByDescending(o => o.porcentual))
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

                md.gGastos.SetSeries(
                    series
                );
                #endregion
            }

            return View(md);
        }


        public ActionResult Balance_AN()
        {
            DateTime fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            Session["TipoReporte"] = Utility.Utilitarios.enum_tipoReporte.rpt_balance_an_sf;
            Session["Periodo1"] = Session["Periodo1"] == null ? fecha.AddMonths(-1).ToShortDateString() : Session["Periodo1"];

            Modelo_SF md = new Modelo_SF();
            md.lComposicionBalance = sp.FGA_Consultar_Composicion_Balance_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString())).ToList();

            try
            {
                if (md.lComposicionBalance.Count() > 0)
                {
                    #region Ingresos
                    List<Serie> listaAnalisis = new List<Serie>();
                    string[] XAxis = new string[7];
                    XAxis[0] = "Bancos comerciales del estado";
                    XAxis[1] = "Bancos creados por leyes especiales";
                    XAxis[2] = "Bancos privados y cooperativos";
                    XAxis[3] = "Empresas financieras no bancarias";
                    XAxis[4] = "Otras entidades financieras";
                    XAxis[5] = "Organizaciones cooperativas";
                    XAxis[6] = "Entidades autorizadas nacional vivienda";
                    int i = 0;

                    listaAnalisis.Add(new Serie(7, md.lComposicionBalance[1].cuenta));
                    listaAnalisis[0].valores[0] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector1 / md.lComposicionBalance[0].sector1) * 100, 2);
                    listaAnalisis[0].valores[1] = md.lComposicionBalance[0].sector2 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector2 / md.lComposicionBalance[0].sector2) * 100, 2);
                    listaAnalisis[0].valores[2] = md.lComposicionBalance[0].sector3 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector3 / md.lComposicionBalance[0].sector3) * 100, 2);
                    listaAnalisis[0].valores[3] = md.lComposicionBalance[0].sector4 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector4 / md.lComposicionBalance[0].sector4) * 100, 2);
                    listaAnalisis[0].valores[4] = md.lComposicionBalance[0].sector5 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector5 / md.lComposicionBalance[0].sector5) * 100, 2);
                    listaAnalisis[0].valores[5] = md.lComposicionBalance[0].sector6 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector6 / md.lComposicionBalance[0].sector6) * 100, 2);
                    listaAnalisis[0].valores[6] = md.lComposicionBalance[0].sector7 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[1].sector7 / md.lComposicionBalance[0].sector7) * 100, 2);

                    listaAnalisis.Add(new Serie(7, md.lComposicionBalance[2].cuenta));
                    listaAnalisis[1].valores[0] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector1 / md.lComposicionBalance[0].sector1) * 100, 2);
                    listaAnalisis[1].valores[1] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector2 / md.lComposicionBalance[0].sector2) * 100, 2);
                    listaAnalisis[1].valores[2] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector3 / md.lComposicionBalance[0].sector3) * 100, 2);
                    listaAnalisis[1].valores[3] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector4 / md.lComposicionBalance[0].sector4) * 100, 2);
                    listaAnalisis[1].valores[4] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector5 / md.lComposicionBalance[0].sector5) * 100, 2);
                    listaAnalisis[1].valores[5] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector6 / md.lComposicionBalance[0].sector6) * 100, 2);
                    listaAnalisis[1].valores[6] = md.lComposicionBalance[0].sector1 == 0 ? 0 : Math.Round((decimal)(md.lComposicionBalance[2].sector7 / md.lComposicionBalance[0].sector7) * 100, 2);

                    HighChart.ConfigChart(ref md.gComposicionBalance, "Balance", null);
                    md.gComposicionBalance.SetXAxis(new XAxis
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
                    md.gComposicionBalance.SetYAxis(HighChart.GetYAxis(1, null, "formatPercent"));
                    md.gComposicionBalance.SetPlotOptions(HighChart.getLabelStackingPercent(80));
                    Series[] series = new Series[listaAnalisis.Count()];
                    Series serie;
                    i = 0;
                    foreach (Serie detalle in listaAnalisis)
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

                    md.gComposicionBalance.SetSeries(
                        series
                    );
                    #endregion
                }
            }
            catch (Exception)
            {
            }

            return View(md);
        }

        public Graficos_SF Graficos()
        {
            int i = 0;
            Graficos_SF m = new Graficos_SF();
            var tak = sp.FGA_Consultar_Variacion_Interanual_SF("SF",
                                 Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                 Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString())).ToList();

            if (tak.Count() > 0)
            {
                #region Variacion
                List<Serie> listaAnalisis = new List<Serie>();
                string[] XAxis = new string[2];
                XAxis[0] = "Origen";
                XAxis[1] = "Aplicación";

                System.Collections.IList list = tak;
                for (int i1 = 0; i1 < list.Count; i1++)
                {
                    FGA_Consultar_Variacion_Interanual_SF_Result detalle = (FGA_Consultar_Variacion_Interanual_SF_Result)list[i1];
                    listaAnalisis.Add(new Serie(2, Utility.Utilitarios.toUpperFirstLetter(detalle.NOM_CUENTA)));

                    if (detalle.TIPO == "ORIGEN")
                    {
                        listaAnalisis[i].valores[0] = detalle.PORCENTAJE;
                        listaAnalisis[i].valores[1] = -1;
                    }
                    else
                    {
                        listaAnalisis[i].valores[0] = -1;
                        listaAnalisis[i].valores[1] = detalle.PORCENTAJE;
                    }

                    i = i + 1;
                }

                HighChart.ConfigChart(ref m.variacion, "Variación", null);
                m.variacion.SetXAxis(new XAxis
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
                m.variacion.SetYAxis(HighChart.GetYAxis(1, null, "formatPercent"));
                m.variacion.SetPlotOptions(HighChart.getLabelStackingPercent(150));
                Series[] series = new Series[listaAnalisis.Count()];
                Series serie;
                series = new Series[tak.Count()];
                i = 0;
                foreach (Serie detalle in listaAnalisis.OrderByDescending(o => o.porcentual))
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

                m.variacion.SetSeries(
                    series
                );
                #endregion
            }
            return m;
        }

        public async Task<ActionResult> GetER_SF(String periodoI, String periodoF)
        {
            try
            {
                var fecha1 = Utility.Utilitarios.ConvertirAFecha(periodoI);
                var fecha2 = Utility.Utilitarios.ConvertirAFecha(periodoF);
                Session["Periodo1"] = fecha1;
                Session["Periodo2"] = fecha2;

                DataTable sheet = new DataTable("Report");
                string path = System.Configuration.ConfigurationManager.AppSettings["TempExcel"].ToString();
                var tak = sp.FGA_Rpt_ER_SF(fecha1, fecha2);
                string[] entidades = tak[0].Entidad.Split(';');

                sheet.Columns.Add("Nombre", typeof(string));
                sheet.Columns.Add("I Periodo", typeof(string));
                foreach (var detalle in entidades)
                    if (!string.IsNullOrEmpty(detalle))
                        sheet.Columns.Add(detalle, typeof(string));

                sheet.Columns.Add("II Periodo", typeof(string));
                foreach (var detalle in entidades)
                    if (!string.IsNullOrEmpty(detalle))
                        sheet.Columns.Add(detalle + ".", typeof(string));

                // sheet.Columns.Add("", typeof(string));
                sheet.Rows.Add(tak[0].Ingreso_Disp.Split(';'));
                sheet.Rows.Add(tak[0].Ingreso_Inst.Split(';'));
                sheet.Rows.Add(tak[0].Prod_Cartera_Vig.Split(';'));
                sheet.Rows.Add(tak[0].Prod_Cartera_Ven.Split(';'));
                sheet.Rows.Add(tak[0].Otros_Ingresos.Split(';'));
                sheet.Rows.Add(tak[0].Total_Ingresos.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Fina_Pub.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Fina_Ent.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Cuentas_Pagar.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Subordinadas.Split(';'));
                sheet.Rows.Add(tak[0].Otros_Gastos_Fina.Split(';'));
                sheet.Rows.Add(tak[0].Total_Gasto_Fina.Split(';'));
                sheet.Rows.Add(tak[0].Recuperacion_Activos.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Deterioro.Split(';'));
                sheet.Rows.Add(tak[0].Result_Fina_Bruto.Split(';'));
                sheet.Rows.Add(tak[0].Ingresos_Diferencial.Split(';'));
                sheet.Rows.Add(tak[0].Perdidas_Diferencial.Split(';'));
                sheet.Rows.Add(tak[0].Result_UD_DC.Split(';'));
                sheet.Rows.Add(tak[0].Total_Ingresos_Oper.Split(';'));
                sheet.Rows.Add(tak[0].Total_Gastos_Oper.Split(';'));
                sheet.Rows.Add(tak[0].Resultado_Oper_Bruto.Split(';'));
                sheet.Rows.Add(tak[0].Gastos_Personal.Split(';'));
                sheet.Rows.Add(tak[0].Otros_Gastos_Admin.Split(';'));
                sheet.Rows.Add(tak[0].Total_Gastos_Admin.Split(';'));
                sheet.Rows.Add(tak[0].Resultado_Oper_Neto.Split(';'));
                sheet.Rows.Add(tak[0].Imp_Utilidades.Split(';'));
                sheet.Rows.Add(tak[0].Participaciones_Utilidad.Split(';'));
                sheet.Rows.Add(tak[0].Result_Periodo.Split(';'));

                DataSet ds = new DataSet();
                ds.Tables.Add(sheet);
                string fullPath = Path.Combine(path, Guid.NewGuid().ToString() + ".xlsx");
                await Task.Run(() => CreateExcelFile.CreateExcelDocument(ds, fullPath, includeAutoFilter: true));
                return DownloadResult(fullPath);
            }
            catch (Exception)
            {
            }

            return View("ER");
        }

        public async Task<ActionResult> GetBalance_SF(string periodoI, string periodoF)
        {
            try
            {
                var fecha1 = Utility.Utilitarios.ConvertirAFecha(periodoI);
                var fecha2 = Utility.Utilitarios.ConvertirAFecha(periodoF);
                Session["Periodo1"] = fecha1;
                Session["Periodo2"] = fecha2;

                DataTable sheet = new DataTable("Report");
                string path = System.Configuration.ConfigurationManager.AppSettings["TempExcel"].ToString();
                var tak = sp.FGA_Rpt_Balance_SF(fecha1, fecha2);
                string[] entidades = tak[0].Entidad.Split(';');

                sheet.Columns.Add("Nombre", typeof(string));
                sheet.Columns.Add("I Periodo", typeof(string));
                foreach (var detalle in entidades)
                    if (!string.IsNullOrEmpty(detalle))
                        sheet.Columns.Add(detalle, typeof(string));

                sheet.Columns.Add("II Periodo", typeof(string));
                foreach (var detalle in entidades)
                    if (!string.IsNullOrEmpty(detalle))
                        sheet.Columns.Add(detalle + ".", typeof(string));

                // sheet.Columns.Add("", typeof(string));
                sheet.Rows.Add(tak[0].INV_INSTR_FINAN.Split(';'));
                sheet.Rows.Add(tak[0].CARTERA_CREDITO.Split(';'));
                sheet.Rows.Add(tak[0].CREDITO_VIGENTE.Split(';'));
                sheet.Rows.Add(tak[0].CREDITO_VENCIDO.Split(';'));
                sheet.Rows.Add(tak[0].CREDITO_CJ.Split(';'));
                sheet.Rows.Add(tak[0].CREDITO_RESTRINGIDO.Split(';'));
                sheet.Rows.Add(tak[0].COSTO_ASOC_CREDITO.Split(';'));
                sheet.Rows.Add(tak[0].CUENTAS_COBRAR.Split(';'));
                sheet.Rows.Add(tak[0].DIFERIDOS_CARTERA_CREDITO.Split(';'));
                sheet.Rows.Add(tak[0].ESTIMACION_DETERIORO.Split(';'));
                sheet.Rows.Add(tak[0].TOTAL_ACTIVO_PRODUC.Split(';'));
                sheet.Rows.Add(tak[0].DISPONIBILIDADES.Split(';'));
                sheet.Rows.Add(tak[0].COMIS_COBRAR.Split(';'));
                sheet.Rows.Add(tak[0].BIENES_REAL.Split(';'));
                sheet.Rows.Add(tak[0].PARTI_OTRS_EMPRESAS.Split(';'));
                sheet.Rows.Add(tak[0].INMUEBLES.Split(';'));
                sheet.Rows.Add(tak[0].OTRS_ACTIVOS.Split(';'));
                sheet.Rows.Add(tak[0].INV_PROPIEDADES.Split(';'));
                sheet.Rows.Add(tak[0].TOTAL_OTRS_ACTIVOS.Split(';'));
                sheet.Rows.Add(tak[0].OBLIG_PUBLICO.Split(';'));
                sheet.Rows.Add(tak[0].CAPTACIONES_VISTA.Split(';'));
                sheet.Rows.Add(tak[0].OTRS_OBLIG_PUB.Split(';'));
                sheet.Rows.Add(tak[0].CAPTACIONES_VISTA.Split(';'));
                sheet.Rows.Add(tak[0].CARGOS_POR_PAGAR.Split(';'));
                sheet.Rows.Add(tak[0].OBLIG_REPORTO.Split(';'));
                sheet.Rows.Add(tak[0].OBLIG_ENTIDADES.Split(';'));
                sheet.Rows.Add(tak[0].TOTAL_PASIVO.Split(';'));
                sheet.Rows.Add(tak[0].CAPITAL_SOCIAL.Split(';'));
                sheet.Rows.Add(tak[0].APORTES_CAPITAL.Split(';'));
                sheet.Rows.Add(tak[0].AJUSTES_PATRIMONIO.Split(';'));
                sheet.Rows.Add(tak[0].RESERVA_PATRIMONIAL.Split(';'));
                sheet.Rows.Add(tak[0].RESULTADO_PERIODO.Split(';'));
                sheet.Rows.Add(tak[0].TOTAL_PATRIMONIO.Split(';'));
                sheet.Rows.Add(tak[0].TOTALPASIVO_PATRIMONIO.Split(';'));

                DataSet ds = new DataSet();
                ds.Tables.Add(sheet);
                string fullPath = Path.Combine(path, Guid.NewGuid().ToString() + ".xlsx");
                await Task.Run(() => CreateExcelFile.CreateExcelDocument(ds, fullPath, includeAutoFilter: true));
                return DownloadResult(fullPath);
            }
            catch (Exception)
            {
            }

            return View("Balance");
        }

        public ActionResult GetGridIngreso()
        {
            try
            {
                var tak = sp.FGA_Consultar_Ingreso_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()));
                var result = from c in tak
                             select new string[] {c.periodo.Value.ToString("MMMM yy"),
                              Utility.Utilitarios.ConvertirAString(c.sector1.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector2.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector3.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector4.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector5.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector6.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector7.Value) + "%"
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
                var tak = sp.FGA_Consultar_Gasto_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()));
                var result = from c in tak
                             select new string[] {c.nombre,
                              Utility.Utilitarios.ConvertirAString(c.sector1.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector2.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector3.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector4.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector5.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector6.Value) + "%",
                              Utility.Utilitarios.ConvertirAString(c.sector7.Value) + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridBalance()
        {
            try
            {
                var tak = sp.FGA_Consultar_Composicion_Balance_SF(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()));
                var result = from c in tak
                             select new string[] {c.cuenta,
                              Utility.Utilitarios.ConvertirAString(c.sector1.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector2.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector3.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector4.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector5.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector6.Value),
                              Utility.Utilitarios.ConvertirAString(c.sector7.Value)
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public PartialViewResult GetPartial()
        {

            return PartialView("_ReporteRango");
        }


        public PartialViewResult GraficaDetalle(int Sector, int Grafico, DateTime PeriodoI)
        {
            Highcharts gp = null;
            HighChart.ConfigChart(ref gp, "Graph", null, 600);
            int i = 0;

            if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCartera || (enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCarteraMensual)
            {
                int meses = (enum_tipoGrafico)Grafico == enum_tipoGrafico.variacionCarteraMensual ? 1 : 12;
                var tak = sp.FGA_Consultar_Variacion_Cartera_Sector(PeriodoI, DateTime.Now, Sector, meses);
                if (tak.Count() > 0)
                {
                   
                    string[] Fechas = new string[tak.Count()];
                    object[] CarteraTotal = new object[tak.Count()];

                    foreach (FGA_Consultar_Variacion_Cartera_Sector_Result detalle in tak)
                    {
                        Fechas[i] = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                        CarteraTotal[i] = detalle.variacion;
                        i += 1;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Fechas));
                    gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    gp.SetSeries(new Series[]
                    {
                        new Series{
                            Name = meses == 1 ? "Variación mensual" : "Variación interanual",
                            Data = new Data(CarteraTotal),
                            Color = HighChart.GetColor(0),
                            PlotOptionsLine = HighChart.getLine()
                        }
                    }
                    );
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.tipoCartera)
            {

                var tak = sp.FGA_Consultar_Tipo_Cartera_Sector(PeriodoI, Sector);
                if (tak.Count() > 0)
                {
                    string[] Actividad = new string[tak.Count()];
                    object[] MontoTotal = new object[tak.Count()];
                    decimal maximo = 0;
                    foreach (FGA_Consultar_Tipo_Cartera_Sector_Result detalle in tak)
                    {
                        Actividad[i] = detalle.Actividad;
                        MontoTotal[i] = detalle.Total.Value;
                        i += 1;

                        if (detalle.Total > maximo)
                            maximo = detalle.Total.Value;
                    }

                    gp.SetXAxis(HighChart.GetXAxis(Actividad));
                    gp.SetYAxis(new YAxis()
                    {
                        Id = "TipoCartera",
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
                    );
                    gp.SetPlotOptions(HighChart.getLabelAmmount());
                    gp.SetSeries(new Series[]
                    {
                        new Series{
                            Name = "Tipo Cartera",
                            Data = new Data(MontoTotal),
                            Color = HighChart.GetColor(0),
                            Type = ChartTypes.Column
                        }
                    }
                    );
                }
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.composicionRangoMora)
            {

                var tak = sp.FGA_Consultar_Cartera_Rango_SF(PeriodoI, PeriodoI.AddMonths(-12));
                object[] PrimerPeriodo = new object[7];
                object[] SegundoPeriodo = new object[7];
                string[] Rangos = new string[7];

                Rangos[0] = "Al Día";
                Rangos[1] = "1 - 30 Días";
                Rangos[2] = "31 - 60 Días";
                Rangos[3] = "61 - 90 Días";
                Rangos[4] = "91 - 180 Días";
                Rangos[5] = "Más de 180 Días";
                Rangos[6] = "Cobro Judicial";

                for(int j=0; j <= 6;j++)
                {
                    PrimerPeriodo[j] = 0;
                    SegundoPeriodo[j] = 0;
                }

                if (tak.Length > 0)
                {
                    PrimerPeriodo[0] = tak[0].AlDia;
                    PrimerPeriodo[1] = tak[0].Rango1_30Dias;
                    PrimerPeriodo[2] = tak[0].Rango31_60Dias;
                    PrimerPeriodo[3] = tak[0].Rango61_90Dias;
                    PrimerPeriodo[4] = tak[0].Rango91_180Dias;
                    PrimerPeriodo[5] = tak[0].Mas180Dias;
                    PrimerPeriodo[6] = tak[0].CobroJudicial;
                    if (tak.Length > 1)
                    {
                        SegundoPeriodo[0] = tak[1].AlDia;
                        SegundoPeriodo[1] = tak[1].Rango1_30Dias;
                        SegundoPeriodo[2] = tak[1].Rango31_60Dias;
                        SegundoPeriodo[3] = tak[1].Rango61_90Dias;
                        SegundoPeriodo[4] = tak[1].Rango91_180Dias;
                        SegundoPeriodo[5] = tak[1].Mas180Dias;
                        SegundoPeriodo[6] = tak[1].CobroJudicial;
                    }
                }

                gp.SetXAxis(new XAxis
                {
                    Categories = Rangos,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                });
                gp.SetPlotOptions(HighChart.getLabelPercent());
                gp.SetYAxis(HighChart.GetYAxis(0, null, "formatPercent"));
                gp.SetSeries(new Series[]
                {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = PeriodoI.ToString("MMMMM yyyy"),
                            Data = new Data(PrimerPeriodo),
                            Color = ColorTranslator.FromHtml("#0B4E91")
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = PeriodoI.AddMonths(-12).ToString("MMMMM yyyy"),
                            Data = new Data(SegundoPeriodo),
                            Color = ColorTranslator.FromHtml("#ed7c2f")
                        }
                }
                );
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.composicionSFNTipoCartera)
            {

                var tak = sp.FGA_Consultar_Composicion_Cartera_Sector(PeriodoI);
                object[] BancosComerciales = new object[tak.Count()];
                object[] BancosLeyesEspeciales = new object[tak.Count()];
                object[] BancosPrivadosyCoope = new object[tak.Count()];
                object[] OtrasEntidadesFinanc = new object[tak.Count()];
                object[] EmpresasFinancieraNoBanca = new object[tak.Count()];
                object[] OrganizacionesCooperativas = new object[tak.Count()];
                object[] EntidadesAutorizadas = new object[tak.Count()];
                string[] Actividades = new string[tak.Count()];

                foreach (FGA_Consultar_Composicion_Cartera_Sector_Result detalle in tak)
                {
                    BancosComerciales[i] = detalle.bancoscomercialesestado;
                    BancosLeyesEspeciales[i] = detalle.bancosleyesespeciales;
                    BancosPrivadosyCoope[i] = detalle.bancosprivadoscoope;
                    OtrasEntidadesFinanc[i] = detalle.otrasentidadesfinancieras;
                    EmpresasFinancieraNoBanca[i] = detalle.empresafinannobancaria;
                    OrganizacionesCooperativas[i] = detalle.organizacionescooperativas;
                    EntidadesAutorizadas[i] = detalle.entidadesautorizadasvivienda;
                    Actividades[i] = detalle.actividad;
                    i += 1;
                }

                gp.SetXAxis(new XAxis
                {
                    Categories = Actividades,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '10px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                });
                gp.SetYAxis(HighChart.GetYAxis(0, 100, "formatPercent"));
                gp.SetPlotOptions(HighChart.getLabelStackingPercent());
                gp.SetSeries(new Series[]
                {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Bancos Comerciales del Estado",
                            Data = new Data(BancosComerciales),
                            Color = HighChart.GetColor(0),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Bancos Creados por Leyes Especiales",
                            Data = new Data(BancosLeyesEspeciales),
                            Color = HighChart.GetColor(5),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "Bancos Privados y Cooperativos",
                            Data = new Data(BancosPrivadosyCoope),
                            Color = HighChart.GetColor(7),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Empresas Financieras no Bancarias",
                            Data = new Data(OtrasEntidadesFinanc),
                            Color = HighChart.GetColor(2),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "Otras Entidades Financieras",
                            Data = new Data(EmpresasFinancieraNoBanca),
                            Color = HighChart.GetColor(1),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Organizaciones Cooperativas de Ahorro y Crédito",
                            Data = new Data(OrganizacionesCooperativas),
                            Color = HighChart.GetColor(4),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "Entidades Autorizadas Sistema Financiero Nacional Vivienda",
                            Data = new Data(EntidadesAutorizadas),
                            Color = HighChart.GetColor(3),
                        }
                }
                );
            }
            else if ((enum_tipoGrafico)Grafico == enum_tipoGrafico.concentracionRangoMora)
            {

                var tak = sp.FGA_Consultar_Composicion_Mora_Sector(PeriodoI);
                object[] AlDia = new object[tak.Count()];
                object[] Rango1_30 = new object[tak.Count()];
                object[] Rango31_60 = new object[tak.Count()];
                object[] Rango61_90 = new object[tak.Count()];
                object[] Rango91_180 = new object[tak.Count()];
                object[] Mas180Dias = new object[tak.Count()];
                object[] CobroJudicial = new object[tak.Count()];
                string[] Sectores = new string[tak.Count()];

      
                foreach (FGA_Consultar_Composicion_Mora_Sector_Result detalle in tak)
                {
                    AlDia[i] = detalle.AlDia;
                    Rango1_30[i] = detalle.Rango1_30Dias;
                    Rango31_60[i] = detalle.Rango31_60Dias;
                    Rango61_90[i] = detalle.Rango61_90Dias;
                    Rango91_180[i] = detalle.Rango91_180Dias;
                    Mas180Dias[i] = detalle.Mas180Dias;
                    CobroJudicial[i] = detalle.CobroJudicial;
                    Sectores[i] = detalle.Sector;
                    i += 1;
                }

                gp.SetXAxis(new XAxis
                {
                    Categories = Sectores,
                    Labels = new XAxisLabels()
                    {
                        Style = "fontSize: '12px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                });
                gp.SetYAxis(HighChart.GetYAxis(1, 100, "formatPercent", AxisTypes.Logarithmic));
                gp.SetPlotOptions(HighChart.getLabelStackingPercent());
                gp.SetSeries(new Series[]
                {
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Al día",
                            Data = new Data(AlDia),
                            Color = HighChart.GetColor(0),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "1 a 30 días",
                            Data = new Data(Rango1_30),
                            Color = HighChart.GetColor(1),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "31 a 60 días",
                            Data = new Data(Rango31_60),
                            Color = HighChart.GetColor(3),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "61 a 90 días",
                            Data = new Data(Rango61_90),
                            Color = HighChart.GetColor(2),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "91 a 180 días",
                            Data = new Data(Rango91_180),
                            Color = HighChart.GetColor(4),
                        },
                        new Series{
                            Type = ChartTypes.Column,
                            Name = "Más de 180 días",
                            Data = new Data(Mas180Dias),
                            Color = HighChart.GetColor(5),
                        },
                         new Series{
                            Type = ChartTypes.Column,
                            Name = "Cobro Judicial",
                            Data = new Data(CobroJudicial),
                            Color = HighChart.GetColor(6),
                        }
                }
                );
            }
            return PartialView("_GraficaDetalle", gp);
        }

        private ZipResult DownloadResult(string path)
        {
            ZipResult zip = new ZipResult();
            zip.AddFile(FileModel.Decode(path));
            return zip;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sp.Close();
                sec.Close();
            }
            base.Dispose(disposing);
        }
    }
}