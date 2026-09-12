using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class BCCR_TipoIndicadorRepository : FGA.Concrete.Repository<BCCR_TipoIndicador>
    {
        public BCCR_TipoIndicadorRepository()
        {
        }

        public override BCCR_TipoIndicador Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }
    }
}