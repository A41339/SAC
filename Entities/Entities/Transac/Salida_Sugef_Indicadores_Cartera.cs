using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Salida_Sugef_Indicadores_Cartera
    {
        [Key()]
        public int Id { get; set; }

        public int IdIndicador { get; set; }
        public Decimal BancoComercialEstado { get; set; }

        public Decimal BancoLeyesEspeciales { get; set; }
        public Decimal BancosPrivadosCoope { get; set; }
        public Decimal EmpresaFinanNoBancaria { get; set; }      
        public Decimal OrganizacionesCooperativas { get; set; }
        public Decimal EntidadesAutorizadasVivienda { get; set; }
        public Decimal OtrasEntidadesFinancieras { get; set; }
        public Decimal Total { get; set; }


        public virtual Sugef_Encabezado IdCarga { get; set; }
    }
}

