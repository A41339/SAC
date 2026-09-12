using System.Linq;

namespace Concrete
{
    public class ProyectoRepository : FGA.Concrete.Repository<FGA.Models.Proyecto>
    {
        public ProyectoRepository()
        {
        }

        public override FGA.Models.Proyecto Get(string id)
        {          
            return DbSet.Include("IdDesarrollador")
                    .Include("IdAdmin")
                    .Include("IdLider")
                    .Include("IdReponsableTI").FirstOrDefault(o => o.Id == id);
        }
    }
}