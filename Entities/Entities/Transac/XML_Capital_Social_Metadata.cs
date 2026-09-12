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
   public partial class XML_Capital_Social_Metadata
    {
        [XmlElement("Contrato")]
        public String Contrato { get; set; }
        [XmlElement("Des_Identificacion")]
        public String Des_Identificacion { get; set; }
        [XmlElement("SaldoReal")]
        public decimal SaldoReal { get; set; }
        [XmlElement("Fechainclusion")]
        public DateTime Fechainclusion { get; set; }
    }

    [MetadataType(typeof(XML_Capital_Social_Metadata))]
    public partial class XML_Capital_Social
    {
    }
}  

