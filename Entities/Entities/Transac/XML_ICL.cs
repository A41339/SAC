using System;
using System.ComponentModel.DataAnnotations;


namespace FGA.Models
{

    public partial class XML_ICL
    {
        [Key()]
        public Int64 Id { get; set; }
        public String CuentaCatalogo { get; set; }
        public String Moneda { get; set; }
        public decimal Monto { get; set; }
        public decimal Factor { get; set; }
        public decimal MontoPonderado { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 