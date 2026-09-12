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
    public class EvalPreguntaMap : EntityTypeConfiguration<EvalPregunta>
    {
        public EvalPreguntaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.OpcionA);
            Property(o => o.OpcionB);
            Property(o => o.OpcionC);
            Property(o => o.OpcionD);

            ToTable("EvalPregunta");
            HasRequired(o => o.IdSubCategoria_Id).WithMany(c => c.Preguntas).HasForeignKey(o => o.IdSubCategoria);
        }
    }
}
