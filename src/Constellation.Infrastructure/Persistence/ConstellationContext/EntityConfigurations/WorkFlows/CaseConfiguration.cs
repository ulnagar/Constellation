namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValueConverters;

internal sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("WorkFlows_Cases");

        builder
            .HasKey(item => item.Id);

        builder
            .Property(item => item.Id)
            .HasConversion(
                id => id.Value,
                value => CaseId.FromValue(value));

        builder
            .Property(item => item.Type)
            .HasConversion(
                type => type.Value,
                value => CaseType.FromValue(value));

        builder
            .HasOne(item => item.Detail)
            .WithOne()
            .HasForeignKey<CaseDetail>(detail => detail.CaseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(item => item.Detail)
            .AutoInclude();

        builder
            .Property(item => item.DetailId)
            .HasConversion(
                id => id.Value,
                value => CaseDetailId.FromValue(value));

        builder
            .HasMany(item => item.Actions)
            .WithOne()
            .HasForeignKey(action => action.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(item => item.Actions)
            .AutoInclude();

        builder
            .Property(item => item.Status)
            .HasConversion(
                status => status.Value,
                value => CaseStatus.FromValue(value));

        builder
            .Property(item => item.DueDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}
