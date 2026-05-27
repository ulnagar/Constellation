namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Canvas.Models;
using Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments", "Assessments");

        builder
            .HasKey(assessment => assessment.Id);

        builder
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(assessment => assessment.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(assessment => assessment.Grade)
            .HasConversion<string>();

        builder
            .Property(assessment => assessment.CanvasCourse)
            .HasConversion(
                course => course.HasValue ? course.Value.ToString() : null,
                value => CanvasCourseCode.FromValue(value));

        builder
            .Property(e => e.CanvasAssessmentLink)
            .HasConversion(
                uri => uri == null ? null : uri.ToString(),
                value => value == null ? null : new Uri(value))
            .HasMaxLength(2048);

        builder
            .HasMany(assessment => assessment.Downloads)
            .WithOne()
            .HasForeignKey(download => download.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(assessment => assessment.Downloads)
            .HasField("_downloads")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder
            .HasMany(assessment => assessment.Students)
            .WithOne()
            .HasForeignKey(student => student.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(assessment => assessment.Students)
            .HasField("_students")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder
            .HasMany(assessment => assessment.Instructions)
            .WithOne()
            .HasForeignKey(instruction => instruction.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(assessment => assessment.Instructions)
            .HasField("_instructions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}