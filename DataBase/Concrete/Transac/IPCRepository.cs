using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class IPCRepository : FGA.Concrete.Repository<Hist_IPC>
    {
        public IPCRepository()
        {
        }

        public override Hist_IPC Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }

        public async override Task<List<Hist_IPC>> GetAll()
        {
            DateTime date = DateTime.Now.AddYears(-3);
            return await DbSet.Where(o => o.Fecha >= date).ToListAsync();

        }
    }
}