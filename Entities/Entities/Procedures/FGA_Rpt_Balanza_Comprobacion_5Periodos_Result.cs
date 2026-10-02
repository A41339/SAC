namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Rpt_Balanza_Comprobacion_5Periodos_Result
    {
        public Nullable<int> Orden { get; set; }
        public string Cuenta { get; set; }
        public string Nombre { get; set; }
        public Nullable<int> Nivel { get; set; }
        public string Padre { get; set; }
        public Nullable<decimal> P1 { get; set; }
        public Nullable<decimal> P2 { get; set; }
        public Nullable<decimal> P3 { get; set; }
        public Nullable<decimal> P4 { get; set; }
        public Nullable<decimal> P5 { get; set; }
        public Nullable<decimal> VariacionAbsoluta { get; set; }
        public Nullable<decimal> VariacionRelativa { get; set; }
    }
}
