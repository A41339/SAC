using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InformeMailService : IInformeMailService
{
    Concrete.InformeMailRepository rep = new Concrete.InformeMailRepository();

    public List<InformeMail> GetInformes(string idEntidad, DateTime Periodo)
    {
        return rep.GetInformes(idEntidad, Periodo);
    }

    public void Add(ref InformeMail entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public InformeMail Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<InformeMail>> GetAllAsync()
    {
        List<InformeMail> list = await rep.GetAll();
        return list;
    }

    public List<DateTime> ListaPeriodos(string idEntidad)
    {
       return rep.ListaPeriodos(idEntidad);
    }

    public void Update(InformeMail entity)
    {
        rep.Update(entity);
    }
}