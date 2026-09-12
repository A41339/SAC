using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class Proy_EEFF
    {
        [Key()]
        public int Id { get; set; }
        public string IdEntidad { get; set; }
        public DateTime PeriodoCorte { get; set; }
        public String TipoPlazo { get; set; }
        public String TipoTasa { get; set; }
        public String TipoCrecimiento { get; set; }
        
        public decimal TasaNuevaColocacion { get; set; }
        public decimal TasaCarteraVigente { get; set; }
        public decimal TasaCarteraVencida { get; set; }
        public decimal TasaInversiones { get; set; }
        public decimal TasaCaptaciones { get; set; }
        public decimal TasaObligaciones { get; set; }
        public decimal ReservasVoluntarias { get; set; }
        public decimal RecuperacionIncobrable { get; set; }


        public decimal CrecimientoCartera { get; set; }
        public decimal CrecimientoCaptaciones { get; set; }
        public decimal CrecimientoCapital { get; set; }
        public decimal GastoAdmin { get; set; }
        public decimal GastoEstimac { get; set; }

    }
}
