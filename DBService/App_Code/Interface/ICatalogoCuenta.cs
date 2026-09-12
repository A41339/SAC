using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ICatalogoCuenta
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<CatalogoCuenta>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<CatalogoCuenta> GetPage(int pageNumber, int size, string filter, ref int records);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    CatalogoCuenta Get(string id);

    [OperationContract]
    void Add(ref CatalogoCuenta entity);

    [OperationContract]
    void Update(CatalogoCuenta entity);

    [OperationContract]
    void Delete(string id);


}
