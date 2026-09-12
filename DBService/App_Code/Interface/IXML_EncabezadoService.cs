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
public interface IXML_EncabezadoService
{
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA.Models.XML_Encabezado> GetByCompany(string idEntidad);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA.Models.XML_Encabezado> GetMonthFiles(string idEntidad, DateTime periodo);

    [OperationContract]
    Boolean IsLoadingFile(string idEntidad, DateTime periodo);

    [OperationContract]
    Boolean IsUpload(string idEntidad, string idArchivo, DateTime periodo);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<XML_Encabezado>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    XML_Encabezado Get(string id);

    [OperationContract]
    void Add(ref XML_Encabezado entity);

    [OperationContract]

    void Update(XML_Encabezado entity);

    [OperationContract]
    void Delete(string id);


}
