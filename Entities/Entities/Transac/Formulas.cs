using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Formulas
    {
        [Key()]
        public int Id { get; set; }

        [DisplayName("Fórmula")]
        public String Formula { get; set; }

        public String Nombre { get; set; }

        [DisplayName("Porcentual")]
        public bool Ind_Porcentaje { get; set; }

        [DisplayName("Usuario")]
        public virtual Entidad Entidad { get; set; }

        public string Entidad_Id { get; set; }
    }
}
