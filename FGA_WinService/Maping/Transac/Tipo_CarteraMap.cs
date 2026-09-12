using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Tipo_CarteraMap : EntityTypeConfiguration<Tipo_Cartera>
    {
        public Tipo_CarteraMap()
        {           
            HasKey(o => o.Codigo);
            Property(o => o.TipoCartera);
            Property(o => o.Nombre);
            ToTable("TipoCartera");            
        }
    }
}
