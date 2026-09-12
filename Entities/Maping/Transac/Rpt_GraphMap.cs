using FGA.Models;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Rpt_GraphMap : EntityTypeConfiguration<Rpt_Graph>
    {
        public Rpt_GraphMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(150);
            Property(o => o.Ind_FGA);
            Property(o => o.Ind_Sector);
            Property(o => o.Ind_SF);
            ToTable("Rpt_Grafico");
        }
    }
}
