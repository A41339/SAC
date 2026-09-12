using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CategoriaRiesgoService : IService<Categoria_Riesgo>
{
    Concrete.CategoriaRiesgoRepository rep = new Concrete.CategoriaRiesgoRepository();

    public void Add(ref Categoria_Riesgo entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Categoria_Riesgo Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Categoria_Riesgo>> GetAllAsync()
    {
        List<Categoria_Riesgo> list = await rep.GetAll();
        return list;
    }

    public void Update(Categoria_Riesgo entity)
    {
        rep.Update(entity);
    }
}