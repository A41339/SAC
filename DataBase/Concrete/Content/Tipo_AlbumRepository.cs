using System.Data.Entity;
using System.Linq;
using FGA.Models;

namespace Concrete
{
    public class Tipo_AlbumRepository : FGA.Concrete.ContentRepository<Tipo_Album>
    {
        public Tipo_AlbumRepository()
        {
        }

        public override Tipo_Album Get(string id)
        {
            int tipo = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == tipo);
        }
    }
}