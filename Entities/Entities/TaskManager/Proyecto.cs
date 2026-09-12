using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public partial class Proyecto
    {
        [Key]
        [Required(ErrorMessage = "El campo id es obligatorio")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Id")]
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [DisplayName("Desarrollador")]
        public int IdDesarrollador_Id { get; set; }
        public virtual Usuario IdDesarrollador { get; set; }

        [DisplayName("Administrador")]
        public int IdAdmin_Id { get; set; }
        public virtual Usuario IdAdmin { get; set; }

        [DisplayName("Validador")]
        public int IdLider_Id { get; set; }
        public virtual Usuario IdLider { get; set; }

        [DisplayName("Responsable")]
        public int IdReponsableTI_Id { get; set; }
        public virtual Usuario IdReponsableTI { get; set; }

        public virtual ICollection<SolicitudCambio> Solicitudes { get; set; }

    }
}
