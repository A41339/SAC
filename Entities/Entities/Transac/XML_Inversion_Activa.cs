using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Inversion_Activa
    {
        [Key()]
        public Int64 Id { get; set; }
        public String IdEmisor { get; set; }
        public String IdInstrumento { get; set; }
        public decimal ValorFacial { get; set; }
        public decimal ValorTransado { get; set; }
        public decimal ValorMercado { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string CuentaContablePrincipal { get; set; }
        public decimal SaldoPrincipal { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 