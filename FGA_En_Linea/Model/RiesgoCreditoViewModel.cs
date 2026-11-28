using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FGA.Model
{
    public class AlertaItemViewModel
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Formula { get; set; } = "";
        public decimal ValorActual { get; set; }
        public decimal ValorAnterior { get; set; }
        public string LimiteTexto { get; set; } = "";
        public decimal? LimiteValor { get; set; }
        public string LimiteOperador { get; set; } = "<="; // "<=", ">="
        public string Estado { get; set; } = "EnRango"; // "Critica", "Advertencia", "EnRango"
        public string EstadoTexto { get; set; } = "En rango";
        public string ValorActualFormateado { get; set; } = "0.0%";
        public string ValorAnteriorFormateado { get; set; } = "0.0%";
        public List<string> HistoricoMeses { get; set; } = new List<string>();
        public List<decimal> HistoricoValores { get; set; } = new List<decimal>();
    }

    public class AlertasTempranasViewModel
    {
        public string IdEntidad { get; set; } = "2";
        public string NombreEntidad { get; set; } = "COOPENAE";
        public string PeriodoActual { get; set; } = "Oct 2026";
        public string PeriodoAnterior { get; set; } = "Sep 2026";
        public int AlertasCriticas { get; set; } = 2;
        public int AlertasAdvertencia { get; set; } = 3;
        public int IndicadoresEnRango { get; set; } = 5;
        public int TotalIndicadores => AlertasCriticas + AlertasAdvertencia + IndicadoresEnRango;
        public decimal IndiceAlertaGlobal { get; set; } = 0.56m;
        public string EstadoGlobal { get; set; } = "Advertencia";
        public string InterpretacionDiagnostica { get; set; } = "";
        public List<AlertaItemViewModel> Indicadores { get; set; } = new List<AlertaItemViewModel>();
        public SelectList Entidades { get; set; }
    }
}
