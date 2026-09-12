using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ISugef_EncabezadoService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Sugef_Encabezado>> GetAllAsync();
    
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Sugef_Encabezado Get(string id);

    [OperationContract]
    void Add(ref Sugef_Encabezado entity);

    [OperationContract]
    void Update(Sugef_Encabezado entity);

    [OperationContract]
    void Delete(string id);


}
