using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    [Table("BCCR_TipoIndicador")]
    public class BCCR_TipoIndicador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Código")]
        public int Id { get; set; }

        [StringLength(50)]
        [DisplayName("Nombre del Indicador")]
        public string Nombre { get; set; }

        [StringLength(1)]
        [DisplayName("¿Es porcentaje?")]
        public string Ind_Porcentaje { get; set; }
    }
}
