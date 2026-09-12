using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class XML_Pasivo_Cuenta_Contable_210Map : EntityTypeConfiguration<XML_Pasivo_Cuenta_Contable_210>
    {
        public XML_Pasivo_Cuenta_Contable_210Map()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdOperacion);
            Property(o => o.IdAcreedor);
            Property(o => o.IdOperacion);
            Property(o => o.TipoMonedaObligacion);
            Property(o => o.TipoTasa);
            Property(o => o.Tasa);
            Property(o => o.CuentaContablePrincipal);
            Property(o => o.SaldoPrincipal);
            Property(o => o.SaldoProducto);
            Property(o => o.FechaFormalizacion);
            Property(o => o.FechaVencimiento);
            Property(o => o.Condicion);
            ToTable("XML_Pasivo_Cuenta_Contable_210");

        }
    }
}
