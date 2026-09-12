using System;
using System.ServiceModel;
using System.Threading.Tasks;

[ServiceContract]
public interface IProcessFileService
{
    [OperationContract]
    string SaveFile(string zipPath, string fileName, string idEntidad, int idUsuario, DateTime fechaCarga);
        
    bool Process(string xmlPath, Int64 idEncabezado);

    [OperationContract(IsOneWay = true)]
    void ProcessIndustria(string xlsPath, Int64 idEncabezado);

    [OperationContract(IsOneWay = true)]
    void ProcessIndicador(string xlsPath, Int64 idEncabezado);

    [OperationContract(IsOneWay = true)]
    void ProcessCartera(string xlsPath, Int64 idEncabezado);
}