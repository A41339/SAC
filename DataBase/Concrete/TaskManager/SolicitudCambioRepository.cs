using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Concrete
{
    public class SolicitudCambioRepository : FGA.Concrete.Repository<FGA.Models.SolicitudCambio>
    {
      
        public SolicitudCambioRepository()
        {
        }

        public override FGA.Models.SolicitudCambio Get(string id)
        {
            int solicitud = int.Parse(id);
            return DbSet.Include("IdSolicitante")
                    .Include("IdAprobador")
                    .Include("IdProyecto")
                    .Include("Seccion")
                    .Include("IdResponsableActual").FirstOrDefault(o => o.Id == solicitud);
        }
        public async override Task<List<FGA.Models.SolicitudCambio>> GetAll()
        {
            int estadoFinalizado = (int)FGA.Enum.Enum_EstadoSolicitud.Finalizado;
            return await DbSet.Include("IdSolicitante")
                 .Include("IdResponsableActual")
                 .Where(o => o.IndEstado <= estadoFinalizado)
                 .OrderByDescending(o=>o.Id).ToListAsync();
        }
    }
}