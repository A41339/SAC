using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Entities.Entities.Procedures;
using FGA.Model;
using FGA.Models;

namespace FGA.Controllers
{
    public class IndicadoresController : BaseController
    {
        #region CargaInicial
        public void LoadPage()
        {
            Load();
            string entId = Session["IdEntidad"]?.ToString() ?? "2";
            DateTime fechaEntidad;
            string keyFecha = "FechaCierre_" + entId;
            if (Session[keyFecha] is DateTime dtCached)
            {
                fechaEntidad = dtCached;
            }
            else
            {
                try
                {
                    fechaEntidad = sp.FGA_Consultar_FechaCierre(entId);
                    Session[keyFecha] = fechaEntidad;
                }
                catch
                {
                    fechaEntidad = DateTime.Now;
                }
            }
            Session["Periodo2"] = Session["Periodo2"] == null ? fechaEntidad.AddMonths(-1).ToShortDateString() : Session["Periodo2"];
            DateTime p2Ind = Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
            string tipoComp = Session["TipoComparacion"]?.ToString() ?? "Interanual";
            if (Session["Periodo1"] == null || (tipoComp.Equals("Interanual", StringComparison.OrdinalIgnoreCase) && Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()) >= p2Ind.AddMonths(-2)))
            {
                Session["Periodo1"] = p2Ind.AddYears(-1).ToShortDateString();
            }
            Session["TipoReporte"] = "S";
        }

        public ActionResult SUGEF()
        {
            LoadPage();
            return View();
        }

        public ActionResult SUGEFIndicador()
        {
            LoadPage();
            return View();
        }

        public ActionResult FGA()
        {
            LoadPage();
            return View();
        }

        public ActionResult Graficas()
        {
            LoadPage();
            return View();
        }

        public ActionResult Suficiencia()
        {
            LoadPage();
            return View();
        }
        #endregion

        #region Buscar
        private void LoadBuscar(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                LoadPage();
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
            }
            catch (Exception)
            {
            }
        }

        [AllowAnonymous]
        public ActionResult LoadIndicador(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                LoadPage();
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
            }
            catch (Exception)
            {
            }

            return View("Indicadores");
        }

        [AllowAnonymous]
        public ActionResult LoadSUGEF(String Entidades, String TipoReporte, DateTime PeriodoI, DateTime PeriodoF)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                LoadPage();
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
                Session["TipoReporte"] = TipoReporte;
            }
            catch (Exception)
            {
            }

            return View("SUGEF");
        }


        [AllowAnonymous]
        public ActionResult LoadFGA(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                LoadPage();
                Session["Periodo1"] = PeriodoI.ToShortDateString();
                Session["Periodo2"] = PeriodoF.ToShortDateString();
            }
            catch (Exception)
            {
            }

            return View("FGA");
        }


        [AllowAnonymous]
        public ActionResult BuscarSUGEF(String Entidades, String TipoReporte, DateTime PeriodoI, DateTime PeriodoF)
        {
            LoadSUGEF(Entidades, TipoReporte, PeriodoI, PeriodoF);
            return View("SUGEF");
        }


        [AllowAnonymous]
        public ActionResult BuscarFGA(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            LoadSUGEF(Entidades, "N", PeriodoI, PeriodoF);
            return View("FGA");
        }


        public ActionResult BuscarSuficiencia(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            LoadBuscar(Entidades, PeriodoI, PeriodoF);
            return View("Suficiencia");
        }

        public ActionResult BuscarSugefIndicador(String Entidades, DateTime PeriodoI, DateTime PeriodoF)
        {
            LoadBuscar(Entidades, PeriodoI, PeriodoF);
            return View("SUGEFIndicador");
        }
        #endregion

        #region GetGrid



        public ActionResult GetGridIndicadorF()
        {
            try
            {
                var tak = sp.FGA_Consultar_Indicadores_Formulas(Session["IdEntidad"].ToString(),
                                                             Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                             Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));
                var result = from c in tak
                             select new string[] { c.TIPO, c.NOMBRE,
                                                   Utility.Utilitarios.ConvertirAString(c.RESULTADO.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                                   Utility.Utilitarios.ConvertirAString(c.RESULTADO_COMPARAR.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                                   "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('F" + c.ID + "', '" + (c.IND_PORCENTAJE == true ? "S" : "N")  + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                                  };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridIndicadorCartera()
        {
            try
            {
                var tak = sp.FGA_Consultar_Indicadores_Cartera(Session["IdEntidad"].ToString(), Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                             Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), DateTime.Now);
                var result = from c in tak
                             select new string[] { c.id.ToString(), c.nombreindicador, Utility.Utilitarios.ConvertirAString(c.monto_1) + (c.ind_porcentaje != true ? string.Empty : "%"),
                                                   Utility.Utilitarios.ConvertirAString(c.monto_2) + (c.ind_porcentaje != true ? string.Empty : "%"),
                                                   Utility.Utilitarios.ConvertirAString(c.promedio_1) + (c.ind_porcentaje != true ? string.Empty : "%"),
                                                   Utility.Utilitarios.ConvertirAString(c.promedio_2) + (c.ind_porcentaje != true ? string.Empty : "%"),
                                                   "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('I" + c.id + "', '" + (c.ind_porcentaje == true ? "S" : "N")  + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                                  };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridIndicadorFormula()
        {
            try
            {
                var ObjUsuario = usr.Get(Env.GetUserInfo("userid"));
                var tak = sp.FGA_Consultar_Calculos_Personalizados(Session["IdEntidad"].ToString(), ObjUsuario.Entidad_Usuario_Id,
                                                             Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                             Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()));
                var result = from c in tak
                             select new string[] { c.NOMBRE, Utility.Utilitarios.ConvertirAString(c.RESULTADO.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                                   Utility.Utilitarios.ConvertirAString(c.RESULTADO_COMPARAR.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                                   "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('P" + c.ID + "', '" + (c.IND_PORCENTAJE == true ? "S" : "N") + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                                  };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {


                return null;
            }
        }


        public ActionResult GetGridCAMEL()
        {
            try
            {
                String resumen = Session["TipoReporte"].ToString();

                var tak = sp.FGA_Consultar_CAMEL(Session["IdEntidad"].ToString(), Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), DateTime.Now, resumen);
                var resultCAMEL = from c in tak
                                  select new string[] { c.IDRPT_CAMEL.ToString(),
                                            resumen == "N" ? c.NOMBRE.Replace(" ", "&nbsp;").TrimEnd() : c.NOMBRE,
                                            Utility.Utilitarios.ConvertirAString(c.MONTO_1) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                            Utility.Utilitarios.ConvertirAString(c.MONTO_2) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                            Utility.Utilitarios.ConvertirAString(c.PROMEDIO_1) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                            Utility.Utilitarios.ConvertirAString(c.PROMEDIO_2) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                            "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('C" + c.IDRPT_CAMEL + "', '" + c.IND_PORCENTAJE + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                            };


                // Consulta Indicador
                var takIndicador = sp.FGA_Consultar_Otros_Indicadores(Session["IdEntidad"].ToString(),
                                                                     Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                                     Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                                                     DateTime.Now);

                var resultIndicador = from c in takIndicador
                                      select new string[] {
                                  c.ID.ToString(),
                                  c.NOMBREINDICADOR,
                                  Utility.Utilitarios.ConvertirAString(c.MONTO_1.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                  Utility.Utilitarios.ConvertirAString(c.MONTO_2.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                  Utility.Utilitarios.ConvertirAString(c.PROMEDIO_1.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                  Utility.Utilitarios.ConvertirAString(c.PROMEDIO_2.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('I" + c.ID + "', '" + (c.IND_PORCENTAJE == true ? "S" : "N") + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                              };

                // Concatenar los resultados de CAMEL e Indicador
                var combinedResults = resultCAMEL.Concat(resultIndicador).ToList();

                // Retornar ambos resultados combinados en una sola propiedad 'aaData'
                return Json(new { aaData = combinedResults }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
            }

            return null;
        }


        /* public ActionResult GetGridCAMEL()
         {
             try
             {


                 // Consulta Indicador
                 var takIndicador = sp.FGA_Consultar_Otros_Indicadores(Session["IdEntidad"].ToString(),
                                                                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                                      Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()),
                                                                      DateTime.Now);

                 var resultIndicador = from c in takIndicador
                                       select new string[] {
                                   c.ID.ToString(),
                                   c.NOMBREINDICADOR,
                                   Utility.Utilitarios.ConvertirAString(c.MONTO_1.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                   Utility.Utilitarios.ConvertirAString(c.MONTO_2.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                   Utility.Utilitarios.ConvertirAString(c.PROMEDIO_1.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                   Utility.Utilitarios.ConvertirAString(c.PROMEDIO_2.Value) + (c.IND_PORCENTAJE != true ? string.Empty : "%"),
                                   "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('I" + c.ID + "', '" + (c.IND_PORCENTAJE == true ? "S" : "N") + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                               };

                 // Concatenar los resultados de CAMEL e Indicador
                 // var combinedResults = resultCAMEL.Concat(resultIndicador).ToList();

                 // Retornar ambos resultados combinados en una sola propiedad 'aaData'
                 return Json(new { aaData = resultIndicador }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
             }

             return null;
         }*/

        
        public ActionResult GetSuficiencia306()
        {
            try
            {
                // Consulta CAMEL
                var resumen = Session["TipoReporte"].ToString();
                Usuario ObjUser = usr.Get(Env.GetUserInfo("userid"));
                var takCAMEL = sp.FGA_Consultar_Suficiencia_306(Session["IdEntidad"].ToString(),
                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                    Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), resumen);

                var resultCAMEL = from c in takCAMEL
                                  select new string[] {
                              c.IDRPT_SUFICIENCIA.ToString(),
                              resumen == "N" ? c.NOMBRE.Replace(" ", "&nbsp;").TrimEnd() : c.NOMBRE,
                              Utility.Utilitarios.ConvertirAString(c.MONTO) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                              Utility.Utilitarios.ConvertirAString(c.MONTO_COMPARAR) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                              c.IDRPT_SUFICIENCIA > 45 && c.IDRPT_SUFICIENCIA < 1000 ? "" :
                              "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('N" + c.IDRPT_SUFICIENCIA + "', '" + c.IND_PORCENTAJE + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                          };
                return Json(new { aaData = resultCAMEL }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetSuficiencia()
        {
            try
            {
                // Obtener las fechas desde la sesi�n
                var periodo1 = Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString());
                var periodo2 = Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());

                // Si es as�, establecer los montos en 0 para todas las filas
                var resumen = Session["TipoReporte"].ToString();
                var takCAMEL = sp.FGA_Consultar_CAMEL(Session["IdEntidad"].ToString(),
                    periodo1,
                    periodo2,
                    DateTime.Now,
                    resumen).Where(o => o.IDRPT_CAMEL == -11);

                // Verificar si ambos a�os son mayores o iguales a 2025
                if (periodo1.Year >= 2025 && periodo2.Year >= 2025)
                {
                

                    var resultCAMEL = from c in takCAMEL
                                      select new string[]
                                      {
                                  c.IDRPT_CAMEL.ToString(),
                                  resumen == "N" ? c.NOMBRE.Replace(" ", "&nbsp;").TrimEnd() : c.NOMBRE,
                                  "0", // Monto 1 ajustado a 0
                                  "0", // Monto 2 ajustado a 0
                                  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('C" + c.IDRPT_CAMEL + "', '" + c.IND_PORCENTAJE + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                      };

                    return Json(new { aaData = resultCAMEL }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var resultCAMEL = from c in takCAMEL
                                      select new string[]
                                      {
                                  c.IDRPT_CAMEL.ToString(),
                                  resumen == "N" ? c.NOMBRE.Replace(" ", "&nbsp;").TrimEnd() : c.NOMBRE,
                                  Utility.Utilitarios.ConvertirAString(c.MONTO_1) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                  Utility.Utilitarios.ConvertirAString(c.MONTO_2) + (c.IND_PORCENTAJE != "S" ? string.Empty : "%"),
                                  "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('C" + c.IDRPT_CAMEL + "', '" + c.IND_PORCENTAJE + "')\"><i class=\"fa fa-line-chart\"></i></a>"
                                      };

                    return Json(new { aaData = resultCAMEL }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                // Puedes loguear el error o retornar una respuesta adecuada
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetGridSugefIndicador()
        {
            try
            {
                var tak = sp.FGA_Consultar_Indicadores_Sugef(Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()),
                                                       Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString()), -1);
                var result = from c in tak
                             select new string[] {c.Id.ToString(), c.Nombre.Replace(" ", "&nbsp;"),
                                            Utility.Utilitarios.ConvertirAString(c.BancoComercialEstado) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.BancoLeyesEspeciales) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.BancosPrivadosCoope) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.EmpresaFinanNoBancaria) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.OtrasEntidadesFinancieras) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.OrganizacionesCooperativas) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.EntidadesAutorizadasVivienda) + "%",
                                            Utility.Utilitarios.ConvertirAString(c.Total) + "%",
                                            "<a data-toggle=\"tooltip\" data-placement=\"top\" title=\"Ver gr\u00E1fico\" class=\"btn btn-xs btn-info btn-indicador-chart\" href=\"javascript:getGraph('" + c.Id + "', 'S')\"><i class=\"fa fa-line-chart\"></i></a>"
            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetCuentas()
        {
            try
            {
                string search = Request.Form.GetValues("search[value]")[0];
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                int totalRecords = 0;
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) / pageSize : 0;
                var tak = cat.GetPage(skip, pageSize, search, ref totalRecords);

                var result = from c in tak
                             select new string[] { c.Cuenta.ToString(),
                                                    Convert.ToString(c.Nombre)
                                                    };

                return Json(new
                {
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public PartialViewResult ListaIndicadores()
        {
            var ObjUsuario = usr.Get(Env.GetUserInfo("userid"));
            return PartialView("_ListaIndicadores", sp.FGA_Consultar_Tipos_Indicadores(Session["IdEntidad"].ToString(), ObjUsuario.Entidad_Usuario_Id).ToList());
        }

        public PartialViewResult GuardarFormula(String Nombre, String Formula, bool IndPorcentaje)
        {
            var ObjUsuario = usr.Get(Env.GetUserInfo("userid"));
            Formulas newFormula = new Formulas();
            newFormula.Formula = Formula;
            newFormula.Nombre = Nombre;
            newFormula.Ind_Porcentaje = IndPorcentaje;
            newFormula.Entidad_Id = ObjUsuario.Entidad_Usuario_Id;

            formu.Add(ref newFormula);
            return PartialView("_ListaIndicadores", sp.FGA_Consultar_Tipos_Indicadores(Session["IdEntidad"].ToString(), ObjUsuario.Entidad_Usuario_Id).ToList());
        }

        public PartialViewResult GraficaDetalle(String Entidades, DateTime PeriodoI, DateTime PeriodoF, String[] cmb_indicadores, int cmb_estilo)
        {
            Highcharts gp = null;
            Session["IdEntidad"] = Entidades;
            Load();
            int numPeriodos = ((PeriodoF.Month + PeriodoF.Year * 12) - (PeriodoI.Month + PeriodoI.Year * 12)) + 1;
            int i = 0;
            bool existePorc = false, existeMonto = false, promedio = false;
            string[] fechas = new string[numPeriodos];
            List<Serie> listaSeries = new List<Serie>();
            ViewBag.Title = string.Empty;

            for (int j = 0; j < cmb_indicadores.Count(); j++)
                listaSeries.Add(new Serie(numPeriodos, cmb_indicadores[j]));

            bool isBalanceType = cmb_indicadores.All(ind => ind != null && (ind.StartsWith("B") || ind.StartsWith("b")));
            if (listaSeries.Count() == 1 && !isBalanceType)
            {
                listaSeries.Add(new Serie(numPeriodos, "Promedio"));
                promedio = true;
            }

            foreach (var indicador in cmb_indicadores)
            {
                var tak = sp.FGA_Consultar_Grafico_Indicador(Entidades, PeriodoI, PeriodoF, indicador);
                int takCount = tak != null ? tak.Length : 0;

                for (int j = 0; j < numPeriodos; j++)
                {
                    if (j < takCount && tak[j] != null)
                    {
                        listaSeries[i].nombre = tak[j].NOMBRE;
                        fechas[j] = tak[j].PERIODO.HasValue ? (tak[j].PERIODO.Value.Month.ToString() + "-" + tak[j].PERIODO.Value.Year.ToString()) : PeriodoI.AddMonths(j).ToString("M-yyyy");
                        listaSeries[i].total += tak[j].MONTO;
                        listaSeries[i].porcentual = tak[j].IND_PORCENTAJE;
                        listaSeries[i].valores[j] = tak[j].MONTO;

                        if (promedio)
                        {
                            ViewBag.Title = tak[j].NOMBRE;
                            listaSeries[i + 1].nombre = tak[j].NOMBRE + " (Promedio)";
                            listaSeries[i + 1].total += tak[j].PROMEDIO;
                            listaSeries[i + 1].porcentual = tak[j].IND_PORCENTAJE;
                            listaSeries[i + 1].valores[j] = tak[j].PROMEDIO;
                        }

                        if (tak[j].IND_PORCENTAJE == "S")
                            existePorc = true;
                        else
                            existeMonto = true;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(fechas[j]))
                        {
                            DateTime dt = PeriodoI.AddMonths(j);
                            fechas[j] = dt.Month.ToString() + "-" + dt.Year.ToString();
                        }
                    }
                }
                if (string.IsNullOrEmpty(ViewBag.Title) && takCount > 0 && tak[0] != null)
                {
                    ViewBag.Title = tak[0].NOMBRE;
                }
                i += 1;
            }

            HighChart.ConfigChart(ref gp, "Graph", null);
            gp.SetXAxis(HighChart.GetXAxis(fechas));
            string format = System.Web.HttpContext.Current.Session[Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '12px', color: 'black'";
            List<YAxis> listaY = new List<YAxis>();
            if (existeMonto)
            {
                listaY.Add(new YAxis()
                {
                    Id = "Monto",
                    GridLineWidth = 0,
                    Title = new YAxisTitle()
                    {
                        Text = "Total",
                    },
                    Labels = new YAxisLabels()
                    {
                        Formatter = "formatMillion",
                        Style = format,
                    }
                });
            }

            if (existePorc)
            {
                listaY.Add(new YAxis()
                {
                    Id = "Porcentual",
                    GridLineWidth = 0,
                    Title = new YAxisTitle()
                    {
                        Text = "Porcentaje",
                        Style = format,
                    },
                    Labels = new YAxisLabels()
                    {
                        Formatter = "formatPercent",
                        Style = format,
                    },
                    Opposite = existeMonto ? true : false
                });
            }

            gp.SetYAxis(listaY.ToArray());
            gp.SetPlotOptions(HighChart.getLabelDecimals());
            int totalSeries = 0;

            foreach (Serie detalle in listaSeries)
            {
                // Verificamos si todos los valores son 0
                if (detalle.valores.All(v => Convert.ToDecimal(v) == 0))
                    continue;

                totalSeries = totalSeries + 1;
            }

            Series[] series = new Series[totalSeries];
            Series serie;
            i = 0;

            foreach (Serie detalle in listaSeries)
            {
                // Verificamos si todos los valores son 0
                if (detalle.valores.All(v => Convert.ToDecimal(v) == 0))
                    continue;

                if (cmb_estilo == 0)
                {
                    serie = new Series
                    {
                        Type = ChartTypes.Line,
                        Name = detalle.nombre,
                        Data = new Data(detalle.valores),
                        Color = HighChart.GetColor(i),
                        YAxis = detalle.porcentual == "S" ? "Porcentual" : "Monto",
                        PlotOptionsLine = i == 0 ? HighChart.getLine() : promedio ? HighChart.getLineDash() : HighChart.getLine()
                    };
                }
                else
                {
                    serie = new Series
                    {
                        Type = ChartTypes.Column,
                        Name = detalle.nombre,
                        Data = new Data(detalle.valores),
                        Color = HighChart.GetColor(i),
                        YAxis = detalle.porcentual == "S" ? "Porcentual" : "Monto"
                    };
                }

                series[i] = serie;
                i += 1;
            }

            gp.SetSeries(
                series
            );

            return PartialView("_GraficaDetalle", gp);
        }


        public PartialViewResult GraficaDetalleSugef(DateTime PeriodoI, DateTime PeriodoF, int Id)
        {
            Highcharts gp = null;
            var tak = sp.FGA_Consultar_Indicadores_Sugef(PeriodoI, PeriodoF, Id);
            string[] Fechas = new string[tak.Count()];
            object[] BancosLeyesEspeciales = new object[tak.Count()];
            object[] BancosPrivados = new object[tak.Count()];
            object[] EmpresasFinancieras = new object[tak.Count()];
            object[] OrganizacionesCooperativas = new object[tak.Count()];
            object[] EntidadesAutorizadas = new object[tak.Count()];
            object[] Total = new object[tak.Count()];
            int i = 0;

            foreach (FGA_Consultar_Indicadores_Sugef_Result detalle in tak)
            {
                Fechas[i] = detalle.Periodo.Value.Month.ToString() + "-" + detalle.Periodo.Value.Year.ToString();
                BancosLeyesEspeciales[i] = detalle.BancoLeyesEspeciales;
                BancosPrivados[i] = detalle.BancosPrivadosCoope;
                EmpresasFinancieras[i] = detalle.EmpresaFinanNoBancaria;
                OrganizacionesCooperativas[i] = detalle.OrganizacionesCooperativas;
                EntidadesAutorizadas[i] = detalle.EntidadesAutorizadasVivienda;
                Total[i] = detalle.Total;
                i += 1;
            }

            ViewBag.Title = tak[0].Nombre;
            HighChart.ConfigChart(ref gp, "SugefIndicador", null);
            gp.SetXAxis(HighChart.GetXAxis(Fechas));
            gp.SetPlotOptions(HighChart.getLabelPercent());
            gp.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
            gp.SetSeries(new Series[]
            {
                new Series{
                    Name = "Bancos creados por leyes especiales",
                    Data = new Data(BancosLeyesEspeciales),
                    Color = HighChart.GetColor(0),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLinePercent()
                },
                new Series{
                    Name = " Bancos privados y cooperativos",
                    Data = new Data(BancosPrivados),
                    Color = HighChart.GetColor(1),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLineDashPercent()
                },
                new Series{
                    Name = "Empresas financieras no bancarias",
                    Data = new Data(EmpresasFinancieras),
                    Color = HighChart.GetColor(2),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLineDashPercent()
                },
                new Series{
                    Name = "Organizaciones cooperativas de ahorro y cr�dito",
                    Data = new Data(OrganizacionesCooperativas),
                    Color = HighChart.GetColor(3),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLineDashPercent()
                },
                new Series{
                    Name = "Entidades autorizadas sistema financiero nacional vivienda",
                    Data = new Data(EntidadesAutorizadas),
                    Color = HighChart.GetColor(4),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLineDashPercent()
                },
                new Series{
                    Name = "Total",
                    Data = new Data(Total),
                    Color = HighChart.GetColor(5),
                    Type = ChartTypes.Line,
                    PlotOptionsLine = HighChart.getLineDashPercent()
                }
            }
            );

            return PartialView("_GraficaDetalle", gp);
        }
        #endregion

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.FormulaService.ServiceOf_FormulasClient formu = new FGA_En_Linea.FormulaService.ServiceOf_FormulasClient();
        private readonly FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient cat = new FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                sp.Close();
                usr.Close();
                formu.Close();
                cat.Close();
            }
            base.Dispose(disposing);
        }
    }
}