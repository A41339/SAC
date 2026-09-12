using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class TBPRepository : FGA.Concrete.Repository<Hist_TBP>
    {
        public TBPRepository()
        {
        }

        public override Hist_TBP Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }

        public async override Task<List<Hist_TBP>> GetAll()
        {
            DateTime date = DateTime.Now.AddYears(-3);
            return await DbSet.Where(o => o.Fecha >= date).ToListAsync();

        }
    }
}