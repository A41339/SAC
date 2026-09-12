using System.Collections.Generic;
using System.Linq;

namespace Concrete
{
    public class TareaRepository : FGA.Concrete.Repository<FGA.Models.Tarea>
    {
        public TareaRepository()
        {
        }

        public override FGA.Models.Tarea Get(string id)
        {
            int tarea = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == tarea);
        }

        public List<FGA.Models.Tarea> GetBySolicitud(int solicitud, int estado) {
            return DbSet.Where(o => o.IdSolicitudCambio_Id == solicitud && (o.IdTipoTarea_Id == estado || estado == -1)).ToList(); 
        }
    }
}