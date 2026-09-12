using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{
    public partial class XML_Pasivo_Cuenta_Contable_210
    {
        [Key()]
        public Int64 Id { get; set; }
        public string IdOperacion { get; set; }
        public string IdAcreedor { get; set; }
        public int TipoMonedaObligacion { get; set; }
        public string TipoTasa { get; set; }
        public decimal Tasa { get; set; }
        public string CuentaContablePrincipal { get; set; }
        public decimal SaldoPrincipal { get; set; }
        public decimal SaldoProducto { get; set; }
        public DateTime FechaFormalizacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Condicion { get; set; }
        public string Tipo { get; set; }
        public int? TipoDepositoFGDLEY9816 { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 