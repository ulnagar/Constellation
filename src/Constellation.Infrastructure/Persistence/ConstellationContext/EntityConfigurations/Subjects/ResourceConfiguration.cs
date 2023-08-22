namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Subjects;

using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Models.Subjects.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Subjects_Resource");

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
            .HasOne(resource => resource.Offering)
            .WithMany()
            .HasForeignKey(resource => resource.OfferingId);

        builder
            .HasDiscriminator(resource => resource.Type)
            .HasValue<AdobeConnectRoomResource>(ResourceType.AdobeConnectRoom)
            .HasValue<MicrosoftTeamResource>(ResourceType.MicrosoftTeam)
            .HasValue<CanvasCourseResource>(ResourceType.CanvasCourse);
    }
}