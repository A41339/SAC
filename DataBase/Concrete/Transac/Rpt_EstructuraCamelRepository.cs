using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FGA.Models;

namespace Concrete
{
    public class Rpt_EstructuraCamelRepository : FGA.Concrete.Repository<FGA.Models.Rpt_EstructuraCamel>
    {
        public Rpt_EstructuraCamelRepository()
        {
        }

        public async override Task<List<Rpt_EstructuraCamel>> GetAll() {

              return await DbSet.Where(o => o.Ind_Publico == "S").ToListAsync();

        }          
    }
}