using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class Bit_Sessiones
    {
        [Key()]
        [DisplayName("Id")]
        public Int64? Id { get; set; }

        public DateTime Fecha { get; set; }

        public String IP { get; set; }

        public int? IdRole { get; set; }

        //[Required(ErrorMessage = "El Rol es obligatorio")]
        [DisplayName("Perfil")]
        public virtual Role Role_Bit { get; set; }

        public string IdEntidad { get; set; }

        //[Required(ErrorMessage = "La Entidad es obligatoria")]
        [DisplayName("Entidad")]
        public virtual Entidad Entidad_Bit { get; set; }

        public int IdUsuario { get; set; }

        [DisplayName("Usuario")]
        public virtual Usuario Usuario_Bit { get; set; }


    }
}
