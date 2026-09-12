using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class RoleMap : EntityTypeConfiguration<Role> 
    {
        public RoleMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Nombre).HasMaxLength(50);
            Property(o => o.EsEntidad);
            Property(o => o.Url);

            ToTable("Role");
        }
    }
}