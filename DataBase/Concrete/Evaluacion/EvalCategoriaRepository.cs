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
    public class EvalCategoriaRepository : Repository<EvalCategoria>
    {
        public EvalCategoriaRepository() : base()
        {
        }

        public override EvalCategoria Get(string id)
        {
            int tarea = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == tarea);
        }

        public override async Task<List<EvalCategoria>> GetAll()
        {
            return await DbSet.Where(o => o.Ind_Estado == "A").Include("SubCategorias").ToListAsync();
        }

        public async Task<List<EvalCategoria>> GetByUser(int idUser)
        {
            //return await DbSet.Where(o => o.Ind_Estado == "A").Include("SubCategorias").ToListAsync();

            var categoria = from s in context.Categorias.Include("SubCategorias")
            join r in context.ResponsableCategorias on s.Id equals r.IdCategoria_Id
            where r.IdUsuario_Id1 == idUser || r.IdUsuario_Id2 == idUser
            select s;

            return await categoria.ToListAsync();
        }

    }
}

