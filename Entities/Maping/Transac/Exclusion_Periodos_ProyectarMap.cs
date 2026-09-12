using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Exclusion_Periodos_ProyectarMap : EntityTypeConfiguration<Exclusion_Periodos_Proyectar>
    {
        public Exclusion_Periodos_ProyectarMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.IdEntidad);
            Property(o => o.Cuenta);
            Property(o => o.Periodo);

            ToTable("Exclusion_Periodos_Proyectar");
            
        }
    }
}
