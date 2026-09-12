using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Rpt_EstructuraCamelMap : EntityTypeConfiguration<Rpt_EstructuraCamel>
    {
        public Rpt_EstructuraCamelMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(150);
                                   
            ToTable("Rpt_EstructuraCamel");
            
        }
    }
}
