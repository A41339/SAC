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
    public partial class XML_Contable_Brecha_Metadata
    {
        [XmlElement("CuentaBrecha")]
        public Int64 CuentaBrecha { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSugef { get; set; }
        [XmlElement("RangoBrecha")]
        public int RangoBrecha { get; set; }
        [XmlElement("MontoBrecha")]
        public Decimal MontoBrecha { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Contable_Brecha_Metadata))]
    public partial class XML_Contable_Brecha
    {
    }
}

