using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    [Serializable]
    [XmlRoot("Encabezado")]
    public partial class XML_Encabezado_Metadata
    {
        [XmlElement("Periodo")]
        public DateTime Periodo { get; set; }
        [XmlElement("IdEntidad")]
        public string IdEntidad { get; set; }
        [XmlElement("Archivo")]
        public string IdArchivo { get; set; }

    }

    [MetadataType(typeof(XML_Encabezado_Metadata))]
    public partial class XML_Encabezado
    {
    }

}


