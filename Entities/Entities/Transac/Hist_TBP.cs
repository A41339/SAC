using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Hist_TBP : IHist
    {
        [Key()]
        [DisplayName("Id")]
        public Int64? Id { get; set; }

        [Required(ErrorMessage = "El campo Fecha es obligatorio")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El campo Detalle es obligatorio")]
        [DisplayName("Monto")]
        public Decimal Monto { get; set; }

        [DisplayName("Desviacion")]
        public Decimal Desviacion { get; set; }

        [DisplayName("Fluctuacion")]
        public Decimal Fluctuacion { get; set; }
    }
}
