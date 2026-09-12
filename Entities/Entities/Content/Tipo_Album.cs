using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Tipo_Album
    {
        [Key()]
        [DisplayName("Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        public virtual ICollection<Album> Album_Ids { get; set; }
    }
}
