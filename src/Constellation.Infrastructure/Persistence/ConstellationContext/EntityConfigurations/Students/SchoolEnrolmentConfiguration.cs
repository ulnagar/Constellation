namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Students;

using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class SchoolEnrolmentConfiguration : IEntityTypeConfiguration<SchoolEnrolment>
{
    public void Configure(EntityTypeBuilder<SchoolEnrolment> builder)
    {
        builder.ToTable("SchoolEnrolments", "Students");

        builder
            .HasKey(enrolment => enrolment.Id);

        builder
            .Property(enrolment => enrolment.Id)
            .HasConversion(
                id => id.Value,
                value => SchoolEnrolmentId.FromValue(value));
    }
}