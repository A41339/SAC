using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AlbumService" in code, svc and config file together.
public class Exclusion_PeriodoService : IExclusion_PeriodoService
{
    Concrete.Exclusion_PeriodoRepository rep = new Concrete.Exclusion_PeriodoRepository();

    public void Add(ref Exclusion_Periodos_Proyectar entity){
        rep.Add(ref entity);
    }

    public void Habilitar(DateTime Periodo, String Cuenta, String IdEntidad) {
        rep.Habilitar(Periodo, Cuenta, IdEntidad);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }
    public Exclusion_Periodos_Proyectar Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Exclusion_Periodos_Proyectar>> GetAllAsync()
    {
        List<Exclusion_Periodos_Proyectar> list = await rep.GetAll();
        return list;
    }

    public void Update(Exclusion_Periodos_Proyectar entity)
    {
        rep.Update(entity);
    }
}
