using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class TrainingCompletionConfiguration : IEntityTypeConfiguration<TrainingCompletion>
    {
        public void Configure(EntityTypeBuilder<TrainingCompletion> builder)
        {
            builder.HasKey(completion => completion.Id);

            builder.Property(completion => completion.Id).ValueGeneratedOnAdd();

            builder.HasOne(completion => completion.Staff).WithMany().OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(completion => completion.Module).WithMany(module => module.Completions).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
