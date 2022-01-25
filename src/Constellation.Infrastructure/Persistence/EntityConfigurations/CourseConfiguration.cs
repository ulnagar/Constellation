using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Offerings)
                .WithOne(o => o.Course);

            builder.HasOne(c => c.HeadTeacher)
                .WithMany(s => s.ResponsibleCourses)
                .HasForeignKey(c => c.HeadTeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(c => c.FullTimeEquivalentValue)
                .HasPrecision(4, 3);
        }
    }
}