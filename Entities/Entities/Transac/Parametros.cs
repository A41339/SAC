using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Parametros
    {
        [Key()]
        [DisplayName("Id")]
        public int Id { get; set; }
        public string Llave { get; set; }
        public string Valor { get; set; }
        public string Descripcion { get; set; }
        
    }
}
