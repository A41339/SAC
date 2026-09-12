using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class ArchivoEstadoMap : EntityTypeConfiguration<ArchivoEstado>
    {
        public ArchivoEstadoMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Nombre).HasMaxLength(50);
                                   
            ToTable("ArchivoEstado");
            
        }
    }
}
