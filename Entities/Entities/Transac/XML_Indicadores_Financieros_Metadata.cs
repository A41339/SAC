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
    public partial class XML_Indicadores_Financieros_Metadata
    {
        [XmlElement("CuentaCatalogo")]
        public Int64 CuentaCatalogo { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSUGEF { get; set; }
        [XmlElement("Moneda")]
        public int Moneda { get; set; }
        [XmlElement("MontoValor")]
        public Decimal MontoValor { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Indicadores_Financieros_Metadata))]
    public partial class XML_Indicadores_Financieros
    {
    }
}

