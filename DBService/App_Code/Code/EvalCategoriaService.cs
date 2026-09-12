using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;
using DataBase.Concrete.Evaluacion;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Error" in code, svc and config file together.
public class EvalCategoriaService : IEvalCategoriaService
{
    EvalCategoriaRepository rep = new EvalCategoriaRepository();

    public void Add(ref EvalCategoria entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public EvalCategoria Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<EvalCategoria>> GetAllAsync()
    {
        List<EvalCategoria> list = await rep.GetAll();
        return list;
    }

    public async Task<List<EvalCategoria>> GetByUser(int idUser)
    {
        List<EvalCategoria> list = await rep.GetByUser(idUser);
        return list;
    }

    public void Update(EvalCategoria entity)
    {
        rep.Update(entity);
    }
}