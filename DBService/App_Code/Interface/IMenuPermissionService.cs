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
public interface IMenuPermissionService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<MenuPermission> GetMenu(int role);


    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<MenuPermission>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    MenuPermission Get(string id);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    void Add(ref MenuPermission entity);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    void Update(MenuPermission entity);

    [OperationContract]
    void Delete(string id);

}
