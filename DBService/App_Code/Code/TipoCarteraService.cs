using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TipoCarteraService : IService<Tipo_Cartera>
{
    Concrete.TipoCarteraRepository rep = new Concrete.TipoCarteraRepository();

    public void Add(ref Tipo_Cartera entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Tipo_Cartera Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Tipo_Cartera>> GetAllAsync()
    {
        List<Tipo_Cartera> list = await rep.GetAll();
        return list;
    }

    public void Update(Tipo_Cartera entity)
    {
        rep.Update(entity);
    }
}
