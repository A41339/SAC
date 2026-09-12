using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Usuarios" in code, svc and config file together.
public class TareaService : ITareaService
{
    Concrete.TareaRepository rep = new Concrete.TareaRepository();
      
    public void Add(ref Tarea entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Tarea Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Tarea>> GetAllAsync()
    {
        List<Tarea> list = await rep.GetAll();
        return list;
    }

    public List<Tarea> GetBySolicitud(int solicitud, int estado)
    {
       return rep.GetBySolicitud(solicitud, estado);
    }

    public void Update(Tarea entity)
    {
        rep.Update(entity);
    }
}
