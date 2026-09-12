using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Indicadores_Financieros
    {
        [Key()]
        public Int64 Id { get; set; }
        public Int64 CuentaCatalogo { get; set; }
        public int TipoCatalogoSUGEF { get; set; }
        public int Moneda { get; set; }
        public Decimal MontoValor { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
}