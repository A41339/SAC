using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class XML_ExcepcionMap : EntityTypeConfiguration<XML_Excepcion>
    {
        public XML_ExcepcionMap()
        {   
            HasKey(q => new
            {
                q.IdArchivo_Id,
                q.IdEntidad_Id
            });

            HasRequired(t => t.IdArchivo)
            .WithMany(t => t.Excepciones)
            .HasForeignKey(t => t.IdArchivo_Id);

            HasRequired(t => t.IdEntidad)
            .WithMany(t => t.Excepciones)
            .HasForeignKey(t => t.IdEntidad_Id);
            
            ToTable("XML_Excepcion");            
        }
    }
}
