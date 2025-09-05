namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AwardNominations;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Awards.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NominationConfiguration
    : IEntityTypeConfiguration<Nomination>
{
    public void Configure(EntityTypeBuilder<Nomination> builder)
    {
        builder.ToTable("Nominations", "AwardNominations");

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
            .HasValue<FirstInSubjectMathematicsNomination>(AwardType.FirstInSubjectMathematics)
            .HasValue<FirstInSubjectScienceTechnologyNomination>(AwardType.FirstInSubjectScienceTechnology)
            .HasValue<AcademicExcellenceNomination>(AwardType.AcademicExcellence)
            .HasValue<AcademicExcellenceMathematicsNomination>(AwardType.AcademicExcellenceMathematics)
            .HasValue<AcademicExcellenceScienceTechnologyNomination>(AwardType.AcademicExcellenceScienceTechnology)
            .HasValue<AcademicAchievementNomination>(AwardType.AcademicAchievement)
            .HasValue<AcademicAchievementMathematicsNomination>(AwardType.AcademicAchievementMathematics)
            .HasValue<AcademicAchievementScienceTechnologyNomination>(AwardType.AcademicAchievementScienceTechnology)
            .HasValue<PrincipalsAwardNomination>(AwardType.PrincipalsAward)
            .HasValue<GalaxyMedalNomination>(AwardType.GalaxyMedal)
            .HasValue<UniversalAchieverNomination>(AwardType.UniversalAchiever);
    }
}
