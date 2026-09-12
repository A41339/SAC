using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "NoticiaService" in code, svc and config file together.
public class NoticiaService : IService<FGA.Models.Noticia>
{
    Concrete.NoticiaRepository rep = new Concrete.NoticiaRepository();

    public void Add(ref Noticia entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Noticia Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Noticia>> GetAllAsync()
    {
        List<Noticia> list = await rep.GetAll();
        return list;
    }

    public void Update(Noticia entity)
    {
        rep.Update(entity);
    }
}
