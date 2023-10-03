namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Offerings;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Offerings_Resources");

        builder
            .HasKey(resource => resource.Id);

        builder
            .Property(resource => resource.Id)
            .HasConversion(
                id => id.Value,
                value => ResourceId.FromValue(value));

        builder
            .Property(resource => resource.Type)
            .HasConversion(
                type => type.Value,
                value => ResourceType.FromValue(value));

        builder
            .Property(resource => resource.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));

        builder
            .HasDiscriminator(resource => resource.Type)
            .HasValue<AdobeConnectRoomResource>(ResourceType.AdobeConnectRoom)
            .HasValue<MicrosoftTeamResource>(ResourceType.MicrosoftTeam)
            .HasValue<CanvasCourseResource>(ResourceType.CanvasCourse);
    }
}