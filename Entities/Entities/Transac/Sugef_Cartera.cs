using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Sugef_Cartera
    {
        [Key()]
        public Int64 Id { get; set; }
        public String NombreSector { get; set; }
        public String Actividad { get; set; }
        public Decimal AlDia { get; set; }
        public Decimal Rango1_30Dias { get; set; }
        public Decimal Rango31_60Dias { get; set; }
        public Decimal Rango61_90Dias { get; set; }
        public Decimal Rango91_180Dias { get; set; }
        public Decimal Mas180Dias { get; set; }
        public Decimal CobroJudicial { get; set; }
        public Decimal Total { get; set; }

        public virtual Sugef_Encabezado IdCarga { get; set; }
    }
}

