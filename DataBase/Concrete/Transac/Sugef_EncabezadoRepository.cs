using System.Linq;

namespace Concrete
{
    public class Sugef_EncabezadoRepository : FGA.Concrete.Repository<FGA.Models.Sugef_Encabezado>
    {
        public Sugef_EncabezadoRepository()
        {
        }

        public override FGA.Models.Sugef_Encabezado Get(string id)
        {
            long enc = long.Parse(id);
            return DbSet.Include("IdUsuario")
                .Include("IdEstado")
                .FirstOrDefault(o => o.Id == enc);
        }      
    }
}