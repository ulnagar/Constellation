namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Offerings;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Models.Canvas.Models;
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

internal sealed class CanvasCourseResourceConfiguration : IEntityTypeConfiguration<CanvasCourseResource>
{
    public void Configure(EntityTypeBuilder<CanvasCourseResource> builder)
    {
        builder
            .Property(resource => resource.SectionId)
            .HasConversion(
                item => item.ToString(),
                value => CanvasSectionCode.FromValue(value));
    }
}