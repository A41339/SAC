using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;

namespace FGA.Models
{
    public partial class SolicitudCambio
    {
        [Key]
        public int Id { get; set; }
        
        [DisplayName("De emergencia")]
        public bool IndEmergencia { get; set; }

        [DisplayName("Estandar")]
        public bool IndEstandar { get; set; }

        [DisplayName("Normal")]
        public bool IndNormal { get; set; }

        [DisplayName("Complejo")]
        public bool IndComplejo { get; set; }

        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public string IdProyecto_Id { get; set; }
        public virtual Proyecto IdProyecto { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Fecha registro")]
        public Nullable<System.DateTime> FechaRegistro { get; set; }

        [DisplayName("Urgencia")]
        public Nullable<int> IdUrgencia { get; set; }

        [DisplayName("Impacto")]
        public Nullable<int> IdImpacto { get; set; }

        [DisplayName("Entorno")]
        public Nullable<int> IdEnterno { get; set; }

        public virtual Menu Seccion { get; set; }

        [DisplayName("Secciones")]
        public Nullable<int> SeccionesImplicadas { get; set; }

        [DisplayName("Afecta el nivel de servicio")]
        public bool IndAfectaServicio { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Fecha Limite")]
        public Nullable<System.DateTime> FechaLimite { get; set; }

        [DisplayName("# afectados")]
        public Nullable<int> IdUsuariosAfectados { get; set; }

        [DisplayName("Motivo del cambio")]
        public string MotivoCambio { get; set; }

        [DisplayName("Objetivo del cambio")]
        public string ObjetivoCambio { get; set; }

        [DisplayName("Detalles del impacto")]
        public string DetalleImpacto { get; set; }

        [DisplayName("Riesgos del cambio")]
        public string RiesgoCambio { get; set; }

        [DisplayName("Efectos de la no implementación")]
        public string EfectoNoImplementacion { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Inicio previsto")]
        public Nullable<System.DateTime> FechaInicioPrev { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Finalización prevista")]
        public Nullable<System.DateTime> FechaFinPrev { get; set; }

        [DisplayName("Nueva funcionalidad")]
        public bool IndNuevaFunc { get; set; }

        [DisplayName("Nueva versión")]
        public bool IndNuevaVersion { get; set; }

        [DisplayName("Nuevo servicio")]
        public bool IndNuevoServ { get; set; }

        [DisplayName("Paso de un entorno a otro")]
        public bool IndPaseEntorno { get; set; }

        [DisplayName("Mantenimiento")]
        public bool IndMantenimiento { get; set; }

        [DisplayName("Mejora de servicio")]
        public bool IndMejoraServ { get; set; }

        [DisplayName("Mejora estética")]
        public bool IndMejoraEstetica { get; set; }

        [DisplayName("Mantenimiento adaptativo")]
        public bool IndMantAdaptativo { get; set; }

        [DisplayName("Mantenimiento correctivo")]
        public bool IndMantCorrectivo { get; set; }

        [DisplayName("Estado")]
        public Nullable<int> IndEstado { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Inicio previsto")]
        public Nullable<System.DateTime> FechaDesarrolloInicio { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Finalización previsto")]
        public Nullable<System.DateTime> FechaDesarrolloFin { get; set; }
        
        [DisplayName("Comentarios")]
        public string ComentariosDesarrollo { get; set; }

        [DisplayName("Avance")]
        public Nullable<int> PorcDesarrollo { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Inicio previsto")]
        public Nullable<System.DateTime> FechaPruebaInicio { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Finalización previsto")]
        public Nullable<System.DateTime> FechaPruebaFin { get; set; }
        
        [DisplayName("Comentarios")]
        public string ComentariosPrueba { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Inicio previsto")]
        public Nullable<System.DateTime> FechaPaseInicio { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Finalización previsto")]
        public Nullable<System.DateTime> FechaPaseFin { get; set; }
        
        [DisplayName("Comentarios")]
        public string ComentariosPase { get; set; }

        [DisplayName("Responsable")]
        public int? IdResponsableActual_Id { get; set; }

        public virtual Usuario IdResponsableActual { get; set; }

        public virtual Usuario IdSolicitante { get; set; }

        [DisplayName("Solicitante")]
        public int? IdSolicitante_Id { get; set; }

        public virtual Usuario IdAprobador { get; set; }

        [DisplayName("Aprobador")]
        public int? IdAprobador_Id { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd MMMM yy}", ApplyFormatInEditMode = true)]
        [DisplayName("Fecha de rechazo")]
        public Nullable<System.DateTime> FechaRechazo { get; set; }

        [DisplayName("Motivo de rechazo")]
        public string MotivoRechazo { get; set; }

        public virtual ICollection<Tarea> Tareas { get; set; }

    }
}
