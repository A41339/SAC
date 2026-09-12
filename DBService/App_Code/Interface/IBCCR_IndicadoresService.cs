using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Concrete;
using FGA.Models;

[ServiceContract]
public interface IBCCR_IndicadoresService
{

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<BCCR_Indicadores>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    BCCR_Indicadores Get(string id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<BCCR_Indicadores>> GetByType(int idType);

    [OperationContract]
    void Add(ref BCCR_Indicadores entity);

    [OperationContract]
    void Update(BCCR_Indicadores entity);

    [OperationContract]
    void Delete(string id);

    [OperationContract]
    List<IndicadorRecienteDTO> GetUltimosValoresPorIndicador();
}