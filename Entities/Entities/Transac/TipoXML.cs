using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class TipoXML
    {
        [Key()]
        [Required(ErrorMessage = "El campo Id es obligatorio")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Código")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }

        [DisplayName("Día Máximo")]
        public int DiaMaximo { get; set; }

        [DisplayName("Prioridad")]
        public int Prioridad { get; set; }

        [DisplayName("IndPrudencial")]
        public bool IndPrudencial { get; set; }

        [DisplayName("IndRiesgos")]
        public bool IndRiesgos { get; set; }

        public virtual ICollection<XML_Encabezado> Encabezados { get; set; }
        
        public virtual ICollection<XML_Excepcion> Excepciones { get; set; }

    }
}
