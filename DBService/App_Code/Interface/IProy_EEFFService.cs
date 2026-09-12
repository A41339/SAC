using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IProy_EEFFService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Proy_EEFF GetProy(string idEntidad, DateTime PeriodoCorte, String TipoPlazo);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Proy_EEFF>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Proy_EEFF Get(string id);

    [OperationContract]
    void Add(ref Proy_EEFF entity);

    [OperationContract]
    void Update(Proy_EEFF entity);

    [OperationContract]
    void Delete(string id);

}
