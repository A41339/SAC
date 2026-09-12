using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class TipoInformeService : IService<TipoInforme>
{
    Concrete.TipoInformeRepository rep = new Concrete.TipoInformeRepository();

    public void Add(ref TipoInforme entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public TipoInforme Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<TipoInforme>> GetAllAsync()
    {
        List<TipoInforme> list = await rep.GetAll();
        return list;
    }

    public void Update(TipoInforme entity)
    {
        rep.Update(entity);
    }
}
