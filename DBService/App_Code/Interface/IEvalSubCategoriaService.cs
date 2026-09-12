using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IEvalSubCategoriaService
{
  
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalSubCategoria>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalSubCategoria>> GetByCategory(int id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    EvalSubCategoria Get(string id);

    [OperationContract]
    void Add(ref EvalSubCategoria entity);

    [OperationContract]
    void Update(EvalSubCategoria entity);

    [OperationContract]
    void Delete(string id);
}
