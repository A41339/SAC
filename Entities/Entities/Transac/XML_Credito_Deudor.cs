using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Credito_Deudor
    {
        [Key()]
        public Int64 Id { get; set; }
        public string IdDeudor { get; set; }
        public string CategoriaRiesgo { get; set; }

        [NotMapped]
        public int TipoCategoriaRiesgoSBD { set; get; }

        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 