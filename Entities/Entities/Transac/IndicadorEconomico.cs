using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FGA.Models
{
    public class IndicadorEconomico
    {
       public DateTime fecha { get; set; }
        public Decimal monto { get; set; }
    }
}
