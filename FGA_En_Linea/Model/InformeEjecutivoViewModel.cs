using System;
using System.Collections.Generic;

namespace FGA.Model
{
    public class UmbralItem
    {
        public string Rango { get; set; }
        public string Nota { get; set; }
        public string Estado { get; set; }
        public bool EsActual { get; set; }
    }

    public class PilarCamelItem
    {
        public string Clave { get; set; }
        public string NombrePilar { get; set; }
        public string IndicadorBase { get; set; }
        public string ValorBaseFormateado { get; set; }
        public decimal ValorBaseNumerico { get; set; }
        public decimal Calificacion { get; set; }
        public string Semaforo { get; set; }
        public string Observaciones { get; set; }
        public string Formula { get; set; }
        public string UmbralesRegulatorios { get; set; }
        public string ExplicacionCalculo { get; set; }
        public List<UmbralItem> Umbrales { get; set; } = new List<UmbralItem>();
    }

    public class InformeEjecutivoViewModel
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string PeriodoCorte { get; set; }
        public string FechaGeneracion { get; set; }

        // CAMEL Global
        public string CategoriaCamel { get; set; } = "A";
        public string DescripcionCamel { get; set; } = "Desempeño Fuerte";
        public string DictamenRegulacion { get; set; }

        // Ratios SUGEF
        public decimal SuficienciaPatrimonial { get; set; }
        public decimal Morosidad90Dias { get; set; }
        public decimal CoberturaProvisiones { get; set; }
        public decimal EficienciaOperativa { get; set; }

        // Pilares CAMEL
        public decimal NotaCapital { get; set; }
        public decimal NotaActivos { get; set; }
        public decimal NotaManejo { get; set; }
        public decimal NotaEvaluacion { get; set; }
        public decimal NotaLiquidez { get; set; }

        public string SemaforoCapital { get; set; } = "Verde";
        public string SemaforoActivos { get; set; } = "Verde";
        public string SemaforoManejo { get; set; } = "Verde";
        public string SemaforoEvaluacion { get; set; } = "Verde";
        public string SemaforoLiquidez { get; set; } = "Verde";

        public string ObsCapital { get; set; }
        public string ObsActivos { get; set; }
        public string ObsManejo { get; set; }
        public string ObsEvaluacion { get; set; }
        public string ObsLiquidez { get; set; }

        // Lista detallada de pilares para vista ejecutiva con ayuda de cálculo
        public List<PilarCamelItem> PilaresCamel { get; set; } = new List<PilarCamelItem>();
    }
}
