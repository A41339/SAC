using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.Evaluacion
{
    public class EvalCategoria
    {
        public int Id { get; set; }
        [DisplayName("Título")]
        public string Titulo { get; set; }
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }
        [DisplayName("Puntos opción A")]
        public int PuntosOpcionA { get; set; }
        [DisplayName("Puntos opción B")]
        public int PuntosOpcionB { get; set; }
        [DisplayName("Puntos opción C")]
        public int PuntosOpcionC { get; set; }
        [DisplayName("Puntos opción D")]
        public int PuntosOpcionD { get; set; }
        [DisplayName("Responsable sugerido")]
        public string ResponsableSugerido { get; set; }
        public string Imagen { get; set; }
        public string Ind_Estado { get; set; }
                
        public virtual ICollection<EvalResponsableCategoria> CategoriasResponsable { get; set; }

        public virtual ICollection<EvalSubCategoria> SubCategorias { get; set; }
    }
}