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
public class EvalResponsableCategoriaService : IEvalResponsableCategoriaService
{
    EvalResponsableCategoriaRespository rep = new EvalResponsableCategoriaRespository();

    public void Add(ref EvalResponsableCategoria entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public int Existe(string idEntidad, int idUsuario, int idCategoria)
    {
        return rep.Existe(idEntidad, idUsuario, idCategoria);
    }

    public EvalResponsableCategoria Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<EvalResponsableCategoria>> GetAllAsync()
    {
        List<EvalResponsableCategoria> list = await rep.GetAll();
        return list;
    }

    public Task<List<EvalResponsableCategoria>> GetByEntidad(string id)
    {
        return rep.GetByEntidad(id);
    }

    public Task<List<EvalResponsableCategoria>> GetByUsusario(int id)
    {
        return rep.GetByUsusario(id);
    }

    public void Update(EvalResponsableCategoria entity)
    {
        rep.Update(entity);
    }
}