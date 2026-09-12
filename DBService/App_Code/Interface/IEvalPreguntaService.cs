using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IEvalPreguntaService
{
  
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalPregunta>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalPregunta>> GetBySubCategory(int id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    EvalPregunta Get(string id);

    [OperationContract]
    void Add(ref EvalPregunta entity);

    [OperationContract]
    void Update(EvalPregunta entity);

    [OperationContract]
    void Delete(string id);
}
