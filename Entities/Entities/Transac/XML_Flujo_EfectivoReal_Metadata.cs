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
    public partial class XML_Flujo_EfectivoReal_Metadata
    {
        [XmlElement("CuentaFlujoEfectivo")]
        public Int64 CuentaFlujoEfectivo { get; set; }
        [XmlElement("TipoCatalogoSugef")]
        public int TipoCatalogoSugef { get; set; }
        [XmlElement("MontoFlujoEfectivo")]
        public Decimal MontoFlujoEfectivo { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Flujo_EfectivoReal_Metadata))]
    public partial class XML_Flujo_EfectivoReal
    {
    }
}

