using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ParametrosService : IParametrosService
{
    Concrete.ParametrosRepository rep = new Concrete.ParametrosRepository();

    public void Add(ref Parametros entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(string id)
    {
        rep.Delete(id);
    }

    public Parametros Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Parametros>> GetAllAsync()
    {
        List<Parametros> list = await rep.GetAll();
        return list;
    }

    public void Update(Parametros entity)
    {
        rep.Update(entity);
    }
}