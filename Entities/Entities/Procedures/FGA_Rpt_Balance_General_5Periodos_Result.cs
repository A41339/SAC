namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Rpt_Balance_General_5Periodos_Result
    {
        public Nullable<int> Orden { get; set; }
        public string Seccion { get; set; }
        public string Concepto { get; set; }
        public Nullable<int> Nivel { get; set; }
        public Nullable<bool> EsNegrita { get; set; }
        public string CodigoContable { get; set; }
        public Nullable<decimal> Periodo1 { get; set; }
        public Nullable<decimal> Periodo2 { get; set; }
        public Nullable<decimal> Periodo3 { get; set; }
        public Nullable<decimal> Periodo4 { get; set; }
        public Nullable<decimal> Periodo5 { get; set; }
        public Nullable<decimal> VariacionAbsoluta { get; set; }
        public Nullable<decimal> VariacionRelativa { get; set; }
        public Nullable<System.DateTime> FechaP1 { get; set; }
        public Nullable<System.DateTime> FechaP2 { get; set; }
        public Nullable<System.DateTime> FechaP3 { get; set; }
        public Nullable<System.DateTime> FechaP4 { get; set; }
        public Nullable<System.DateTime> FechaP5 { get; set; }
    }
}
