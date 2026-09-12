using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TBPService" in code, svc and config file together.
public class TBPService : IService<Hist_TBP>
{
    Concrete.TBPRepository rep = new Concrete.TBPRepository();

    public void Add(ref Hist_TBP entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Hist_TBP Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Hist_TBP>> GetAllAsync()
    {
        List<Hist_TBP> list = await rep.GetAll();
        return list;
    }

    public void Update(Hist_TBP entity)
    {
        rep.Update(entity);
    }
}
