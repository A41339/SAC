using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Error" in code, svc and config file together.
public class ErrorService : IService<Error>
{
    Concrete.ErrorRepository rep = new Concrete.ErrorRepository();

    public void Add(ref Error entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Error Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<Error>> GetAllAsync()
    {
        List<Error> list = await rep.GetAll();
        return list;
    }

    public void Update(Error entity)
    {
        rep.Update(entity);
    }
}
