using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class Tipo_AlbumMap : EntityTypeConfiguration<Tipo_Album>
    {
        public Tipo_AlbumMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Nombre).HasMaxLength(50);

            ToTable("Tipo_Album");

        }
    }
}
