using System.Data.Entity;
using System.Linq;
using FGA.Models;

namespace Concrete
{
    public class SeccionRepository : FGA.Concrete.ContentRepository<Seccion>
    {
        public SeccionRepository()
        {
        }
        public override Seccion Get(string id)
        {
            int sec = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == sec);
        }
    }
}