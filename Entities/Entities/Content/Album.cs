using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;

namespace FGA.Models
{
    public class Album
    {
        [Key()]
        [DisplayName("Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "El campo Autor es obligatorio")]
        [DisplayName("Autor")]
        public string Autor { get; set; }

        [AllowHtml]
        public string Detalle { get; set; }

        [StringLength(2000, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Titulo { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Fecha{ get; set; }
          
        public virtual Tipo_Album Tipo { get; set; }

        [DisplayName("Tipo")]
        public int? TipoAlbum_Id { get; set; }

    }
}
