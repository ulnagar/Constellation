namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Email;

using Core.Models.Messaging.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EmailLinkConfiguration : IEntityTypeConfiguration<EmailLink>
{
    public void Configure(EntityTypeBuilder<EmailLink> builder)
    {
        builder.ToTable("EmailLinks", "Messages");

        builder
            .HasKey(link => new { link.EmailId, link.UrlHash });

        builder
            .Property(link => link.UrlHash)
            .IsRequired()
            .HasColumnType("binary(32)");

        builder
            .Property(link => link.DestinationUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder
            .HasOne<EmailMessage>()
            .WithMany(message => message.Links)
            .HasForeignKey(link => link.EmailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(link => link.EmailId);
    }
}
