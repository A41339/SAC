using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Entidades" in code, svc and config file together.
public class Sugef_Info_ContableService : ISugef_Info_ContableService
{
    Concrete.Sugef_Info_ContableRepository rep = new Concrete.Sugef_Info_ContableRepository();

    public void Add(ref Sugef_Info_Contable entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Sugef_Info_Contable Get(string id)
    {
        return rep.Get(id);
    }


    public async Task<List<Sugef_Info_Contable>> GetAllAsync()
    {
        List<Sugef_Info_Contable> list = await rep.GetAll();
        return list;
    }

    public void Update(Sugef_Info_Contable entity)
    {
        rep.Update(entity);
    }
}
