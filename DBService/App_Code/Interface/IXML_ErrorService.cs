using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IXML_ErrorService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA.Models.XML_Errores> GetByEncabezado(long id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<XML_Errores>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    XML_Errores Get(string id);

    [OperationContract]
    void Add(ref XML_Errores entity);

    [OperationContract]
    void Update(XML_Errores entity);

    [OperationContract]
    void Delete(string id);

}
