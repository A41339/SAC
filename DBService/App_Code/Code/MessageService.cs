using FGA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MessageService : IMessageService
{
    Concrete.MessageRepository rep = new Concrete.MessageRepository();

    public void Add(ref Message entity)
    {
        rep.Add(ref entity);
    }
    public void Delete(String id)
    {
        rep.Delete(id);
    }

    public Message Get(string id)
    {
        return rep.Get(id);
    }

    public List<Message> GetLastMessages(string toUser, string fromUser, int num_LastMessages)
    {
        return rep.GetLastMessages(toUser, fromUser, num_LastMessages);
    }

    public async Task<List<Message>> GetAllAsync()
    {
        List<Message> list = await rep.GetAll();
        return list;
    }

    public void Update(Message entity)
    {
        rep.Update(entity);
    }
}
