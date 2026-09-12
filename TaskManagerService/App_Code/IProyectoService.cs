using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IProyectoService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Proyecto>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Proyecto Get(string id);

    [OperationContract]
    void Add(ref Proyecto entity);

    [OperationContract]
    void Update(Proyecto entity);

    [OperationContract]
    void Delete(string id);

}
