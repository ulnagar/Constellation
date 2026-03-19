namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Email;

using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.ToTable("Email", "Messages");

        builder
            .HasKey(e => e.Id);

        builder
            .Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => EmailId.FromValue(value));

        // Owned type — flattened into EmailMessages table as From_Name, From_Email
        builder
            .OwnsOne(e => e.From, owned =>
            {
                owned.WithOwner();
                
                owned
                    .Property(r => r.Name)
                    .HasColumnName("From_Name")
                    .IsRequired()
                    .HasMaxLength(200);

                owned
                    .Property(r => r.Email)
                    .HasColumnName("From_Email")
                    .IsRequired()
                    .HasMaxLength(320);
            });

        // Nullable owned type for ReplyTo
        builder
            .OwnsOne(e => e.ReplyTo, owned =>
            {
                owned.WithOwner();

                owned
                    .Property(r => r.Name)
                    .HasColumnName("ReplyTo_Name")
                    .HasMaxLength(200);

                owned
                    .Property(r => r.Email)
                    .HasColumnName("ReplyTo_Email")
                    .HasMaxLength(320);
            });

        builder
            .Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(998);

        builder
            .Property(e => e.BodyText)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder
            .Property(e => e.BodyHtml)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder
            .Property(e => e.Provider)
            .HasMaxLength(100);

        builder
            .Property(e => e.ProviderMessageId)
            .HasMaxLength(500);

        builder
            .Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder
            .Property(e => e.TemplateId)
            .HasMaxLength(200);

        builder
            .Property(e => e.Tags)
            .HasColumnType("nvarchar(max)");

        builder
            .Property(e => e.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder
            .HasMany(e => e.Recipients)
            .WithOne()
            .HasForeignKey(e => e.EmailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Recipients)
            .HasField("_recipients")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(e => e.Recipients)
            .AutoInclude();

        builder
            .HasMany(e => e.TrackingEvents)
            .WithOne()
            .HasForeignKey(e => e.EmailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.TrackingEvents)
            .HasField("_events")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder
            .HasIndex(e => e.ProviderMessageId)
            .HasDatabaseName("IX_Messages_Email_ProviderMessageId");

        builder
            .HasIndex(e => e.Status)
            .HasDatabaseName("IX_Messages_Email_Status");

        builder
            .HasIndex(e => e.SentAt)
            .HasDatabaseName("IX_Messages_Email_SentAt");
    }
}
