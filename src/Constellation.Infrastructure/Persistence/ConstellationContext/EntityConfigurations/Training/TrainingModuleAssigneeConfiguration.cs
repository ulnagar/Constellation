namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training;

using Constellation.Core.Models.Training;
using Core.Models;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingModuleAssigneeConfiguration : IEntityTypeConfiguration<TrainingModuleAssignee>
{
    public void Configure(EntityTypeBuilder<TrainingModuleAssignee> builder)
    {
        builder.ToTable("Assignees", "Training");

        builder
            .HasKey(member => new { member.ModuleId, member.StaffId });

        builder
            .Property(member => member.ModuleId)
            .HasConversion(
                id => id.Value,
                value => TrainingModuleId.FromValue(value));

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(member => member.StaffId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}