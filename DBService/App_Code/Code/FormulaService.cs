using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FormulaService : IService<Formulas>
{
    Concrete.FormulaRepository rep = new Concrete.FormulaRepository();

    public void Add(ref Formulas entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Formulas Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Formulas>> GetAllAsync()
    {
        List<Formulas> list = await rep.GetAll();
        return list;
    }

    public void Update(Formulas entity)
    {
        rep.Update(entity);
    }
}