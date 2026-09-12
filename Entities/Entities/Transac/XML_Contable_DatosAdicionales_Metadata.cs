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
    public partial class XML_Contable_DatosAdicionales_Metadata
    {
        [XmlElement("CuentaCatalogo")]
        public Int64 CuentaCatalogo { get; set; }
        [XmlElement("TipoCatalogoSUGEF")]
        public int TipoCatalogoSugef { get; set; }
        [XmlElement("TipoMonedaDato")]
        public int TipoMonedaDato { get; set; }
        [XmlElement("MontoDatoAdicional")]
        public Decimal MontoDatoAdicional { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }

    [MetadataType(typeof(XML_Contable_DatosAdicionales_Metadata))]
    public partial class XML_Contable_DatosAdicionales
    {
    }
}

