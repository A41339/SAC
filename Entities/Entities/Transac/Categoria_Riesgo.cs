using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Categoria_Riesgo
    {
        [Key()]
        public String Codigo { get; set; }
        public String CategoriaRiesgo { get; set; }
        public bool Analisis { get; set; }

    }
}
