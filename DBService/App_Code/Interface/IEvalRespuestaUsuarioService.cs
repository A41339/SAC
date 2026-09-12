using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IEvalRespuestaUsuarioService
{
   
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalRespuestaUsuario>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    EvalRespuestaUsuario Get(string id);

    [OperationContract]
    void Add(ref EvalRespuestaUsuario entity);

    [OperationContract]
    void Update(EvalRespuestaUsuario entity);

    [OperationContract]
    void Delete(string id);
}
