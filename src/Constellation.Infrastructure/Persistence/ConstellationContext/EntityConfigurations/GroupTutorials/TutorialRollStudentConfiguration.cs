namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialRollStudentConfiguration : IEntityTypeConfiguration<TutorialRollStudent>
{
    public void Configure(EntityTypeBuilder<TutorialRollStudent> builder)
    {
        builder.ToTable("GroupTutorials_RollStudent");

        builder
            .HasKey(x => new
            {
                x.TutorialRollId,
                x.StudentId
            });

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<TutorialRoll>()
            .WithMany(r => r.Students)
            .HasForeignKey(x => x.TutorialRollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
