using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using FGA.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using FGA.Utility;
using Entities.Entities.Procedures;
using System.Drawing;

namespace FGA.Controllers
{
    public class IRLController : BaseController
    {

        #region Variables

        private List<FGA_Consultar_Datos_Historicos_Result> datosHistoricos = new List<FGA_Consultar_Datos_Historicos_Result>();
        private List<FGA_Generar_Proyeccion_Result> datosProyeccion = new List<FGA_Generar_Proyeccion_Result>();

        private List<FGA_Consultar_Datos_Historicos_Result> getHistoricos()
        {
            return (List<FGA_Consultar_Datos_Historicos_Result>)Session["DatosHistoricos"];
        }

        private void setHistoricos(List<FGA_Consultar_Datos_Historicos_Result> lista)
        {
            Session["DatosHistoricos"] = lista;
        }

        private List<FGA_Generar_Proyeccion_Result> getProyeccion()
        {
            return (List<FGA_Generar_Proyeccion_Result>)Session["DatosProyeccion"];
        }

        private void setProyeccion(List<FGA_Generar_Proyeccion_Result> lista)
        {
            Session["DatosProyeccion"] = lista;
        }

        private List<FGA_Consultar_Estado_Proyeccion_Result> getIRL()
        {
            return (List<FGA_Consultar_Estado_Proyeccion_Result>)Session["DatosIRL"];
        }

        private void setIRL(List<FGA_Consultar_Estado_Proyeccion_Result> lista)
        {
            Session["DatosIRL"] = lista;
        }

        private void CargaInicial()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-6).ToShortDateString();

            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            ViewBag.IdProyeccion = 0;
        }

        #endregion

        #region Insumos

        public ActionResult Insumo()
        {
            CargaInicial();
            return View();
        }

        public ActionResult Buscar(String Entidades, String PeriodoI, String PeriodoF)
        {
            Session["IdEntidad"] = Entidades;
            Load();
            Session["Periodo1"] = PeriodoI;
            Session["Periodo2"] = PeriodoF;
            return View("Insumo");
        }

        public ActionResult GetRiesgoLiquidez()
        {
            try
            {
                var tak = sp.FGA_Consultar_Riesgo_Liquidez_Rango(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));
                var result = from c in tak
                             select new string[] { c.PERIODO.ToShortDateString(),
                                                   Utilitarios.ConvertirAString(c.CONTRACTUAL_AUMENTA),
                                                   Utilitarios.ConvertirAString(c.DEPOSITO_AUMENTA),
                                                   Utilitarios.ConvertirAString(c.CONTRACTUAL_NUEVO),
                                                   Utilitarios.ConvertirAString(c.DEPOSITO_NUEVO),
                                                   Utilitarios.ConvertirAString(c.CDP_NUEVO),
                                                   Utilitarios.ConvertirAString(c.CONTRACTUAL_DISMINUYE),
                                                   Utilitarios.ConvertirAString(c.DEPOSITO_DISMINUYE),
                                                   Utilitarios.ConvertirAString(c.CONTRACTUAL_CANCELADO),
                                                   Utilitarios.ConvertirAString(c.DEPOSITO_CANCELADO),
                                                   Utilitarios.ConvertirAString(c.CDP_CANCELADO)
                         };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGastoIngreso()
        {
            try
            {
                var tak = sp.FGA_Consultar_Gastos_Ingresos(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));
                var result = from c in tak
                             select new string[] { c.PERIODO.Value.ToShortDateString(),
                                                   Utilitarios.ConvertirAString(c.INGRESO_OPERACIONAL.Value),
                                                   Utilitarios.ConvertirAString(c.GASTO_OPERACIONAL.Value),
                                                   Utilitarios.ConvertirAString(c.GASTO_ADMINISTRATIVO.Value),
                                                   Utilitarios.ConvertirAString(c.GASTO_FINANCIERO.Value),
                                                   Utilitarios.ConvertirAString(c.VARIACION_CS.Value),
                         };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetColocacion()
        {
            try
            {
                var tak = sp.FGA_Consultar_Credito_Categoria_Rango(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                                   Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString())).Where(o => o.IndNueva.Value == true);
                var result = from c in tak select new string[] { c.Periodo.Value.ToShortDateString(), Utilitarios.ConvertirAString(c.SALDO.Value) };
                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetColocacionNueva()
        {
            try
            {
                var tak = sp.FGA_Consultar_Credito_Categoria_Rango(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                            Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString())).Where(o => o.IndNueva.Value == true);

                var result = from c in tak
                             select new string[] { c.Periodo.Value.ToShortDateString(),
                                                   Utilitarios.ConvertirAString(c.A1.Value),
                                                   Utilitarios.ConvertirAString(c.A2.Value),
                                                   Utilitarios.ConvertirAString(c.B1.Value),
                                                   Utilitarios.ConvertirAString(c.B2.Value),
                                                   Utilitarios.ConvertirAString(c.C1.Value),
                                                   Utilitarios.ConvertirAString(c.C2.Value),
                                                   Utilitarios.ConvertirAString(c.D.Value),
                                                   Utilitarios.ConvertirAString(c.E.Value),
                         };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetColocacionMantiene()
        {
            try
            {
                var tak = sp.FGA_Consultar_Credito_Categoria_Rango(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                            Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString())).Where(o => o.IndNueva.Value == false);

                var result = from c in tak
                             select new string[] { c.Periodo.Value.ToShortDateString(),
                                                   Utilitarios.ConvertirAString(c.A1.Value),
                                                   Utilitarios.ConvertirAString(c.A2.Value),
                                                   Utilitarios.ConvertirAString(c.B1.Value),
                                                   Utilitarios.ConvertirAString(c.B2.Value),
                                                   Utilitarios.ConvertirAString(c.C1.Value),
                                                   Utilitarios.ConvertirAString(c.C2.Value),
                                                   Utilitarios.ConvertirAString(c.D.Value),
                                                   Utilitarios.ConvertirAString(c.E.Value),
                         };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        #endregion

        #region Proyeccion

        public ActionResult Index()
        {
            CargaInicial();
            Session["PeriodosHist"] = 36;
            Session["PeriodosProy"] = 26;
            Session["Cuenta"] = string.Empty;
            CargarCuentas();
            return View();
        }

        private void CargarCuentas()
        {
            var listaCuentas = sp.FGA_Consultar_Cuentas_Proyectar(Session["IdEntidad"].ToString(), true);
            ViewBag.Cuentas = new SelectList(listaCuentas, "Cuenta", "Nombre", Session["Cuenta"]);
        }

        public ActionResult BuscarHistorico(String Entidades, String Cuentas, DateTime PeriodoC, int PeriodosHist, int PeriodosProy)
        {
            Session["IdEntidad"] = Entidades;
            Load();
            Session["PeriodosHist"] = PeriodosHist;
            Session["PeriodosProy"] = PeriodosProy;
            Session["Periodo2"] = PeriodoC.ToShortDateString();
            Session["Cuenta"] = Cuentas;

            Models.TipoProyeccion tipoProyeccion = proy.Existe(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), Session["Cuenta"].ToString());
            ViewBag.IdProyeccion = tipoProyeccion is null ? 0 : tipoProyeccion.Id;
            this.setProyeccion(null);
            CargarCuentas();
            return View("Index");
        }

        public JsonResult Add(DateTime Periodo, String IdEntidad, String Cuenta)
        {
            Resultado ObjResultado = new Resultado();
            String detalle = string.Empty;

            try
            {
                var exclusion = new Models.Exclusion_Periodos_Proyectar
                {
                    Cuenta = Cuenta,
                    IdEntidad = IdEntidad,
                    Periodo = Periodo
                };

                exc.Add(ref exclusion);
                ObjResultado.exito = true;

            }
            catch (Exception e)
            {
                ObjResultado.exito = false;
                ObjResultado.mensaje = e.Message;
            }

            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Remove(DateTime Periodo, String IdEntidad, String Cuenta)
        {
            Resultado ObjResultado = new Resultado();
            String detalle = string.Empty;

            try
            {
                var exclusion = new Models.Exclusion_Periodos_Proyectar
                {
                    Cuenta = Cuenta,
                    IdEntidad = IdEntidad,
                    Periodo = Periodo
                };

                exc.Habilitar(Periodo, Cuenta, IdEntidad);
                ObjResultado.exito = true;
            }
            catch (Exception e)
            {
                ObjResultado.exito = false;
                ObjResultado.mensaje = e.Message;
            }

            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetHistorico()
        {
            try
            {
                var cuenta = Session["Cuenta"].ToString();
                var entidad = Session["IdEntidad"].ToString();

                if (!string.IsNullOrEmpty(cuenta))
                {
                    this.setHistoricos(sp.FGA_Consultar_Datos_Historicos(entidad, Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), int.Parse(Session["PeriodosHist"].ToString()), cuenta).ToList());
                    var result = from c in getHistoricos()
                                 select new string[] { c.PERIODO.Value.ToShortDateString(), Utilitarios.ConvertirAString(c.SALDO.Value),
                                                       c.EXCLUIDO == "S" ?  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Incluir\" href=\"javascript:remove('" + c.PERIODO.Value.ToShortDateString() + "', '" + entidad + "','" + cuenta + "')\"><i class=\"btn btn-xs btn-danger icon fa fa-remove\"></i></a>"
                                                                         :  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Borrar\" href=\"javascript:add('" + c.PERIODO.Value.ToShortDateString() + "', '" + entidad + "','" + cuenta + "')\"><i class=\"btn btn-xs btn-success icon fa fa-check\"></i></a>"
                                 };
                    return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
            }
            return Json(new { aaData = new string[] { } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProyeccion()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["Cuenta"].ToString()))
                {
                    this.setProyeccion(sp.FGA_Generar_Proyeccion(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                                                int.Parse(Session["PeriodosHist"].ToString()), int.Parse(Session["PeriodosProy"].ToString()),
                                                                Session["Cuenta"].ToString()).ToList());

                    var tak = this.getProyeccion();
                    Models.TipoProyeccion tipoProyeccion = proy.Existe(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), Session["Cuenta"].ToString());
                    ViewBag.IdProyeccion = tipoProyeccion is null ? 0 : tipoProyeccion.Id;

                    if (tak.Count() > 0)
                    {
                        List<string[]> result = new List<string[]>();
                        decimal[] backtestingList = { tak[0].BACKTESTING_AR.Value, tak[0].BACKTESTING_MENSUAL.Value, tak[0].BACKTESTING_TRIMESTRAL.Value, tak[0].BACKTESTING_SEMESTRAL.Value, tak[0].BACKTESTING_ANUAL.Value };
                        decimal cercano = backtestingList.Aggregate((x, y) => Math.Abs(x - 100) < Math.Abs(y - 100) ? x : y);
                        string htmlCercano = "<label class=\"label label-success label-large\">{0}%</label>";
                        string[] backtesting = { "<label style=\"color:red\">Backtesting</label>",
                        tak[0].BACKTESTING_AR.Value == cercano ?
                        string.Format(htmlCercano, Utilitarios.ConvertirAString(tak[0].BACKTESTING_AR.Value)) : Utilitarios.ConvertirAString(tak[0].BACKTESTING_AR.Value) + "%",
                        tak[0].BACKTESTING_MENSUAL.Value == cercano ?
                        string.Format(htmlCercano, Utilitarios.ConvertirAString(tak[0].BACKTESTING_MENSUAL.Value)) : Utilitarios.ConvertirAString(tak[0].BACKTESTING_MENSUAL.Value) + "%",
                        tak[0].BACKTESTING_TRIMESTRAL.Value == cercano ?
                        string.Format(htmlCercano, Utilitarios.ConvertirAString(tak[0].BACKTESTING_TRIMESTRAL.Value)) : Utilitarios.ConvertirAString(tak[0].BACKTESTING_TRIMESTRAL.Value) + "%",
                        tak[0].BACKTESTING_SEMESTRAL.Value == cercano ?
                        string.Format(htmlCercano, Utilitarios.ConvertirAString(tak[0].BACKTESTING_SEMESTRAL.Value)) : Utilitarios.ConvertirAString(tak[0].BACKTESTING_SEMESTRAL.Value) + "%",
                        tak[0].BACKTESTING_ANUAL.Value == cercano ?
                        string.Format(htmlCercano, Utilitarios.ConvertirAString(tak[0].BACKTESTING_ANUAL.Value)) : Utilitarios.ConvertirAString(tak[0].BACKTESTING_ANUAL.Value) + "%",
                        };

                        result.Add(backtesting);
                        IEnumerable<string[]> proy = from c in tak
                                                     select new string[] { c.PERIODO.Value.ToShortDateString(), Utilitarios.ConvertirAString(c.PROYECCION_AR.Value),
                                                   Utilitarios.ConvertirAString(c.PROYECCION_MENSUAL.Value),  Utilitarios.ConvertirAString(c.PROYECCION_TRIMESTAL.Value),
                                                   Utilitarios.ConvertirAString(c.PROYECCION_SEMESTRAL.Value), Utilitarios.ConvertirAString(c.PROYECCION_ANUAL.Value)
                         };

                        return Json(new { aaData = result.Concat(proy) }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    setProyeccion(null);
            }
            catch (Exception)
            {
            }

            return Json(new { aaData = new string[] { } }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _VerProyeccion()
        {
            Models.TipoProyeccion tipoProyeccion = proy.Existe(Session["IdEntidad"].ToString(), Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), Session["Cuenta"].ToString());
            ViewBag.IdProyeccion = tipoProyeccion is null ? 0 : tipoProyeccion.Id;
            return PartialView("_VerProyeccion");
        }

        #region Graficos

        public JsonResult GuardarProyeccion(int tipo, int periodosHist, string cuenta, DateTime periodo, string idEntidad, int periodosProy)
        {
            Resultado ObjResultado = new Resultado();
            String valores = string.Empty;

            try
            {
                foreach (FGA_Generar_Proyeccion_Result detalle in getProyeccion())
                {
                    if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.AR)
                        valores += Utilitarios.ConvertirAString(detalle.PROYECCION_AR.Value) + ";";
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Mensual)
                        valores += Utilitarios.ConvertirAString(detalle.PROYECCION_MENSUAL.Value) + ";";
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Trimestral)
                        valores += Utilitarios.ConvertirAString(detalle.PROYECCION_TRIMESTAL.Value) + ";";
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Semestral)
                        valores += Utilitarios.ConvertirAString(detalle.PROYECCION_SEMESTRAL.Value) + ";";
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Anual)
                        valores += Utilitarios.ConvertirAString(detalle.PROYECCION_ANUAL.Value) + ";";
                }

                Models.Proyeccion proyeccion = new Models.Proyeccion();
                proyeccion.Cuenta = cuenta;
                proyeccion.IdEntidad = idEntidad;
                proyeccion.Periodo = periodo;
                proyeccion.PeriodosProyectar = periodosHist;
                proyeccion.IdProyeccion = tipo;
                proyeccion.Valores = valores;
                proy.Add(ref proyeccion);
                ObjResultado.exito = true;
            }
            catch (Exception e)
            {
                ObjResultado.exito = false;
                ObjResultado.mensaje = e.Message;
            }

            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BorrarProyeccion(string cuenta, DateTime periodo, string idEntidad)
        {
            Resultado ObjResultado = new Resultado();
            try
            {
                sp.FGA_Borrar_Proyeccion(idEntidad, periodo, cuenta);
                ObjResultado.exito = true;
            }
            catch (Exception e)
            {
                ObjResultado.exito = false;
                ObjResultado.mensaje = e.Message;
            }

            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GraficaDetalle(int tipo)
        {
            Highcharts gp = null;
            if (getProyeccion() != null)
            {
                int i = 0;
                int numHistoricos = getHistoricos().Count();
                int numProyeccion = getProyeccion().Count();
                int numPeriodos = numHistoricos + numProyeccion;
                string[] fechas = new string[numPeriodos];
                List<Serie> listaSeries = new List<Serie>();
                listaSeries.Add(new Serie(numPeriodos, "Histórico"));
                listaSeries.Add(new Serie(numPeriodos, "Proyección"));

                foreach (FGA_Consultar_Datos_Historicos_Result detalle in getHistoricos())
                {
                    fechas[i] = detalle.PERIODO.Value.ToShortDateString();
                    listaSeries[0].valores[i] = detalle.SALDO.Value;
                    listaSeries[1].valores[i] = 0;
                    i += 1;
                }

                foreach (FGA_Generar_Proyeccion_Result detalle in getProyeccion())
                {
                    fechas[i] = detalle.PERIODO.Value.ToShortDateString();
                    listaSeries[0].valores[i] = 0;

                    if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.AR)
                        listaSeries[1].valores[i] = detalle.PROYECCION_AR.Value;
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Mensual)
                        listaSeries[1].valores[i] = detalle.PROYECCION_MENSUAL.Value;
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Trimestral)
                        listaSeries[1].valores[i] = detalle.PROYECCION_TRIMESTAL.Value;
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Semestral)
                        listaSeries[1].valores[i] = detalle.PROYECCION_SEMESTRAL.Value;
                    else if ((Utilitarios.enum_proyeccion)tipo == Utilitarios.enum_proyeccion.Anual)
                        listaSeries[1].valores[i] = detalle.PROYECCION_ANUAL.Value;

                    i += 1;
                }

                HighChart.ConfigChart(ref gp, "Graph", null);
                gp.SetXAxis(new XAxis
                {
                    Categories = fechas,
                    Labels = new XAxisLabels()
                    {
                        Step = 6,
                        Style = "fontSize: '10px', color: 'black'",
                    },
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                });

                gp.SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = false
                        },
                    }
                });

                gp.SetYAxis(new YAxis()
                {
                    Title = new YAxisTitle()
                    {
                        Text = "Saldo en millones " + Session["Cuenta"].ToString(),
                    },
                    Labels = new YAxisLabels()
                    {
                        Formatter = "formatMillion",
                        Style = "fontSize: '10px', color: 'black'"
                    }
                });

                Series[] series = new Series[2];
                Series serie;
                i = 0;

                foreach (Serie detalle in listaSeries)
                {
                    serie = new Series
                    {
                        Type = ChartTypes.Column,
                        Name = detalle.nombre,
                        Data = new Data(detalle.valores),
                        Color = HighChart.GetColor(i)
                    };

                    series[i] = serie;
                    i += 1;
                }

                gp.SetSeries(
                    series
                );
            }
            return PartialView("_GraficaDetalle", gp);
        }

        #endregion

        #endregion

        #region IRL

        public ActionResult IRL()
        {
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());

            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-6).ToShortDateString();

            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            setIRL(null);
            Load();
            return View();
        }

        public JsonResult ValidarIRL(String IdEntidad, DateTime Periodo)
        {
            Resultado ObjResultado = new Resultado();
            String detalle = string.Empty;

            try
            {
                var validacion = sp.FGA_Validar_Proyecciones_IRL(IdEntidad, Periodo);
                if (validacion.Length <= 0)
                    ObjResultado.exito = true;
                else
                {
                    foreach (var error in validacion)
                        detalle += "<label>" + error + "</label>";

                    ObjResultado.exito = false;
                    ObjResultado.mensaje = detalle;
                }
            }
            catch (Exception e)
            {
                ObjResultado.exito = false;
                ObjResultado.mensaje = e.Message;
            }

            return Json(new { resultado = ObjResultado }, JsonRequestBehavior.AllowGet);
        }

        public void GetGraphs(String IdEntidad, DateTime Periodo, ref Modelo_IRL view)
        {
            var info = getIRL();
            if (info.Count() > 0)
            {
                #region IRL

                string[] Rangos = new string[6];
                object[] Brecha = new object[6];
                object[] Acumulada = new object[6];

                var brecha = info.Where(o => o.ID == Utilitarios.idIRLBanda);
                var acumulada = info.Where(o => o.ID == Utilitarios.idIRLBandaAcum);
                decimal max = 0;
                decimal min = 10000;

                Brecha[0] = brecha.FirstOrDefault().RANGO1.Value;
                Brecha[1] = brecha.FirstOrDefault().RANGO2.Value;
                Brecha[2] = brecha.FirstOrDefault().RANGO3.Value;
                Brecha[3] = brecha.FirstOrDefault().RANGO4.Value;
                Brecha[4] = brecha.FirstOrDefault().RANGO5.Value;
                Brecha[5] = brecha.FirstOrDefault().RANGO6.Value;

                Acumulada[0] = acumulada.FirstOrDefault().RANGO1.Value;
                Acumulada[1] = acumulada.FirstOrDefault().RANGO2.Value;
                Acumulada[2] = acumulada.FirstOrDefault().RANGO3.Value;
                Acumulada[3] = acumulada.FirstOrDefault().RANGO4.Value;
                Acumulada[4] = acumulada.FirstOrDefault().RANGO5.Value;
                Acumulada[5] = acumulada.FirstOrDefault().RANGO6.Value;

                for (int i = 0; i < 6; i++)
                {
                    Rangos[i] = "Banda " + (i + 1).ToString();
                    min = min < decimal.Parse(Acumulada[i].ToString()) ? min : decimal.Parse(Acumulada[i].ToString());
                    min = min < decimal.Parse(Brecha[i].ToString()) ? min : decimal.Parse(Brecha[i].ToString());
                    max = max > decimal.Parse(Acumulada[i].ToString()) ? max : decimal.Parse(Acumulada[i].ToString());
                    max = max > decimal.Parse(Brecha[i].ToString()) ? max : decimal.Parse(Brecha[i].ToString());
                }

                HighChart.ConfigChart(ref view.IRLGraph, "IRL", null);
                view.IRLGraph.SetPlotOptions(HighChart.getLabelPercent());
                view.IRLGraph.SetXAxis(new XAxis
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
                view.IRLGraph.SetYAxis(HighChart.GetYAxis(min - 1M, max + 1M, "formatPercent"));

                view.IRLGraph.SetSeries(new Series[]
                {
                    new Series{
                        Name = "Razón IRL por banda",
                        Data = new Data(Brecha),
                        Color = ColorTranslator.FromHtml("#0B4E91"),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLinePercent()
                    },
                    new Series{
                        Name = "Razón IRL acumulado",
                        Data = new Data(Acumulada),
                        Color = ColorTranslator.FromHtml("#ed7c2f"),
                        Type = ChartTypes.Line,
                        PlotOptionsLine = HighChart.getLinePercent()
                    }
                }
                );

                #endregion

                #region Brecha

                Brecha = new object[6];
                object[] BrechaM = new object[6];

                brecha = info.Where(o => o.ID == Utilitarios.idBrecha);
                var brechaM = info.Where(o => o.ID == Utilitarios.idBrechaM);

                Brecha[0] = brecha.FirstOrDefault().RANGO1.Value;
                Brecha[1] = brecha.FirstOrDefault().RANGO2.Value;
                Brecha[2] = brecha.FirstOrDefault().RANGO3.Value;
                Brecha[3] = brecha.FirstOrDefault().RANGO4.Value;
                Brecha[4] = brecha.FirstOrDefault().RANGO5.Value;
                Brecha[5] = brecha.FirstOrDefault().RANGO6.Value;

                BrechaM[0] = null;
                BrechaM[1] = null;
                BrechaM[2] = null;
                BrechaM[3] = null;
                BrechaM[4] = brechaM.FirstOrDefault().RANGO5.Value;
                BrechaM[5] = brechaM.FirstOrDefault().RANGO6.Value;

                HighChart.ConfigChart(ref view.BrechaLiquidezGraph, "Brechas", null);
                view.BrechaLiquidezGraph.SetPlotOptions(HighChart.getLabelAmmount(20));
                view.BrechaLiquidezGraph.SetXAxis(new XAxis
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
                view.BrechaLiquidezGraph.SetYAxis(HighChart.GetYAxis(null, null, "formatMillion"));
                view.BrechaLiquidezGraph.SetSeries(new Series[]
                {
                    new Series{
                        Name = "Brecha de la banda",
                        Data = new Data(Brecha),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Column
                    },
                    new Series{
                        Name = "Brecha de la banda mensual",
                        Data = new Data(BrechaM),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Column
                    }
                }
                );

                #endregion
            }
        }

        public ActionResult GetIndiceResumen()
        {
            try
            {
                var tak = getIRL().Where(o => o.ID >= 16);
                var result = from c in tak
                             select new string[] {c.ID.ToString(), c.NOMBRE,
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO1.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO1.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO2.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO2.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO3.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO3.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO4.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO4.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO5.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO5.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO6.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO6.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO7) : Utilitarios.ConvertirAString((decimal)c.RANGO7) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO8) : Utilitarios.ConvertirAString((decimal)c.RANGO8) + "%",
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return Json(new { aaData = new[] { "", "", "", "", "", "", "", "", "", "" } }, JsonRequestBehavior.AllowGet); ;
        }

        public ActionResult ConsultarIRL(String IdEntidad, DateTime Periodo, String Efectivo, String Bccr, String Ent_finan,
             String Ent_exterior, String Inv_Bccr, String Inv_sp_no_finan, String Inv_Ots_Entidades)
        {
            setIRL(sp.FGA_Consultar_Estado_Proyeccion(IdEntidad, Periodo).ToList());
            Session["IdEntidad"] = IdEntidad;
            Session["Periodo2"] = Periodo.ToShortDateString();
            Load();

            Modelo_IRL view = new Modelo_IRL();
            view.listaCuentas = sp.FGA_Consultar_IRL_Cuentas(IdEntidad, Periodo,
                string.IsNullOrEmpty(Efectivo) ? (decimal?)null : Utilitarios.ConvertirADecimal(Efectivo),
                string.IsNullOrEmpty(Bccr) ? (decimal?)null : Utilitarios.ConvertirADecimal(Bccr),
                string.IsNullOrEmpty(Ent_finan) ? (decimal?)null : Utilitarios.ConvertirADecimal(Ent_finan),
                string.IsNullOrEmpty(Ent_exterior) ? (decimal?)null : Utilitarios.ConvertirADecimal(Ent_exterior),
                string.IsNullOrEmpty(Inv_Bccr) ? (decimal?)null : Utilitarios.ConvertirADecimal(Inv_Bccr),
                string.IsNullOrEmpty(Inv_sp_no_finan) ? (decimal?)null : Utilitarios.ConvertirADecimal(Inv_sp_no_finan),
                string.IsNullOrEmpty(Inv_Ots_Entidades) ? (decimal?)null : Utilitarios.ConvertirADecimal(Inv_Ots_Entidades)).ToList();

            GetGraphs(IdEntidad, Periodo, ref view);
            return PartialView("_VerIRLCuentas", view);
        }


        public ActionResult GetIndice()
        {
            try
            {
                var tak = getIRL().Where(o => o.ID != 18);
                var result = from c in tak
                             select new string[] {c.ID.ToString(), c.NOMBRE,
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO1.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO1.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO2.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO2.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO3.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO3.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO4.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO4.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO5.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO5.Value) + "%",
                                c.IND_PORCENTAJE.Value == 0 ? Utilitarios.ConvertirAString((decimal)c.RANGO6.Value) : Utilitarios.ConvertirAString((decimal)c.RANGO6.Value) + "%",
                                 Utilitarios.ConvertirAString(c.IND_PORCENTAJE.Value),
                                 Utilitarios.ConvertirAString(c.IND_SUBRAYADO.Value),
                                 Utilitarios.ConvertirAString(c.IND_NARANJA.Value),
                                 Utilitarios.ConvertirAString(c.IND_AZUL.Value)
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return Json(new { aaData = new[] { "", "", "", "", "", "", "", "", "", "" } }, JsonRequestBehavior.AllowGet); ;
        }
        #endregion

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.ProyeccionService.ProyeccionServiceClient proy = new FGA_En_Linea.ProyeccionService.ProyeccionServiceClient();
        private readonly FGA_En_Linea.Exclusion_ProyectarService.Exclusion_PeriodoServiceClient exc = new FGA_En_Linea.Exclusion_ProyectarService.Exclusion_PeriodoServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                sp.Close();
                usr.Close();
                proy.Close();
                exc.Close();
            }
            base.Dispose(disposing);
        }
    }
}