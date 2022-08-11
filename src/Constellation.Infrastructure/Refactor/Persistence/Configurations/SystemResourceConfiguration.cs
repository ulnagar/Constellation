namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SystemResourceConfiguration : IEntityTypeConfiguration<SystemResource>
{
    public void Configure(EntityTypeBuilder<SystemResource> builder)
    {
        builder.OwnsOne(resource => resource.ResourceType);

        builder.HasDiscriminator<string>("ResourceOwner")
            .HasValue<ClassResource>(nameof(ClassResource))
            .HasValue<FacultyResource>(nameof(FacultyResource))
            .HasValue<GradeResource>(nameof(GradeResource));
    }
}

public class ClassResourceConfiguration : IEntityTypeConfiguration<ClassResource>
{
    public void Configure(EntityTypeBuilder<ClassResource> builder)
    {
        builder.HasOne(resource => resource.Class).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}

public class FacultyResourceConfiguration : IEntityTypeConfiguration<FacultyResource>
{
    public void Configure(EntityTypeBuilder<FacultyResource> builder)
    {
        builder.HasOne(resource => resource.Faculty).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}

public class GradeResourceConfiguration : IEntityTypeConfiguration<GradeResource>
{
    public void Configure(EntityTypeBuilder<GradeResource> builder)
    {
        builder.HasOne(resource => resource.Grade).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
