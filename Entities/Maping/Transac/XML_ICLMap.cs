using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_ICLMap : EntityTypeConfiguration<XML_ICL>
    {
        public XML_ICLMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.CuentaCatalogo);
            Property(o => o.Factor);
            Property(o => o.MontoPonderado);
            Property(o => o.Moneda);
            Property(o => o.Monto);

            ToTable("XML_ICL");            
        }
    }
}
