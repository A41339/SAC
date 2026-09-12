using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;


// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Tipo_AlbumService" in code, svc and config file together.
public class Tipo_AlbumService : IService<Tipo_Album>
{
    Concrete.Tipo_AlbumRepository rep = new Concrete.Tipo_AlbumRepository();

    public void Add(ref Tipo_Album entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }
    public Tipo_Album Get(string id)
    {
        return rep.Get(id);
    }
    public async Task<List<Tipo_Album>> GetAllAsync()
    {
        List<Tipo_Album> list = await rep.GetAll();
        return list;
    }

    public void Update(Tipo_Album entity)
    {
        rep.Update(entity);
    }
}
