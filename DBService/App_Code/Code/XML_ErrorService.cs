using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Usuarios" in code, svc and config file together.
public class XML_ErrorService : IXML_ErrorService
{
    Concrete.XML_ErroresRepository rep = new Concrete.XML_ErroresRepository();

    public List<FGA.Models.XML_Errores> GetByEncabezado(long id)
    {
        return rep.GetByEncabezado(id);
    }

    public void Add(ref XML_Errores entity)
    {
        rep.Add(ref entity);
    }

    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public XML_Errores Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<XML_Errores>> GetAllAsync()
    {
        List<XML_Errores> list = await rep.GetAll();
        return list;
    }

    public void Update(XML_Errores entity)
    {
        rep.Update(entity);
    }
}