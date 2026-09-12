using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public partial class Sugef_Errores
    {
        [Key()]
        public Int64 Id { get; set; }
        public virtual Sugef_Encabezado IdEncabezado { get; set; }
        public string Detalle { get; set; }        
    }
}  

