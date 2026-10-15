namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.Application;

using ConstellationContext.Converters;
using Converters;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");

        builder
            .HasKey(entry => entry.Id);

        builder
            .HasOne<EnrolmentPeriod>()
            .WithMany()
            .HasForeignKey(entry => entry.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<EnrolmentStudent>()
            .WithMany()
            .HasForeignKey(entry => entry.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(entry => entry.Parents)
            .WithMany();

        builder
            .Navigation(entry => entry.Parents)
            .AutoInclude();

        builder
            .Property(entry => entry.CurrentSchoolCode)
            .IsRequired(false)
            .HasConversion<StronglyTypedIdValueConverter<SchoolCode, string>>();

        builder
            .Property(entry => entry.DestinationSchoolCode)
            .IsRequired(false)
            .HasConversion<StronglyTypedIdValueConverter<SchoolCode, string>>();

        builder
            .Property(entry => entry.Program)
            .HasConversion<ProgramConverter>();

        builder
            .Property(entry => entry.Status)
            .HasConversion<string>();

        builder
            .Property(entry => entry.SelectedCourses)
            .HasField("_courses")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(new JsonListConverter<CourseSelection>())
            .Metadata.SetValueComparer(new JsonListComparer<CourseSelection>());
    }
}