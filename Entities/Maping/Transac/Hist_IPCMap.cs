using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Hist_IPCMap : EntityTypeConfiguration<Hist_IPC>
    {
        public Hist_IPCMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Fecha);
            Property(o => o.Monto).HasPrecision(18, 5);
            Property(o => o.Desviacion).HasPrecision(22, 15);
            Property(o => o.Fluctuacion).HasPrecision(22, 15);
            Property(o => o.VariacionInter).HasPrecision(18, 5);

            ToTable("Hist_IPC");            
        }
    }
}
