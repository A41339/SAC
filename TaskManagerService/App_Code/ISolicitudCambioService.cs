using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ISolicitudCambioService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<SolicitudCambio>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    SolicitudCambio Get(string id);

    [OperationContract]
    void Add(ref SolicitudCambio entity);

    [OperationContract]
    void Update(SolicitudCambio entity);

    [OperationContract]
    void Delete(string id);

}
