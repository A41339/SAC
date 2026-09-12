using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IEntidadService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<string> GetMails(string id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Entidad GetByIden(string id);
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Entidad>> GetAllAsync();
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Entidad Get(string id);

    [OperationContract]
    void Add(ref Entidad entity);

    [OperationContract]
    void Update(Entidad entity);

    [OperationContract]
    void Delete(string id);


}
