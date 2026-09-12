using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FGA.Models;

namespace Concrete
{
    public class LiborRepository : FGA.Concrete.Repository<Hist_Libor>
    {
        public LiborRepository()
        {
        }

        public override Hist_Libor Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }

        public async override Task<List<Hist_Libor>> GetAll()
        {
            DateTime date = DateTime.Now.AddYears(-3);
            return await DbSet.Where(o => o.Fecha >= date).ToListAsync();
        }
    }
}