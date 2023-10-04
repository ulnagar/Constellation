namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
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
            .Property(roll => roll.Id)
            .HasConversion(
                rollId => rollId.Value,
                value => TutorialRollId.FromValue(value));

        builder
            .HasOne<GroupTutorial>()
            .WithMany(tutorial => tutorial.Rolls)
            .HasForeignKey(roll => roll.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(roll => roll.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(roll => roll.Students);

        builder
            .Property(roll => roll.SessionDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Navigation(roll => roll.Students)
            .AutoInclude();
    }
}
