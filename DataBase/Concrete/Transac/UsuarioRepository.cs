using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Concrete
{
    public class UsuarioRepository : FGA.Concrete.Repository<FGA.Models.Usuario>
    {
        public UsuarioRepository()
        {
        }

        public override FGA.Models.Usuario Get(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                int user = int.Parse(id);
                return DbSet.Include("Entidad_Usuario")
                    .Include("Role_Usuario")
                    .Include("Estado_Usuario")
                    .Include("Sexo_Usuario")
                    .FirstOrDefault(o => o.Id == user);
            }

            return null;
        }
        public async override Task<List<FGA.Models.Usuario>> GetAll()
        {
            return await DbSet.Include("Entidad_Usuario")
                .Include("Estado_Usuario")
                .Include("Role_Usuario")
                 .Include("Sexo_Usuario").ToListAsync();
        }

        public FGA.Models.Usuario GetByEmail(string correo)
        {
            return DbSet.FirstOrDefault(o => o.Correo == correo);
        }

        public FGA.Models.Usuario GetByCredentials(string id, string password)
        {
            return DbSet.Include("Entidad_Usuario")
                .Include("Role_Usuario")
                .Include("Estado_Usuario")
                 .Include("Sexo_Usuario")
                .FirstOrDefault(i => i.Identificacion == id && i.Contrasena == password);
        }

        public FGA.Models.Usuario GetByIden(string identificacion)
        {
            return DbSet.Include("Entidad_Usuario")
                .Include("Role_Usuario")
                .Include("Estado_Usuario")
                 .Include("Sexo_Usuario")
                .FirstOrDefault(i => i.Identificacion == identificacion);
        }

        public List<FGA.Models.Usuario> GetByCompany(string idEntidad)
        {
            return DbSet.Include("Entidad_Usuario")
                .Include("Role_Usuario")
                .Include("Estado_Usuario")
                 .Include("Sexo_Usuario")
                .Where(i => i.Entidad_Usuario.Id == idEntidad).ToList();
        }

        public override void Add(ref FGA.Models.Usuario entity)
        {
            entity.Entidad_Usuario = null;
            entity.Estado_Usuario = null;
            entity.Role_Usuario = null;
            entity.Sexo_Usuario = null;

            DbSet.Add(entity);
            context.SaveChanges();
            context.Entry(entity).GetDatabaseValues();
        }
    }
}