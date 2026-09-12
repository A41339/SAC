using System.Linq;

namespace Concrete
{
    public class MenuRepository : FGA.Concrete.Repository<FGA.Models.Menu>
    {
        public MenuRepository()
        {

        }

        public override FGA.Models.Menu Get(string id)
        {
            int menu = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == menu);
        }
    }
}