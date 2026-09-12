using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class SexoMap : EntityTypeConfiguration<Sexo> 
    {
        public SexoMap()
        {
             HasKey(o => o.Id);
             Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
             Property(o => o.Nombre).HasMaxLength(50);
       
            ToTable("Sexo");

        }
    }
}
