namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SystemLinkConfiguration : IEntityTypeConfiguration<SystemLink>
{
    public void Configure(EntityTypeBuilder<SystemLink> builder)
    {
        builder.ToTable("SystemLinks", "Students");

        builder
            .HasKey(enrolment => new { enrolment.StudentId, enrolment.System });

        builder
            .Property(enrolment => enrolment.System)
            .HasConversion(
                system => system.Value,
                value => SystemType.FromValue(value));
    }
}