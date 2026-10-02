namespace Entities.Entities.Procedures
{
    using System;

    public partial class FGA_Obtener_Alertas_Financieras_Result
    {
        public int Id { get; set; }
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public System.DateTime PeriodoActual { get; set; }
        public System.DateTime PeriodoAnterior { get; set; }
        public string Cuenta { get; set; }
        public string NombreCuenta { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal VariacionMonto { get; set; }
        public decimal VariacionPorcentaje { get; set; }
        public string TipoAlerta { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public System.DateTime FechaGeneracion { get; set; }
        public bool Leido { get; set; }
        public Nullable<System.DateTime> FechaLeido { get; set; }
        public string UsuarioLeido { get; set; }
        public string Estado { get; set; }
    }
}
