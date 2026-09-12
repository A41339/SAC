using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Rango_Calce_Plazo
    {
        [Key()]
        public Int64 Id { get; set; }
        public Int64 CuentaCalcePlazo { get; set; }
        public int TipoCatalogoSugef { get; set; }
        public int RangoCalcePlazo { get; set; }
        public Decimal MontoCalcePlazo { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 