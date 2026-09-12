using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

public class TipoXMLService : IService<TipoXML>
{

    Concrete.TipoXMLRepository rep = new Concrete.TipoXMLRepository();

    public void Add(ref TipoXML entity)
    {
        rep.Add(ref entity);
    }
    
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public TipoXML Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<TipoXML>> GetAllAsync()
    {
        List<TipoXML> list = await rep.GetAll();
        return list;
    }

    public void Update(TipoXML entity)
    {
        rep.Update(entity);
    }
}
