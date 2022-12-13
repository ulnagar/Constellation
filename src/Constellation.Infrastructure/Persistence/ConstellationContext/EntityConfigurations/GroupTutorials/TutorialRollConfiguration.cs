namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialRollConfiguration : IEntityTypeConfiguration<TutorialRoll>
{
    public void Configure(EntityTypeBuilder<TutorialRoll> builder)
    {
        builder.ToTable("GroupTutorials_Roll");

        builder
            .HasKey(e => e.Id);

        builder
            .HasOne<GroupTutorial>()
            .WithMany(t => t.Rolls)
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(e => e.Students);
    }
}
