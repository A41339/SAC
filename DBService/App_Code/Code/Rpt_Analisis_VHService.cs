using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Rpt_Analisis_VHService" in code, svc and config file together.
public class Rpt_Analisis_VHService : IService<Rpt_Analisis_VH>
{
    Concrete.Rpt_Analisis_VHRepository rep = new Concrete.Rpt_Analisis_VHRepository();

    public void Add(ref Rpt_Analisis_VH entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Rpt_Analisis_VH Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Rpt_Analisis_VH>> GetAllAsync()
    {
        List<Rpt_Analisis_VH> list = await rep.GetAll();
        return list;
    }

    public void Update(Rpt_Analisis_VH entity)
    {
        rep.Update(entity);
    }
}
