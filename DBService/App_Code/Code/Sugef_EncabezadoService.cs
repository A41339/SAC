using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Entidades" in code, svc and config file together.
public class Sugef_EncabezadoService : ISugef_EncabezadoService
{
    Concrete.Sugef_EncabezadoRepository rep = new Concrete.Sugef_EncabezadoRepository();

    public void Add(ref Sugef_Encabezado entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Sugef_Encabezado Get(string id)
    {
        return rep.Get(id);
    }


    public async Task<List<Sugef_Encabezado>> GetAllAsync()
    {
        List<Sugef_Encabezado> list = await rep.GetAll();
        return list;
    }

    public void Update(Sugef_Encabezado entity)
    {
        rep.Update(entity);
    }
}
