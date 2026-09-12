using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace FGA.Models
{
    public class InformeMail
    {
        [Key()]
        [DisplayName("Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio")]
        [DisplayName("Periodo")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime Periodo { get; set; }

        [DisplayName("Texto a remitir")]
        public String Texto { get; set; }

        [DisplayName("Destinatarios")]
        public String Destinatarios { get; set; }

        [Required(ErrorMessage = "La fecha de carga es obligatoria")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCarga { get; set; }

        public string Archivo { get; set; }

        public bool Enviado { get; set; }

        public int IdInforme_Id { get; set; }
        public virtual TipoInforme IdInforme { get; set; }

        public string IdEntidad_Id { get; set; }
        public virtual Entidad IdEntidad { get; set; }

        public int IdUsuario_Id { get; set; }
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
