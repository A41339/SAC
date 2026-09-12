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
public class EvalRespuestaUsuarioService : IService<EvalRespuestaUsuario>
{
    EvalRespuestaUsuarioRepository rep = new EvalRespuestaUsuarioRepository();

    public void Add(ref EvalRespuestaUsuario entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public EvalRespuestaUsuario Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<EvalRespuestaUsuario>> GetAllAsync()
    {
        List<EvalRespuestaUsuario> list = await rep.GetAll();
        return list;
    }

    public void Update(EvalRespuestaUsuario entity)
    {
        rep.Update(entity);
    }
}