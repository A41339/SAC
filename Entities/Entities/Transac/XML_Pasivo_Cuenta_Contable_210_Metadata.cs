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
    public partial class XML_Pasivo_Cuenta_Contable_210_Metadata
    {
        [XmlElement("IdAcreedor")]
        public string IdAcreedor { get; set; }
        [XmlElement("IdOperacion")]
        public string IdOperacion { get; set; }
        [XmlElement("TipoMonedaObligacion")]
        public int TipoMonedaObligacion { get; set; }
        [XmlElement("TipoTasa")]
        public string TipoTasa { get; set; }
        [XmlElement("Tasa")]
        public decimal Tasa { get; set; }
        [XmlElement("CuentaContablePrincipal")]
        public string CuentaContablePrincipal { get; set; }
        [XmlElement("SaldoPrincipal")]
        public decimal SaldoPrincipal { get; set; }
        [XmlElement("SaldoProducto")]
        public decimal SaldoProducto { get; set; }
        [XmlElement("FechaFormalizacion")]
        public DateTime FechaFormalizacion { get; set; }
        [XmlElement("FechaVencimiento")]
        public DateTime FechaVencimiento { get; set; }
        [XmlElement("TipoDepositoFGDLEY9816")]
        public int TipoDepositoFGDLEY9816 { get; set; }
    }


    [MetadataType(typeof(XML_Pasivo_Cuenta_Contable_210_Metadata))]
    public partial class XML_Pasivo_Cuenta_Contable_210
    {
    }

}