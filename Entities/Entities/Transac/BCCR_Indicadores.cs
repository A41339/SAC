using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    [Table("BCCR_Indicadores")]
    public class BCCR_Indicadores
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Id")]
        public long Id { get; set; }

        [DisplayName("Tipo de Indicador")]
        public int? IdIndicador { get; set; }

        [DisplayName("Fecha")]
        public DateTime? Fecha { get; set; }

        [DisplayName("Monto")]
        public decimal? Monto { get; set; }

        // Navegación a la tabla relacionada
        [ForeignKey("IdIndicador")]
        public virtual BCCR_TipoIndicador TipoIndicador { get; set; }
    }
}
