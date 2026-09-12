using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class SexoService : IService<Sexo>
{
    Concrete.SexoRepository rep = new Concrete.SexoRepository();

    public void Add(ref Sexo entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Sexo Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Sexo>> GetAllAsync()
    {
        List<Sexo> list = await rep.GetAll();
        return list;
    }

    public void Update(Sexo entity)
    {
        rep.Update(entity);
    }
}

