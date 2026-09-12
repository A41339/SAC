using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FGA.Models;

namespace Concrete
{
    public class MenuPermissionRepository : FGA.Concrete.Repository<FGA.Models.MenuPermission>
    {
        public MenuPermissionRepository()
        {
        }

        public List<FGA.Models.MenuPermission> GetMenu(int rol)
        {
            return DbSet.Include("Menu_MenuId").Where(o => o.RoleId == rol).ToList();
        }

        public async override Task<List<MenuPermission>> GetAll()
        {
            return await DbSet.Include("Menu_MenuId").ToListAsync();
        }
    }
}