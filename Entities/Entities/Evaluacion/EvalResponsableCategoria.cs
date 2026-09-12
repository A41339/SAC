using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.Evaluacion
{
    public class EvalResponsableCategoria
    {
        public int Id { get; set; }

        public int IdCategoria_Id { get; set; }
        public virtual EvalCategoria IdCategoria { get; set; }

        public int IdUsuario_Id1 { get; set; }
        public virtual Usuario IdUsuario1 { get; set; }

        public int IdUsuario_Id2 { get; set; }
        public virtual Usuario IdUsuario2 { get; set; }

        public string IdEntidad_Id { get; set; }
        public virtual Entidad IdEntidad { get; set; }

        public string Confirmada { get; set; }
    }
}