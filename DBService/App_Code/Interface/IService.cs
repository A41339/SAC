using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IService<T>
{


    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<T>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    T Get(string id);

    [OperationContract]
    void Add(ref T entity);

    [OperationContract]
    void Update(T entity);

    [OperationContract]
    void Delete(string id);

    
}
