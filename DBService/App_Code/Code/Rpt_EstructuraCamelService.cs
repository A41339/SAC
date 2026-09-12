using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;
// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Rpt_EstructuraCamelService" in code, svc and config file together.
public class Rpt_EstructuraCamelService : IService<Rpt_EstructuraCamel>
{
    Concrete.Rpt_EstructuraCamelRepository rep = new Concrete.Rpt_EstructuraCamelRepository();

    public void Add(ref Rpt_EstructuraCamel entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Rpt_EstructuraCamel Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Rpt_EstructuraCamel>> GetAllAsync()
    {
        List<Rpt_EstructuraCamel> list = await rep.GetAll();
        return list;
    }

    public void Update(Rpt_EstructuraCamel entity)
    {
        rep.Update(entity);
    }
}
