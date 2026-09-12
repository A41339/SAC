using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Entidades" in code, svc and config file together.
public class EntidadService : IEntidadService
{
    Concrete.EntidadRepository rep = new Concrete.EntidadRepository();

    public void Add(ref Entidad entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Entidad Get(string id)
    {
        return rep.Get(id);
    }

    public Entidad GetByIden(string id)
    {
        return rep.GetByIden(id);
    }

    public List<string> GetMails(string id){
        return rep.GetMails(id);
    }


    public async Task<List<Entidad>> GetAllAsync()
    {
        List<Entidad> list = await rep.GetAll();
        return list;
    }

    public void Update(Entidad entity)
    {
        rep.Update(entity);
    }
}
