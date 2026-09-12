using System.Collections.Generic;
using System.Linq;

namespace Concrete
{
    public class XML_ErroresRepository : FGA.Concrete.Repository<FGA.Models.XML_Errores>
    {
        public XML_ErroresRepository()
        {
        }

        public List<FGA.Models.XML_Errores> GetByEncabezado(long id)
        {
            return DbSet.Include("IdEncabezado")
                .Where(o => o.IdEncabezado.Id == id)
                .ToList();
        }
    }
}