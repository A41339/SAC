using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Noticia
    {
        [Key()]
        [DisplayName("Id")]
        public int? Id { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Required(ErrorMessage = "El campo Fecha es obligatorio")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El campo Detalle es obligatorio")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Detalle")]
        public string Detalle { get; set; }

        [StringLength(500, MinimumLength = 0, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Link")]
        public string Link { get; set; }

        [DisplayName("Mostrar fecha")]
        public bool Ind_Mostrar_Fecha { get; set; }

    }
}
