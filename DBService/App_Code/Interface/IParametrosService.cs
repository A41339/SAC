using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IParametrosService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Parametros>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Parametros Get(string id);

    [OperationContract]
    void Add(ref Parametros entity);

    [OperationContract]
    void Update(Parametros entity);

    [OperationContract]
    void Delete(string id);
}
