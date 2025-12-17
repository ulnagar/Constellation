namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.EmergencyConsole;

using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SentMessageConfiguration :IEntityTypeConfiguration<SentMessage>
{
    public void Configure(EntityTypeBuilder<SentMessage> builder)
    {
        builder.ToTable("SentMessages", "EmergencyConsole");

        builder
            .HasKey(message => message.Id);

        builder
            .Property(message => message.Id)
            .HasConversion(
                id => id.Value,
                value => EventId.FromValue(value));

        builder
            .HasMany(message => message.Statuses)
            .WithOne()
            .HasForeignKey(status => status.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(message => message.Statuses)
            .AutoInclude();
    }
}