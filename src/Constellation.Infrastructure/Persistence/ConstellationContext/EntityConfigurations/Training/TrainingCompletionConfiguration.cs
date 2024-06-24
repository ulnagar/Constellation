namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training;

using Constellation.Core.Models;
using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValueConverters;

internal sealed class TrainingCompletionConfiguration : IEntityTypeConfiguration<TrainingCompletion>
{
    public void Configure(EntityTypeBuilder<TrainingCompletion> builder)
    {
        builder.ToTable("Completions", "Training");

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
    }
}