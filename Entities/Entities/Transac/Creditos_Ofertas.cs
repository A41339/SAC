using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_Ofertas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EntidadOferente_Id { get; set; }
        
        [ForeignKey("EntidadOferente_Id")]
        public virtual Entidad EntidadOferente { get; set; }

        [Required]
        public int UsuarioPublicador_Id { get; set; }
        
        [ForeignKey("UsuarioPublicador_Id")]
        public virtual Usuario UsuarioPublicador { get; set; }

        [Required]
        [DisplayName("Monto")]
        public decimal Mon_Monto { get; set; }

        [Required]
        [DisplayName("Moneda")]
        public int Moneda_Id { get; set; }
        [ForeignKey("Moneda_Id")]
        public virtual Creditos_Moneda Moneda { get; set; }

        [Required]
        [DisplayName("Tasa Interés Anual")]
        public decimal TasaInteresAnual { get; set; }

        [Required]
        [DisplayName("Plazo (Meses)")]
        public int PlazoMeses { get; set; }

        [StringLength(500)]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [StringLength(20)]
        [DisplayName("Teléfono SMS")]
        public string TelefonoNotificacionSMS { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Correo Notificación")]
        public string EmailNotificacion { get; set; }

        [Required]
        [DisplayName("Fecha Publicación")]
        public DateTime Fec_Publicacion { get; set; }

        [DisplayName("Fecha Vencimiento")]
        public DateTime? Fec_Vencimiento { get; set; }

        public bool Ind_EstadoActiva { get; set; }

        public DateTime? Fec_UltimaModificacion { get; set; }
        public int? UsuarioModifica_Id { get; set; }
        [ForeignKey("UsuarioModifica_Id")]
        public virtual Usuario UsuarioModifica { get; set; }

        public virtual ICollection<Creditos_Solicitudes> Solicitudes { get; set; }
        public virtual ICollection<Creditos_OfertaRequisitos> Requisitos { get; set; }
    }
}
