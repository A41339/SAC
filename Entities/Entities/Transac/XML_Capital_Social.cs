using System;
using System.ComponentModel.DataAnnotations;


namespace FGA.Models
{

    public partial class XML_Capital_Social
    {
        [Key()]
        public Int64 Id { get; set; }
        public String Contrato { get; set; }
        public String Des_Identificacion { get; set; }
        public decimal SaldoReal { get; set; }     
        public DateTime Fechainclusion { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
    }
} 