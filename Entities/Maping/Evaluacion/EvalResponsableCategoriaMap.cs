using Entities.Entities.Evaluacion;
using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class EvalResponsableCategoriaMap : EntityTypeConfiguration<EvalResponsableCategoria>
    {
        public EvalResponsableCategoriaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.IdCategoria_Id);
            Property(o => o.IdEntidad_Id);
            Property(o => o.IdUsuario_Id1);
            Property(o => o.IdUsuario_Id2);
            Property(o => o.Confirmada);

            ToTable("EvalResponsableCategoria");

            HasRequired(c => c.IdEntidad).WithMany(o => o.CategoriasResponsable).HasForeignKey(o => o.IdEntidad_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdCategoria).WithMany(o => o.CategoriasResponsable).HasForeignKey(o => o.IdCategoria_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdUsuario1).WithMany(o => o.CategoriasResponsable1).HasForeignKey(o => o.IdUsuario_Id1).WillCascadeOnDelete(false);
            HasRequired(c => c.IdUsuario2).WithMany(o => o.CategoriasResponsable2).HasForeignKey(o => o.IdUsuario_Id2).WillCascadeOnDelete(false);
        }
    }
}
