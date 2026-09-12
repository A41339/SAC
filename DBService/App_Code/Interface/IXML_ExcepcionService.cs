using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IXML_ExcepcionService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<XML_Excepcion>> GetAllAsync();

    [OperationContract]
    void Add(ref XML_Excepcion entity);

    [OperationContract]
    void Update(XML_Excepcion entity);

    [OperationContract]
    void Delete(XML_Excepcion id);

    [OperationContract]
    List<XML_Excepcion> GetByFile(string idFile);


}
