namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MessageEventConfiguration :IEntityTypeConfiguration<MessageEvent>
{
    public void Configure(EntityTypeBuilder<MessageEvent> builder)
    {
        builder.ToTable("MessageEvents", "EmergencyConsole");

        builder
            .HasKey(message => message.Id);

        builder
            .Property(message => message.Id)
            .HasConversion(
                id => id.Value,
                value => EventId.FromValue(value));

        builder
            .HasMany(message => message.Recipients)
            .WithOne()
            .HasForeignKey(status => status.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(message => message.Recipients)
            .AutoInclude();
    }
}