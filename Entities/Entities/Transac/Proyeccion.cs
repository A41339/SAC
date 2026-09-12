using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Proyeccion
    {
        [Key()]
        [DisplayName("Id")]
        public int Id { get; set; }
        public virtual TipoProyeccion TipoProyeccion_Id { get; set; }
        public int IdProyeccion { get; set; }
        public string IdEntidad { get; set; }
        public DateTime Periodo { get; set; }
        public string Cuenta { get; set; }
        public int PeriodosProyectar { get; set; }
        public string Valores { get; set; }


    }
}
