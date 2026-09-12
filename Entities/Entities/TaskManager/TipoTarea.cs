using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public partial class TipoTarea
    {
        [Key]
        [DisplayName("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }
               
        public virtual ICollection<Tarea> Tareas { get; set; }

    }
}
