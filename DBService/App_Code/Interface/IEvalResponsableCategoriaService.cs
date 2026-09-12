using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Entities.Entities.Evaluacion;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IEvalResponsableCategoriaService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalResponsableCategoria>> GetByUsusario(int id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalResponsableCategoria>> GetByEntidad(string id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    int Existe(string idEntidad, int idUsuario, int idCategoria);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<EvalResponsableCategoria>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    EvalResponsableCategoria Get(string id);

    [OperationContract]
    void Add(ref EvalResponsableCategoria entity);

    [OperationContract]
    void Update(EvalResponsableCategoria entity);

    [OperationContract]
    void Delete(string id);
}
