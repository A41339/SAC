using FGA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Concrete
{
    public class NotificacionesRepository : FGA.Concrete.Repository<FGA.Models.Notificaciones>
    {
        public NotificacionesRepository()
        {
        }

        public override Notificaciones Get(string id)
        {
            int notificacion = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == notificacion);
        }
    }
}