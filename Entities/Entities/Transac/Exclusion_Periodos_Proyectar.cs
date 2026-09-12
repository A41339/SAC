using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class Exclusion_Periodos_Proyectar
    {
        [Key()]
        public int Id { get; set; }
        
        public string IdEntidad { get; set; }

        public string Cuenta { get; set; }

        public DateTime Periodo { get; set; }

    }
}
