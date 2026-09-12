using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Bit_SessionesMap : EntityTypeConfiguration<Bit_Sessiones>
    {
        public Bit_SessionesMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Fecha);
            Property(o => o.IP);
            Property(o => o.IdEntidad);
            Property(o => o.IdRole);
            Property(o => o.IdUsuario);

            HasRequired(c => c.Usuario_Bit).WithMany(o => o.Sessiones).HasForeignKey(o => o.IdUsuario).WillCascadeOnDelete(false);
            HasRequired(c => c.Entidad_Bit).WithMany(o => o.Sessiones).HasForeignKey(o => o.IdEntidad).WillCascadeOnDelete(false);
            HasRequired(c => c.Role_Bit).WithMany(o => o.Sessiones).HasForeignKey(o => o.IdRole).WillCascadeOnDelete(false);

            ToTable("Bit_Sessiones");            
        }
    }
}
