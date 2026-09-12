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
    public class EvalSubCategoriaRepository : Repository<EvalSubCategoria>
    {
        public EvalSubCategoriaRepository() : base()
        {
        }

        public override EvalSubCategoria Get(string id)
        {
            int pregunta = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == pregunta);
        }

        public async Task<List<EvalSubCategoria>> GetByCategory(int id)
        {
            return await DbSet.Where(o => o.IdCategoria == id).ToListAsync();
        }

    }
}