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
    public class EvalResponsableCategoriaRespository : Repository<EvalResponsableCategoria>
    {
        public EvalResponsableCategoriaRespository() : base()
        {
        }

        public override EvalResponsableCategoria Get(string id)
        {
            int idResp = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == idResp);
        }

        public async Task<List<EvalResponsableCategoria>> GetByUsusario(int id)
        {
            return await DbSet.Where(o => o.IdUsuario_Id1 == id || o.IdUsuario_Id2 == id).ToListAsync();
        }

        public async Task<List<EvalResponsableCategoria>> GetByEntidad(string id)
        {
            return await DbSet.Where(o => o.IdEntidad_Id == id).ToListAsync();
        }

        public int Existe(string idEntidad, int idUsuario, int idCategoria)
        {
            var id = 0;

            try
            {
                id = DbSet.Where(o => o.IdEntidad_Id == idEntidad && o.IdCategoria_Id == idCategoria).FirstOrDefault().Id;
            }
            catch (Exception)
            {
            }

            return id;
        }
    }
}