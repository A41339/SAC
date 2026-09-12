using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ISectorService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Sector>> GetAllAsync();
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Sector Get(string id);

    [OperationContract]
    void Add(ref Sector entity);

    [OperationContract]
    void Update(Sector entity);

    [OperationContract]
    void Delete(string id);


}
