using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class SolicitudCambioService : ISolicitudCambioService
{
    Concrete.SolicitudCambioRepository rep = new Concrete.SolicitudCambioRepository();
    
    public void Add(ref SolicitudCambio entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public SolicitudCambio Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<SolicitudCambio>> GetAllAsync()
    {
        List<SolicitudCambio> list = await rep.GetAll();
        return list;
    }

    public void Update(SolicitudCambio entity)
    {
        rep.Update(entity);
    }
}
