using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FGA.Model
{
    public class Modelo_Evaluacion
    {
        public List<Entities.Entities.Evaluacion.EvalCategoria> listaCategorias;
        public List<Entities.Entities.Evaluacion.EvalCategoria> misCategorias;
        public List<Entities.Entities.Evaluacion.EvalSubCategoria> listaSubCategoria;
        public List<Entities.Entities.Evaluacion.EvalPregunta> listaPreguntas;
        public List<Entities.Entities.Procedures.FGA_Consultar_Evaluacion_Result> evaluaciones;
        public List<Entities.Entities.Evaluacion.EvalResponsableCategoria> listaResponsables;
        public List<FGA.Models.Usuario> listaUsuarios;
        public bool eval;
    }
}