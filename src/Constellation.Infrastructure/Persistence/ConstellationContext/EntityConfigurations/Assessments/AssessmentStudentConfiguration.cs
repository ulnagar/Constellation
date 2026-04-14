namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Students;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentStudentConfiguration : IEntityTypeConfiguration<AssessmentStudent>
{
    public void Configure(EntityTypeBuilder<AssessmentStudent> builder)
    {
        builder.ToTable("Students", "Assessments");

        builder
            .HasKey(student => student.Id);

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(student => student.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .ComplexProperty(student => student.Student)
            .IsRequired();

        builder
            .ComplexProperty(student => student.Student)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(student => student.Student)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(student => student.Student)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .HasMany(student => student.Provisions)
            .WithOne()
            .HasForeignKey(provision => provision.AssessmentStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(student => student.Provisions)
            .HasField("_provisions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder
            .HasMany(student => student.Submissions)
            .WithOne()
            .HasForeignKey(submission => submission.AssessmentStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(student => student.Submissions)
            .HasField("_submissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}