using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class NotificacionesMap : EntityTypeConfiguration<Notificaciones>
    {
        public NotificacionesMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Destinatarios);
            Property(o => o.FechaCarga);
            Property(o => o.IdEntidadId);
            Property(o => o.IdUsuarioId);
            Property(o => o.Texto);
            Property(o => o.Enviado);
            Property(o => o.Asunto);

            HasRequired(c => c.IdUsuario).WithMany(o => o.Notificaciones);
            HasRequired(c => c.IdEntidad).WithMany(o => o.Notificaciones);
            ToTable("Notificaciones");            
        }
    }
}
