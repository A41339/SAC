using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FGA.Models;

namespace Concrete
{
    public class CargaAsincronicaRepository : FGA.Concrete.Repository<CargaAsincronica>
    {
        public CargaAsincronicaRepository()
        {
        }

        public override CargaAsincronica Get(string id)
        {
            long log = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == log);
        }

        public async Task<List<CargaAsincronica>> GetPendientes()
        {
            return await DbSet.Where(o => o.IndEstado == 0).ToListAsync();
        }
    }
}