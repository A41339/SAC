using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ISugef_Info_ContableService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Sugef_Info_Contable>> GetAllAsync();
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Sugef_Info_Contable Get(string id);

    [OperationContract]
    void Add(ref Sugef_Info_Contable entity);

    [OperationContract]
    void Update(Sugef_Info_Contable entity);

    [OperationContract]
    void Delete(string id);


}
