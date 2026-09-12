using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Entidades" in code, svc and config file together.
public class SectorService : ISectorService
{
    Concrete.SectorRepository rep = new Concrete.SectorRepository();

    public void Add(ref Sector entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Sector Get(string id)
    {
        return rep.Get(id);
    }


    public async Task<List<Sector>> GetAllAsync()
    {
        List<Sector> list = await rep.GetAll();
        return list;
    }

    public void Update(Sector entity)
    {
        rep.Update(entity);
    }
}
