using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class EntidadMap : EntityTypeConfiguration<Entidad>
    {
        public EntidadMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasMaxLength(5).IsRequired();
            Property(o => o.Identificacion).IsRequired();
            Property(o => o.Nombre).HasMaxLength(100).IsRequired();
            Property(o => o.Contacto);
            Property(o => o.Activo).IsRequired();
            Property(o => o.Ind_Cargar);
            Property(o => o.Ind_Validar);
            Property(o => o.Ind_Evaluacion);
            Property(o => o.CorreoInforme);

            HasRequired(c => c.Perfil_Entidad).WithMany(o => o.Entidad_Ids).HasForeignKey(o => o.Perfil_Entidad_Id).WillCascadeOnDelete(false);
            ToTable("Entidad");

        }
    }
}

