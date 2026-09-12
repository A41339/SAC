using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using FGA.Models;

namespace Concrete
{
    public class LogRepository : FGA.Concrete.Repository<Log>
    {
        public LogRepository()
        {
        }

        public override Log Get(string id)
        {
            int log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }
    }
}