using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class CalendarioService : ICalendarioService
{
    readonly Concrete.CalendarioRepository rep = new Concrete.CalendarioRepository();

    public Calendario GetByDate(DateTime periodo)
    {
        return rep.GetByDate(periodo);
    }

    public void Add(ref Calendario entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Calendario Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Calendario>> GetAllAsync()
    {
        List<Calendario> list = await rep.GetAll();
        return list;
    }


    public void Update(Calendario entity)
    {
        rep.Update(entity);
    }
}
