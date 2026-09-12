using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_EstadoSolicitud
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public bool Ind_Activo { get; set; }

        public virtual ICollection<Creditos_Solicitudes> Solicitudes { get; set; }
    }
}
