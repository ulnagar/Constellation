using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class AdobeConnectRoomConfiguration : IEntityTypeConfiguration<AdobeConnectRoom>
    {
        public void Configure(EntityTypeBuilder<AdobeConnectRoom> builder)
        {
            builder.HasKey(r => r.ScoId);

            builder.Property(r => r.ScoId)
                .ValueGeneratedNever();

            builder.HasMany(r => r.OfferingSessions)
                .WithOne(s => s.Room);
        }
    }
}