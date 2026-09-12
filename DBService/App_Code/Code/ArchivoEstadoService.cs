using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ArchivoEstado" in code, svc and config file together.
public class ArchivoEstadoService : IService<ArchivoEstado>
{
    Concrete.ArchivoEstadoRepository rep = new Concrete.ArchivoEstadoRepository();

    public void Add(ref ArchivoEstado entity)
    {
       rep.Add(ref entity);
    }
    
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public ArchivoEstado Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<ArchivoEstado>> GetAllAsync()
    {
        List<ArchivoEstado> list = await rep.GetAll();
        return list;
    }
  
    public void Update(ArchivoEstado entity)
    {
        rep.Update(entity);
    }
}
