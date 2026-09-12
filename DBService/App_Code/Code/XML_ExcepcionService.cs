using FGA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "XMLExcepcion_Service" in code, svc and config file together.
public class XML_ExcepcionService : IXML_ExcepcionService
{
    readonly Concrete.XML_ExcepcionRepository rep = new Concrete.XML_ExcepcionRepository();

    public void Add(ref XML_Excepcion entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(XML_Excepcion entity)
    {
        rep.Delete(entity);
    }
    
    public async Task<List<XML_Excepcion>> GetAllAsync()
    {
        List<XML_Excepcion> list = await rep.GetAll();
        return list;
    }

    public List<XML_Excepcion> GetByFile(string idFile)
    {
        List<XML_Excepcion> list = rep.GetByFile(idFile);
        return list;
    }

    public void Update(XML_Excepcion entity)
    {
        rep.Update(entity);
    }
}
