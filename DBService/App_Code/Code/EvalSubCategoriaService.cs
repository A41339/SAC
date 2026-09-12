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
public class EvalSubCategoriaService : IEvalSubCategoriaService
{
    EvalSubCategoriaRepository rep = new EvalSubCategoriaRepository();

    public void Add(ref EvalSubCategoria entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public EvalSubCategoria Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<EvalSubCategoria>> GetAllAsync()
    {
        List<EvalSubCategoria> list = await rep.GetAll();
        return list;
    }

    public async Task<List<EvalSubCategoria>> GetByCategory(int id)
    {
        List<EvalSubCategoria> list = await rep.GetByCategory(id);
        return list;

    }

    public void Update(EvalSubCategoria entity)
    {
        rep.Update(entity);
    }
}