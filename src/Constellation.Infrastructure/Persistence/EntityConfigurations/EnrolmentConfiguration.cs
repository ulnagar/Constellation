using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
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
        }
    }
}