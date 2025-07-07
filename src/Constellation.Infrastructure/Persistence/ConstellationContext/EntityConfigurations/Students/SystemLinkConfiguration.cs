namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Constellation.Core.Enums;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StudentSystemLinkConfiguration : IEntityTypeConfiguration<StudentSystemLink>
{
    public void Configure(EntityTypeBuilder<StudentSystemLink> builder)
    {
        builder.ToTable("SystemLinks", "Students");

        builder
            .HasKey(systemLink => new { systemLink.StudentId, systemLink.System });

        builder
            .Property(systemLink => systemLink.System)
            .HasConversion(
                system => system.Value,
                value => SystemType.FromValue(value));
    }
}