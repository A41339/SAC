using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Flujo_EfectivoRealMap : EntityTypeConfiguration<XML_Flujo_EfectivoReal>
    {
        public XML_Flujo_EfectivoRealMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaFlujoEfectivo);
            Property(o => o.MontoFlujoEfectivo);
            Property(o => o.TipoCatalogoSugef);

            ToTable("XML_Flujo_EfectivoReal");

        }
    }
}
