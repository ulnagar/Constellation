using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Offerings)
                .WithOne(o => o.Course);

            builder.Property(c => c.FullTimeEquivalentValue)
                .HasPrecision(4, 3);

            builder.HasOne(course => course.Faculty)
                .WithMany()
                .HasForeignKey(course => course.FacultyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}