using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class Bit_SessionesService : IService<Bit_Sessiones>
{
    Concrete.Bit_SessionesRepository rep = new Concrete.Bit_SessionesRepository();

    public void Add(ref Bit_Sessiones entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Bit_Sessiones Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Bit_Sessiones>> GetAllAsync()
    {
        List<Bit_Sessiones> list = await rep.GetAll();
        return list;
    }

    public void Update(Bit_Sessiones entity)
    {
        rep.Update(entity);
    }
}
