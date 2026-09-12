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
   public partial class XML_Credito_Cuota_Atrasada_Metadata
    {

        [XmlElement("IdDeudor")]
        public string IdDeudor { get; set; }

        [XmlElement("IdOperacionCredito")]
        [XmlElement("IdOperacion")]
        public string IdOperacion { get; set; }

        public int DiasAtraso { get; set; }

        [XmlElement("MontoCuotaAtrasada")]
        public decimal MontoCuotaAtrasada { get; set; }
    }

    [MetadataType(typeof(XML_Credito_Cuota_Atrasada_Metadata))]
    public partial class XML_Credito_Cuota_Atrasada
    {
    }
}  

