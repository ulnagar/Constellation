namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Tutorials;

using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialConfiguration : IEntityTypeConfiguration<Tutorial>
{
    public void Configure(EntityTypeBuilder<Tutorial> builder)
    {
        builder.ToTable("Tutorials", "Tutorials");

        builder
            .HasKey(tutorial => tutorial.Id);

        builder
            .Property(tutorial => tutorial.Id)
            .HasConversion(
                id => id.Value,
                value => TutorialId.FromValue(value));

        builder
            .Property(tutorial => tutorial.Name)
            .HasConversion(
                name => name.Value,
                value => TutorialName.FromValue(value));

        builder
            .HasMany(tutorial => tutorial.Teams)
            .WithOne()
            .HasForeignKey(team => team.TutorialId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Navigation(tutorial => tutorial.Teams)
            .AutoInclude();

        builder
            .HasMany(tutorial => tutorial.Sessions)
            .WithOne()
            .HasForeignKey(session => session.TutorialId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Navigation(tutorial => tutorial.Sessions)
            .AutoInclude();
    }
}