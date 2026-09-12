using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Concrete
{
    public class TipoInformeRepository : FGA.Concrete.Repository<FGA.Models.TipoInforme>
    {
        public TipoInformeRepository()
        {
        }

        public override FGA.Models.TipoInforme Get(string id)
        {
            int tipo = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == tipo);
        }
    }
}