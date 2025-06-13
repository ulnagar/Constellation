namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training;

using Constellation.Core.Models.Training;
using Core.Models.StaffMembers;
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
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(member => member.StaffId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}