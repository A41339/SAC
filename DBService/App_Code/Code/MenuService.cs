using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Menu" in code, svc and config file together.
public class MenuService : IService<Menu>
{
    Concrete.MenuRepository rep = new Concrete.MenuRepository();

    public void Add(ref Menu entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Menu Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Menu>> GetAllAsync()
    {
        List<Menu> list = await rep.GetAll();
        return list;
    }

    public void Update(Menu entity)
    {
        rep.Update(entity);
    }
}
