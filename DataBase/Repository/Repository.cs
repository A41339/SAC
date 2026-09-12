using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using FGA.Models;

namespace FGA.Concrete
{
    public class Repository<T> where T : class
    {
        protected SIContext context;

        protected DbSet<T> DbSet { get; set; }

        public Repository()
        {
            context = new SIContext();
            DbSet = context.Set<T>();
        }

        public Repository(SIContext context)
        {
            this.context = context;
        }

        public async virtual Task<List<T>> GetAll()
        {
            return await DbSet.ToListAsync();
        }

        public virtual T Get(string id)
        {
            return DbSet.Find(id);
        }

        public virtual void Add(ref T entity)
        {
            DbSet.Add(entity);
            context.SaveChanges();
            context.Entry(entity).GetDatabaseValues();
        }

        public virtual void Update(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }

        public virtual void Delete(String id)
        {
            T entity = Get(id);
            DbSet.Remove(entity);
            context.SaveChanges();
        }

        public  void Dispose()
        {
            context.Dispose();
        }
    }
}