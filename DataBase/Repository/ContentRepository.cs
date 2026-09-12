using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using FGA.Models;

namespace FGA.Concrete
{
    public class ContentRepository<T> where T : class
    {
        protected ContentContext context;

        protected DbSet<T> DbSet { get; set; }

        public ContentRepository()
        {
            context = new ContentContext();
            DbSet = context.Set<T>();
        }

        public ContentRepository(ContentContext context)
        {
            this.context = context;
        }

        public virtual List<T> GetAllJoin(String reference1, String reference2, String reference3, String reference4)
        {
            if (!string.IsNullOrEmpty(reference1)) {
                if (!string.IsNullOrEmpty(reference2))
                {
                    if (!string.IsNullOrEmpty(reference3))
                    {
                        if (!string.IsNullOrEmpty(reference4))
                        {
                            return DbSet.Include(reference1).Include(reference2).Include(reference3).Include(reference4).ToList();
                        }
                        else {
                            return DbSet.Include(reference1).Include(reference2).Include(reference3).ToList();
                        }
                    }
                    else {
                        return DbSet.Include(reference1).Include(reference2).ToList();
                    }
                }
                else {
                    return DbSet.Include(reference1).ToList();
                }
            }
            else {
                return DbSet.ToList();
            }
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
