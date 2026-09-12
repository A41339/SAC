using System.Linq;

namespace Concrete
{
    public class ParametrosRepository : FGA.Concrete.Repository<FGA.Models.Parametros>
    {
        public ParametrosRepository()
        {

        }

        public override FGA.Models.Parametros Get(string id)
        {
            int param = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == param);
        }
    }
}