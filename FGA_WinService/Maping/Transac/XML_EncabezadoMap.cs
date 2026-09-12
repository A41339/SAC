using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class XML_EncabezadoMap : EntityTypeConfiguration<XML_Encabezado>
    {
        public XML_EncabezadoMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.FechaCarga);
            Property(o => o.Periodo);
            Property(o => o.Cantidad);
            Property(o => o.IdArchivo_Id);
            Property(o => o.IdEntidad_Id);
            Property(o => o.IdEstado_Id);
            Property(o => o.IdUsuario_Id);

            HasRequired(c => c.IdArchivo).WithMany(o => o.Encabezados).HasForeignKey(o => o.IdArchivo_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdEntidad).WithMany(o => o.Encabezados).HasForeignKey(o => o.IdEntidad_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdEstado).WithMany(o => o.Encabezados).HasForeignKey(o => o.IdEstado_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdUsuario).WithMany(o => o.Encabezados).HasForeignKey(o => o.IdUsuario_Id).WillCascadeOnDelete(false);


            ToTable("XML_Encabezado");            
        }
    }
}
