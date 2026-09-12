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
   public partial class XML_Credito_Operacion_DirInd_Metadata
    {
        [XmlElement("IdDeudor")]
        public string IdDeudor { get; set; }
        [XmlElement("IdOperacion")]
        [XmlElement("IdOperacionCredito")]        
        public int IdOperacion { get; set; }
        [XmlElement("TipoCarteraCrediticia")]
        [XmlElement("TipoCartera")]
        public int TipoCartera { get; set; }
        [XmlElement("TipoEstadoOperacionCrediticia")]
        [XmlElement("EstadoOperacionCrediticia")]
        public int EstadoOperacionCrediticia { get; set; }
        [XmlElement("CuentaContablePrincipal")]
        public string CuentaContablePrincipal { get; set; }
        [XmlElement("CuentaContableProductosPorCobrar")]
        [XmlElement("CuentaContableProducto")]
        public string CuentaContableProducto { get; set; }
        [XmlElement("SaldoPrincipalOperacionCrediticia")]
        [XmlElement("SaldoPrincipal")]
        public decimal SaldoPrincipal { get; set; }
        [XmlElement("SaldoProductosPorCobrar")]
        [XmlElement("SaldoProductos")]
        public decimal SaldoProductos { get; set; }
        [XmlElement("SaldoComisionesOperacionesContingentes")]
        [XmlElement("SaldoComisiones")]
        public decimal SaldoComisiones { get; set; }
        [XmlElement("FechaVencimiento")]
        public DateTime FechaVencimiento { get; set; }
        [XmlElement("TasaInteresNominalVigente")]
        public decimal TasaInteresNominalVigente { get; set; }
        [XmlElement("MontoCuotaPrincipalActual")]
        public decimal MontoCuotaPrincipalActual { get; set; }
        [XmlElement("CodigoCategoriaRiesgo")]
        public string CodigoCategoriaRiesgo { get; set; }
        [XmlElement("IndicadorOperacionModificada")]
        public String IndicadorOperacionModificada { get; set; }
        [XmlElement("TipoSegmento")]
        public String TipoSegmento { get; set; }
        [XmlElement("CodigoEtapa")]
        public String CodigoEtapa { get; set; }
        [XmlElement("TipoMonedaOperacion")]
        public String TipoMonedaOperacion { get; set; }
        [XmlElement("MontoDesembolsado")]
        public decimal MontoDesembolsado { get; set; }
        [XmlElement("FechaFormalizacion")]
        public DateTime FechaFormalizacion { get; set; }
        [XmlElement("EAD")]
        public decimal EAD { get; set; }
        [XmlElement("MontoCuotaInteresesActual")]
        public decimal MontoCuotaInteresesActual { get; set; }
        [XmlElement("IndicadorOperacionNueva")]
        public String IndicadorOperacionNueva { get; set; }
        [XmlElement("MontoEstimacionEspecifica")]
        public decimal MontoEstimacionEspecifica { get; set; }
        [XmlElement("MontoFormalizadoOperacionCrediticia")]
        public decimal MontoFormalizadoOperacionCrediticia { get; set; }
    }

    [MetadataType(typeof(XML_Credito_Operacion_DirInd_Metadata))]
    public partial class XML_Credito_Operacion_DirInd
    {
    }
}  

