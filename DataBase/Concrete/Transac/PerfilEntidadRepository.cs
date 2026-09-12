using System.Linq;

namespace Concrete
{
    public class PerfilEntidadRepository : FGA.Concrete.Repository<FGA.Models.PerfilEntidad>
    {
        public PerfilEntidadRepository()
        {

        }

        public override FGA.Models.PerfilEntidad Get(string id)
        {
            int idSexo = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == idSexo);
        }
    }
}