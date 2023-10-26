namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NominationConfiguration
    : IEntityTypeConfiguration<Nomination>
{
    public void Configure(EntityTypeBuilder<Nomination> builder)
    {
        builder.ToTable("Awards_Nominations");

        builder
            .HasKey(nomination => nomination.Id);

        builder
            .Property(nomination => nomination.Id)
            .HasConversion(
                id => id.Value,
                value => AwardNominationId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(nomination => nomination.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(nomination => nomination.AwardType)
            .HasConversion(
                entity => entity.Value,
                value => AwardType.FromValue(value));

        builder
            .HasDiscriminator(nomination => nomination.AwardType)
            .HasValue<FirstInSubjectNomination>(AwardType.FirstInSubject)
            .HasValue<AcademicExcellenceNomination>(AwardType.AcademicExcellence)
            .HasValue<AcademicAchievementNomination>(AwardType.AcademicAchievement)
            .HasValue<PrincipalsAwardNomination>(AwardType.PrincipalsAward)
            .HasValue<GalaxyMedalNomination>(AwardType.GalaxyMedal)
            .HasValue<UniversalAchieverNomination>(AwardType.UniversalAchiever);
    }
}
