using FGA.Models;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Rpt_Analisis_VHMap : EntityTypeConfiguration<Rpt_Analisis_VH>
    {
        public Rpt_Analisis_VHMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(150);

            ToTable("Rpt_Analisis_VH");

        }
    }
}
