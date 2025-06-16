namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models.Attendance.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Training.Identifiers;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CaseDetailConfiguration : IEntityTypeConfiguration<CaseDetail>
{
    public void Configure(EntityTypeBuilder<CaseDetail> builder)
    {
        builder.ToTable("WorkFlows_CaseDetails");

        builder
            .HasKey(detail => detail.Id);

        builder
            .Property(detail => detail.Id)
            .HasConversion(
                id => id.Value,
                value => CaseDetailId.FromValue(value));

        builder
            .HasDiscriminator<string>("CaseDetailType")
            .HasValue<AttendanceCaseDetail>(nameof(AttendanceCaseDetail))
            .HasValue<ComplianceCaseDetail>(nameof(ComplianceCaseDetail))
            .HasValue<TrainingCaseDetail>(nameof(TrainingCaseDetail));
    }
}

internal sealed class AttendanceCaseDetailConfiguration : IEntityTypeConfiguration<AttendanceCaseDetail>
{
    public void Configure(EntityTypeBuilder<AttendanceCaseDetail> builder)
    {
        builder
            .Property(detail => detail.AttendanceValueId)
            .HasConversion(
                id => id.Value,
                value => AttendanceValueId.FromValue(value));

        builder
            .Property(detail => detail.Severity)
            .HasConversion(
                severity => severity.Value,
                value => AttendanceSeverity.FromValue(value));

        builder
            .Property(detail => detail.StudentId)
            .HasColumnName(nameof(AttendanceCaseDetail.StudentId));

        builder
            .Property(detail => detail.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(detail => detail.Name)
            .HasColumnName(nameof(AttendanceCaseDetail.Name));

        builder
            .Property(detail => detail.Grade)
            .HasColumnName(nameof(AttendanceCaseDetail.Grade));

        builder
            .Property(detail => detail.SchoolCode)
            .HasColumnName(nameof(AttendanceCaseDetail.SchoolCode));

        builder
            .Property(detail => detail.SchoolName)
            .HasColumnName(nameof(AttendanceCaseDetail.SchoolName));
    }
}

internal sealed class ComplianceCaseDetailConfiguration : IEntityTypeConfiguration<ComplianceCaseDetail>
{
    public void Configure(EntityTypeBuilder<ComplianceCaseDetail> builder)
    {
        builder
            .Property(detail => detail.StudentId)
            .HasColumnName(nameof(ComplianceCaseDetail.StudentId));

        builder
            .Property(detail => detail.StudentId)
            .HasConversion(
                id => id.Value,
                value => StudentId.FromValue(value));

        builder
            .Property(detail => detail.Name)
            .HasColumnName(nameof(ComplianceCaseDetail.Name));

        builder
            .Property(detail => detail.Grade)
            .HasColumnName(nameof(ComplianceCaseDetail.Grade));

        builder
            .Property(detail => detail.SchoolCode)
            .HasColumnName(nameof(ComplianceCaseDetail.SchoolCode));

        builder
            .Property(detail => detail.SchoolName)
            .HasColumnName(nameof(ComplianceCaseDetail.SchoolName));

        builder
            .Property(member => member.CreatedById)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));
    }
}

internal sealed class TrainingCaseDetailConfiguration : IEntityTypeConfiguration<TrainingCaseDetail>
{
    public void Configure(EntityTypeBuilder<TrainingCaseDetail> builder)
    {
        builder
            .Property(detail => detail.StaffId)
            .HasColumnName(nameof(TrainingCaseDetail.StaffId));
        
        builder
            .Property(member => member.StaffId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));

        builder
            .Property(detail => detail.Name)
            .HasColumnName(nameof(TrainingCaseDetail.Name));

        builder
            .Property(detail => detail.TrainingModuleId)
            .HasColumnName(nameof(TrainingCaseDetail.TrainingModuleId))
            .HasConversion(
                id => id.Value,
                value => TrainingModuleId.FromValue(value));

        builder
            .Property(detail => detail.ModuleName)
            .HasColumnName(nameof(TrainingCaseDetail.ModuleName));

        builder
            .Property(detail => detail.DueDate)
            .HasColumnName(nameof(TrainingCaseDetail.DueDate));
    }
}