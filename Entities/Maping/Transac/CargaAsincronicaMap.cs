using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class CargaAsincronicaMap : EntityTypeConfiguration<CargaAsincronica>
    {
        public CargaAsincronicaMap()
        {
           
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.IndEstado);
            Property(o => o.IdEntidad);
            Property(o => o.FilePath);
                                   
            ToTable("CargaAsincronica");
            
        }
    }
}
