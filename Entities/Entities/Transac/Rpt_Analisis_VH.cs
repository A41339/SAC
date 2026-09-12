using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FGA.Models
{
    public class Rpt_Analisis_VH
    {
        [Key()]
        public int Id { get; set; }

        public string Nombre { get; set; }

    }
}
