using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "LiborService" in code, svc and config file together.
public class LiborService :  IService<Hist_Libor>
{
    Concrete.LiborRepository rep = new Concrete.LiborRepository();

    public void Add(ref Hist_Libor entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Hist_Libor Get(string id)
    {
        return rep.Get(id);
    }
    
    public async Task<List<Hist_Libor>> GetAllAsync()
    {
        List<Hist_Libor> list = await rep.GetAll();
        return list;
    }

    public void Update(Hist_Libor entity)
    {
        rep.Update(entity);
    }
}
