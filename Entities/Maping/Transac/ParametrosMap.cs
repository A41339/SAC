using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class ParametrosMap : EntityTypeConfiguration<Parametros>
    {
        public ParametrosMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Llave);
            Property(o => o.Valor);
            Property(o => o.Descripcion);
            ToTable("Parametros");            
        }
    }
}
