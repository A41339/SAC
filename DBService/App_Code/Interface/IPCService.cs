using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TBPService" in code, svc and config file together.
public class IPCService : IService<Hist_IPC>
{
    Concrete.IPCRepository rep = new Concrete.IPCRepository();

    public void Add(ref Hist_IPC entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Hist_IPC Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Hist_IPC>> GetAllAsync()
    {
        List<Hist_IPC> list = await rep.GetAll();
        return list;
    }

    public void Update(Hist_IPC entity)
    {
        rep.Update(entity);
    }
}
