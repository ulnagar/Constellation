namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training.Modules;

using Constellation.Core.Models;
using Constellation.Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValueConverters;

internal sealed class TrainingCompletionConfiguration : IEntityTypeConfiguration<TrainingCompletion>
{
    public void Configure(EntityTypeBuilder<TrainingCompletion> builder)
    {
        builder.ToTable("Training_Modules_Completions");

        builder
            .HasKey(completion => completion.Id);

        builder
            .Property(completion => completion.Id)
            .HasConversion(
                recordId => recordId.Value,
                value => TrainingCompletionId.FromValue(value));

        builder
            .Property(completion => completion.CompletedDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .HasOne<Staff>()
            .WithMany(staff => staff.TrainingCompletionRecords)
            .HasForeignKey(completion => completion.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(completion => completion.Module)
            .WithMany(module => module.Completions)
            .OnDelete(DeleteBehavior.NoAction);
    }
}