namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students;
using Core.Models.Faculties.Identifiers;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers;
using Core.Models.Students.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .HasValue<StudentOfferingMSTeamOperation>("StudentOffering")
            .HasValue<StudentTutorialMSTeamOperation>(nameof(StudentTutorialMSTeamOperation));
    }
}

public class StudentTutorialMSTeamOperationConfiguration : IEntityTypeConfiguration<StudentTutorialMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<StudentTutorialMSTeamOperation> builder)
    {
        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(o => o.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(o => o.StudentId)
            .HasColumnName(nameof(StudentId))
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(o => o.TutorialId)
            .HasColumnName(nameof(TutorialId))
            .HasConversion(
                id => id.Value,
                value => TutorialId.FromValue(value));
    }
}

public class StudentMSTeamOperationConfiguration : IEntityTypeConfiguration<StudentMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<StudentMSTeamOperation> builder)
    {
        builder.HasOne(o => o.Student)
            .WithMany()
            .HasForeignKey(o => o.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(operation => operation.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));
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
            .WithMany()
            .HasForeignKey(o => o.StaffId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder
            .Property(member => member.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
    }
}

public class GroupMSTeamOperationConfiguration : IEntityTypeConfiguration<GroupMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<GroupMSTeamOperation> builder)
    {
        builder
            .Property(operation => operation.FacultyId)
            .HasConversion(
                id => id.Value,
                value => FacultyId.FromValue(value));

        builder
            .HasOne(operation => operation.Faculty)
            .WithMany()
            .HasForeignKey(operation => operation.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class GroupTutorialCreatedMSTeamOperationConfiguration : IEntityTypeConfiguration<GroupTutorialCreatedMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<GroupTutorialCreatedMSTeamOperation> builder)
    {
        builder
            .HasOne(operation => operation.GroupTutorial)
            .WithMany()
            .HasForeignKey(operation => operation.TutorialId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(operation => operation.TutorialId)
            .HasColumnName(nameof(GroupTutorialId))
            .HasConversion(
                id => id.Value,
                value => GroupTutorialId.FromValue(value));
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
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(operation => operation.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(member => member.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
    }
}

public class TeacherEmployedMSTeamOperationConfiguration : IEntityTypeConfiguration<TeacherEmployedMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<TeacherEmployedMSTeamOperation> builder)
    {
        builder
            .Property(operation => operation.StaffId)
            .HasColumnName(nameof(TeacherEmployedMSTeamOperation.StaffId));

        builder
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(operation => operation.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(member => member.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
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
            .Property(operation => operation.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(operation => operation.StudentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ContactAddedMSTeamOperationConfiguration : IEntityTypeConfiguration<ContactAddedMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<ContactAddedMSTeamOperation> builder)
    {
        builder
            .Property(operation => operation.ContactId)
            .HasConversion(
                id => id.Value,
                value => SchoolContactId.FromValue(value));
    }
}

public class StudentEnrolledMSTeamOperationConfiguration : IEntityTypeConfiguration<StudentEnrolledMSTeamOperation>
{
    public void Configure(EntityTypeBuilder<StudentEnrolledMSTeamOperation> builder)
    {
        builder
            .Property(operation => operation.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));
    }
}