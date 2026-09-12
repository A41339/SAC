using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Sexo
    {
        [Key()]
        [DisplayName("Id")] 
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }

        public virtual ICollection<Usuario> Usuario_Ids { get; set; }
    }
}
