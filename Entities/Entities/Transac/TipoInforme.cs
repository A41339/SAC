using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class TipoInforme
    {
        [Key()]
         public int Id { get; set; }

        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [DisplayName("Estado")]
        public Boolean Estado { get; set; }

        [DisplayName("Texto")]
        public string Texto { get; set; }

        public virtual ICollection<InformeMail> Informes { get; set; }
    }
}
