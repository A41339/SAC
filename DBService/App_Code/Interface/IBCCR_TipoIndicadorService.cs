using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TBPService" in code, svc and config file together.
[ServiceContract]
public interface IBCCR_TipoIndicadorService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<BCCR_TipoIndicador>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    BCCR_TipoIndicador Get(string id);

    [OperationContract]
    void Add(ref BCCR_TipoIndicador entity);

    [OperationContract]
    void Update(BCCR_TipoIndicador entity);

    [OperationContract]
    void Delete(string id);
}