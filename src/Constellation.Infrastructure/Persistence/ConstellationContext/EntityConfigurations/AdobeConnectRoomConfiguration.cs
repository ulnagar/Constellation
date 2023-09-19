namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AdobeConnectRoomConfiguration : IEntityTypeConfiguration<AdobeConnectRoom>
{
    public void Configure(EntityTypeBuilder<AdobeConnectRoom> builder)
    {
        builder
            .HasKey(r => r.ScoId);

        builder
            .Property(r => r.ScoId)
            .ValueGeneratedNever();
    }
}