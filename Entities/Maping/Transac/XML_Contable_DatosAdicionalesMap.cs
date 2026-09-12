using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Contable_DatosAdicionalesMap : EntityTypeConfiguration<XML_Contable_DatosAdicionales>
    {
        public XML_Contable_DatosAdicionalesMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaCatalogo);
            Property(o => o.MontoDatoAdicional);
            Property(o => o.TipoCatalogoSugef);
            Property(o => o.TipoMonedaDato);

            ToTable("XML_Contable_DatosAdicionales");

        }
    }
}
