namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.OfferingEnrolment;

using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OfferingEnrolmentConfiguration : IEntityTypeConfiguration<OfferingEnrolment>
{
    public void Configure(EntityTypeBuilder<OfferingEnrolment> builder)
    {
        builder.ToTable("OfferingEnrolments");

        builder
            .HasKey(enrolment => enrolment.Id);

        builder
            .Property(enrolment => enrolment.Id)
            .HasConversion(
                id => id.Value,
                value => OfferingEnrolmentId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
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