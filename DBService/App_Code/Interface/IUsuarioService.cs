using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IUsuarioService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Usuario GetByEmail(string email);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Usuario GetByCredentials(string id, string password);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    FGA.Models.Usuario GetByIden(string identificacion);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<FGA.Models.Usuario> GetByCompany(string idEntidad);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Usuario>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Usuario Get(string id);

    [OperationContract]
    void Add(ref Usuario entity);

    [OperationContract]
    void Update(Usuario entity);

    [OperationContract]
    void Delete(string id);

}
