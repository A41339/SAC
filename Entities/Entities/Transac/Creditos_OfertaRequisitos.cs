using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Creditos_OfertaRequisitos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Oferta_Id { get; set; }
        [ForeignKey("Oferta_Id")]
        public virtual Creditos_Ofertas Oferta { get; set; }

        [Required]
        public int TipoDocumento_Id { get; set; }
        [ForeignKey("TipoDocumento_Id")]
        public virtual Creditos_TipoDocumento TipoDocumento { get; set; }
    }
}
