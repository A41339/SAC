using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class SectorMap : EntityTypeConfiguration<Sector>
    {
        public SectorMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(150).IsRequired();
            Property(o => o.Activo).IsRequired();
 
            ToTable("Sector");

        }
    }
}

