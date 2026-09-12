using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class ProyeccionMap : EntityTypeConfiguration<Proyeccion>
    {
        public ProyeccionMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Cuenta).HasMaxLength(8);
            Property(o => o.IdEntidad);
            Property(o => o.IdProyeccion);
            Property(o => o.Periodo);
            Property(o => o.PeriodosProyectar);
            Property(o => o.Valores);

            HasRequired(c => c.TipoProyeccion_Id).WithMany(o => o.ListaProyecciones).HasForeignKey(o => o.IdProyeccion).WillCascadeOnDelete(false);

            ToTable("Proyeccion");            
        }
    }
}
