using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class UsuarioEstadoMap : EntityTypeConfiguration<UsuarioEstado>
    {
        public UsuarioEstadoMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Id).HasMaxLength(1);
            Property(o => o.Nombre).HasMaxLength(100);
                                   
            ToTable("UsuarioEstado");
            
        }
    }
}
