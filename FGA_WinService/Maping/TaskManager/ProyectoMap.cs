using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class ProyectoMap : EntityTypeConfiguration<Proyecto>
    {
        public ProyectoMap()
        {

            HasKey(o => o.Id);
            Property(o => o.IdAdmin_Id);
            Property(o => o.IdDesarrollador_Id);
            Property(o => o.IdLider_Id);
            Property(o => o.IdReponsableTI_Id);
            Property(o => o.Nombre);
                       
            HasRequired(c => c.IdAdmin).WithMany(o => o.IdAdmins).HasForeignKey(o => o.IdAdmin_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdDesarrollador).WithMany(o => o.IdDesarrolladores).HasForeignKey(o => o.IdDesarrollador_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdLider).WithMany(o => o.IdLideres).HasForeignKey(o => o.IdLider_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdReponsableTI).WithMany(o => o.IdReponsablesTI).HasForeignKey(o => o.IdReponsableTI_Id).WillCascadeOnDelete(false);

            ToTable("Proyecto", "TaskManagement");
            
        }
    }
}
