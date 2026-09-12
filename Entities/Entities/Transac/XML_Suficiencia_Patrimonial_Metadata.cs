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
    public partial class XML_Suficiencia_Patrimonial_Metadata
    {
        [XmlElement("CuentaCatalogo")]
        public Int64 CuentaCatalogo { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSugef { get; set; }
        [XmlElement("Moneda")]
        public int Moneda { get; set; }
        [XmlElement("Monto")]
        public Decimal Monto { get; set; }
        [XmlElement("Ponderacion")]
        public Decimal Ponderacion { get; set; }
        [XmlElement("GradualidadPonderacion")]
        public Decimal GradualidadPonderacion { get; set; }
        [XmlElement("MontoPonderado")]
        public Decimal MontoPonderado { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Suficiencia_Patrimonial_Metadata))]
    public partial class XML_Suficiencia_Patrimonial
    {
    }
}

