namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Email;

using Core.Models.Messaging.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EmailMessageRecipientConfiguration : IEntityTypeConfiguration<EmailMessageRecipient>
{
    public void Configure(EntityTypeBuilder<EmailMessageRecipient> builder)
    {
        builder.ToTable("EmailRecipients", "Messages");

        builder
            .HasKey(e => new
            {
                e.EmailId,
                e.Email
            });

        builder
            .Property(email => email.Name)
            .HasColumnName(nameof(EmailMessageRecipient.Name))
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(r => r.Email)
            .HasColumnName(nameof(EmailMessageRecipient.Email))
            .IsRequired()
            .HasMaxLength(320);

        builder
            .Property(e => e.RecipientType)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder
            .HasIndex(e => new { e.EmailId, e.RecipientType });

        builder
            .HasIndex(e => e.Email);
    }
}