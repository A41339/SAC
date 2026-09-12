using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;


namespace FGA.Maping
{
    public class Sugef_EncabezadoMap : EntityTypeConfiguration<Sugef_Encabezado>
    {
        public Sugef_EncabezadoMap()
        {        
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.FechaCarga);
            Property(o => o.Periodo);
            Property(o => o.Cantidad);
            Property(o => o.IdEstado_Id);
            Property(o => o.IdUsuario_Id);
            Property(o => o.Nombre);

            HasRequired(c => c.IdEstado).WithMany(o => o.Sugef_Encabezados).HasForeignKey(o => o.IdEstado_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdUsuario).WithMany(o => o.Sugef_Encabezados).HasForeignKey(o => o.IdUsuario_Id).WillCascadeOnDelete(false);


            ToTable("Sugef_Encabezado");            
        }
    }
}
