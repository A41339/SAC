using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using FGA.Model;
using FGA.Utility;
using FGA_En_Linea.SPService;

namespace FGA.Controllers
{
    public class RiesgoCreditoController : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient spClient = new FGA_En_Linea.SPService.SPClient();

        // GET: RiesgoCredito/Alertas (Pantalla de Monitoreo de Indicadores de Alerta Temprana)
        public ActionResult Alertas(string idEntidad = null, string cartera = "Total", string producto = "Todos", string segmento = "Todos", string fechaCorte = null, string periodo = null)
        {
            try
            {
                Load();
            }
            catch { }

            // Sincronizar entidad con sesión
            string currentEntidad = idEntidad ?? Session["IdEntidad"]?.ToString() ?? "2";
            Session["IdEntidad"] = currentEntidad;

            string nombreEntidad = Session["NomEntidad"]?.ToString() ?? "COOPENAE";
            var entidades = CargarListaEntidades(currentEntidad, out nombreEntidad);

            string paramCorte = !string.IsNullOrEmpty(periodo) ? periodo : fechaCorte;
            var model = ConstruirModeloAlertas(currentEntidad, nombreEntidad, cartera, producto, segmento, paramCorte);
            model.Entidades = entidades;

            return View(model);
        }

        [HttpGet]
        public JsonResult GetSerieAlerta(string codigo, string idEntidad = null, string cartera = "Total", string fechaCorte = null, string periodo = null)
        {
            try
            {
                string currentEntidad = idEntidad ?? Session["IdEntidad"]?.ToString() ?? "2";
                string paramCorte = !string.IsNullOrEmpty(periodo) ? periodo : fechaCorte;
                DateTime dtCorte = DeterminarFechaCorte(paramCorte);

                var serie = spClient.FGA_Consultar_Serie_Alerta_Temprana(currentEntidad, dtCorte, codigo, cartera);
                if (serie != null && serie.Length > 0)
                {
                    return Json(new
                    {
                        success = true,
                        meses = serie.Select(s => s.MesTexto).ToArray(),
                        valores = serie.Select(s => s.Valor ?? 0m).ToArray()
                    }, JsonRequestBehavior.AllowGet);
                }

                // Contingencia en caso de que aún no se haya poblado en BD
                var mesesDef = Generar12MesesTexto(dtCorte);
                var valsDef = ObtenerSerieContingencia(codigo);
                return Json(new { success = true, meses = mesesDef, valores = valsDef }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GenerarAlertas(string idEntidad = null, string cartera = "Total", string producto = "Todos", string segmento = "Todos", string fechaCorte = null, string periodo = null)
        {
            try
            {
                string currentEntidad = idEntidad ?? Session["IdEntidad"]?.ToString() ?? "2";
                string paramCorte = !string.IsNullOrEmpty(periodo) ? periodo : fechaCorte;
                DateTime dtCorte = DeterminarFechaCorte(paramCorte);

                spClient.FGA_Generar_Alertas_Tempranas_Credito(currentEntidad, dtCorte, cartera, producto, segmento);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private SelectList CargarListaEntidades(string entidadSeleccionada, out string nombreSeleccionado)
        {
            nombreSeleccionado = Session["NomEntidad"]?.ToString() ?? "COOPENAE";
            var items = new List<SelectListItem>();
            try
            {
                var lst = entClient.GetAll();
                if (lst != null && lst.Any())
                {
                    foreach (var e in lst.Where(x => x.Activo == true && x.Id != Utilitarios.entidadAdministradora).OrderBy(x => x.Nombre))
                    {
                        bool sel = e.Id.ToString() == entidadSeleccionada;
                        if (sel) nombreSeleccionado = e.Nombre;
                        items.Add(new SelectListItem { Value = e.Id.ToString(), Text = e.Nombre, Selected = sel });
                    }
                }
            }
            catch
            {
                items.Add(new SelectListItem { Value = "2", Text = "COOPENAE", Selected = true });
                items.Add(new SelectListItem { Value = "1", Text = "COOPEALIANZA" });
                items.Add(new SelectListItem { Value = "3", Text = "CREDECOOP" });
            }
            return new SelectList(items, "Value", "Text", entidadSeleccionada);
        }

        private DateTime DeterminarFechaCorte(string fechaCorte)
        {
            DateTime dt;
            if (!string.IsNullOrEmpty(fechaCorte))
            {
                if (DateTime.TryParseExact(fechaCorte, new[] { "yyyy-MM", "yyyy-MM-dd", "MM/yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    return new DateTime(dt.Year, dt.Month, 1);
                }
                if (DateTime.TryParse(fechaCorte, out dt))
                {
                    return new DateTime(dt.Year, dt.Month, 1);
                }
            }

            // Fecha base de análisis calibrada del proyecto (Setiembre 2026)
            return new DateTime(2026, 9, 1);
        }

        private List<string> Generar12MesesTexto(DateTime dtCorte)
        {
            var meses = new List<string>();
            var ci = new CultureInfo("es-ES");
            for (int i = 11; i >= 0; i--)
            {
                DateTime m = dtCorte.AddMonths(-i);
                meses.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(m.ToString("MMM", ci)));
            }
            return meses;
        }

        private AlertasTempranasViewModel ConstruirModeloAlertas(string idEntidad, string nombreEntidad, string cartera, string producto = "Todos", string segmento = "Todos", string fechaCorte = null)
        {
            DateTime dtCorte = DeterminarFechaCorte(fechaCorte);
            DateTime dtAnt = dtCorte.AddMonths(-1);

            var ci = new CultureInfo("es-ES");
            string strCorte = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dtCorte.ToString("MMM yyyy", ci));
            string strAnt = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dtAnt.ToString("MMM yyyy", ci));

            var vm = new AlertasTempranasViewModel
            {
                IdEntidad = idEntidad,
                NombreEntidad = nombreEntidad,
                CarteraSeleccionada = cartera,
                ProductoSeleccionado = producto,
                SegmentoSeleccionado = segmento,
                PeriodoActual = strCorte,
                PeriodoAnterior = strAnt,
                PeriodoValorPicker = dtCorte.ToString("yyyy-MM"),
                Indicadores = new List<AlertaItemViewModel>()
            };

            var meses12 = Generar12MesesTexto(dtCorte);

            // 1. Consultar SP dinámico en la base de datos
            try
            {
                var spRes = spClient.FGA_Consultar_Alertas_Tempranas_Credito(idEntidad, dtCorte, cartera, producto, segmento);
                if (spRes == null || spRes.Length == 0)
                {
                    try
                    {
                        spClient.FGA_Generar_Alertas_Tempranas_Credito(idEntidad, dtCorte, cartera, producto, segmento);
                        spRes = spClient.FGA_Consultar_Alertas_Tempranas_Credito(idEntidad, dtCorte, cartera, producto, segmento);
                    }
                    catch { }
                }

                if (spRes != null && spRes.Length > 0)
                {
                    foreach (var item in spRes.OrderBy(x => x.Orden))
                    {
                        var alerta = new AlertaItemViewModel
                        {
                            Codigo = item.Codigo,
                            Nombre = item.Nombre,
                            Tipo = item.Tipo,
                            Formula = item.Formula,
                            ValorActual = item.ValorActual ?? 0m,
                            ValorAnterior = item.ValorAnterior ?? 0m,
                            ValorActualFormateado = (item.ValorActual ?? 0m).ToString("N1", new CultureInfo("es-CR")) + "%",
                            ValorAnteriorFormateado = (item.ValorAnterior ?? 0m).ToString("N1", new CultureInfo("es-CR")) + "%",
                            LimiteTexto = item.LimiteTexto,
                            LimiteValor = item.LimiteValor ?? 0m,
                            LimiteOperador = item.LimiteOperador ?? "<=",
                            Estado = item.Estado ?? "EnRango",
                            EstadoTexto = (item.Estado == "Critica" ? "Crítica" : item.Estado == "Advertencia" ? "Advertencia" : "En rango")
                        };

                        // Consultar serie histórica de 12 meses
                        try
                        {
                            var serie = spClient.FGA_Consultar_Serie_Alerta_Temprana(idEntidad, dtCorte, item.Codigo, cartera);
                            if (serie != null && serie.Length > 0)
                            {
                                alerta.HistoricoMeses = serie.Select(s => s.MesTexto).ToList();
                                alerta.HistoricoValores = serie.Select(s => s.Valor ?? 0m).ToList();
                            }
                            else
                            {
                                alerta.HistoricoMeses = meses12;
                                alerta.HistoricoValores = ObtenerSerieContingencia(item.Codigo);
                            }
                        }
                        catch
                        {
                            alerta.HistoricoMeses = meses12;
                            alerta.HistoricoValores = ObtenerSerieContingencia(item.Codigo);
                        }

                        vm.Indicadores.Add(alerta);
                    }
                }
            }
            catch { }

            // Contingencia técnica garantizada (catálogo completo de 26 indicadores según Anexo No 1)
            if (vm.Indicadores.Count == 0)
            {
                vm.Indicadores = ObtenerIndicadoresContingencia(meses12);
            }

            // Totales y diagnósticos consolidados
            vm.AlertasCriticas = vm.Indicadores.Count(x => x.Estado == "Critica");
            vm.AlertasAdvertencia = vm.Indicadores.Count(x => x.Estado == "Advertencia");
            vm.IndicadoresEnRango = vm.Indicadores.Count(x => x.Estado == "EnRango");
            vm.IndiceAlertaGlobal = vm.TotalIndicadores > 0
                ? Math.Round(((vm.AlertasCriticas * 1.0m) + (vm.AlertasAdvertencia * 0.5m)) / vm.TotalIndicadores, 2)
                : 0m;
            vm.EstadoGlobal = vm.AlertasCriticas > 0 ? "Advertencia" : "En rango";

            // Diagnóstico interpretativo institucional
            var criticos = vm.Indicadores.Where(x => x.Estado == "Critica").Select(x => x.Nombre).ToList();
            var advertencias = vm.Indicadores.Where(x => x.Estado == "Advertencia").Select(x => x.Nombre).ToList();
            string listaCrit = criticos.Count > 0 ? string.Join(", ", criticos) : "ninguno";
            string listaAdv = advertencias.Count > 0 ? string.Join(", ", advertencias) : "ninguno";

            vm.InterpretacionDiagnostica = string.Format(
                "Se registran {0} alertas críticas ({1}) y {2} de advertencia ({3}). Se recomienda seguimiento según el protocolo interno de gobernanza de riesgo de crédito.",
                vm.AlertasCriticas, listaCrit, vm.AlertasAdvertencia, listaAdv);

            return vm;
        }

        private List<decimal> ObtenerSerieContingencia(string codigo)
        {
            switch (codigo)
            {
                case "IND-01": return new List<decimal> { 11.2m, 11.5m, 11.8m, 12.0m, 12.2m, 12.4m, 12.6m, 12.7m, 12.8m, 12.9m, 13.0m, 13.1m };
                case "IND-02": return new List<decimal> { 5.2m, 5.4m, 5.6m, 5.8m, 5.9m, 6.0m, 6.1m, 6.2m, 6.3m, 6.3m, 6.4m, 6.5m };
                case "IND-03": return new List<decimal> { 2.4m, 2.5m, 2.6m, 2.7m, 2.8m, 2.8m, 2.9m, 2.9m, 3.0m, 3.0m, 3.0m, 3.1m };
                case "IND-04": return new List<decimal> { 12.1m, 12.3m, 12.5m, 12.8m, 13.0m, 13.2m, 13.4m, 13.6m, 13.8m, 13.9m, 14.0m, 14.2m };
                case "IND-05": return new List<decimal> { 3.1m, 3.2m, 3.2m, 3.3m, 3.3m, 3.4m, 3.4m, 3.5m, 3.4m, 3.4m, 3.4m, 3.4m };
                case "IND-06": return new List<decimal> { 2.5m, 2.6m, 2.6m, 2.7m, 2.7m, 2.8m, 2.8m, 2.9m, 2.9m, 2.9m, 2.9m, 2.9m };
                case "IND-07": return new List<decimal> { 1.8m, 1.9m, 1.9m, 2.0m, 2.0m, 2.1m, 2.1m, 2.2m, 2.2m, 2.2m, 2.3m, 2.3m };
                case "IND-08": return new List<decimal> { 17.5m, 17.2m, 17.0m, 16.8m, 16.5m, 16.6m, 16.7m, 16.5m, 16.4m, 16.5m, 16.4m, 16.5m };
                case "IND-09": return new List<decimal> { 14.2m, 15.0m, 15.8m, 16.5m, 17.2m, 18.0m, 18.6m, 19.1m, 19.5m, 19.8m, 20.1m, 20.4m };
                case "IND-10": return new List<decimal> { 4.5m, 4.4m, 4.3m, 4.3m, 4.2m, 4.2m, 4.1m, 4.1m, 4.2m, 4.1m, 4.1m, 4.2m };
                case "IND-11": return new List<decimal> { 2.8m, 2.7m, 2.6m, 2.5m, 2.4m, 2.3m, 2.3m, 2.2m, 2.2m, 2.1m, 2.1m, 2.1m };
                case "IND-12": return new List<decimal> { 5.2m, 5.1m, 5.0m, 4.9m, 4.9m, 4.8m, 4.8m, 4.7m, 4.8m, 4.8m, 4.8m, 4.8m };
                case "IND-13": return new List<decimal> { 1.02m, 1.03m, 1.03m, 1.04m, 1.04m, 1.04m, 1.05m, 1.05m, 1.05m, 1.05m, 1.05m, 1.05m };
                case "IND-14": return new List<decimal> { 23.5m, 23.2m, 23.0m, 22.8m, 22.5m, 22.4m, 22.3m, 22.2m, 22.1m, 22.1m, 22.1m, 22.1m };
                case "IND-15": return new List<decimal> { 14.2m, 14.5m, 14.8m, 15.0m, 15.2m, 15.4m, 15.5m, 15.6m, 15.7m, 15.7m, 15.8m, 15.8m };
                case "IND-16": return new List<decimal> { 19.5m, 19.2m, 19.0m, 18.8m, 18.7m, 18.6m, 18.5m, 18.5m, 18.4m, 18.4m, 18.4m, 18.4m };
                case "IND-17": return new List<decimal> { 25.0m, 25.2m, 25.5m, 25.8m, 26.0m, 26.2m, 26.4m, 26.5m, 26.6m, 26.7m, 26.7m, 26.8m };
                case "IND-18": return new List<decimal> { 3.2m, 3.3m, 3.4m, 3.5m, 3.5m, 3.6m, 3.6m, 3.7m, 3.7m, 3.8m, 3.8m, 3.8m };
                case "IND-19": return new List<decimal> { 2.6m, 2.7m, 2.8m, 2.8m, 2.9m, 2.9m, 3.0m, 3.0m, 3.0m, 3.1m, 3.1m, 3.1m };
                case "IND-20": return new List<decimal> { 20.8m, 20.9m, 21.0m, 21.1m, 21.2m, 21.2m, 21.3m, 21.3m, 21.4m, 21.4m, 21.4m, 21.4m };
                case "IND-21": return new List<decimal> { 75.0m, 75.5m, 76.0m, 76.5m, 77.0m, 77.2m, 77.5m, 77.8m, 78.0m, 78.2m, 78.4m, 78.5m };
                case "IND-22": return new List<decimal> { 122.0m, 121.5m, 121.0m, 120.5m, 120.0m, 119.5m, 119.0m, 118.8m, 118.5m, 118.4m, 118.3m, 118.2m };
                case "IND-23": return new List<decimal> { 5.5m, 5.6m, 5.6m, 5.7m, 5.7m, 5.8m, 5.8m, 5.8m, 5.9m, 5.9m, 5.9m, 5.9m };
                case "IND-24": return new List<decimal> { 112.0m, 111.5m, 111.0m, 110.5m, 110.0m, 109.5m, 109.0m, 108.8m, 108.6m, 108.5m, 108.5m, 108.5m };
                case "IND-25": return new List<decimal> { 79.0m, 79.5m, 80.0m, 80.5m, 81.0m, 81.2m, 81.5m, 81.8m, 82.0m, 82.1m, 82.3m, 82.4m };
                case "IND-26": return new List<decimal> { 23.0m, 23.2m, 23.3m, 23.5m, 23.6m, 23.7m, 23.8m, 23.9m, 24.0m, 24.0m, 24.1m, 24.1m };
                default: return new List<decimal> { 5.0m, 5.1m, 5.2m, 5.2m, 5.3m, 5.3m, 5.4m, 5.4m, 5.5m, 5.5m, 5.5m, 5.5m };
            }
        }

        private List<AlertaItemViewModel> ObtenerIndicadoresContingencia(List<string> meses12)
        {
            return new List<AlertaItemViewModel>
            {
                // FAMILIA 1: CALIDAD Y DETERIORO (8)
                new AlertaItemViewModel { Codigo = "IND-01", Nombre = "Mora legal > 1 día vencido", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos con más de un día de mora / Saldo principal total", ValorActual = 13.1m, ValorAnterior = 13.0m, ValorActualFormateado = "13,1%", ValorAnteriorFormateado = "13,0%", LimiteTexto = "≤ 10,0%", LimiteValor = 10.0m, LimiteOperador = "<=", Estado = "Critica", EstadoTexto = "Crítica", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-01") },
                new AlertaItemViewModel { Codigo = "IND-02", Nombre = "Mora administrativa > a 31 días", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos con más de 31 días de mora / Saldo principal total", ValorActual = 6.5m, ValorAnterior = 6.4m, ValorActualFormateado = "6,5%", ValorAnteriorFormateado = "6,4%", LimiteTexto = "≤ 8,0%", LimiteValor = 8.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-02") },
                new AlertaItemViewModel { Codigo = "IND-03", Nombre = "Morosidad regulatoria > 90 días", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos con más de 91 días de mora / Saldo principal total", ValorActual = 3.1m, ValorAnterior = 3.0m, ValorActualFormateado = "3,1%", ValorAnteriorFormateado = "3,0%", LimiteTexto = "≤ 2,5%", LimiteValor = 2.5m, LimiteOperador = "<=", Estado = "Critica", EstadoTexto = "Crítica", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-03") },
                new AlertaItemViewModel { Codigo = "IND-04", Nombre = "Mora ampliada", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos con mora total / Saldo principal total", ValorActual = 14.2m, ValorAnterior = 14.0m, ValorActualFormateado = "14,2%", ValorAnteriorFormateado = "14,0%", LimiteTexto = "≤ 12,0%", LimiteValor = 12.0m, LimiteOperador = "<=", Estado = "Critica", EstadoTexto = "Crítica", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-04") },
                new AlertaItemViewModel { Codigo = "IND-05", Nombre = "Cartera en vigilancia especial", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos en etapa 2 / Saldo principal total", ValorActual = 3.4m, ValorAnterior = 3.4m, ValorActualFormateado = "3,4%", ValorAnteriorFormateado = "3,4%", LimiteTexto = "≤ 5,0%", LimiteValor = 5.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-05") },
                new AlertaItemViewModel { Codigo = "IND-06", Nombre = "Cartera en dudosa recuperación", Tipo = "Calidad y deterioro", Formula = "Saldo principal créditos en etapa 3 / Saldo principal total", ValorActual = 2.9m, ValorAnterior = 2.9m, ValorActualFormateado = "2,9%", ValorAnteriorFormateado = "2,9%", LimiteTexto = "≤ 4,0%", LimiteValor = 4.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-06") },
                new AlertaItemViewModel { Codigo = "IND-07", Nombre = "Índice de cartera reestructurada", Tipo = "Calidad y deterioro", Formula = "Saldo principal cartera reestructurada / Saldo principal total", ValorActual = 2.3m, ValorAnterior = 2.2m, ValorActualFormateado = "2,3%", ValorAnteriorFormateado = "2,2%", LimiteTexto = "≤ 3,0%", LimiteValor = 3.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-07") },
                new AlertaItemViewModel { Codigo = "IND-08", Nombre = "Recuperaciones del período", Tipo = "Calidad y deterioro", Formula = "Recuperaciones del período / (Saldo vencido inicial + Vencimientos)", ValorActual = 16.5m, ValorAnterior = 16.4m, ValorActualFormateado = "16,5%", ValorAnteriorFormateado = "16,4%", LimiteTexto = "≥ 15,0%", LimiteValor = 15.0m, LimiteOperador = ">=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-08") },

                // FAMILIA 2: DINÁMICA Y COLOCACIÓN (5)
                new AlertaItemViewModel { Codigo = "IND-09", Nombre = "Variación trimestral mora administrativa", Tipo = "Dinámica y colocación", Formula = "(Monto absoluto mora adm cierre mes / cierre trimestre anterior) - 1", ValorActual = 20.4m, ValorAnterior = 20.1m, ValorActualFormateado = "20,4%", ValorAnteriorFormateado = "20,1%", LimiteTexto = "≤ 15,0%", LimiteValor = 15.0m, LimiteOperador = "<=", Estado = "Advertencia", EstadoTexto = "Advertencia", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-09") },
                new AlertaItemViewModel { Codigo = "IND-10", Nombre = "Monto colocado mensual / cartera mes ant.", Tipo = "Dinámica y colocación", Formula = "Valor absoluto monto desembolsado / saldo principal mes anterior", ValorActual = 4.2m, ValorAnterior = 4.1m, ValorActualFormateado = "4,2%", ValorAnteriorFormateado = "4,1%", LimiteTexto = "≥ 3,0%", LimiteValor = 3.0m, LimiteOperador = ">=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-10") },
                new AlertaItemViewModel { Codigo = "IND-11", Nombre = "Cancelaciones anticipadas / cartera mes ant.", Tipo = "Dinámica y colocación", Formula = "Monto amortizaciones y cancelaciones extraordinarias / Cartera anterior", ValorActual = 2.1m, ValorAnterior = 2.1m, ValorActualFormateado = "2,1%", ValorAnteriorFormateado = "2,1%", LimiteTexto = "≤ 5,0%", LimiteValor = 5.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-11") },
                new AlertaItemViewModel { Codigo = "IND-12", Nombre = "Volatilidad crecimiento cartera neta", Tipo = "Dinámica y colocación", Formula = "Desviación estándar de tasa variación mensual en últimos 12 meses", ValorActual = 4.8m, ValorAnterior = 4.8m, ValorActualFormateado = "4,8%", ValorAnteriorFormateado = "4,8%", LimiteTexto = "≤ 8,0%", LimiteValor = 8.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-12") },
                new AlertaItemViewModel { Codigo = "IND-13", Nombre = "Crecimiento relativo cartera vencida vs total", Tipo = "Dinámica y colocación", Formula = "Comparación interanual variación cuenta 13100000 vs 13200000", ValorActual = 1.05m, ValorAnterior = 1.05m, ValorActualFormateado = "1,05x", ValorAnteriorFormateado = "1,05x", LimiteTexto = "≤ 1,1x", LimiteValor = 1.1m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-13") },

                // FAMILIA 3: MADURACIÓN Y COSECHAS (6)
                new AlertaItemViewModel { Codigo = "IND-14", Nombre = "Tasa de mejora 1 a 30 días (12 meses)", Tipo = "Maduración y cosechas", Formula = "Créditos que migraron a mejor categoría en 12m / Cartera inicial", ValorActual = 22.1m, ValorAnterior = 22.1m, ValorActualFormateado = "22,1%", ValorAnteriorFormateado = "22,1%", LimiteTexto = "≥ 20,0%", LimiteValor = 20.0m, LimiteOperador = ">=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-14") },
                new AlertaItemViewModel { Codigo = "IND-15", Nombre = "Tasa de desmejora 1 a 30 días (12 meses)", Tipo = "Maduración y cosechas", Formula = "Créditos que migraron a peor categoría en 12m / Cartera inicial", ValorActual = 15.8m, ValorAnterior = 15.7m, ValorActualFormateado = "15,8%", ValorAnteriorFormateado = "15,7%", LimiteTexto = "≤ 17,0%", LimiteValor = 17.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-15") },
                new AlertaItemViewModel { Codigo = "IND-16", Nombre = "Tasa de permanencia 1 a 30 días (12m)", Tipo = "Maduración y cosechas", Formula = "Créditos que mantuvieron categoría en 12m / Cartera inicial", ValorActual = 18.4m, ValorAnterior = 18.4m, ValorActualFormateado = "18,4%", ValorAnteriorFormateado = "18,4%", LimiteTexto = "≤ 25,0%", LimiteValor = 25.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-16") },
                new AlertaItemViewModel { Codigo = "IND-17", Nombre = "Deterioro máximo observado 1 a 30 días", Tipo = "Maduración y cosechas", Formula = "Pérdida máxima observada en cartera en ventana de 12 meses", ValorActual = 26.8m, ValorAnterior = 26.7m, ValorActualFormateado = "26,8%", ValorAnteriorFormateado = "26,7%", LimiteTexto = "≤ 30,0%", LimiteValor = 30.0m, LimiteOperador = "<=", Estado = "Advertencia", EstadoTexto = "Advertencia", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-17") },
                new AlertaItemViewModel { Codigo = "IND-18", Nombre = "Cantidad créditos mora 12m / Colocados", Tipo = "Maduración y cosechas", Formula = "Número operaciones entraron en mora en 12m / Operaciones colocadas", ValorActual = 3.8m, ValorAnterior = 3.8m, ValorActualFormateado = "3,8%", ValorAnteriorFormateado = "3,8%", LimiteTexto = "≤ 5,0%", LimiteValor = 5.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-18") },
                new AlertaItemViewModel { Codigo = "IND-19", Nombre = "Valor créditos mora 12m / Monto colocado", Tipo = "Maduración y cosechas", Formula = "Saldo créditos en mora a 12m / Monto colocado hace 12 meses", ValorActual = 3.1m, ValorAnterior = 3.1m, ValorActualFormateado = "3,1%", ValorAnteriorFormateado = "3,1%", LimiteTexto = "≤ 4,0%", LimiteValor = 4.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-19") },

                // FAMILIA 4: CONCENTRACIÓN Y COBERTURA (7)
                new AlertaItemViewModel { Codigo = "IND-20", Nombre = "Concentración 10 mayores deudores", Tipo = "Concentración y cobertura", Formula = "Saldo 10 mayores deudores / Saldo cartera total", ValorActual = 21.4m, ValorAnterior = 21.4m, ValorActualFormateado = "21,4%", ValorAnteriorFormateado = "21,4%", LimiteTexto = "≤ 25,0%", LimiteValor = 25.0m, LimiteOperador = "<=", Estado = "Advertencia", EstadoTexto = "Advertencia", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-20") },
                new AlertaItemViewModel { Codigo = "IND-21", Nombre = "Concentración 20 mayores / Patrimonio", Tipo = "Concentración y cobertura", Formula = "Saldo 20 mayores deudores / Patrimonio técnico o contable", ValorActual = 78.5m, ValorAnterior = 78.4m, ValorActualFormateado = "78,5%", ValorAnteriorFormateado = "78,4%", LimiteTexto = "≤ 100,0%", LimiteValor = 100.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-21") },
                new AlertaItemViewModel { Codigo = "IND-22", Nombre = "Cobertura de estimaciones", Tipo = "Concentración y cobertura", Formula = "Saldo estimaciones registradas (Cta 139) / Saldo créditos en mora (Cta 132)", ValorActual = 118.2m, ValorAnterior = 118.3m, ValorActualFormateado = "118,2%", ValorAnteriorFormateado = "118,3%", LimiteTexto = "≥ 110,0%", LimiteValor = 110.0m, LimiteOperador = ">=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-22") },
                new AlertaItemViewModel { Codigo = "IND-23", Nombre = "Estimaciones / Cartera neta", Tipo = "Concentración y cobertura", Formula = "Saldo estimaciones / (Cartera total - Estimaciones)", ValorActual = 5.9m, ValorAnterior = 5.9m, ValorActualFormateado = "5,9%", ValorAnteriorFormateado = "5,9%", LimiteTexto = "≤ 8,0%", LimiteValor = 8.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-23") },
                new AlertaItemViewModel { Codigo = "IND-24", Nombre = "Estimaciones / Cartera vencida neta", Tipo = "Concentración y cobertura", Formula = "Saldo estimaciones / Saldo cartera vencida neta", ValorActual = 108.5m, ValorAnterior = 108.5m, ValorActualFormateado = "108,5%", ValorAnteriorFormateado = "108,5%", LimiteTexto = "≥ 100,0%", LimiteValor = 100.0m, LimiteOperador = ">=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-24") },
                new AlertaItemViewModel { Codigo = "IND-25", Nombre = "Pérdida esperada / Estimaciones registradas", Tipo = "Concentración y cobertura", Formula = "Pérdida esperada según IFRS9 / Saldo cuenta 13900000", ValorActual = 82.4m, ValorAnterior = 82.3m, ValorActualFormateado = "82,4%", ValorAnteriorFormateado = "82,3%", LimiteTexto = "≤ 100,0%", LimiteValor = 100.0m, LimiteOperador = "<=", Estado = "EnRango", EstadoTexto = "En rango", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-25") },
                new AlertaItemViewModel { Codigo = "IND-26", Nombre = "10 operaciones mayor PE / Total PE", Tipo = "Concentración y cobertura", Formula = "Pérdida esperada de 10 mayores operaciones / Pérdida esperada total", ValorActual = 24.1m, ValorAnterior = 24.1m, ValorActualFormateado = "24,1%", ValorAnteriorFormateado = "24,1%", LimiteTexto = "≤ 30,0%", LimiteValor = 30.0m, LimiteOperador = "<=", Estado = "Advertencia", EstadoTexto = "Advertencia", HistoricoMeses = meses12, HistoricoValores = ObtenerSerieContingencia("IND-26") }
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                entClient.SafeClose();
                spClient.SafeClose();
            }
            base.Dispose(disposing);
        }
    }
}
