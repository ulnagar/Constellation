namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Assessments.ValueObjects;
using Core.Models.Students;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StudentProvisionConfiguration : IEntityTypeConfiguration<StudentProvision>
{
    public void Configure(EntityTypeBuilder<StudentProvision> builder)
    {
        builder.ToTable("StudentProvisions", "Assessments");

        builder
            .HasKey(studentProvision => studentProvision.Id);

        builder
            .HasOne<Provision>()
            .WithMany()
            .HasForeignKey(studentProvision => studentProvision.ProvisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(studentProvision => studentProvision.ProvisionCode)
            .HasConversion(
                code => code.Value,
                value => ProvisionCode.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(studentProvision => studentProvision.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .ComplexProperty(studentProvision => studentProvision.Student)
            .IsRequired();

        builder
            .ComplexProperty(studentProvision => studentProvision.Student)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(studentProvision => studentProvision.Student)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(studentProvision => studentProvision.Student)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .HasIndex(studentProvision => new { studentProvision.Year, studentProvision.StudentId });
    }
}