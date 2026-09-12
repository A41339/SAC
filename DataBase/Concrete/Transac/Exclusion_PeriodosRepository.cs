using System;
using System.Linq;
using FGA.Models;

namespace Concrete
{
    public class Exclusion_PeriodoRepository : FGA.Concrete.Repository<Exclusion_Periodos_Proyectar>
    {
        public Exclusion_PeriodoRepository()
        {
        }

        public void Habilitar(DateTime Periodo, String Cuenta, String IdEntidad)
        {
            var exclusion = this.DbSet.Where(o => o.Periodo == Periodo && o.IdEntidad == IdEntidad && o.Cuenta == Cuenta).FirstOrDefault();
            DbSet.Remove(exclusion);
            context.SaveChanges();

       }
    }
}