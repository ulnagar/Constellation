namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Subjects;

using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Subjects_Courses");

        builder
            .HasKey(course => course.Id);

        builder
            .Property(course => course.Id)
            .HasConversion(
                id => id.Value,
                value => CourseId.FromValue(value));

        builder
            .HasMany(course => course.Offerings)
            .WithOne()
            .HasForeignKey(offering => offering.CourseId);

        builder
            .Property(course => course.FullTimeEquivalentValue)
            .HasPrecision(4, 3);

        builder
            .HasOne<Faculty>()
            .WithMany()
            .HasForeignKey(course => course.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(course => course.Code)
            .HasMaxLength(3)
            .IsRequired();

        builder
            .HasIndex(course => course.Code);
    }
}