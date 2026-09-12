using System.Linq;

namespace Concrete
{
    public class TipoTareaRepository : FGA.Concrete.Repository<FGA.Models.TipoTarea>
    {
        public TipoTareaRepository()
        {
        }

        public override FGA.Models.TipoTarea Get(string id)
        {
            int tipoTarea = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == tipoTarea);
        }
    }
}