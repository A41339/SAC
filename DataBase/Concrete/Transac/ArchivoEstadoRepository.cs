using System.Linq;

namespace Concrete
{
    public class ArchivoEstadoRepository : FGA.Concrete.Repository<FGA.Models.ArchivoEstado>
    {
        public ArchivoEstadoRepository()
        {
        }

        public override FGA.Models.ArchivoEstado Get(string id)
        {
            int estado = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == estado);
        }
    }
}