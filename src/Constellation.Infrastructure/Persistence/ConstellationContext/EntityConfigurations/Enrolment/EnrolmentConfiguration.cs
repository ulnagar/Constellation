namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Enrolment;

using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Core.Models.Enrolments.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder.ToTable("Enrolments");

        builder
            .HasKey(enrolment => enrolment.Id);

        builder
            .Property(enrolment => enrolment.Id)
            .HasConversion(
                id => id.Value,
                value => EnrolmentId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany(s => s.Enrolments)
            .HasForeignKey(e => e.StudentId);

        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(e => e.OfferingId);

        builder
            .Property(enrolment => enrolment.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));
    }
}