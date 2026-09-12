using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Rpt_EstructuraCamel
    {

        [Key()]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string CuentaContable { get; set; }

        public int IdArchivo_Id { get; set; }
       
        public string CuentaCompleta { get; set; }

        public string Ind_Publico { get; set; }

        public string Ind_Porcentaje { get; set; }

        public int Rango { get; set; }

    }
}
