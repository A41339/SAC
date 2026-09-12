using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Credito_Cuota_Atrasada
    {
        [Key()]
        public Int64 Id { get; set; }
        public string IdDeudor { get; set; }
        public string IdOperacion { get; set; }
        public int DiasAtraso { get; set; }
        public decimal MontoCuotaAtrasada { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 