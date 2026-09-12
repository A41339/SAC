using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class TipoProyeccionMap : EntityTypeConfiguration<TipoProyeccion>
    {
        public TipoProyeccionMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(50);                                  
            ToTable("TipoProyeccion");            
        }
    }
}
