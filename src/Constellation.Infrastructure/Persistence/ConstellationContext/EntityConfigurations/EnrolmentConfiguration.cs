namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder
            .HasOne<Student>()
            .WithMany(s => s.Enrolments)
            .HasForeignKey(e => e.StudentId);

        builder.HasOne<Offering>()
            .WithMany()
            .HasForeignKey(e => e.OfferingId);

        builder
            .Property(enrolment => enrolment.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));
    }
}