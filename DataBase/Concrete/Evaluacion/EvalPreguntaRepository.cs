using Entities.Entities.Evaluacion;
using FGA.Concrete;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Concrete.Evaluacion
{
    public class EvalPreguntaRepository : Repository<EvalPregunta>
    {
        public EvalPreguntaRepository() : base()
        {
        }

        public override EvalPregunta Get(string id)
        {
            int pregunta = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == pregunta);
        }


        public async Task<List<EvalPregunta>> GetBySubCategory(int id)
        {
            return await DbSet.Where(o => o.IdSubCategoria == id).ToListAsync();
        }

    }
}