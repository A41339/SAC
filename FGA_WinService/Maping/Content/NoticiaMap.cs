using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class NoticiaMap : EntityTypeConfiguration<Noticia>
    {
        public NoticiaMap()
        {

            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Detalle).HasMaxLength(250);
            Property(o => o.Link).HasMaxLength(500);
            Property(o => o.Fecha);

            ToTable("Noticia");
            
        }
    }
}
