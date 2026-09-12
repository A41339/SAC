using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_ComisionConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Porcentaje Comisión")]
        public decimal PorcentajeComision { get; set; }

        [Required]
        [DisplayName("Vigencia Desde")]
        public DateTime Fec_VigenciaDesde { get; set; }

        [DisplayName("Vigencia Hasta")]
        public DateTime? Fec_VigenciaHasta { get; set; }

        public bool Ind_Activa { get; set; }

        public int UsuarioActualiza_Id { get; set; }
        
        public virtual Usuario UsuarioActualiza { get; set; }
    }
}
