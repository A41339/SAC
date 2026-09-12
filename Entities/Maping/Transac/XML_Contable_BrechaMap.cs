using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Contable_BrechaMap : EntityTypeConfiguration<XML_Contable_Brecha>
    {
        public XML_Contable_BrechaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaBrecha);
            Property(o => o.MontoBrecha);
            Property(o => o.RangoBrecha);
            Property(o => o.TipoCatalogoSugef);
 
            ToTable("XML_Contable_Brecha");

        }
    }
}
