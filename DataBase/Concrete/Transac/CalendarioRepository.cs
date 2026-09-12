using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Concrete
{
    public class CalendarioRepository : FGA.Concrete.Repository<FGA.Models.Calendario>
    {
        public CalendarioRepository()
        {
        }

        public FGA.Models.Calendario GetByDate(DateTime periodo)
        {
            return DbSet.FirstOrDefault(o => o.Periodo == periodo);
        }

        public override FGA.Models.Calendario Get(string id)
        {
            int calendario = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == calendario);
        }

        public async override Task<List<FGA.Models.Calendario>> GetAll()
        {
            return await  DbSet.Where(o => o.Enviado == false).Take(24).OrderByDescending(o => o.Periodo).ToListAsync();
        }
    }
}