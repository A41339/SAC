using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface ITareaService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Tarea> GetBySolicitud(int solicitud, int estado);
      
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Tarea>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Tarea Get(string id);

    [OperationContract]
    void Add(ref Tarea entity);

    [OperationContract]
    void Update(Tarea entity);

    [OperationContract]
    void Delete(string id);

}
