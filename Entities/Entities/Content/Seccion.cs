using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Seccion
    {
        [Key()]
        [DisplayName("Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [StringLength(2000, MinimumLength = 0, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Detalle1 { get; set; }

        [StringLength(2000, MinimumLength = 0, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Detalle2 { get; set; }
        
        [StringLength(2000, MinimumLength = 0, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Detalle3 { get; set; }

        [StringLength(2000, MinimumLength = 0, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Detalle4 { get; set; }

    }
}
