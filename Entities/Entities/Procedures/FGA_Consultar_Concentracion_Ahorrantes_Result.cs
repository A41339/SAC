namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Consultar_Concentracion_Ahorrantes_Result
    {
        public Nullable<System.DateTime> Periodo { get; set; }
        public Nullable<decimal> MontoTop10 { get; set; }
        public Nullable<decimal> MontoTop20 { get; set; }
        public Nullable<decimal> MontoTotal { get; set; }
        public Nullable<decimal> PorcTop10 { get; set; }
        public Nullable<decimal> PorcTop20 { get; set; }
        public Nullable<int> CantidadAhorrantes { get; set; }
    }
}
