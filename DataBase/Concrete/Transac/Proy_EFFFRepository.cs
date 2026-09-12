using System.Linq;
using System;

namespace Concrete
{
    public class Proy_EEFFRepository : FGA.Concrete.Repository<FGA.Models.Proy_EEFF>
    {
        public Proy_EEFFRepository()
        {
        }

        public override void Delete(string id)
        {
            int Key = int.Parse(id);
            FGA.Models.Proy_EEFF entity = DbSet.Where(o => o.Id == Key).FirstOrDefault();
            DbSet.Remove(entity);
            context.SaveChanges();
        }

        public FGA.Models.Proy_EEFF GetProy(string idEntidad, DateTime fechaCorte, String tipoPlazo)
        {
            var proy = DbSet.FirstOrDefault(o => o.IdEntidad == idEntidad && o.TipoPlazo == tipoPlazo && o.PeriodoCorte == fechaCorte);

            if (proy == null)
            {
                proy = DbSet.FirstOrDefault(o => o.IdEntidad == idEntidad && o.PeriodoCorte == fechaCorte);
                if (proy != null)
                {
                    proy.TipoPlazo = tipoPlazo;
                }
            }

            return proy;
        }
    }
}