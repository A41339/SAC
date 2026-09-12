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
   public partial class XML_Contable_Estado_Metadata
    {
        [XmlElement("Cuenta")]
        public string Cuenta { get; set; }
        [XmlElement("Credito")]
        public decimal Credito { get; set; }
        [XmlElement("Debito")]
        public decimal Debito { get; set; }
        [XmlElement("SaldoFinal")]
        public decimal SaldoFinal { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSUGEF { get; set; }
    }

    [MetadataType(typeof(XML_Contable_Estado_Metadata))]
    public partial class XML_Contable_Estado
    {
    }
}  

