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
    public class EvalSubCategoriaMap : EntityTypeConfiguration<EvalSubCategoria>
    {
        public EvalSubCategoriaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Enunciado);

            ToTable("EvalSubCategoria");
            HasRequired(o => o.IdCategoria_Id).WithMany(c => c.SubCategorias).HasForeignKey(o => o.IdCategoria);
        }
    }
}
