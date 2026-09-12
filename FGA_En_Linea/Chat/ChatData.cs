using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FGA.Models
{
    public class ChatData
    {
        public static ChatData Instance = new ChatData();

        public class User
        {
            public string Id { get; set; } = "";
            public string Company { get; set; } = "";
            public string Name { get; set; } = "";
            public string Status { get; set; } = "";
            public List<string> Contacts { get; set; } = new List<string>();
            public string Logo { get; set; } = "";
            public string ConnectionId { get; set; } = "";
        }
               
        public static  void AddMessageToHistory(string userName, string contactName, string message)
        {
            using (var msg = new FGA_En_Linea.MessageService.MessageServiceClient())
            {
                Message history = new Message()
                {
                    Content = message,
                    FromUser = userName,
                    ToUser = contactName,
                    Time = DateTime.Now
                };
                            
                 msg.Add(ref history);
            }
        }

        public static async Task<List<Message>> GetMessageHistory(string userName, string contactName, int unreadMessages)
        {
            using (var msg = new FGA_En_Linea.MessageService.MessageServiceClient())
            {
                var messages = await msg.GetLastMessagesAsync(userName, contactName, unreadMessages);
                return messages.OrderBy(o=>o.Time).ToList();
            }
        }
    }
}