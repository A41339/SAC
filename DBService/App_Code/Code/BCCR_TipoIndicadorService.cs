using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TBPService" in code, svc and config file together.
public class BCCR_TipoIndicadorService : IBCCR_TipoIndicadorService
{
    Concrete.BCCR_TipoIndicadorRepository rep = new Concrete.BCCR_TipoIndicadorRepository();

    public void Add(ref BCCR_TipoIndicador entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public BCCR_TipoIndicador Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<BCCR_TipoIndicador>> GetAllAsync()
    {
        List<BCCR_TipoIndicador> list = await rep.GetAll();
        return list;
    }

    public void Update(BCCR_TipoIndicador entity)
    {
        rep.Update(entity);
    }
}
