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
    public partial class XML_Inversion_Activa_Metadata
    {
        [XmlElement("IdEmisor")]
        public String IdEmisor { get; set; }
        [XmlElement("IdInstrumento")]
        public String IdInstrumento { get; set; }
        [XmlElement("ValorFacial")]
        public decimal ValorFacial { get; set; }
        [XmlElement("ValorTransado")]
        public decimal ValorTransado { get; set; }
        [XmlElement("ValorMercado")]
        public decimal ValorMercado { get; set; }
        [XmlElement("FechaVencimiento")]
        public DateTime FechaVencimiento { get; set; }
        [XmlElement("CuentaContablePrincipal")]
        public string CuentaContablePrincipal { get; set; }
        [XmlElement("SaldoPrincipal")]
        public decimal SaldoPrincipal { get; set; }
    }

    [MetadataType(typeof(XML_Inversion_Activa_Metadata))]
    public partial class XML_Inversion_Activa
    {
    }

}