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
            Property(o => o.Contacto).HasMaxLength(500).IsRequired();
            Property(o => o.Activo).IsRequired();
            Property(o => o.Ind_Cargar);
            Property(o => o.Ind_Validar);

            ToTable("Entidad");

        }
    }
}

