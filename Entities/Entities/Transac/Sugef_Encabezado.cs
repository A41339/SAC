using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{

    public partial class Sugef_Encabezado
    {
        [Key()]
        public Int64 Id { get; set; }
        public DateTime FechaCarga { get; set; }

        public int IdUsuario_Id { get; set; }
        public virtual Usuario IdUsuario { get; set; }
              
        public int IdEstado_Id { get; set; }
        public virtual ArchivoEstado IdEstado { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime Periodo { get; set; }
        [Display(Name ="Registros")]
        public int Cantidad { get; set; }

        public string Nombre { get; set; }

    }
}  

