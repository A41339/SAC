using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class SolicitudCambioMap : EntityTypeConfiguration<SolicitudCambio>
    {
        public SolicitudCambioMap()
        {

            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.ComentariosDesarrollo);
            Property(o => o.ComentariosPase);
            Property(o => o.ComentariosPrueba);
            Property(o => o.Descripcion);
            Property(o => o.DetalleImpacto);
            Property(o => o.EfectoNoImplementacion);
            Property(o => o.FechaDesarrolloFin);
            Property(o => o.FechaDesarrolloInicio);
            Property(o => o.FechaFinPrev);
            Property(o => o.FechaInicioPrev);
            Property(o => o.FechaLimite);
            Property(o => o.FechaPaseFin);
            Property(o => o.FechaPaseInicio);
            Property(o => o.FechaPruebaFin);
            Property(o => o.FechaPruebaInicio);
            Property(o => o.FechaRegistro);
            Property(o => o.IdAprobador_Id);
            Property(o => o.IdEnterno);
            Property(o => o.IdImpacto);
            Property(o => o.IdProyecto_Id);
            Property(o => o.IdResponsableActual_Id);
            Property(o => o.IdSolicitante_Id);
            Property(o => o.IdUrgencia);
            Property(o => o.IdUsuariosAfectados);
            Property(o => o.IndAfectaServicio);
            Property(o => o.IndComplejo);
            Property(o => o.IndEmergencia);
            Property(o => o.IndEstado);
            Property(o => o.IndEstandar);
            Property(o => o.IndMantAdaptativo);
            Property(o => o.IndMantCorrectivo);
            Property(o => o.IndMantenimiento);
            Property(o => o.IndMejoraEstetica);
            Property(o => o.IndMejoraServ);
            Property(o => o.IndNormal);
            Property(o => o.IndNuevaFunc);
            Property(o => o.IndNuevaVersion);
            Property(o => o.IndNuevoServ);
            Property(o => o.IndPaseEntorno);
            Property(o => o.MotivoCambio);
            Property(o => o.ObjetivoCambio);
            Property(o => o.PorcDesarrollo);
            Property(o => o.RiesgoCambio);
            Property(o => o.SeccionesImplicadas);
            Property(o => o.MotivoRechazo);
            Property(o => o.FechaRechazo);

            HasRequired(c => c.IdAprobador).WithMany(o => o.IdAprobadores).HasForeignKey(o => o.IdAprobador_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdProyecto).WithMany(o => o.Solicitudes).HasForeignKey(o => o.IdProyecto_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdResponsableActual).WithMany(o => o.IdResponsablesActual).HasForeignKey(o => o.IdResponsableActual_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdSolicitante).WithMany(o => o.IdSolicitantes).HasForeignKey(o => o.IdSolicitante_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.Seccion).WithMany(o => o.Solicitudes).HasForeignKey(o => o.SeccionesImplicadas).WillCascadeOnDelete(false);

            ToTable("SolicitudCambio", "TaskManagement");
            
        }
    }
}
