namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Enrolment;

using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Core.Models.Enrolments.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder.ToTable("Enrolments");

        builder
            .HasKey(enrolment => enrolment.Id);

        builder
            .Property(enrolment => enrolment.Id)
            .HasConversion(
                id => id.Value,
                value => EnrolmentId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(e => e.StudentId);

        builder
            .HasDiscriminator<string>("EnrolmentType")
            .HasValue<OfferingEnrolment>(nameof(OfferingEnrolment))
            .HasValue<TutorialEnrolment>(nameof(TutorialEnrolment));
    }
}

public class OfferingEnrolmentConfiguration : IEntityTypeConfiguration<OfferingEnrolment>
{
    public void Configure(EntityTypeBuilder<OfferingEnrolment> builder)
    {
        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(e => e.OfferingId);

        builder
            .Property(enrolment => enrolment.OfferingId)
            .HasConversion(
                id => id.Value,
                value => OfferingId.FromValue(value));
    }
}

public class TutorialEnrolmentConfiguration : IEntityTypeConfiguration<TutorialEnrolment>
{
    public void Configure(EntityTypeBuilder<TutorialEnrolment> builder)
    {
        builder
            .HasOne<Tutorial>()
            .WithMany()
            .HasForeignKey(e => e.TutorialId);

        builder
            .Property(enrolment => enrolment.TutorialId)
            .HasConversion(
                id => id.Value,
                value => TutorialId.FromValue(value));
    }
}