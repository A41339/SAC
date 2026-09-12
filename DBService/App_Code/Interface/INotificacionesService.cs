using FGA.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

[ServiceContract]
public interface INotificacionesService {

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Notificaciones>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Notificaciones Get(string id);

    [OperationContract]
    void Add(ref Notificaciones entity);

    [OperationContract]
    void Update(Notificaciones entity);

    [OperationContract]
    void Delete(string id);
}
