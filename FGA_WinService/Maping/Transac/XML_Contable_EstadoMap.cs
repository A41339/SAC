using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Contable_EstadoMap : EntityTypeConfiguration<XML_Contable_Estado>
    {
        public XML_Contable_EstadoMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Cuenta);
            Property(o => o.TipoCatalogoSUGEF);
            Property(o => o.Debito);
            Property(o => o.Credito);
            Property(o => o.SaldoFinal);
            ToTable("XML_Contable_Estado");
            
        }
    }
}
