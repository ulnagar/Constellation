namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models.Attendance.Identifiers;
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

        //builder
        //    .Property(detail => detail.CaseId)
        //    .HasConversion(
        //        id => id.Value,
        //        value => CaseId.FromValue(value));

        builder
            .HasDiscriminator<string>("CaseDetailType")
            .HasValue<AttendanceCaseDetail>(nameof(AttendanceCaseDetail));
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
    }
}