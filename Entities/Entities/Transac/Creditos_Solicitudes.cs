using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Creditos_Solicitudes
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Oferta")]
        public int Oferta_Id { get; set; }
        public virtual Creditos_Ofertas Oferta { get; set; }

        [ForeignKey("EntidadSolicitante")]
        public string EntidadSolicitante_Id { get; set; }
        public virtual Entidad EntidadSolicitante { get; set; }

        [ForeignKey("UsuarioSolicitante")]
        public int UsuarioSolicitante_Id { get; set; }
        public virtual Usuario UsuarioSolicitante { get; set; }

        [Required]
        [DisplayName("Monto Solicitado")]
        public decimal Mon_MontoSolicitado { get; set; }

        [Required]
        [DisplayName("Monto Comisión SAC")]
        public decimal Mon_MontoComisionSAC { get; set; }

        [ForeignKey("ComisionConfig")]
        public int? ComisionConfig_Id { get; set; }
        public virtual Creditos_ComisionConfig ComisionConfig { get; set; }

        [StringLength(500)]
        public string Justificacion { get; set; }

        [Required]
        [StringLength(20)]
        public string TelefonoNotificacionSMS { get; set; }

        [Required]
        [StringLength(100)]
        public string EmailNotificacion { get; set; }

        [Required]
        public DateTime Fec_Solicitud { get; set; }

        [ForeignKey("EstadoSolicitud")]
        public int EstadoSolicitud_Id { get; set; }
        public virtual Creditos_EstadoSolicitud EstadoSolicitud { get; set; }

        public DateTime? Fec_Resolucion { get; set; }
        [ForeignKey("UsuarioResolucion")]
        public int? UsuarioResolucion_Id { get; set; }
        public virtual Usuario UsuarioResolucion { get; set; }

        [StringLength(500)]
        public string MotivoRechazo { get; set; }

        public DateTime? Fec_Desembolso { get; set; }
        
        [StringLength(100)]
        public string ReferenciaBancaria { get; set; }

        [StringLength(500)]
        public string ComentariosDesembolso { get; set; }

        public virtual ICollection<Creditos_Documentos> Documentos { get; set; }
        public virtual ICollection<Creditos_SMSLog> SMSLogs { get; set; }
    }
}
