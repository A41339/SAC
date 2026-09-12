using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Categoria_RiesgoMap : EntityTypeConfiguration<Categoria_Riesgo>
    {
        public Categoria_RiesgoMap()
        {           
            HasKey(o => o.Codigo);
            Property(o => o.CategoriaRiesgo);
            Property(o => o.Analisis);
            ToTable("Categoria_Riesgo");            
        }
    }
}
