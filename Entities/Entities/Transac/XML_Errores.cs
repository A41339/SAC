using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace FGA.Models
{
    public partial class XML_Errores
    {
        [Key()]
        public Int64 Id { get; set; }
        public virtual XML_Encabezado IdEncabezado { get; set; }
        public string Detalle { get; set; }        
    }
}  

