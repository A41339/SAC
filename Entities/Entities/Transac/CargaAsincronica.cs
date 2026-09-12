using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class CargaAsincronica
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Entidad")]
        public string IdEntidad { get; set; }

        public string FilePath { get; set; }

        public int? IndEstado { get; set; }

        public int IdUsuario { get; set; }

        public virtual Entidad Entidad { get; set; }
    }
}
