namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.ToTable("Templates", "EmergencyConsole");

        builder
            .HasKey(template => template.Id);

        builder
            .Property(template => template.Id)
            .HasConversion(
                id => id.Value,
                value => TemplateId.FromValue(value));

        builder
            .Property(template => template.TemplateType)
            .HasConversion(
                type => type.Value,
                value => MessageType.FromValue(value));
    }
}
