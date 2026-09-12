using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class SeccionMap : EntityTypeConfiguration<Seccion>
    {
        public SeccionMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Nombre).HasMaxLength(50);
            Property(o => o.Detalle1).HasMaxLength(2000);
            Property(o => o.Detalle2).HasMaxLength(2000);
            Property(o => o.Detalle3).HasMaxLength(2000);
            Property(o => o.Detalle4).HasMaxLength(2000);
            ToTable("Seccion");
            
        }
    }
}
