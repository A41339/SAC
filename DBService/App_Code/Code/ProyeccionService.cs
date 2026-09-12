using FGA.Models;
using System;

public class ProyeccionService : IProyeccionService
{
    Concrete.ProyeccionRepository rep = new Concrete.ProyeccionRepository();

    public void Add(ref Proyeccion entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(string id)
    {
        rep.Delete(id);
    }

    public TipoProyeccion Existe(string IdEntidad, DateTime Periodo, String Cuenta)
    {
       return rep.Existe(Periodo, IdEntidad, Cuenta);
    }

    public Proyeccion Get(string id)
    {
        return rep.Get(id);
    }

    public void Update(Proyeccion entity)
    {
       rep.Update(entity);
    }
}