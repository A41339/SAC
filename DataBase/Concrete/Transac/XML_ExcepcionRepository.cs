using FGA.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Concrete
{
    public class XML_ExcepcionRepository : FGA.Concrete.Repository<XML_Excepcion>
    {
        public XML_ExcepcionRepository()
        {
        }
        
        public async override Task<List<XML_Excepcion>> GetAll()
        {
            return await  DbSet.ToListAsync();
        }

        public List<XML_Excepcion> GetByFile(string idFile)
        {
            return DbSet.Where(o=>o.IdArchivo_Id == idFile).ToList();
        }

        public void Delete(XML_Excepcion entity)
        {
            XML_Excepcion delete = DbSet.FirstOrDefault(o => o.IdArchivo_Id == entity.IdArchivo_Id && o.IdEntidad_Id == entity.IdEntidad_Id);

            DbSet.Remove(delete);
            context.SaveChanges();
        }
    }
}