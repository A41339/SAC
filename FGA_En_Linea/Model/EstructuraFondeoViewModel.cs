using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class EstructuraFondeoIndexViewModel
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string Periodo1 { get; set; }
        public string Periodo2 { get; set; }
        public string PeriodoInicialGrafico { get; set; }
        public string PeriodoFinalGrafico { get; set; }
        public int TipoGraficoSeleccionado { get; set; } = 1;
        public List<IndicadorFondeoItem> Indicadores { get; set; } = new List<IndicadorFondeoItem>();
    }

    public class IndicadorFondeoItem
    {
        public int Orden { get; set; }
        public string Nombre { get; set; }
        public string Formula { get; set; }
        public decimal ValorP1 { get; set; }
        public decimal ValorP2 { get; set; }
        public decimal Variacion => ValorP2 - ValorP1;
        public bool EsPorcentaje { get; set; } = true;
        public int TipoGraficoAsociado { get; set; } = 1;

        public string ValorP1Formateado => EsPorcentaje ? string.Format("{0:N2}%", ValorP1) : string.Format("{0:N2}", ValorP1);
        public string ValorP2Formateado => EsPorcentaje ? string.Format("{0:N2}%", ValorP2) : string.Format("{0:N2}", ValorP2);
        public string VariacionFormateada
        {
            get
            {
                string signo = Variacion >= 0 ? "+" : "";
                return EsPorcentaje ? string.Format("{0}{1:N2} p.p.", signo, Variacion) : string.Format("{0}{1:N2}", signo, Variacion);
            }
        }
    }

    public class GraficoFondeoData
    {
        public int TipoGrafico { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string NotaPie { get; set; }
        public List<string> Categorias { get; set; } = new List<string>();
        public List<GraficoFondeoSerie> Series { get; set; } = new List<GraficoFondeoSerie>();
        public bool TieneDobleEje { get; set; }
        public string EjeYIzquierdoTitulo { get; set; }
        public string EjeYDerechoTitulo { get; set; }
        public bool Apilado { get; set; }
        public string ApiladoTipo { get; set; } // "normal" or "percent"
        public string TipoGraficoHighcharts { get; set; } = "column"; // "column", "line", "bar"
        public GraficoTablaData TablaData { get; set; } = new GraficoTablaData();
    }

    public class GraficoFondeoSerie
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<object> Data { get; set; } = new List<object>();
        public int YAxis { get; set; } = 0;
        public string Color { get; set; }
        public string Stack { get; set; }
        public string TooltipSuffix { get; set; } = "%";
        public bool ShowInLegend { get; set; } = true;
        public bool EnableDataLabels { get; set; } = false;
        public string DataLabelFormat { get; set; } = "{point.y:.2f}%";
    }

    public class GraficoTablaData
    {
        public string Titulo { get; set; }
        public List<string> Columnas { get; set; } = new List<string>();
        public List<GraficoTablaFila> Filas { get; set; } = new List<GraficoTablaFila>();
    }

    public class GraficoTablaFila
    {
        public string Nombre { get; set; }
        public List<string> Valores { get; set; } = new List<string>();
    }
}
