using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IExclusion_PeriodoService
{

    [OperationContract]
    void Habilitar(DateTime Periodo, String Cuenta, String IdEntidad);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Exclusion_Periodos_Proyectar>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Exclusion_Periodos_Proyectar Get(string id);

    [OperationContract]
    void Add(ref Exclusion_Periodos_Proyectar entity);

    [OperationContract]
    void Update(Exclusion_Periodos_Proyectar entity);

    [OperationContract]
    void Delete(string id);

}
