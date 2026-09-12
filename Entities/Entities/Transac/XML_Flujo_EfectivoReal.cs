using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Flujo_EfectivoReal
    {
        [Key()]
        public Int64 Id { get; set; }
        public Int64 CuentaFlujoEfectivo { get; set; }
        public int TipoCatalogoSugef { get; set; }
        public Decimal MontoFlujoEfectivo { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 