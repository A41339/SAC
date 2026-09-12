using System.Linq;

namespace Concrete
{
    public class SexoRepository : FGA.Concrete.Repository<FGA.Models.Sexo>
    {
        public SexoRepository()
        {

        }

        public override FGA.Models.Sexo Get(string id)
        {
            int idSexo = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == idSexo);
        }
    }
}