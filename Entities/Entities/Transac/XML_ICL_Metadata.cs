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
    public partial class XML_ICL_Metadata
    {
        [XmlElement("CuentaCatalogo")]
        public String CuentaCatalogo { get; set; }
        [XmlElement("Moneda")]
        public String Moneda { get; set; }
        [XmlElement("Monto")]
        public decimal Monto { get; set; }
        [XmlElement("Factor")]
        public decimal Factor { get; set; }
        [XmlElement("MontoPonderado")]
        public decimal MontoPonderado { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; } 

    }

    [MetadataType(typeof(XML_ICL_Metadata))]
    public partial class XML_ICL
    {
    }
}  

