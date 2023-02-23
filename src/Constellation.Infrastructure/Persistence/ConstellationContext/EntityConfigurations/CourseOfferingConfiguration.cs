using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
    {
        public void Configure(EntityTypeBuilder<CourseOffering> builder)
        {
            builder.HasKey(o => o.Id);

            builder.HasOne(o => o.Course)
                .WithMany(c => c.Offerings)
                .HasForeignKey(o => o.CourseId);

            builder.HasMany(o => o.Enrolments)
                .WithOne(e => e.Offering);

            builder.HasMany(o => o.Sessions)
                .WithOne(s => s.Offering);

            builder.HasMany(o => o.ClassCovers)
                .WithOne();
        }
    }
}