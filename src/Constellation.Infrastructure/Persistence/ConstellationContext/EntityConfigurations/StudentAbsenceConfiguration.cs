using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StudentWholeAbsenceConfiguration : IEntityTypeConfiguration<StudentWholeAbsence>
    {
        public void Configure(EntityTypeBuilder<StudentWholeAbsence> builder)
        {
            builder.ToTable("WholeAbsences");

            builder.HasKey(a => a.Id);

            builder
                .Property(a => a.OfferingId)
                .HasConversion(
                    id => id.Value,
                    value => OfferingId.FromValue(value));

            builder.HasOne(a => a.Student)
                .WithMany(s => s.WholeAbsences)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(a => a.Notifications)
                .WithOne(n => n.Absence)
                .HasForeignKey(n => n.AbsenceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(a => a.Responses)
                .WithOne(r => r.Absence)
                .HasForeignKey(r => r.AbsenceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class StudentWholeAbsenceNotificationConfiguration : IEntityTypeConfiguration<WholeAbsenceNotification>
    {
        public void Configure(EntityTypeBuilder<WholeAbsenceNotification> builder)
        {
            builder.ToTable("WholeAbsenceNotifications");
        }
    }

    public class StudentWholeAbsenceResponseConfiguration : IEntityTypeConfiguration<WholeAbsenceResponse>
    {
        public void Configure(EntityTypeBuilder<WholeAbsenceResponse> builder)
        {
            builder.ToTable("WholeAbsenceResponses");
        }
    }

    public class StudentPartialAbsenceConfiguration : IEntityTypeConfiguration<StudentPartialAbsence>
    {
        public void Configure(EntityTypeBuilder<StudentPartialAbsence> builder)
        {
            builder.ToTable("PartialAbsences");

            builder.HasKey(a => a.Id);

            builder
                .Property(a => a.OfferingId)
                .HasConversion(
                    id => id.Value,
                    value => OfferingId.FromValue(value));

            builder.HasOne(a => a.Student)
                .WithMany(s => s.PartialAbsences)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(a => a.Notifications)
                .WithOne(n => n.Absence)
                .HasForeignKey(n => n.AbsenceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(a => a.Responses)
                .WithOne(r => r.Absence)
                .HasForeignKey(r => r.AbsenceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class StudentPartialAbsenceNotificationConfiguration : IEntityTypeConfiguration<PartialAbsenceNotification>
    {
        public void Configure(EntityTypeBuilder<PartialAbsenceNotification> builder)
        {
            builder.ToTable("PartialAbsenceNotifications");
        }
    }

    public class StudentPartialAbsenceResponseConfiguration : IEntityTypeConfiguration<PartialAbsenceResponse>
    {
        public void Configure(EntityTypeBuilder<PartialAbsenceResponse> builder)
        {
            builder.ToTable("PartialAbsenceResponses");
        }
    }

    public class StudentPartialAbsenceVerificationNotificationConfiguration : IEntityTypeConfiguration<PartialAbsenceVerificationNotification>
    {
        public void Configure(EntityTypeBuilder<PartialAbsenceVerificationNotification> builder)
        {
            builder.ToTable("PartialAbsenceVerifications");
        }
    }
}