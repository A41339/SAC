
using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class FormulasMap : EntityTypeConfiguration<Formulas>
    {
        public FormulasMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Formula);
            Property(o => o.Nombre);
            Property(o => o.Ind_Porcentaje);
            Property(o => o.Entidad_Id);
            HasRequired(c => c.Entidad).WithMany(o => o.IdFormulas).HasForeignKey(o => o.Entidad_Id).WillCascadeOnDelete(false);

            ToTable("Formulas");

        }
    }
}
