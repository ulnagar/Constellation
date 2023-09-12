﻿using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
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
                .HasValue<ContactAddedMSTeamOperation>("ContactAdded")
                .HasValue<TeacherAssignmentMSTeamOperation>("TeacherAssigned")
                .HasValue<StudentOfferingMSTeamOperation>("StudentOffering");
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
        }
    }

    public class GroupMSTeamOperationConfiguration : IEntityTypeConfiguration<GroupMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<GroupMSTeamOperation> builder)
        {
            builder.HasOne(operation => operation.Faculty)
                .WithMany()
                .HasForeignKey(operation => operation.FacultyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class GroupTutorialCreatedMSTeamOperationConfiguration : IEntityTypeConfiguration<GroupTutorialCreatedMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<GroupTutorialCreatedMSTeamOperation> builder)
        {
            builder.HasOne(operation => operation.GroupTutorial)
                .WithMany()
                .HasForeignKey(operation => operation.TutorialId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class OfferingMSTeamOperationConfiguration : IEntityTypeConfiguration<OfferingMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<OfferingMSTeamOperation> builder)
        {
            builder.HasOne(operation => operation.Offering)
                .WithMany()
                .HasForeignKey(operation => operation.OfferingId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(a => a.OfferingId)
                .HasConversion(
                    id => id.Value,
                    value => OfferingId.FromValue(value));
        }
    }

    public class TeacherAssignmentMSTeamOperationConfiguration : IEntityTypeConfiguration<TeacherAssignmentMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<TeacherAssignmentMSTeamOperation> builder)
        {
            builder
                .Property(operation => operation.StaffId)
                .HasColumnName(nameof(TeacherAssignmentMSTeamOperation.StaffId));

            builder
                .HasOne<Staff>()
                .WithMany()
                .HasForeignKey(operation => operation.StaffId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class StudentOfferingMSTeamOperationConfiguration : IEntityTypeConfiguration<StudentOfferingMSTeamOperation>
    {
        public void Configure(EntityTypeBuilder<StudentOfferingMSTeamOperation> builder)
        {
            builder
                .Property(operation => operation.StudentId)
                .HasColumnName(nameof(StudentOfferingMSTeamOperation.StudentId));

            builder
                .HasOne<Student>()
                .WithMany()
                .HasForeignKey(operation => operation.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}