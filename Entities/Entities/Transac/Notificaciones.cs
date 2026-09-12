using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class Notificaciones
    {
        [Key()]
        [DisplayName("Código")]
        public int Id { get; set; }

        [DisplayName("Texto a remitir")]
        public String Texto { get; set; }

        [DisplayName("Destinatarios")]
        public String Destinatarios { get; set; }

        [Required(ErrorMessage = "La fecha de carga es obligatoria")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCarga { get; set; }

        public bool Enviado { get; set; }

        public string Asunto { get; set; }

        public string IdEntidadId { get; set; }
        public virtual Entidad IdEntidad { get; set; }

        public int IdUsuarioId { get; set; }
        public virtual Usuario IdUsuario { get; set; }


    }
}

/*
 	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdEntidad_Id] [nvarchar](5) NULL,
	[Texto] [nvarchar](max) NULL,
	[Destinatarios] [nvarchar](max) NULL,
	[FechaCarga] [datetime] NULL,
	[IdUsuario_Id] [int] NULL,
	[Enviado] [bit] NULL,
 */
