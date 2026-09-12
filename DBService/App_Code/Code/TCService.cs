using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TCService" in code, svc and config file together.
public class TCService : IService<Hist_TipoCambio>
{
    Concrete.TCRepository rep = new Concrete.TCRepository();

    public void Add(ref Hist_TipoCambio entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Hist_TipoCambio Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Hist_TipoCambio>> GetAllAsync()
    {
        List<Hist_TipoCambio> list = await rep.GetAll();
        return list;
    }

    public void Update(Hist_TipoCambio entity)
    {
        rep.Update(entity);
    }
}
