using FGA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.Evaluacion
{
    public class EvalRespuestaUsuario
    {
        public int Id { get; set; }
        public int OpcionSeleccionada { get; set; }

        public int IdPregunta_Id { get; set; }
        public virtual EvalPregunta IdPregunta { get; set; }

        public string IdEntidad_Id { get; set; }
        public virtual Entidad IdEntidad { get; set; }

        public int IdUsuario_Id { get; set; }
        public virtual Usuario IdUsuario { get; set; }
    }
}
