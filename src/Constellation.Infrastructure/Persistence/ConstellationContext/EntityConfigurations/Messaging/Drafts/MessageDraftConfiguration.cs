namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Messaging.Drafts;

using Converters;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Drafts.Identifiers;
using Core.Models.Messaging.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MessageDraftConfiguration : IEntityTypeConfiguration<MessageDraft>
{
    public void Configure(EntityTypeBuilder<MessageDraft> builder)
    {
        builder.ToTable("Drafts", "Messages");

        builder
            .HasKey(draft => draft.UserId);

        builder
            .Property(draft => draft.Type)
            .HasConversion(
                type => type.Value,
                value => MessageType.FromValue(value));

        builder
            .OwnsMany(draft => draft.Recipients,
                navigation =>
                {
                    navigation
                        .Property(recipient => recipient.Id)
                        .HasConversion(
                            id => id.Value,
                            value => new MessageRecipientId(value));

                    navigation
                        .Property(recipient => recipient.EmailAddress)
                        .HasConversion<EmailAddressConverter>();

                    navigation
                        .Property(recipient => recipient.PhoneNumber)
                        .HasConversion<PhoneNumberConverter>();

                    navigation.ToJson();
                });
    }
}
