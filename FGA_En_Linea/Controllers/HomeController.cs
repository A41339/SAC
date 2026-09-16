using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Entities.Entities.Procedures;
using FGA.Model;
using FGA.Models;
using FGA.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FGA.Controllers
{
    public class HomeController : BaseController
    {

        #region Variables

        private List<FGA_Consultar_Estado_Proyeccion_Result> getIRL()
        {
            return (List<FGA_Consultar_Estado_Proyeccion_Result>)Session["DatosIRL"];
        }

        private void setIRL(List<FGA_Consultar_Estado_Proyeccion_Result> lista)
        {
            Session["DatosIRL"] = lista;
        }
        #endregion

        public ActionResult Index()
        {
            Load();
            Session["Check"] = true;
            DateTime fechaEntidad = Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());

            if (Session["Periodo1"] is null)
                Session["Periodo1"] = fechaEntidad.AddMonths(-3).ToShortDateString();

            if (Session["Periodo2"] is null)
                Session["Periodo2"] = fechaEntidad.AddMonths(-1).ToShortDateString();

            return View();
        }

        public ActionResult GetContent(String IdEntidad, string Periodo1, string Periodo2)
        {
            try
            {
                Load();
                DateTime p1 = Utilitarios.ConvertirAFecha(Periodo1);
                DateTime p2 = Utilitarios.ConvertirAFecha(Periodo2);

                Session["Periodo1"] = p1.ToShortDateString();
                Session["Periodo2"] = p2.ToShortDateString();

                Dashboard view = CargarDashboard(IdEntidad, p1, p2);
                return PartialView("_Intro", view);
            }
            catch (Exception ex)
            {
                try
                {
                    DateTime p1 = DateTime.Now.AddMonths(-3);
                    DateTime p2 = DateTime.Now.AddMonths(-1);
                    Dashboard view = CargarDashboard(IdEntidad, p1, p2);
                    return PartialView("_Intro", view);
                }
                catch
                {
                    return PartialView("_Intro", new Dashboard());
                }
            }
        }

        private Dashboard CargarDashboard(string IdEntidad, DateTime PeriodoI, DateTime PeriodoF)
        {
            if (string.IsNullOrEmpty(IdEntidad) && Session["IdEntidad"] != null)
            {
                IdEntidad = Session["IdEntidad"].ToString();
            }
            Session["IdEntidad"] = IdEntidad;
            Load();

            var view = new Dashboard();
            var info = new List<FGA_Consultar_Estado_Proyeccion_Result>();
            var takTasas = new List<FGA_Consultar_Modelo_Tasas_Result>();
            var takRenta = new List<FGA_Consultar_Modelo_Margen_Result>();
            var takMora = new List<FGA_Consultar_Grafico_Mora_Cartera_Result>();
            var takCS = new List<FGA_Consultar_Variacion_CS_Result>();
            var takCartera = new List<FGA_Consultar_CarteraTotal_Result>();
            var takSuficiencia = new List<FGA_Consultar_Grafico_Suficiencia_Result>();

            try
            {
                if (!string.IsNullOrEmpty(IdEntidad))
                {
                    var entRes = ent.Get(IdEntidad);
                    if (entRes != null)
                    {
                        view.prudencial = entRes.Perfil_Entidad_Id == 2;
                    }
                }
            }
            catch { }

            Parallel.Invoke(
                () => {
                    try { info = sp.FGA_Consultar_Estado_Proyeccion(IdEntidad, PeriodoF)?.ToList() ?? new List<FGA_Consultar_Estado_Proyeccion_Result>(); } catch { }
                },
                () => {
                    try { takTasas = sp.FGA_Consultar_Modelo_Tasas(IdEntidad, PeriodoI, PeriodoF, 0)?.ToList() ?? new List<FGA_Consultar_Modelo_Tasas_Result>(); } catch { }
                },
                () => {
                    try { takRenta = sp.FGA_Consultar_Modelo_Margen(IdEntidad, PeriodoI, PeriodoF)?.ToList() ?? new List<FGA_Consultar_Modelo_Margen_Result>(); } catch { }
                },
                () => {
                    try { takCartera = sp.FGA_Consultar_CarteraTotal(IdEntidad, PeriodoI, PeriodoF, 12)?.ToList() ?? new List<FGA_Consultar_CarteraTotal_Result>(); } catch { }
                },
                () => {
                    try { takMora = sp.FGA_Consultar_Grafico_Mora_Cartera(IdEntidad, PeriodoI, PeriodoF)?.ToList() ?? new List<FGA_Consultar_Grafico_Mora_Cartera_Result>(); } catch { }
                },
                () => {
                    try { takSuficiencia = sp.FGA_Consultar_Grafico_Suficiencia(IdEntidad, PeriodoI, PeriodoF)?.ToList() ?? new List<FGA_Consultar_Grafico_Suficiencia_Result>(); } catch { }
                },
                () => {
                    try { takCS = sp.FGA_Consultar_Variacion_CS(IdEntidad, PeriodoI, PeriodoF)?.ToList() ?? new List<FGA_Consultar_Variacion_CS_Result>(); } catch { }
                },
                () =>
                {
                    if (!string.IsNullOrEmpty(IdEntidad))
                    {
                        try
                        {
                            var result = sp.FGA_Consultar_Dashboard(IdEntidad, PeriodoI, PeriodoF);

                            if (result != null && result.Length > 0)
                            {
                                SetDashboardValue(view.Riesgo, result, view.idRiesgo);
                                SetDashboardValue(view.Activo, result, view.idActivo);
                                SetDashboardValue(view.CalceMes, result, view.idCalceMes);
                                SetDashboardValue(view.Morosidad, result, view.idMorosidad);
                                SetDashboardValue(view.Compromiso, result, view.idCompromiso);
                                SetDashboardValue(view.Calce3Meses, result, view.idCalce3Mes);
                                SetDashboardValue(view.CostoAdmin, result, view.idCostoAdmin);
                                SetDashboardValue(view.Suficiencia, result, view.idSuficiencia);
                                SetDashboardValue(view.RiesgoTasa, result, view.idRiesgoTasa);
                                SetDashboardValue(view.RiesgoCambiario, result, view.idRiesgoCambiario);
                                SetDashboardValue(view.PerdidaAcumulada, result, view.idPerdidaAcumulada);
                                SetDashboardValue(view.PerdidaEsperada, result, view.idPerdidaEsperada);

                                try
                                {
                                    SetDashboardValue(view.ICL, result, view.idICL);
                                    SetDashboardValue(view.Apalancamiento, result, view.idApalancamiento);
                                    SetDashboardValue(view.CN1, result, view.idCN1);
                                    SetDashboardValue(view.CNN1, result, view.idCCN1);

                                    SetDashboardValue(view.ResultadoPeriodo, result, view.idResultadoPeriodo);
                                    SetDashboardValue(view.CarteraCredito, result, view.idCarteraCredito);
                                    SetDashboardValue(view.ObligacionesPublico, result, view.idObligacionesPublico);

                                    SetDashboardValue(view.Deudores100, result, view.idDeudores100);
                                    SetDashboardValue(view.InversionesTitulos, result, view.idInversionesTitulos);
                                    SetDashboardValue(view.CaptacionesPlazo, result, view.idCaptacionesPlazo);
                                    SetDashboardValue(view.EstimacionesMora, result, view.idEstimacionesMora);
                                    SetDashboardValue(view.Rentabilidad, result, view.idRentabilidad);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
            );

            try { setIRL(info); } catch { }

            try { GetVariacionCartera(ref view, takCartera); } catch { }
            try { GetCapitalizacion(ref view, takCS); } catch { }
            try { GetTasas(ref view, takTasas); } catch { }
            try { GetRentabilidad(ref view, takRenta); } catch { }
            try { GetMora(PeriodoI, PeriodoF, ref view, takMora); } catch { }
            try { GetSuficiencia(PeriodoI, PeriodoF, ref view, takSuficiencia); } catch { }
            try { GetBrechas(ref view, info); } catch { }

            return view;
        }

        private void SetDashboardValue(decimal[] targetArray, FGA_Consultar_Dashboard_Result[] results, int id)
        {
            var items = results.Where(o => o.ID == id).ToList();
            if (items.Count > 1)
            {
                var itemsWithPeriod = items.Where(o => o.PERIODO.HasValue).OrderBy(o => o.PERIODO).ToList();
                var first = itemsWithPeriod.Count > 0 ? itemsWithPeriod.First() : items.First();
                var last = itemsWithPeriod.Count > 0 ? itemsWithPeriod.Last() : items.Last();

                targetArray[0] = first.MONTO ?? 0;
                targetArray[1] = last.MONTO ?? 0;
            }
            else if (items.Count == 1)
            {
                var item = items[0];
                targetArray[0] = item.MONTO ?? 0;
                targetArray[1] = item.CALIFICACION ?? item.MONTO ?? 0;
            }
            else
            {
                targetArray[0] = 0;
                targetArray[1] = 0;
            }
        }


        public void GetVariacionCartera(ref Dashboard view, List<FGA_Consultar_CarteraTotal_Result> tak)
        {
            if (tak.Count() > 0)
            {
                HighChart.ConfigChart(ref view.VariacionCartera, "VariacionCartera", null, 320);
                HighChart.ConfigChart(ref view.pVariacionCartera, "pVariacionCartera", null);

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

                view.VariacionCartera.SetXAxis(HighChart.GetXAxis(Fechas));
                view.VariacionCartera.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                view.VariacionCartera.SetPlotOptions(HighChart.getLabelAmmount());
                view.pVariacionCartera.SetXAxis(HighChart.GetXAxis(Fechas));
                view.pVariacionCartera.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                view.pVariacionCartera.SetPlotOptions(HighChart.getLabelAmmount());

                var series = new Series[]
                {
                    new Series{
                        Name = "Variaci\u00f3n interanual neta",
                        Data = new Data(CarteraTotalNeta),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    },
                    new Series
                    {
                        Name = "Variaci\u00f3n interanual bruta",
                        Data = new Data(CarteraTotalBruta),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(2)
                    }
                };

                view.VariacionCartera.SetSeries(series);
                view.pVariacionCartera.SetSeries(series);

            }
        }

        public void GetCapitalizacion(ref Dashboard view, List<FGA_Consultar_Variacion_CS_Result> tak)
        {
            if (tak.Count() > 0)
            {
                HighChart.ConfigChart(ref view.CapitalSocial, "VariacionCapitalSocial", null, 320);
                HighChart.ConfigChart(ref view.pCapitalSocial, "pVariacionCapitalSocial", null);

                string[] Fechas = new string[tak.Count()];
                object[] Variacion = new object[tak.Count()];
                int i = 0;

                foreach (FGA_Consultar_Variacion_CS_Result detalle in tak)
                {
                    Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                    Variacion[i] = detalle.VARIACION;
                    i += 1;
                }

                view.CapitalSocial.SetXAxis(HighChart.GetXAxis(Fechas));
                view.CapitalSocial.SetPlotOptions(HighChart.getLabelPercent());
                view.CapitalSocial.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                view.pCapitalSocial.SetXAxis(HighChart.GetXAxis(Fechas));
                view.pCapitalSocial.SetPlotOptions(HighChart.getLabelPercent());
                view.pCapitalSocial.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
                var series = new Series[]
                {
                    new Series{
                        Name = "Variaci\u00f3n anual del capital social",
                        Data = new Data(Variacion),
                        Color = HighChart.GetColor(4),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    }
                };

                view.CapitalSocial.SetSeries(series);
                view.pCapitalSocial.SetSeries(series);
            }
        }

        public void GetSuficiencia(DateTime PeriodoI, DateTime PeriodoF, ref Dashboard view, List<FGA_Consultar_Grafico_Suficiencia_Result> tak)
        {
            if (tak.Count() > 0)
            {
                HighChart.ConfigChart(ref view.pSuficiencia, "pSuficiencia", (ChartTypes?)600);

                var agrupadoPorPeriodo = tak
                .GroupBy(x => x.Periodo)
                .OrderBy(g => g.Key)
                .ToList();

                var maxMonto = tak.Max(o => o.Monto).Value + 2;

                string[] Fechas = new string[agrupadoPorPeriodo.Count];
                object[] Suficiencia = new object[agrupadoPorPeriodo.Count];
                object[] CN1 = new object[agrupadoPorPeriodo.Count];
                object[] CNN1 = new object[agrupadoPorPeriodo.Count];
                object[] Apalancamiento = new object[agrupadoPorPeriodo.Count];

                int i = 0;

                foreach (var grupo in agrupadoPorPeriodo)
                {
                    Fechas[i] = grupo.Key.Value.Month.ToString () + "-" + grupo.Key.Value.Year.ToString();
                    Suficiencia[i] = grupo.FirstOrDefault(x => x.Nombre.Trim().Equals("Suficiencia", StringComparison.OrdinalIgnoreCase))?.Monto ?? 0;
                    CN1[i] = grupo.FirstOrDefault(x => x.Nombre.Trim().Equals("CN1", StringComparison.OrdinalIgnoreCase))?.Monto ?? 0;
                    CNN1[i] = grupo.FirstOrDefault(x => x.Nombre.Trim().Equals("CCN1", StringComparison.OrdinalIgnoreCase))?.Monto ?? 0;
                    Apalancamiento[i] = grupo.FirstOrDefault(x => x.Nombre.Trim().Equals("Indicador de Apalancamiento", StringComparison.OrdinalIgnoreCase))?.Monto ?? 0;
                    i++;
                }

                view.pSuficiencia.SetXAxis(HighChart.GetXAxis(Fechas));
                view.pSuficiencia.SetPlotOptions(HighChart.getLabelPercent());
                view.pSuficiencia.SetYAxis(HighChart.GetYAxisMax(null, maxMonto + 1, "formatPercent"));

                var series = new Series[]
                {
                    new Series{
                        Name = "Suficiencia",
                        Data = new Data(Suficiencia),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    },
                    new Series{
                        Name = "CN1",
                        Data = new Data(CN1),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(2)
                    },
                    new Series{
                        Name = "CCN1",
                        Data = new Data(CNN1),
                        Color = HighChart.GetColor(2),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(2)
                    },
                    new Series{
                        Name = "Apalancamiento",
                        Data = new Data(Apalancamiento),
                        Color = HighChart.GetColor(4),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(2)
                    }
                };

                view.pSuficiencia.SetSeries(series);
            }
        }

        public void GetBrechas(ref Dashboard view, List<FGA_Consultar_Estado_Proyeccion_Result> info)
        {
            try
            {
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

                    XAxis x = HighChart.GetXAxis(Rangos);

                    HighChart.ConfigChart(ref view.BrechaLiquidez, "IRL", null, 320);
                    view.BrechaLiquidez.SetPlotOptions(HighChart.getLabelPercent());
                    view.BrechaLiquidez.SetXAxis(x);
                    view.BrechaLiquidez.SetYAxis(HighChart.GetYAxis(min - 1M, max + 1M, "formatPercent"));

                    HighChart.ConfigChart(ref view.pBrechaLiquidez, "pBrechaLiquidez", null);
                    view.pBrechaLiquidez.SetPlotOptions(HighChart.getLabelPercent());
                    view.pBrechaLiquidez.SetXAxis(x);
                    view.pBrechaLiquidez.SetYAxis(HighChart.GetYAxis(min - 1M, max + 1M, "formatPercent"));

                    var series = new Series[]
                        {
                            new Series{
                                Name = "Raz\u00f3n IRL por banda",
                                Data = new Data(Brecha),
                                Color = HighChart.GetColor(0),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplinePercent(3)
                            },
                            new Series{
                                Name = "Raz\u00f3n IRL acumulado",
                                Data = new Data(Acumulada),
                                Color = HighChart.GetColor(1),
                                Type = ChartTypes.Spline,
                                PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                            }
                        };

                    view.BrechaLiquidez.SetSeries(series);
                    view.pBrechaLiquidez.SetSeries(series);
                    #endregion
                }
            }
            catch (Exception)
            {
            }
        }

        public void GetTasas(ref Dashboard view, List<FGA_Consultar_Modelo_Tasas_Result> takTasas)
        {

            if (takTasas.Count() > 0)
            {
                int i = 0;
                object[] TasaPasivo = new object[takTasas.Count()];
                object[] TasaActivo = new object[takTasas.Count()];
                string[] Fechas = new string[takTasas.Count()];
                object[] Margen = new object[takTasas.Count()];

                foreach (FGA_Consultar_Modelo_Tasas_Result detalle in takTasas)
                {
                    Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                    TasaPasivo[i] = detalle.TASAPASIVA;
                    TasaActivo[i] = detalle.TASAACTIVA;
                    Margen[i] = detalle.MARGENPONDERADO;
                    i += 1;
                }

                HighChart.ConfigChart(ref view.DiferencialTasas, "Margen", null, 320);
                view.DiferencialTasas.SetXAxis(HighChart.GetXAxis(Fechas));
                view.DiferencialTasas.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                HighChart.ConfigChart(ref view.pDiferencialTasas, "pDiferencialTasas", null);
                view.pDiferencialTasas.SetXAxis(HighChart.GetXAxis(Fechas));
                view.pDiferencialTasas.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

                var series = new Series[]
                {
                    new Series{
                        Name = "Margen Ponderado",
                        Data = new Data(Margen),
                        Color = HighChart.GetColor(2),
                        Type = ChartTypes.Areaspline,
                        PlotOptionsAreaspline = HighChart.getAreasplinePercent(0.18, 2)
                    },
                    new Series{
                        Name = "Tasa Activa Impl\u00edcita",
                        Data = new Data(TasaActivo),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    },
                    new Series{
                        Name = "Tasa Pasiva Impl\u00edcita",
                        Data = new Data(TasaPasivo),
                        Color = HighChart.GetColor(1),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplineDashPercent(2)
                    }
                };

                view.DiferencialTasas.SetSeries(series);
                view.pDiferencialTasas.SetSeries(series);
            }
        }

        public void GetRentabilidad(ref Dashboard view, List<FGA_Consultar_Modelo_Margen_Result> takRenta)
        {
            int i = 0;
            string[] Fechas = new string[takRenta.Count()];
            object[] Monto = new object[takRenta.Count()];
            object[] Porcentaje = new object[takRenta.Count()];
            object[] MargenTotal = new object[takRenta.Count()];
            object[] MargenOperativo = new object[takRenta.Count()];
            object[] MargenFinanciero = new object[takRenta.Count()];

            foreach (FGA_Consultar_Modelo_Margen_Result detalle in takRenta)
            {
                /*Margen Financiero*/
                Fechas[i] = detalle.PERIODO.Month.ToString() + "-" + detalle.PERIODO.Year.ToString();
                Monto[i] = detalle.MARGENINTERMFINANC;
                Porcentaje[i] = detalle.COBERTURAINTERM;

                /*Cobertura*/
                MargenFinanciero[i] = detalle.COBERTURAINTERM;
                MargenOperativo[i] = detalle.MARGENOPERATIVO;
                MargenTotal[i] = detalle.MARGENTOTAL;
                i += 1;
            }

            HighChart.ConfigChart(ref view.IndicadoresRentabilidad, "CoberturaMargenes", null, 320);
            view.IndicadoresRentabilidad.SetXAxis(HighChart.GetXAxis(Fechas));
            view.IndicadoresRentabilidad.SetPlotOptions(HighChart.getLabelPercent());
            view.IndicadoresRentabilidad.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));
          
            HighChart.ConfigChart(ref view.pIndicadoresRentabilidad, "pCoberturaMargenes", null);
            view.pIndicadoresRentabilidad.SetXAxis(HighChart.GetXAxis(Fechas));
            view.pIndicadoresRentabilidad.SetPlotOptions(HighChart.getLabelPercent());
            view.pIndicadoresRentabilidad.SetYAxis(HighChart.GetYAxis(null, null, "formatPercent"));

            var series = new Series[]
            {
                    new Series{
                        Name = "Margen Financiero",
                        Data = new Data(MargenFinanciero),
                        Color = HighChart.GetColor(0),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    },
                    new Series{
                        Name = "Margen Operativo",
                        Data = new Data(MargenOperativo),
                        Color = HighChart.GetColor(3),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(2)
                    },
                    new Series{
                        Name = "Margen Total",
                        Data = new Data(MargenTotal),
                        Color = HighChart.GetColor(2),
                        Type = ChartTypes.Spline,
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    }
           };

            view.IndicadoresRentabilidad.SetSeries(series);
            view.pIndicadoresRentabilidad.SetSeries(series);
        }

        public void GetMora(DateTime PeriodoI, DateTime PeriodoF, ref Dashboard view, List<FGA_Consultar_Grafico_Mora_Cartera_Result> takMora)
        {
            try
            {
                if (takMora.Count() > 0)
                {
                    int numPeriodos = takMora.Count();

                    HighChart.ConfigChart(ref view.Mora, "Mora", null, 320);
                    HighChart.ConfigChart(ref view.pMora, "pGraphMora", null);

                    object[] ListaMora = new object[takMora.Count() - 1];
                    string[] fechas = new string[numPeriodos];
                    object[] AlDia = new object[numPeriodos];

                    List<Serie> listaSeries = new List<Serie>();
                    listaSeries.Add(new Serie(numPeriodos, "1 - 30 d\u00edas"));
                    listaSeries.Add(new Serie(numPeriodos, "31 - 60 d\u00edas"));
                    listaSeries.Add(new Serie(numPeriodos, "61 - 90 d\u00edas"));
                    listaSeries.Add(new Serie(numPeriodos, "91 - 180 d\u00edas"));
                    listaSeries.Add(new Serie(numPeriodos, "M\u00e1s de 180 d\u00edas"));
                    listaSeries.Add(new Serie(numPeriodos, "Cobro Judicial"));

                    for (int j = 0; j < numPeriodos; j++)
                    {
                        fechas[j] = takMora[j].PERIODO.Value.Month.ToString() + "-" + takMora[j].PERIODO.Value.Year.ToString();
                        AlDia[j] = takMora[j].ALDIA;
                        listaSeries[0].valores[j] = takMora[j].HASTA30;
                        listaSeries[1].valores[j] = takMora[j].HASTA60;
                        listaSeries[2].valores[j] = takMora[j].HASTA90;
                        listaSeries[3].valores[j] = takMora[j].HASTA180;
                        listaSeries[4].valores[j] = takMora[j].MAS180;
                        listaSeries[5].valores[j] = takMora[j].CJ;
                    }

                    view.Mora.SetXAxis(HighChart.GetXAxis(fechas));
                    view.pMora.SetXAxis(HighChart.GetXAxis(fechas));

                    List<YAxis> yAsis = new List<YAxis>()
                    {
                        new YAxis()
                        {
                            Id = "Mora",
                            GridLineWidth = 1,
                            GridLineColor = ColorTranslator.FromHtml("#f1f5f9"),
                            GridLineDashStyle = DashStyles.Dash,
                            LineColor = ColorTranslator.FromHtml("#cbd5e1"),
                            Title = new YAxisTitle()
                            {
                                Text = "Mora",
                                Style = "fontSize: '11px', color: '#64748b', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            },
                            Labels = new YAxisLabels()
                            {
                                Formatter = "formatPercent",
                                Style = "fontSize: '11px', color: '#64748b', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            },
                            Opposite = true,
                            Min = 0
                        },
                        new YAxis()
                        {
                            Id = "AlDia",
                            GridLineWidth = 0,
                            Title = new YAxisTitle()
                            {
                                Text = "Al d\u00eda",
                                Style = "fontSize: '0px', color: 'transparent'",
                            },
                            Labels = new YAxisLabels()
                            {
                                Style = "fontSize: '0px', color: 'transparent'",
                            },
                        }
                    };

                    view.Mora.SetYAxis(yAsis.ToArray());
                    view.pMora.SetYAxis(yAsis.ToArray());

                    view.Mora.SetPlotOptions(HighChart.getPlotOptionsColumn(4, true));
                    view.pMora.SetPlotOptions(HighChart.getPlotOptionsColumn(4, true));

                    var series = new Series[listaSeries.Count() + 1];
                    Series serie;
                    int i = 0;

                    // Paleta graduada semafórica para los tramos de mora (centralizada en HighChart)
                    Color[] moraColores = new Color[]
                    {
                        HighChart.GetMoraColor(0), // 1 - 30 días: Verde esmeralda (#10b981)
                        HighChart.GetMoraColor(1), // 31 - 60 días: Cyan (#06b6d4)
                        HighChart.GetMoraColor(2), // 61 - 90 días: Ámbar (#f59e0b)
                        HighChart.GetMoraColor(3), // 91 - 180 días: Naranja cálido (#ea580c)
                        HighChart.GetMoraColor(4), // Más de 180 días: Rojo alerta (#e11d48)
                        HighChart.GetMoraColor(5)  // Cobro Judicial: Borgoña / Rojo oscuro (#991b1b)
                    };

                    foreach (Serie detalle in listaSeries.OrderByDescending(o => o.total))
                    {
                        serie = new Series
                        {
                            Type = ChartTypes.Column,
                            Name = detalle.nombre,
                            Data = new Data(detalle.valores),
                            Color = i < moraColores.Length ? moraColores[i] : HighChart.GetColor(i),
                            YAxis = "Mora",
                            PlotOptionsColumn = HighChart.getColumnStyled(4, true)
                        };

                        series[i] = serie;
                        i += 1;
                    }

                    serie = new Series
                    {
                        Type = ChartTypes.Spline,
                        Name = "Al d\u00eda",
                        Data = new Data(AlDia),
                        Color = HighChart.GetColor(0),
                        YAxis = "AlDia",
                        PlotOptionsSpline = HighChart.getSplinePercent(3)
                    };

                    series[i] = serie;
                    view.Mora.SetSeries(series);
                    view.pMora.SetSeries(series);
                }
            }
            catch (Exception)
            {
            }
        }

        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sp.Close();
                ent.Close();
            }

            base.Dispose(disposing);
        }
    }
}