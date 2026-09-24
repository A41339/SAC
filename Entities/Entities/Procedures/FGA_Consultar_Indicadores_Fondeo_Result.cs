namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Consultar_Indicadores_Fondeo_Result
    {
        public Nullable<int> Orden { get; set; }
        public string Nombre { get; set; }
        public Nullable<decimal> ValorP1 { get; set; }
        public Nullable<decimal> ValorP2 { get; set; }
        public Nullable<bool> EsPorcentaje { get; set; }
    }
}
