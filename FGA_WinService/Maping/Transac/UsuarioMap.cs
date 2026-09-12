using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class UsuarioMap : EntityTypeConfiguration<Usuario>
    {
        public UsuarioMap()
        {

            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Correo).HasMaxLength(100);
            Property(o => o.Contrasena).HasMaxLength(100);
            Property(o => o.Nombre).HasMaxLength(100);
            Property(o => o.Identificacion);
            Property(o => o.Entidad_Usuario_Id);
            Property(o => o.Estado_Usuario_Id);
            Property(o => o.Role_Usuario_Id);
            Property(o => o.Sexo_Usuario_Id);

            HasRequired(c => c.Role_Usuario).WithMany(o => o.Usuario_Ids).HasForeignKey(o => o.Role_Usuario_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.Entidad_Usuario).WithMany(o => o.Usuario_Ids).HasForeignKey(o => o.Entidad_Usuario_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.Estado_Usuario).WithMany(o => o.Usuario_Ids).HasForeignKey(o => o.Estado_Usuario_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.Sexo_Usuario).WithMany(o => o.Usuario_Ids).HasForeignKey(o => o.Sexo_Usuario_Id).WillCascadeOnDelete(false);

            ToTable("Usuario");
            
        }
    }
}
