using FGA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Concrete
{
    public class CatalogoCuentaRepository : FGA.Concrete.Repository<FGA.Models.CatalogoCuenta>
    {
        public CatalogoCuentaRepository()
        {
        }

        public List<CatalogoCuenta> GetPage(int pageNumber, int size, string filter, ref int records)
        {
            records = DbSet.Where(o => o.Cuenta.Contains(filter)).Count();

            return DbSet.Where(o => o.Cuenta.Contains(filter)).OrderBy(o => o.Cuenta).Skip(size * pageNumber)
                .Take(size).ToList();
        }
    }
}