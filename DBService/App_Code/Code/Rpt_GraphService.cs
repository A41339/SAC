using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Rpt_Analisis_VHService" in code, svc and config file together.
public class Rpt_GraphService : IService<Rpt_Graph>
{
    Concrete.Rpt_GraphRepository rep = new Concrete.Rpt_GraphRepository();

    public void Add(ref Rpt_Graph entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Rpt_Graph Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Rpt_Graph>> GetAllAsync()
    {
        List<Rpt_Graph> list = await rep.GetAll();
        return list;
    }

    public void Update(Rpt_Graph entity)
    {
        rep.Update(entity);
    }
}
