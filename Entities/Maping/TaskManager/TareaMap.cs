using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class TareaMap : EntityTypeConfiguration<Tarea>
    {
        public TareaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdSolicitudCambio_Id);
            Property(o => o.IdTipoTarea_Id);

            HasRequired(c => c.IdSolicitudCambio).WithMany(o => o.Tareas).HasForeignKey(o => o.IdSolicitudCambio_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdTipoTarea).WithMany(o => o.Tareas).HasForeignKey(o => o.IdTipoTarea_Id).WillCascadeOnDelete(false);
            
            ToTable("Tarea", "TaskManagement");

        }
    }
}
