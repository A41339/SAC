using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Creditos_TipoDocumento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public bool Ind_Activo { get; set; }

        public virtual ICollection<Creditos_OfertaRequisitos> OfertaRequisitos { get; set; }
        public virtual ICollection<Creditos_Documentos> DocumentosRecibidos { get; set; }
    }
}
