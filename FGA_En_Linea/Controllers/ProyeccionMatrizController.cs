using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using FGA.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;

namespace FGA.Controllers
{
    public class ProyeccionMatrizController : BaseController
    {

        public ActionResult Index()
        {
            Load();
            DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
            Session["PeriodoI"] = fechaEntidad.AddMonths(-13).ToShortDateString();
            Session["PeriodoF"] = fechaEntidad.AddMonths(-1).ToShortDateString();
            Session["PeriodoActual"] = fechaEntidad.ToShortDateString();
            Session["PeriodoEstimar"] = 4;
            Session["PeriodoCrecimiento"] = 36;
            Session["TipoTasa"] = 0;

            var tak = sp.FGA_Generar_Proy_Matrices(4, 36, fechaEntidad.AddMonths(-13), fechaEntidad.AddMonths(-1), Session["IdEntidad"].ToString(), 0);
            Session["tak"] = tak;
            Modelo_PMatriz model = new Modelo_PMatriz();
            if (tak.Count() > 0) { 
                model.TasaCrecimiento = tak[0].TASA.Value;
            }

            model.AlertaProyeccion = 1;
            model.Buscar = false;
            return View(model);
        }

        public void DistribucionMora(ref Modelo_PMatriz model, Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[] info)
        {
            HighChart.ConfigChart(ref model.DistribucionMora, "DsitribucionMora", null);
            string[] XAxis = new string[info.Count()];
            object[] RAlDia = new object[info.Count()];
            object[] R1_30 = new object[info.Count()];
            object[] R31_60 = new object[info.Count()];
            object[] R61_90 = new object[info.Count()];
            object[] R91_180 = new object[info.Count()];
            object[] Mas_180 = new object[info.Count()];
            object[] CJ = new object[info.Count()];
            int i = 0;

            foreach (var detalle in info)
            {
                XAxis[i] = detalle.PERIODO.Value.ToShortDateString();
                RAlDia[i] = detalle.PALDIA;
                R1_30[i] = detalle.PR1_30DIAS;
                R31_60[i] = detalle.PR31_60DIAS;
                R61_90[i] = detalle.PR61_90DIAS;
                R91_180[i] = detalle.PR91_180DIAS;
                Mas_180[i] = detalle.PMAS180;
                CJ[i] = detalle.PCJ;
                i += 1;
            }
            model.DistribucionMora.SetXAxis(HighChart.GetXAxis(XAxis));
            model.DistribucionMora.SetYAxis(new List<YAxis>
                {
                    new YAxis()
                    {
                        Id = "DistrubucionMora",
                        GridLineWidth = 0,
                        Title = new YAxisTitle()
                        {
                            Text = "Mora",
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
                        Id = "AlDia",
                        GridLineWidth = 1,
                        Title = new YAxisTitle()
                        {
                            Text = "Al día",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                        Labels = new YAxisLabels()
                        {
                            Formatter = "formatPercent",
                            Style = "fontSize: '12px', color: 'black'",
                        },
                    }
                }.ToArray());
            model.DistribucionMora.SetPlotOptions(HighChart.getLabelPercent());
            model.DistribucionMora.SetSeries(new Series[]
            {
                    new Series{
                        Type = ChartTypes.Spline,
                        Name = "Al día",
                        Data = new Data(RAlDia),
                        Color = HighChart.GetColor(0),
                        YAxis = "AlDia",
                        PlotOptionsLine = HighChart.getLine()
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "1-30 días",
                        Data = new Data(R1_30),
                        Color =  HighChart.GetColor(1),
                        YAxis = "DistrubucionMora"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "31-60 días",
                        Data = new Data(R31_60),
                        Color =  HighChart.GetColor(2),
                        YAxis = "DistrubucionMora"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "61-90 días",
                        Data = new Data(R61_90),
                        Color =  HighChart.GetColor(3),
                        YAxis = "DistrubucionMora"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "91-180 días",
                        Data = new Data(R91_180),
                        Color =  HighChart.GetColor(4),
                        YAxis = "DistrubucionMora"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Más de 180 días",
                        Data = new Data(Mas_180),
                        Color =  HighChart.GetColor(5),
                        YAxis = "DistrubucionMora"
                    },
                    new Series{
                        Type = ChartTypes.Column,
                        Name = "Cobro Judicial",
                        Data = new Data(CJ),
                        Color =  HighChart.GetColor(6),
                        YAxis = "DistrubucionMora"
                    }
            }
            );
        }

        public void MoraEstimada(ref Modelo_PMatriz model, Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[] info)
        {
            /*Concentración al día*/
            object[] UnDia = new object[info.Count()];
            object[] TreintaDias = new object[info.Count()];
            string[] Periodos = new string[info.Count()];
            int i = 0;
            decimal maxMora = 0;

            foreach (var mora in info)
            {
                Periodos[i] = mora.PERIODO.Value.ToShortDateString();
                UnDia[i] = mora.CARTERA_VENC_1DIA.Value;
                TreintaDias[i] = mora.CARTERA_VENC_30DIA.Value;
                if (mora.CARTERA_VENC_1DIA > maxMora)
                    maxMora = mora.CARTERA_VENC_1DIA.Value;
                if (mora.CARTERA_VENC_30DIA > maxMora)
                    maxMora = mora.CARTERA_VENC_30DIA.Value;
                i = i + 1;
            }

            HighChart.ConfigChart(ref model.MoraEstimada, "MoraEstimada", null);
            model.MoraEstimada.SetXAxis(new XAxis
            {
                Categories = Periodos,
                Labels = new XAxisLabels()
                {
                    Style = "fontSize: '12px', color: 'black'",
                },
                Title = new XAxisTitle()
                {
                    Text = " "
                }
            });
            model.MoraEstimada.SetYAxis(HighChart.GetYAxis(0, maxMora+1, "formatPercent"));
            model.MoraEstimada.SetPlotOptions(HighChart.getLabelPercent());
            model.MoraEstimada.SetSeries(new Series[]
            {
                new Series{
                    Type = ChartTypes.Column,
                    Name = "Cartera vencida a más de 1 día",
                    Data = new Data(UnDia),
                    Color = ColorTranslator.FromHtml("#0B4E91")
                },
                new Series{
                    Type = ChartTypes.Column,
                    Name = "Cartera vencida a más de 30 días",
                    Data = new Data(TreintaDias),
                    Color = ColorTranslator.FromHtml("#ed7c2f")//7d7d7d
                }
            }
            );
        }


        public ActionResult Buscar(String Entidades, DateTime PeriodoI, DateTime PeriodoF, int PeriodoEstimar, int PeriodoCrecimiento, decimal Tasa, string cmb_tipoTasa)
        {
            try
            {
                Session["IdEntidad"] = Entidades;
                Session["PeriodoI"] = PeriodoI.ToShortDateString();
                Session["PeriodoF"] = PeriodoF.ToShortDateString();
                Session["PeriodoEstimar"] = PeriodoEstimar;
                Session["PeriodoCrecimiento"] = PeriodoCrecimiento;
                Session["TipoTasa"] = cmb_tipoTasa;

                DateTime fechaEntidad = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString());
                Session["PeriodoActual"] = fechaEntidad.ToShortDateString();
                Load();
            }
            catch (Exception)
            {
            }

            var tasaC = cmb_tipoTasa == "0" ? 0 : (Tasa / int.Parse(cmb_tipoTasa));
            var tak = sp.FGA_Generar_Proy_Matrices(PeriodoEstimar, PeriodoCrecimiento, PeriodoI, PeriodoF, Entidades, tasaC);
            Session["tak"] = tak;
            Modelo_PMatriz model = new Modelo_PMatriz();
            if (tak.Count() > 0)
            {
                model.TasaCrecimiento = cmb_tipoTasa == "0" ? tak[0].TASA.Value : (tak[0].TASA.Value * int.Parse(cmb_tipoTasa)); 
                model.AlertaProyeccion = 1;
            }

            model.Buscar = true;
            MoraEstimada(ref model, tak);
            DistribucionMora(ref model, tak);
            return View("Index", model);
        }

        public ActionResult GetGrid()
        {
            try
            {
                var tak = (Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[])Session["tak"];

                var result = from c in tak
                             select new string[] {
                               c.PERIODO.Value.ToShortDateString(),  Utility.Utilitarios.ConvertirAString(c.ALDIA.Value),
                                  Utility.Utilitarios.ConvertirAString(c.R1_30DIAS.Value),
                                  Utility.Utilitarios.ConvertirAString(c.R31_60DIAS.Value),
                                  Utility.Utilitarios.ConvertirAString(c.R61_90DIAS.Value),
                                  Utility.Utilitarios.ConvertirAString(c.R91_180DIAS.Value),
                                  Utility.Utilitarios.ConvertirAString(c.MAS180.Value),
                                  Utility.Utilitarios.ConvertirAString(c.CJ.Value)
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public ActionResult GetGridMora()
        {
            try
            {
                var tak = (Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[])Session["tak"];

                var result = from c in tak
                             select new string[] {
                               c.PERIODO.Value.ToShortDateString(),  Utility.Utilitarios.ConvertirAString(c.PALDIA.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PR1_30DIAS.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PR31_60DIAS.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PR61_90DIAS.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PR91_180DIAS.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PMAS180.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.PCJ.Value) + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridDMora()
        {
            try
            {
                var tak = (Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[])Session["tak"];

                var result = from c in tak
                             select new string[] {
                               c.PERIODO.Value.ToShortDateString(),  Utility.Utilitarios.ConvertirAString(c.CARTERA_VENC_1DIA.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.CARTERA_VENC_30DIA.Value) + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public ActionResult GetGridDetalle()
        {
            try
            {
                var tak = (Entities.Entities.Procedures.FGA_Generar_Proy_Matrices_Result[])Session["tak"];

                var result = from c in tak
                             select new string[] {
                               c.PERIODO.Value.ToShortDateString(),  Utility.Utilitarios.ConvertirAString(c.CARTERA_TOTAL.Value),
                                  Utility.Utilitarios.ConvertirAString(c.CARTERA_MORA.Value),
                                  Utility.Utilitarios.ConvertirAString(c.ESTIMACION_X_NIVEL_MORA.Value),
                                  Utility.Utilitarios.ConvertirAString(c.PMORA_TOTAL.Value) + "%",
                                  Utility.Utilitarios.ConvertirAString(c.ESTIMACION_X_MORA.Value)  + "%"
                            };

                return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }

            return null;
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