using System;
using System.ServiceModel;
using FGA.Models;

[ServiceContract]
public interface IProyeccionService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Proyeccion Get(string id);

    [OperationContract]
    void Add(ref Proyeccion entity);

    [OperationContract]
    void Update(Proyeccion entity);

    [OperationContract]
    void Delete(string id);

    [OperationContract]
    TipoProyeccion Existe(string IdEntidad, DateTime Periodo, String Cuenta);

}
