using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Creditos_OfertaRequisitos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Oferta_Id { get; set; }
        public virtual Creditos_Ofertas Oferta { get; set; }

        [Required]
        public int TipoDocumento_Id { get; set; }
        public virtual Creditos_TipoDocumento TipoDocumento { get; set; }
    }
}
