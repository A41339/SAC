using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class PerfilEntidadMap : EntityTypeConfiguration<PerfilEntidad> 
    {
        public PerfilEntidadMap()
        {
             HasKey(o => o.Id);
             Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
             Property(o => o.Nombre).HasMaxLength(150);
       
            ToTable("PerfilEntidad");

        }
    }
}
