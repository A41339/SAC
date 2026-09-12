using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Credito_Cuota_AtrasadaMap : EntityTypeConfiguration<XML_Credito_Cuota_Atrasada>
    {
        public XML_Credito_Cuota_AtrasadaMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IdDeudor);
            Property(o => o.IdOperacion);
            Property(o => o.MontoCuotaAtrasada);
            ToTable("XML_Credito_Cuota_Atrasada");            
        }
    }
}
