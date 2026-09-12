using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGA.Models
{
    public class File
    {
        public String Archivo { get; set; }
        public String Codigo { get; set; }
        public DateTime Periodo { get; set; }
        public String Entidad { get; set; }
        public String Ruta { get; set; }
        public String Error { get; set; }
    }
}
