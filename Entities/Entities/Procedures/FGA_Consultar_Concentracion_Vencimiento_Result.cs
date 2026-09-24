namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Consultar_Concentracion_Vencimiento_Result
    {
        public Nullable<System.DateTime> Periodo { get; set; }
        public Nullable<decimal> ALaVista { get; set; }
        public Nullable<decimal> De1A90Dias { get; set; }
        public Nullable<decimal> De91A180Dias { get; set; }
        public Nullable<decimal> De181A270Dias { get; set; }
        public Nullable<decimal> De271A360Dias { get; set; }
        public Nullable<decimal> De1A3Anos { get; set; }
        public Nullable<decimal> De3AnosEnAdelante { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<decimal> PorcALaVista { get; set; }
        public Nullable<decimal> PorcDe1A90Dias { get; set; }
        public Nullable<decimal> PorcDe91A180Dias { get; set; }
        public Nullable<decimal> PorcDe181A270Dias { get; set; }
        public Nullable<decimal> PorcDe271A360Dias { get; set; }
        public Nullable<decimal> PorcDe1A3Anos { get; set; }
        public Nullable<decimal> PorcDe3AnosEnAdelante { get; set; }
    }
}
