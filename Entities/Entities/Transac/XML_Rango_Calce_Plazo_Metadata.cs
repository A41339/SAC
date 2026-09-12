using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{
    [Serializable]
    [XmlRoot("Registro")]
    public partial class XML_Rango_Calce_Plazo_Metadata
    {
        [XmlElement("CuentaCalcePlazo")]
        public Int64 CuentaCalcePlazo { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSugef { get; set; }
        [XmlElement("RangoCalcePlazo")]
        public int RangoCalcePlazo { get; set; }
        [XmlElement("MontoCalcePlazo")]
        public Decimal MontoCalcePlazo { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Rango_Calce_Plazo_Metadata))]
    public partial class XML_Rango_Calce_Plazo
    {
    }
}

