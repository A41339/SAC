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
   public partial class XML_Credito_Deudor_Metadata
    {

        [XmlElement("IdDeudor")]
        public string IdDeudor { get; set; }

        [XmlElement("TipoComportamientoPago")]
        [XmlElement("CategoriaRiesgo")]
        public string CategoriaRiesgo { get; set; }

        [XmlElement("TipoCategoriaRiesgoSBD")]
        public string TipoCategoriaRiesgoSBD { get; set; }
        
    }

    [MetadataType(typeof(XML_Credito_Deudor_Metadata))]
    public partial class XML_Credito_Deudor
    {
    }
}  

