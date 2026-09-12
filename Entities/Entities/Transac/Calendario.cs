using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class Calendario
    {
        [Key()]
         public int Id { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime Periodo { get; set; }
        
        [Required(ErrorMessage = "El día de notificación es obligatorio")]
        [DisplayName("Día que se debe enviar la notificación")] 
        public int DiaNotificacion { get; set; }

        [Required(ErrorMessage = "El día límite es obligatorio")]
        [DisplayName("Día límite para la carga")]
        public int DiaLimite { get; set; }

        [DisplayName("Enviado")]
        public bool Enviado { get; set; }

        [DisplayName("Ind_Vencido")]
        public bool Ind_Vencido { get; set; }

    }
}
