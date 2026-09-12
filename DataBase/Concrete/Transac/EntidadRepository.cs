using FGA.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Concrete
{
    public class EntidadRepository : FGA.Concrete.Repository<Entidad>
    {
        public EntidadRepository()
        {
        }

        public Entidad GetByIden(string id)
        {
            return DbSet
                .FirstOrDefault(i => i.Identificacion == id);
        }

        public List<string> GetMails(string id)
        {
            return DbSet
               .Where(i => i.Id == id || id == "-1").Select( o=> o.CorreoInforme).ToList();
        }
    }
}
