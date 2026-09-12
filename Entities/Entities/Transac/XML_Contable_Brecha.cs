using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Contable_Brecha
    {
        [Key()]
        public Int64 Id { get; set; }
        public Int64 CuentaBrecha { get; set; }
        public int TipoCatalogoSugef { get; set; }
        public int RangoBrecha { get; set; }     
        public Decimal MontoBrecha { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 