using System.Collections.Generic;
using System.Linq;

namespace Concrete
{
    public class MessageRepository : FGA.Concrete.Repository<FGA.Models.Message>
    {
        public MessageRepository()
        {
        }

        public override FGA.Models.Message Get(string id)
        {
            int msg = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == msg);
        }

        public List<FGA.Models.Message> GetLastMessages(string toUser, string fromUser, int lastNumber)
        {           
            return DbSet.Where(o => (o.ToUser == toUser && o.FromUser == fromUser) || (o.ToUser == fromUser && o.FromUser == toUser)).OrderByDescending(o=>o.Time).Take(lastNumber).ToList();
        }
    }
}