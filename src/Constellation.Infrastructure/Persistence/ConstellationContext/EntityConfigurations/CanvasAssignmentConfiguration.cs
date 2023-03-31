namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CanvasAssignmentConfiguration : IEntityTypeConfiguration<CanvasAssignment>
{
    public void Configure(EntityTypeBuilder<CanvasAssignment> builder)
    {
        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.Id).ValueGeneratedOnAdd();

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(assignment => assignment.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
