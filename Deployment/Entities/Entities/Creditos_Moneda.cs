using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Creditos_Moneda
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public bool Ind_Activo { get; set; }

        public virtual ICollection<Creditos_Ofertas> Ofertas { get; set; }
    }
}
