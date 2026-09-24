namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Consultar_Cantidad_Asociados_Ahorrantes_Result
    {
        public Nullable<System.DateTime> Periodo { get; set; }
        public Nullable<int> AsociadosActivos { get; set; }
        public Nullable<int> AsociadosInactivos { get; set; }
        public Nullable<int> CantidadAhorrantes { get; set; }
    }
}
