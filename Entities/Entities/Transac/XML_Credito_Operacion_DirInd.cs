using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Credito_Operacion_DirInd
    {
        [Key()]
        public Int64 Id { get; set; }
        public string IdDeudor { get; set; }
        public string IdOperacion { get; set; }
        public int TipoCartera { get; set; }
        public int EstadoOperacionCrediticia { get; set; }
        public string CuentaContablePrincipal { get; set; }
        public string CuentaContableProducto { get; set; }
        public decimal SaldoPrincipal { get; set; }
        public decimal SaldoProductos { get; set; }
        public decimal SaldoComisiones { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal TasaInteresNominalVigente { get; set; }
        public decimal MontoCuotaPrincipalActual { get; set; }
        public bool IndNueva { get; set; }
        public string CodigoCategoriaRiesgo { get; set; }
        public String IndicadorOperacionModificada { get; set; }
        public String TipoSegmento { get; set; }
        public String CodigoEtapa { get; set; }
        public String TipoMonedaOperacion { get; set; }
        public decimal MontoDesembolsado { get; set; }
        public DateTime FechaFormalizacion { get; set; }
        public decimal EAD { get; set; }
        public decimal MontoCuotaInteresesActual { get; set; }
        public String IndicadorOperacionNueva { get; set; }
        public decimal MontoEstimacionEspecifica { get; set; }
        public decimal MontoFormalizadoOperacionCrediticia { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 