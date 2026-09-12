using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;

namespace FGA.Models
{
    public class TipoProyeccion
    {
        [Key()]
        [Required(ErrorMessage = "El campo Id es obligatorio")]
        [DisplayName("Id")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }

        public virtual ICollection<Proyeccion> ListaProyecciones { get; set; }

    }
}
