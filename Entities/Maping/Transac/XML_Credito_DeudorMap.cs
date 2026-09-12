using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Credito_DeudorMap : EntityTypeConfiguration<XML_Credito_Deudor>
    {
        public XML_Credito_DeudorMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdDeudor);
            Property(o => o.CategoriaRiesgo);
            ToTable("XML_Credito_Deudor");           
        }
    }
}
