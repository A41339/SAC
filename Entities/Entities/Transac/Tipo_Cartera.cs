using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Tipo_Cartera
    {
        [Key()]
        public String Codigo { get; set; }
        public int TipoCartera { get; set; }
        public String Nombre { get; set; }

    }
}
