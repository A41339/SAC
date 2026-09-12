using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class PerfilEntidadService : IService<PerfilEntidad>
{
    Concrete.PerfilEntidadRepository rep = new Concrete.PerfilEntidadRepository();

    public void Add(ref PerfilEntidad entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public PerfilEntidad Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<PerfilEntidad>> GetAllAsync()
    {
        List<PerfilEntidad> list = await rep.GetAll();
        return list;
    }

    public void Update(PerfilEntidad entity)
    {
        rep.Update(entity);
    }
}

