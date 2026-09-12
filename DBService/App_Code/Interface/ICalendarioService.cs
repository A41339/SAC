using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

[ServiceContract]
public interface ICalendarioService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Calendario GetByDate(DateTime periodo);
      
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Calendario>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Calendario Get(string id);

    [OperationContract]
    void Add(ref Calendario entity);

    [OperationContract]
    void Update(Calendario entity);

    [OperationContract]
    void Delete(string id);
}
