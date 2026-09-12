using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{

    public partial class XML_Contable_Estado
    {
        [Key()]
        public Int64 Id { get; set; }
        public string Cuenta { get; set; }
        public decimal Credito { get; set; }
        public decimal Debito { get; set; }
        public int TipoCatalogoSUGEF { get; set; }
        public decimal SaldoFinal { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 