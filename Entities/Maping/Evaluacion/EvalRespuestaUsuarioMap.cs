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
    public class EvalRespuestaUsuarioMap : EntityTypeConfiguration<EvalRespuestaUsuario>
    {
        public EvalRespuestaUsuarioMap()
        {
            HasKey(o => o.Id);
            Property(o => o.OpcionSeleccionada);

            ToTable("EvalRespuestaUsuario");

            HasRequired(c => c.IdEntidad).WithMany(o => o.Respuestas).HasForeignKey(o => o.IdEntidad_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdPregunta).WithMany(o => o.Respuestas).HasForeignKey(o => o.IdPregunta_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdUsuario).WithMany(o => o.Respuestas).HasForeignKey(o => o.IdUsuario_Id).WillCascadeOnDelete(false);

        }
    }
}
