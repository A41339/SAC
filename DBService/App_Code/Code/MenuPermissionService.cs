using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MenuPermission" in code, svc and config file together.
public class MenuPermissionService : IMenuPermissionService
{
    Concrete.MenuPermissionRepository rep = new Concrete.MenuPermissionRepository();

    public void Add(ref MenuPermission entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public MenuPermission Get(string id)
    {
        return rep.Get(id);
    }

    public List<MenuPermission> GetMenu(int role)
    {
        return rep.GetMenu(role);
    }

    public async Task<List<MenuPermission>> GetAllAsync()
    {
        List<MenuPermission> list = await rep.GetAll();
        return list;
    }

    public void Update(MenuPermission entity)
    {
        rep.Update(entity);
    }
}
