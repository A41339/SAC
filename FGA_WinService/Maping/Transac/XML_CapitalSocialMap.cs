using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_Capital_SocialMap : EntityTypeConfiguration<XML_Capital_Social>
    {
        public XML_Capital_SocialMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Contrato);
            Property(o => o.Des_Identificacion);
            Property(o => o.Fechainclusion);
            Property(o => o.SaldoReal);
            ToTable("XML_Capital_Social");            
        }
    }
}
