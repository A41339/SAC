using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace FGA.Maping
{
    public class MenuMap : EntityTypeConfiguration<Menu> 
    {
        public MenuMap()
        {
             HasKey(o => o.Id);
             Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
             Property(o => o.MenuText).HasMaxLength(100);
             Property(o => o.MenuURL).HasMaxLength(400);
             HasOptional(c => c.Menu2).WithMany(o => o.Menu_ParentIds).HasForeignKey(o => o.ParentId);
             Property(o => o.MenuIcon).HasMaxLength(100);
             ToTable("Menu");
 

        }
    }
}
