using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class XML_ErroresMap : EntityTypeConfiguration<XML_Errores>
    {
        public XML_ErroresMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Detalle);
            ToTable("XML_Errores");            
        }
    }
}
