using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class TipoInformeMap : EntityTypeConfiguration<TipoInforme>
    {
        public TipoInformeMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Nombre);
            Property(o => o.Estado);
           
            ToTable("TipoInforme");            
        }
    }
}
