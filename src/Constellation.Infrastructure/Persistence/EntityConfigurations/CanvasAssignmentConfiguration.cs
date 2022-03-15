using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class CanvasAssignmentConfiguration : IEntityTypeConfiguration<CanvasAssignment>
    {
        public void Configure(EntityTypeBuilder<CanvasAssignment> builder)
        {
            builder.HasKey(assignment => assignment.Id);

            builder.Property(assignment => assignment.Id).ValueGeneratedOnAdd();

            builder.HasOne(assignment => assignment.Course)
                .WithMany()
                .HasForeignKey(assignment => assignment.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
