using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FGA.Model;
using FGA.Utility;

namespace FGA.Controllers
{
    public class RiesgoCreditoController : BaseController
    {
        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();

        // GET: RiesgoCredito/Alertas (Pantalla de Indicadores de Alerta Temprana)
        public ActionResult Alertas(string idEntidad = null, string cartera = "Total", string producto = "Todos", string segmento = "Todos")
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

            var model = ConstruirModeloAlertas(currentEntidad, nombreEntidad, cartera);
            model.Entidades = entidades;

            return View(model);
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

        private AlertasTempranasViewModel ConstruirModeloAlertas(string idEntidad, string nombreEntidad, string cartera)
        {
            var vm = new AlertasTempranasViewModel
            {
                IdEntidad = idEntidad,
                NombreEntidad = nombreEntidad,
                PeriodoActual = "Oct 2026",
                PeriodoAnterior = "Sep 2026",
                AlertasCriticas = 2,
                AlertasAdvertencia = 3,
                IndicadoresEnRango = 5,
                IndiceAlertaGlobal = 0.56m,
                EstadoGlobal = "Advertencia",
                InterpretacionDiagnostica = "Se registran 2 alertas críticas y 3 de advertencia, asociadas principalmente a la mora >90 días, la tasa de desmejora y la concentración de los 10 mayores deudores. Se recomienda seguimiento según el protocolo interno."
            };

            var meses12 = new List<string> { "Nov", "Dic", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Set", "Oct" };

            vm.Indicadores = new List<AlertaItemViewModel>
            {
                new AlertaItemViewModel
                {
                    Codigo = "IND-01",
                    Nombre = "Mora >90 días",
                    Tipo = "Calidad y deterioro",
                    Formula = "Saldo principal créditos > 90 días / Saldo principal total",
                    ValorActual = 6.8m,
                    ValorAnterior = 6.5m,
                    ValorActualFormateado = "6,8%",
                    ValorAnteriorFormateado = "6,5%",
                    LimiteTexto = "≤ 5,0%",
                    LimiteValor = 5.0m,
                    LimiteOperador = "<=",
                    Estado = "Critica",
                    EstadoTexto = "Crítica",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 4.9m, 5.2m, 5.5m, 5.7m, 5.9m, 6.0m, 6.2m, 6.3m, 6.4m, 6.5m, 6.6m, 6.8m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-02",
                    Nombre = "Mora ampliada (1-90 + >90)",
                    Tipo = "Calidad y deterioro",
                    Formula = "Saldo principal créditos con mora > 0 días / Saldo principal total",
                    ValorActual = 8.6m,
                    ValorAnterior = 8.1m,
                    ValorActualFormateado = "8,6%",
                    ValorAnteriorFormateado = "8,1%",
                    LimiteTexto = "≤ 7,0%",
                    LimiteValor = 7.0m,
                    LimiteOperador = "<=",
                    Estado = "Advertencia",
                    EstadoTexto = "Advertencia",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 6.8m, 7.0m, 7.2m, 7.4m, 7.6m, 7.8m, 7.9m, 8.0m, 8.2m, 8.1m, 8.3m, 8.6m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-03",
                    Nombre = "Tasa de desmejora (12 meses)",
                    Tipo = "Dinámica crediticia",
                    Formula = "Créditos que migraron a peor categoría en 12m / Cartera inicial",
                    ValorActual = 19.1m,
                    ValorAnterior = 18.5m,
                    ValorActualFormateado = "19,1%",
                    ValorAnteriorFormateado = "18,5%",
                    LimiteTexto = "≤ 17,0%",
                    LimiteValor = 17.0m,
                    LimiteOperador = "<=",
                    Estado = "Critica",
                    EstadoTexto = "Crítica",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 15.2m, 15.6m, 16.0m, 16.4m, 16.8m, 17.1m, 17.5m, 17.8m, 18.1m, 18.5m, 18.8m, 19.1m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-04",
                    Nombre = "Tasa de mejora (12 meses)",
                    Tipo = "Dinámica crediticia",
                    Formula = "Créditos que migraron a mejor categoría en 12m / Cartera inicial",
                    ValorActual = 18.4m,
                    ValorAnterior = 19.0m,
                    ValorActualFormateado = "18,4%",
                    ValorAnteriorFormateado = "19,0%",
                    LimiteTexto = "≥ 20,0%",
                    LimiteValor = 20.0m,
                    LimiteOperador = ">=",
                    Estado = "Advertencia",
                    EstadoTexto = "Advertencia",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 22.0m, 21.5m, 21.2m, 20.8m, 20.4m, 20.1m, 19.8m, 19.5m, 19.2m, 19.0m, 18.7m, 18.4m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-05",
                    Nombre = "Tasa de permanencia (12 meses)",
                    Tipo = "Dinámica crediticia",
                    Formula = "Créditos que mantuvieron categoría en 12m / Cartera inicial",
                    ValorActual = 62.5m,
                    ValorAnterior = 62.0m,
                    ValorActualFormateado = "62,5%",
                    ValorAnteriorFormateado = "62,0%",
                    LimiteTexto = "≥ 60,0%",
                    LimiteValor = 60.0m,
                    LimiteOperador = ">=",
                    Estado = "EnRango",
                    EstadoTexto = "En rango",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 62.8m, 62.9m, 62.4m, 62.1m, 61.8m, 61.9m, 62.0m, 62.2m, 62.1m, 62.0m, 62.3m, 62.5m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-06",
                    Nombre = "Deterioro máximo observado",
                    Tipo = "Calidad y deterioro",
                    Formula = "Pérdida máxima observada en cartera en ventana de 12 meses",
                    ValorActual = 26.8m,
                    ValorAnterior = 26.1m,
                    ValorActualFormateado = "26,8%",
                    ValorAnteriorFormateado = "26,1%",
                    LimiteTexto = "≤ 30,0%",
                    LimiteValor = 30.0m,
                    LimiteOperador = "<=",
                    Estado = "EnRango",
                    EstadoTexto = "En rango",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 24.5m, 24.8m, 25.1m, 25.4m, 25.7m, 25.9m, 26.0m, 26.2m, 26.4m, 26.1m, 26.5m, 26.8m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-07",
                    Nombre = "Concentración 10 mayores deudores",
                    Tipo = "Concentración",
                    Formula = "Saldo 10 mayores deudores / Saldo cartera total",
                    ValorActual = 25.4m,
                    ValorAnterior = 24.9m,
                    ValorActualFormateado = "25,4%",
                    ValorAnteriorFormateado = "24,9%",
                    LimiteTexto = "≤ 25,0%",
                    LimiteValor = 25.0m,
                    LimiteOperador = "<=",
                    Estado = "Advertencia",
                    EstadoTexto = "Advertencia",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 23.8m, 24.0m, 24.2m, 24.3m, 24.5m, 24.6m, 24.7m, 24.8m, 24.9m, 24.9m, 25.1m, 25.4m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-08",
                    Nombre = "Cobertura de estimaciones",
                    Tipo = "Cobertura",
                    Formula = "Saldo estimaciones registradas (Cta 139) / Saldo créditos en mora",
                    ValorActual = 112.0m,
                    ValorAnterior = 109.5m,
                    ValorActualFormateado = "112,0%",
                    ValorAnteriorFormateado = "109,5%",
                    LimiteTexto = "≥ 110,0%",
                    LimiteValor = 110.0m,
                    LimiteOperador = ">=",
                    Estado = "EnRango",
                    EstadoTexto = "En rango",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 115.0m, 114.2m, 113.5m, 112.8m, 111.4m, 110.5m, 110.0m, 109.8m, 109.5m, 109.5m, 110.8m, 112.0m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-09",
                    Nombre = "Crecimiento de cartera (a/a)",
                    Tipo = "Dinámica crediticia",
                    Formula = "(Cartera bruta mes actual / Cartera bruta hace 12 meses) - 1",
                    ValorActual = 9.8m,
                    ValorAnterior = 10.1m,
                    ValorActualFormateado = "9,8%",
                    ValorAnteriorFormateado = "10,1%",
                    LimiteTexto = "≤ 12,0%",
                    LimiteValor = 12.0m,
                    LimiteOperador = "<=",
                    Estado = "EnRango",
                    EstadoTexto = "En rango",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 11.5m, 11.2m, 11.0m, 10.8m, 10.5m, 10.4m, 10.2m, 10.1m, 10.0m, 10.1m, 9.9m, 9.8m }
                },
                new AlertaItemViewModel
                {
                    Codigo = "IND-10",
                    Nombre = "Índice de reestructuraciones",
                    Tipo = "Calidad y deterioro",
                    Formula = "Saldo créditos modificados y reestructurados / Saldo cartera total",
                    ValorActual = 2.3m,
                    ValorAnterior = 2.1m,
                    ValorActualFormateado = "2,3%",
                    ValorAnteriorFormateado = "2,1%",
                    LimiteTexto = "≤ 3,0%",
                    LimiteValor = 3.0m,
                    LimiteOperador = "<=",
                    Estado = "EnRango",
                    EstadoTexto = "En rango",
                    HistoricoMeses = meses12,
                    HistoricoValores = new List<decimal> { 1.8m, 1.9m, 1.9m, 2.0m, 2.0m, 2.1m, 2.1m, 2.2m, 2.1m, 2.1m, 2.2m, 2.3m }
                }
            };

            return vm;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                entClient.SafeClose();
            }
            base.Dispose(disposing);
        }
    }
}
