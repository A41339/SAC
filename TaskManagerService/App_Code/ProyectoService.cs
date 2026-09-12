using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class ProyectoService : IProyectoService
{
    Concrete.ProyectoRepository rep = new Concrete.ProyectoRepository();
    
    public void Add(ref Proyecto entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Proyecto Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Proyecto>> GetAllAsync()
    {
        List<Proyecto> list = await rep.GetAll();
        return list;
    }

    public void Update(Proyecto entity)
    {
        rep.Update(entity);
    }
}
