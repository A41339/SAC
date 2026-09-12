using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Usuarios" in code, svc and config file together.
public class UsuarioService : IUsuarioService
{
    Concrete.UsuarioRepository rep = new Concrete.UsuarioRepository();

    public FGA.Models.Usuario GetByIden (string identificacion){
       return rep.GetByIden(identificacion);
    }

    public List<FGA.Models.Usuario> GetByCompany(string idEntidad) {
       return rep.GetByCompany(idEntidad);
    }

    public Usuario GetByCredentials(string id, string password)
    {
        return rep.GetByCredentials(id, password);
    }

    public Usuario GetByEmail(string email)
    {
        return rep.GetByEmail(email);
    }

    public void Add(ref Usuario entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Usuario Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        List<Usuario> list = await rep.GetAll();
        return list;
    }


    public void Update(Usuario entity)
    {
        rep.Update(entity);
    }
}
