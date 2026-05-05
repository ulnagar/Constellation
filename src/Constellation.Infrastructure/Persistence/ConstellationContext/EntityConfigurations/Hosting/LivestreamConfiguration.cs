namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Hosting;

using Core.Models.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class LivestreamConfiguration : IEntityTypeConfiguration<Livestream>
{
    public void Configure(EntityTypeBuilder<Livestream> builder)
    {
        builder.ToTable("Livestreams", "Hosting");
 
        builder
            .HasKey(livestream => livestream.Id);
    }
}