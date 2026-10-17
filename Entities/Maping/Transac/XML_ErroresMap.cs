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
            Property(o => o.IdEncabezado_Id);

            HasOptional(o => o.IdEncabezado).WithMany().HasForeignKey(o => o.IdEncabezado_Id).WillCascadeOnDelete(false);

            ToTable("XML_Errores");            
        }
    }
}
