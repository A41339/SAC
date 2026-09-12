using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class Log
    {
        [Key()]
        [DisplayName("Id")]
        public int Id { get; set; }

        [DisplayName("Controlador")]
        public string Controller { get; set; }

        [DisplayName("Action")]
        public string Action { get; set; }

        [DisplayName("Mensaje")]
        public string Mensaje { get; set; }

        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [DisplayName("IdUsuario_Id")]
        public int IdUsuario_Id { get; set; }

    }
}
