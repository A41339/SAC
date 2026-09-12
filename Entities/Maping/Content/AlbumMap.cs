using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class AlbumMap : EntityTypeConfiguration<Album>
    {
        public AlbumMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Autor).HasMaxLength(150);
            Property(o => o.Detalle);
            Property(o => o.Titulo).HasMaxLength(50);
            Property(o => o.Fecha);
            Property(o => o.TipoAlbum_Id);

            HasRequired(c => c.Tipo).WithMany(o => o.Album_Ids).HasForeignKey(o => o.TipoAlbum_Id).WillCascadeOnDelete(false);


            ToTable("Album");

        }
    }
}
