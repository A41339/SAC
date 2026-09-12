using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class AlbumRepository : FGA.Concrete.ContentRepository<Album>
    {
        public AlbumRepository()
        {
        }

        public override Album Get(string id)
        {
            int album = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == album);
        }

        public async override Task<List<Album>> GetAll()
        {
            return await DbSet.Include("Tipo").ToListAsync();
        }
    }
}