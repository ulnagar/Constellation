using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
    {
        public void Configure(EntityTypeBuilder<TrainingModule> builder)
        {
            builder.HasKey(module => module.Id);

            builder.Property(module => module.Id)
                .ValueGeneratedOnAdd();

            builder.HasMany(module => module.Completions)
                .WithOne(completion => completion.Module)
                .HasForeignKey(completion => completion.TrainingModuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
