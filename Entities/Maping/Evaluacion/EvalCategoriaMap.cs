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
    public class EvalCategoriaMap : EntityTypeConfiguration<EvalCategoria>
    {
        public EvalCategoriaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Titulo).HasMaxLength(100);
            Property(o => o.Descripcion).HasMaxLength(255);

            Property(o => o.PuntosOpcionA);
            Property(o => o.PuntosOpcionB);
            Property(o => o.PuntosOpcionC);
            Property(o => o.PuntosOpcionD);
            Property(o => o.ResponsableSugerido);
            ToTable("EvalCategoria");
        }
    }
}
