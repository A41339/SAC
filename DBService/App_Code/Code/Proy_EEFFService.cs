using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class Proy_EEFFService : IProy_EEFFService
{
    Concrete.Proy_EEFFRepository rep = new Concrete.Proy_EEFFRepository();

    public void Add(ref Proy_EEFF entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Proy_EEFF Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Proy_EEFF>> GetAllAsync()
    {
        List<Proy_EEFF> list = await rep.GetAll();
        return list;
    }

    public Proy_EEFF GetProy(string idEntidad, DateTime PeriodoCorte, String TipoPlazo)
    {
        return rep.GetProy(idEntidad, PeriodoCorte, TipoPlazo);
    }

    public void Update(Proy_EEFF entity)
    {
        rep.Update(entity);
    }
}

