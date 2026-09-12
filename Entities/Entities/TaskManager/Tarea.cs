using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public partial class Tarea
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Detalle")]
        public string Detalle { get; set; }

        [DisplayName("Solicitud")]
        public int IdSolicitudCambio_Id { get; set; }
        public virtual SolicitudCambio IdSolicitudCambio { get; set; }

        [DisplayName("Tipo")]
        public int IdTipoTarea_Id { get; set; }
        public virtual TipoTarea IdTipoTarea { get; set; }
               

    }
}
