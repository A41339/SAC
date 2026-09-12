using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using FGA.Models;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEntidades" in both code and config file together.
[ServiceContract]
public interface IMessageService
{
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Message> GetLastMessages(string toUser, string fromUser, int num_LastMessages);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Task<List<Message>> GetAllAsync();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Message Get(string id);

    [OperationContract]
    void Add(ref Message entity);

    [OperationContract]
    void Update(Message entity);

    [OperationContract]
    void Delete(string id);

}
