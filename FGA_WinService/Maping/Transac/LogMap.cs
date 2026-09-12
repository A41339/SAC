using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class LogMap : EntityTypeConfiguration<Log>
    {
        public LogMap()
        {
           
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Action);
            Property(o => o.Controller);
            Property(o => o.Fecha);
            Property(o => o.Mensaje);
                                   
            ToTable("Log");
            
        }
    }
}
