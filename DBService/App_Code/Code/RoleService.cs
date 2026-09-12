using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Role" in code, svc and config file together.
public class RoleService : IService<Role>
{
    Concrete.RoleRepository rep = new Concrete.RoleRepository();

    public void Add(ref Role entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Role Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Role>> GetAllAsync()
    {
        List<Role> list = await rep.GetAll();
        return list;
    }

    public void Update(Role entity)
    {
        rep.Update(entity);
    }
}
