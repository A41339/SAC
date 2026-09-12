using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Entities.Entities.Evaluacion;
using System;

namespace FGA.Models
{
    public class Entidad
    {
        [Key()]
        [Required(ErrorMessage = "El campo Id es obligatorio")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Código")]
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo Identificación es obligatorio")]
        [DisplayName("Identificación")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }
             
        [DisplayName("Correos separados por punto y coma (;)")]
        public string Contacto { get; set; }
        
        [Required]
        [DisplayName("Está Activa")] 
        public bool Activo { get; set; }
       
        [DataType(DataType.ImageUrl, ErrorMessage = "El campo {0} no contiene una dirección válida.")]
        public string Logo { get; set; }

        [DisplayName("Permitir carga")]
        public bool Ind_Cargar { get; set; }

        [DisplayName("Validar archivos")]
        public bool Ind_Validar { get; set; }

        [DisplayName("Evaluación SBR")]
        public bool Ind_Evaluacion { get; set; }

        [DisplayName("Informes")]
        public string CorreoInforme { get; set; }

        public Int64 Perfil_Entidad_Id { get; set; }

        //[Required(ErrorMessage = "La Entidad es obligatoria")]
        [DisplayName("Perfil")]
        public virtual PerfilEntidad Perfil_Entidad { get; set; }

        public virtual ICollection<Bit_Sessiones> Sessiones { get; set; }
        public virtual ICollection<Formulas> IdFormulas { get; set; }
        public virtual ICollection<Usuario> Usuario_Ids { get; set; }
        public virtual ICollection<XML_Encabezado> Encabezados { get; set; }
        public virtual ICollection<XML_Excepcion> Excepciones { get; set; }
        public virtual ICollection<InformeMail> Informes { get; set; }
        public virtual ICollection<Notificaciones> Notificaciones { get; set; }

        public virtual ICollection<EvalResponsableCategoria> CategoriasResponsable { get; set; }
        public virtual ICollection<EvalRespuestaUsuario> Respuestas { get; set; }
    }
}