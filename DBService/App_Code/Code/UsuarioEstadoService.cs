using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UsuarioEstado" in code, svc and config file together.
public class UsuarioEstadoService : IService<UsuarioEstado>
{
    Concrete.UsuarioEstadoRepository rep = new Concrete.UsuarioEstadoRepository();

    public void Add(ref UsuarioEstado entity)
    {
        rep.Add(ref entity);
    }
    
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public UsuarioEstado Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<UsuarioEstado>> GetAllAsync()
    {
        List<UsuarioEstado> list = await rep.GetAll();
        return list;
    }

    public void Update(UsuarioEstado entity)
    {
        rep.Update(entity);
    }
}
