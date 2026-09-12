using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Sugef_Info_Contable
    {
        [Key()]
        public Int64 Id { get; set; }
        public String Cuenta { get; set; }
        public Decimal BancosComercialesEstado { get; set; }
        public Decimal BancosLeyesEspeciales { get; set; }
        public Decimal BancosPrivadosCoope { get; set; }
        public Decimal EmpresaFinanNoBancaria { get; set; }
        public Decimal OtrasEntidadesFinancieras { get; set; }
        public Decimal OrganizacionesCooperativas { get; set; }
        public Decimal EntidadesAutorizadasVivienda { get; set; }
        public Decimal Total { get; set; }
        public virtual Sugef_Encabezado IdCarga { get; set; }
    }
}

