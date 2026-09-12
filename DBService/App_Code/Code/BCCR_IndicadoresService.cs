using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Concrete;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TBPService" in code, svc and config file together.
public class BCCR_IndicadoresService : IBCCR_IndicadoresService
{
    Concrete.BCCR_IndicadoresRepository rep = new Concrete.BCCR_IndicadoresRepository();

    public void Add(ref BCCR_Indicadores entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public BCCR_Indicadores Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<BCCR_Indicadores>> GetByType(int idType)
    {
        return await rep.GetByType(idType);
    }

    public List<IndicadorRecienteDTO> GetUltimosValoresPorIndicador() {
        return rep.GetUltimosValoresPorIndicador();
    }

    public async Task<List<BCCR_Indicadores>> GetAllAsync()
    {
        List<BCCR_Indicadores> list = await rep.GetAll();
        return list;
    }

    public void Update(BCCR_Indicadores entity)
    {
        rep.Update(entity);
    }
}
