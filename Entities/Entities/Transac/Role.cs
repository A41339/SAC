using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Role
    {
        [Key()]
        [DisplayName("Id")] 
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [DisplayName("Nombre")] 
        public string Nombre { get; set; }

        public string Url { get; set; }

        [Required(ErrorMessage = "El campo Activo es obligatorio")]
        [DisplayName("Activo")]
        public bool Activo { get; set; }

        public bool EsEntidad { get; set; }

        public virtual ICollection<MenuPermission> MenuPermission_RoleIds { get; set; }

        public virtual ICollection<Usuario> Usuario_Ids { get; set; }

        public virtual ICollection<Bit_Sessiones> Sessiones { get; set; }
    }
}
