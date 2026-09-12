using System.Linq;

namespace Concrete
{
    public class RoleRepository : FGA.Concrete.Repository<FGA.Models.Role>
    {
        public RoleRepository()
        {

        }
        public override FGA.Models.Role Get(string id)
        {
            int rol = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == rol);
        }
    }
}