using Entities.Entities.Evaluacion;
using FGA.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Concrete.Evaluacion
{
    public class EvalRespuestaUsuarioRepository : Repository<EvalRespuestaUsuario>
    {
        public EvalRespuestaUsuarioRepository() : base()
        {
        }

        public override void Add(ref EvalRespuestaUsuario entity)
        {
            int id = this.Existe(entity.IdEntidad_Id, entity.IdPregunta_Id);
            if (id != 0)
            {
                var reg = DbSet.Find(id);
                DbSet.Remove(reg);
                context.SaveChanges();

                //string eliminar = id.ToString();
                //base.Delete(eliminar);
            }

            base.Add(ref entity);
        }

        public int Existe(string idEntidad, int idPregunta)
        {
            int id = 0;
            try
            {
                id = DbSet.Where(o => o.IdEntidad_Id == idEntidad && o.IdPregunta_Id == idPregunta).FirstOrDefault().Id;
            }
            catch (Exception)
            {
                id = 0;
            }

            return id;
        }
    }
}