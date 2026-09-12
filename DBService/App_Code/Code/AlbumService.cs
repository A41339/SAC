using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AlbumService" in code, svc and config file together.
public class AlbumService : IService<Album>
{
    Concrete.AlbumRepository rep = new Concrete.AlbumRepository();

    public void Add(ref Album entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }
    public Album Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Album>> GetAllAsync()
    {
        List<Album> list = await rep.GetAll();
        return list;
    }

    public void Update(Album entity)
    {
        rep.Update(entity);
    }
}
