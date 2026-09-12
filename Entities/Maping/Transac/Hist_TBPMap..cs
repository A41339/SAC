using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Hist_TBPMap : EntityTypeConfiguration<Hist_TBP>
    {
        public Hist_TBPMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Fecha);
            Property(o => o.Monto).HasPrecision(18, 5);
            Property(o => o.Desviacion).HasPrecision(22, 15);
            Property(o => o.Fluctuacion).HasPrecision(22, 15);

            ToTable("Hist_TBP");
        }
    }
}
