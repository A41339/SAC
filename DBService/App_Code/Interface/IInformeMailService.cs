using FGA.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

[ServiceContract]
public interface IInformeMailService {

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<InformeMail> GetInformes(string idEntidad, DateTime Periodo);

    [OperationContract]
    List<DateTime> ListaPeriodos(string idEntidad);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<InformeMail>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    InformeMail Get(string id);

    [OperationContract]
    void Add(ref InformeMail entity);

    [OperationContract]
    void Update(InformeMail entity);

    [OperationContract]
    void Delete(string id);
}
