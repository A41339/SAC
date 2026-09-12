using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class CalendarioMap : EntityTypeConfiguration<Calendario>
    {
        public CalendarioMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Periodo);
            Property(o => o.DiaNotificacion);
            Property(o => o.DiaLimite);
            Property(o => o.Enviado);
            Property(o => o.Ind_Vencido);

            ToTable("Calendario");
            
        }
    }
}