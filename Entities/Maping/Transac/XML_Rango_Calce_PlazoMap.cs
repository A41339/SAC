using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Rango_Calce_PlazoMap : EntityTypeConfiguration<XML_Rango_Calce_Plazo>
    {
        public XML_Rango_Calce_PlazoMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaCalcePlazo);
            Property(o => o.MontoCalcePlazo);
            Property(o => o.RangoCalcePlazo);
            Property(o => o.TipoCatalogoSugef);
     
            ToTable("XML_Rango_Calce_Plazo");

        }
    }
}
