using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class MenuPermissionMap : EntityTypeConfiguration<MenuPermission> 
    {
        public MenuPermissionMap()
        {

            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasOptional(c => c.Menu_MenuId).WithMany(o => o.MenuPermission_MenuIds).HasForeignKey(o => o.MenuId);
            HasRequired(c => c.Role_RoleId).WithMany(o => o.MenuPermission_RoleIds).HasForeignKey(o => o.RoleId).WillCascadeOnDelete(true);
            ToTable("MenuPermission");


        }
    }
}
