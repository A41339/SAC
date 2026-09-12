using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SeccionService" in code, svc and config file together.
public class SeccionService : IService<Seccion>
{
    Concrete.SeccionRepository rep = new Concrete.SeccionRepository();

    public void Add(ref Seccion entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Seccion Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Seccion>> GetAllAsync()
    {
        List<Seccion> list = await rep.GetAll();
        return list;
    }

    public void Update(Seccion entity)
    {
        rep.Update(entity);
    }
}

