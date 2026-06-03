namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.StudentOnboarding;

using Converters;
using Core.Models.Identifiers;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications", "Onboarding");

        builder
            .HasKey(entry => entry.Id);

        builder
            .HasOne(entry => entry.Applicant)
            .WithMany()
            .HasForeignKey(entry => entry.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(entry => entry.Program)
            .HasConversion(
                program => program.Value,
                value => Program.FromValue(value));

        builder
            .Property(entry => entry.Year)
            .HasMaxLength(4);

        builder
            .Property(entry => entry.SchoolCode)
            .HasConversion<StronglyTypedIdValueConverter<SchoolCode, string>>()
            .IsRequired(false);

        builder
            .Property(entry => entry.Phase)
            .HasConversion(
                phase => phase.Value,
                value => ApplicationPhase.FromValue(value));

        builder
            .Property(entry => entry.Status)
            .HasConversion(
                status => status.Value,
                value => ApplicationStatus.FromValue(value));

        builder
            .HasMany(entry => entry.Parents)
            .WithMany(entry => entry.Applications)
            .UsingEntity(join =>
                join.ToTable("ApplicationParents", "Onboarding"))
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(entry => entry.Applicant)
            .AutoInclude();

        builder
            .Navigation(entry => entry.Parents)
            .AutoInclude();
    }
}