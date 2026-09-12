using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Sugef_CarteraMap : EntityTypeConfiguration<Sugef_Cartera>
    {
        public Sugef_CarteraMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Actividad);
            Property(o => o.NombreSector);
            Property(o => o.AlDia);
            Property(o => o.Rango1_30Dias);
            Property(o => o.Rango31_60Dias);
            Property(o => o.Rango61_90Dias);
            Property(o => o.Rango91_180Dias);
            Property(o => o.Mas180Dias);
            Property(o => o.CobroJudicial);
            Property(o => o.Total);

            ToTable("Sugef_Cartera");            
        }
    }
}
