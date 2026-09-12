using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class ArchivoEstado
    {
        [Key()]
        [Required(ErrorMessage = "El campo Id es obligatorio")]
        [DisplayName("Código")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }

        public virtual ICollection<XML_Encabezado> Encabezados { get; set; }
        public virtual ICollection<Sugef_Encabezado> Sugef_Encabezados { get; set; }
    }
}
