using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class MSTeamOperationConfiguration : IEntityTypeConfiguration<MSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<MSTeamOperation> builder)
        {
            builder.HasDiscriminator<string>("UserType")
                .HasValue<StudentMSTeamOperation>("Student")
                .HasValue<CasualMSTeamOperation>("Casual")
                .HasValue<TeacherMSTeamOperation>("Teacher")
                .HasValue<GroupMSTeamOperation>("Group")
                .HasValue<StudentEnrolledMSTeamOperation>("StudentEnrolled")
                .HasValue<TeacherEmployedMSTeamOperation>("TeacherEmployed")
                .HasValue<ContactAddedMSTeamOperation>("ContactAdded");
        }
    }

    public class StudentMSTeamOperationConfiguration : IEntityTypeConfiguration<StudentMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<StudentMSTeamOperation> builder)
        {
            builder.HasOne(o => o.Student)
                .WithMany(s => s.MSTeamOperations)
                .HasForeignKey(o => o.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class CasualMSTeamOperationConfiguration : IEntityTypeConfiguration<CasualMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<CasualMSTeamOperation> builder)
        {
            builder.HasOne(o => o.Casual)
                .WithMany(c => c.MSTeamOperations)
                .HasForeignKey(o => o.CasualId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(o => o.Cover)
                .WithMany(c => c.MSTeamOperations as ICollection<CasualMSTeamOperation>)
                .HasForeignKey(o => o.CoverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class TeacherMSTeamOperationConfiguration : IEntityTypeConfiguration<TeacherMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<TeacherMSTeamOperation> builder)
        {
            builder.HasOne(o => o.Staff)
                .WithMany(s => s.MSTeamOperations)
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(o => o.Cover)
                .WithMany(s => s.MSTeamOperations as ICollection<TeacherMSTeamOperation>)
                .HasForeignKey(o => o.CoverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}