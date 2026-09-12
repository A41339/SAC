using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Usuarios" in code, svc and config file together.
public class XML_EncabezadoService : IXML_EncabezadoService, IDisposable
{
    Concrete.XML_EncabezadoRepository rep = new Concrete.XML_EncabezadoRepository();
    
    public List<XML_Encabezado> GetByCompany(string idEntidad)
    {
        return rep.GetByCompany(idEntidad);
    }

    public List<XML_Encabezado> GetMonthFiles(string idEntidad, DateTime periodo)
    {
        return rep.GetMonthFiles(idEntidad, periodo);
    }

    public Boolean IsLoadingFile(string idEntidad, DateTime periodo)
    {
       return rep.IsLoadingFile(idEntidad, periodo);
    }

    public Boolean IsUpload(string idEntidad, string idArchivo, DateTime periodo)
    {
        return rep.IsUpload(idEntidad, idArchivo, periodo);
    }

    public void Add(ref XML_Encabezado entity)
    {
        rep.Add(ref entity);
    }
    
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public XML_Encabezado Get(string id)
    {
        return rep.Get(id);
    }

    public async Task<List<XML_Encabezado>> GetAllAsync()
    {
        List<XML_Encabezado> list = await rep.GetAll();
        return list;
    }

    public void Update(XML_Encabezado entity)
    {
        rep.Update(entity);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}