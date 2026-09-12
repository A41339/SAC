using FGA.Models;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class MessageMap : EntityTypeConfiguration<Message>
    {
        public MessageMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Content);
            Property(o => o.FromUser).HasMaxLength(150).IsRequired();
            Property(o => o.ToUser).HasMaxLength(150).IsRequired();
            Property(o => o.Time);

            ToTable("Message");

        }
    }
}

