using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_SMSLog
    {
        [Key]
        public int Id { get; set; }

        public int? Solicitud_Id { get; set; }
        [ForeignKey("Solicitud_Id")]
        public virtual Creditos_Solicitudes Solicitud { get; set; }

        [Required]
        [StringLength(20)]
        [DisplayName("Teléfono Destino")]
        public string TelefonoDestino { get; set; }

        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; }

        [Required]
        public DateTime Fec_Envio { get; set; }

        public bool Ind_Exitoso { get; set; }

        public string RespuestaProveedor { get; set; }
    }
}
