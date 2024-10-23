namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ThirdPartyConsent;

using Core.Models.Students;
using Core.Models.Subjects;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ConsentRequirementConfiguration : IEntityTypeConfiguration<ConsentRequirement>
{
    public void Configure(EntityTypeBuilder<ConsentRequirement> builder)
    {
        builder.ToTable("ConsentRequirements", "ThirdParty");

        builder
            .HasKey(requirement => requirement.Id);

        builder
            .Property(requirement => requirement.Id)
            .HasConversion(
                id => id.Value,
                value => ConsentRequirementId.FromValue(value));

        builder
            .HasOne<Application>()
            .WithMany()
            .HasForeignKey(requirement => requirement.ApplicationId);

        builder
            .Property(requirement => requirement.ApplicationId)
            .HasConversion(
                id => id.Value,
                value => ApplicationId.FromValue(value));

        builder
            .HasDiscriminator<string>("Type")
            .HasValue<GradeConsentRequirement>("Grade")
            .HasValue<CourseConsentRequirement>("Course")
            .HasValue<StudentConsentRequirement>("Student");
    }
}

internal sealed class GradeConsentRequirementConfiguration : IEntityTypeConfiguration<GradeConsentRequirement>
{
    public void Configure(EntityTypeBuilder<GradeConsentRequirement> builder)
    {
    }
}

internal sealed class CourseConsentRequirementConfiguration : IEntityTypeConfiguration<CourseConsentRequirement>
{
    public void Configure(EntityTypeBuilder<CourseConsentRequirement> builder)
    {
        builder
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(requirement => requirement.CourseId);
    }
}

internal sealed class StudentConsentRequirementConfiguration : IEntityTypeConfiguration<StudentConsentRequirement>
{
    public void Configure(EntityTypeBuilder<StudentConsentRequirement> builder)
    {
        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(requirement => requirement.StudentId);
    }
}