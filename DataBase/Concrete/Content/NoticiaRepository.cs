using System.Data.Entity;
using System.Linq;
using FGA.Models;

namespace Concrete
{
    public class NoticiaRepository : FGA.Concrete.ContentRepository<Noticia>
    {
        public NoticiaRepository()
        {
        }

        public override Noticia Get(string id)
        {
            int nota = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == nota);
        }
    }
}