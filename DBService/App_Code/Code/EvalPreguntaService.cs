using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;
using DataBase.Concrete.Evaluacion;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Error" in code, svc and config file together.
public class EvalPreguntaService : IEvalPreguntaService
{
    EvalPreguntaRepository rep = new EvalPreguntaRepository();

    public void Add(ref EvalPregunta entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public EvalPregunta Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<EvalPregunta>> GetAllAsync()
    {
        List<EvalPregunta> list = await rep.GetAll();
        return list;
    }

    public async Task<List<EvalPregunta>> GetBySubCategory(int id)
    {
        List<EvalPregunta> list = await rep.GetBySubCategory(id);
        return list;

    }

    public void Update(EvalPregunta entity)
    {
        rep.Update(entity);
    }
}