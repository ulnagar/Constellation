using Constellation.Core.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
    {
        public void Configure(EntityTypeBuilder<Enrolment> builder)
        {
            builder.HasOne(e => e.Student)
                .WithMany(s => s.Enrolments)
                .HasForeignKey(e => e.StudentId);

            builder.HasOne(e => e.Offering)
                .WithMany(o => o.Enrolments)
                .HasForeignKey(e => e.OfferingId);

            builder
                .Property(enrolment => enrolment.OfferingId)
                .HasConversion(
                    id => id.Value,
                    value => OfferingId.FromValue(value));
        }
    }
}