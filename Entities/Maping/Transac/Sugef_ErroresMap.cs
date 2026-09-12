using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Sugef_ErroresMap : EntityTypeConfiguration<Sugef_Errores>
    {
        public Sugef_ErroresMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Detalle);
            ToTable("Sugef_Errores");            
        }
    }
}