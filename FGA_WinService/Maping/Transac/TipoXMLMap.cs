using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class TipoXMLMap : EntityTypeConfiguration<TipoXML>
    {
        public TipoXMLMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(50);
            Property(o => o.DiaMaximo);
            Property(o => o.Prioridad);

            ToTable("TipoXML");
        }
    }
}

