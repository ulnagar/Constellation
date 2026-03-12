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
            .OwnsOne(e => e.Recipient, owned =>
            {
                owned
                    .Property(r => r.Name)
                    .HasColumnName("Name")
                    .IsRequired()
                    .HasMaxLength(200);

                owned
                    .Property(r => r.Email)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(320);
            });

        builder
            .HasKey(e => new
            {
                e.EmailId, 
                Email = EF.Property<string>(e, "Recipient.Email")
            });

        builder
            .Property(e => e.RecipientType)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder
            .HasIndex(e => new
            {
                e.EmailId,
                e.RecipientType
            })
            .HasDatabaseName("IX_Messages_EmailRecipients_EmailId_RecipientType");

        builder
            .HasIndex(e => EF.Property<string>(e, "Recipient.Email"))
            .HasDatabaseName("IX_Messages_EmailRecipients_Email");
    }
}