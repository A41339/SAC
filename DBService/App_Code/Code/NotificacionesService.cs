using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NotificacionesService : INotificacionesService
{
    Concrete.NotificacionesRepository rep = new Concrete.NotificacionesRepository();

    public void Add(ref Notificaciones entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Notificaciones Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Notificaciones>> GetAllAsync()
    {
        List<Notificaciones> list = await rep.GetAll();
        return list;
    }

    public void Update(Notificaciones entity)
    {
        rep.Update(entity);
    }
}