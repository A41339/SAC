using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.Evaluacion
{    
    public class EvalPregunta
    {
        public int Id { get; set; }

        public virtual EvalSubCategoria IdSubCategoria_Id { get; set; }
        public int IdSubCategoria { get; set; }

        public string Enunciado { get; set; }
        public string OpcionA { get; set; }
        public string OpcionB { get; set; }
        public string OpcionC { get; set; }
        public string OpcionD { get; set; }
                
        public virtual ICollection<EvalRespuestaUsuario> Respuestas { get; set; }


    }
}
