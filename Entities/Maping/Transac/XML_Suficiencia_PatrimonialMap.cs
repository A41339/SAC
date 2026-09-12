using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class XML_Suficiencia_PatrimonialMap : EntityTypeConfiguration<XML_Suficiencia_Patrimonial>
    {
        public XML_Suficiencia_PatrimonialMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaCatalogo);
            Property(o => o.GradualidadPonderacion);
            Property(o => o.TipoCatalogoSugef);
            Property(o => o.Monto);
            Property(o => o.MontoPonderado);
            Property(o => o.Ponderacion);
            Property(o => o.Moneda);

            ToTable("XML_Suficiencia_Patrimonial");

        }
    }
}
