namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Views;

using ConstellationContext.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class MessagingHistoryIndexRowConfiguration : IEntityTypeConfiguration<MessagingHistoryIndexRow>
{
    public void Configure(EntityTypeBuilder<MessagingHistoryIndexRow> builder)
    {
        builder.HasNoKey();
        builder.ToView("MessagingHistoryIndex", "Messages");
    }
}
