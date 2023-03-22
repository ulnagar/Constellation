namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingCompletionConfiguration : IEntityTypeConfiguration<TrainingCompletion>
{
    public void Configure(EntityTypeBuilder<TrainingCompletion> builder)
    {
        builder.ToTable("MandatoryTraining_Completions");

        builder
            .HasKey(completion => completion.Id);

        builder
            .Property(completion => completion.Id)
            .HasConversion(
                recordId => recordId.Value,
                value => TrainingCompletionId.FromValue(value));

        builder
            .HasOne<Staff>()
            .WithMany(staff => staff.TrainingCompletionRecords)
            .HasForeignKey(completion => completion.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(completion => completion.Module)
            .WithMany(module => module.Completions)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(completion => completion.StoredFile)
            .WithMany()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
