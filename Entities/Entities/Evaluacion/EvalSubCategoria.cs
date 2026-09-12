using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.Evaluacion
{    
    public class EvalSubCategoria
    {
        public int Id { get; set; }

        public virtual EvalCategoria IdCategoria_Id { get; set; }
        public int IdCategoria { get; set; }
        public string Enunciado { get; set; }
                
        public virtual ICollection<EvalPregunta> Preguntas { get; set; }

    }
}
