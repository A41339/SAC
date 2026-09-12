using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CatalogoCuentaService : ICatalogoCuenta
{
    Concrete.CatalogoCuentaRepository rep = new Concrete.CatalogoCuentaRepository();

    public void Add(ref CatalogoCuenta entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public CatalogoCuenta Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<CatalogoCuenta>> GetAllAsync()
    {
        List<CatalogoCuenta> list = await rep.GetAll();
        return list;
    }

    public List<CatalogoCuenta> GetPage(int pageNumber, int size, string filter, ref int records)
    {
        List<CatalogoCuenta> list = rep.GetPage(pageNumber, size, filter, ref records);
        return list;
    }


    public void Update(CatalogoCuenta entity)
    {
        rep.Update(entity);
    }

}
