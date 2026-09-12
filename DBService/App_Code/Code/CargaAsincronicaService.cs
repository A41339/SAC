using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "LiborService" in code, svc and config file together.
public class CargaAsincronicaService : ICargaAsincronicaService, IDisposable
{
    Concrete.CargaAsincronicaRepository rep = new Concrete.CargaAsincronicaRepository();

    public void Add(ref CargaAsincronica entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public CargaAsincronica Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<CargaAsincronica>> GetPendientesAsyc()
    {
        List<CargaAsincronica> list = await rep.GetPendientes();
        return list;
    }


    public async Task<List<CargaAsincronica>> GetAllAsync()
    {
        List<CargaAsincronica> list = await rep.GetAll();
        return list;
    }

    public void Update(CargaAsincronica entity)
    {
        rep.Update(entity);
    }
}
