using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Entities.Entities.Evaluacion;

namespace FGA.Models
{
    public class Usuario
    {
        [Key()]
        [DisplayName("Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "El campo Identificacion es obligatorio")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Identificación")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])([A-Za-z\d$@$!%*?&]|[^ ]){8,15}$/", ErrorMessage = "La clave debe tener máyusculas, minúsculas")]
        [DisplayName("Contraseña")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "El campo Correo es obligatorio")]
        [DataType(DataType.EmailAddress, ErrorMessage = "El campo Correo debe tener un formato válido.")]
        [DisplayName("Email")]
        public string Correo { get; set; }

        [DisplayName("Puesto")]
        public string Puesto { get; set; }

        [DisplayName("Teléfono")]
        public string Telefono { get; set; }

        public string CambiarClave { get; set; }

        public string OTP { get; set; }

        public int? Role_Usuario_Id { get; set; }

        //[Required(ErrorMessage = "El Rol es obligatorio")]
        [DisplayName("Perfil")]
        public virtual Role Role_Usuario { get; set; }

        public string Entidad_Usuario_Id { get; set; }

        //[Required(ErrorMessage = "La Entidad es obligatoria")]
        [DisplayName("Entidad")]
        public virtual Entidad Entidad_Usuario { get; set; }

        public string Estado_Usuario_Id { get; set; }

        [DisplayName("Estado")]
        public virtual UsuarioEstado Estado_Usuario { get; set; }
        
        [DisplayName("Género")]
        public virtual Sexo Sexo_Usuario { get; set; }

        public int Sexo_Usuario_Id { get; set; }

        public virtual ICollection<Bit_Sessiones> Sessiones { get; set; }
        public virtual ICollection<XML_Encabezado> Encabezados { get; set; }
        public virtual ICollection<Sugef_Encabezado> Sugef_Encabezados { get; set; }
        public virtual ICollection<Proyecto> IdDesarrolladores { get; set; }
        public virtual ICollection<Proyecto> IdAdmins { get; set; }
        public virtual ICollection<Proyecto> IdLideres { get; set; }
        public virtual ICollection<Proyecto> IdReponsablesTI { get; set; }
        public virtual ICollection<SolicitudCambio> IdSolicitantes { get; set; }
        public virtual ICollection<SolicitudCambio> IdAprobadores { get; set; }
        public virtual ICollection<SolicitudCambio> IdResponsablesActual { get; set; }
        public virtual ICollection<InformeMail> Informes { get; set; }
        public virtual ICollection<Notificaciones> Notificaciones { get; set; }
        public virtual ICollection<EvalResponsableCategoria> CategoriasResponsable1 { get; set; }
        public virtual ICollection<EvalResponsableCategoria> CategoriasResponsable2 { get; set; }
        public virtual ICollection<EvalRespuestaUsuario> Respuestas { get; set; }
    }
}
