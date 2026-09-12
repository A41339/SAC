using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class InformeMailMap : EntityTypeConfiguration<InformeMail>
    {
        public InformeMailMap()
        {           
            HasKey(o => o.Id);
            Property(o => o.Archivo);
            Property(o => o.Destinatarios);
            Property(o => o.FechaCarga);
            Property(o => o.IdEntidad_Id);
            Property(o => o.IdInforme_Id);
            Property(o => o.IdUsuario_Id);
            Property(o => o.Periodo);
            Property(o => o.Texto);
            Property(o => o.Enviado);

            HasRequired(c => c.IdUsuario).WithMany(o => o.Informes).HasForeignKey(o => o.IdUsuario_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdInforme).WithMany(o => o.Informes).HasForeignKey(o => o.IdInforme_Id).WillCascadeOnDelete(false);
            HasRequired(c => c.IdEntidad).WithMany(o => o.Informes).HasForeignKey(o => o.IdEntidad_Id).WillCascadeOnDelete(false);

            ToTable("InformeMail");            
        }
    }
}
