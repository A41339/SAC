using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ICargaAsincronicaService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<CargaAsincronica>> GetAllAsync();
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    CargaAsincronica Get(string id);

    [OperationContract]
    void Add(ref CargaAsincronica entity);

    [OperationContract]
    void Update(CargaAsincronica entity);

    [OperationContract]
    void Delete(string id);

    [OperationContract] 
    Task<List<CargaAsincronica>> GetPendientesAsyc();
}
