using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace FGA.Models
{
    public class ContentContext : DbContext
    {
        public ContentContext()
            : base("name=ContentConnectionString")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }
      
        public virtual DbSet<Noticia> Noticias { get; set; }
        public virtual DbSet<Seccion> Secciones { get; set; }
        public virtual DbSet<Tipo_Album> Tipo_Album { get; set; }
        public virtual DbSet<Album> Albumes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new FGA.Maping.Tipo_AlbumMap());
            modelBuilder.Configurations.Add(new FGA.Maping.AlbumMap());
            modelBuilder.Configurations.Add(new FGA.Maping.NoticiaMap());
            modelBuilder.Configurations.Add(new FGA.Maping.SeccionMap());
            base.OnModelCreating(modelBuilder);
        }       
    }
}