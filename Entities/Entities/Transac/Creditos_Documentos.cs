using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_Documentos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Solicitud_Id { get; set; }
        [ForeignKey("Solicitud_Id")]
        public virtual Creditos_Solicitudes Solicitud { get; set; }

        [Required]
        public int TipoDocumento_Id { get; set; }
        [ForeignKey("TipoDocumento_Id")]
        public virtual Creditos_TipoDocumento TipoDocumento { get; set; }

        [Required]
        [StringLength(500)]
        public string RutaArchivoPDF { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivoOriginal { get; set; }

        [Required]
        public DateTime Fec_Carga { get; set; }

        [Required]
        public int UsuarioCarga_Id { get; set; }
        [ForeignKey("UsuarioCarga_Id")]
        public virtual Usuario UsuarioCarga { get; set; }

        public bool Ind_FirmaValida { get; set; }

        public int? ValidadoPorSAC_Id { get; set; }
        [ForeignKey("ValidadoPorSAC_Id")]
        public virtual Usuario ValidadoPorSAC { get; set; }

        public DateTime? Fec_ValidacionSAC { get; set; }

        [StringLength(500)]
        public string ComentariosRevision { get; set; }
    }
}
