namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Assessments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentInstructionsConfiguration : IEntityTypeConfiguration<AssessmentInstruction>
{
    public void Configure(EntityTypeBuilder<AssessmentInstruction> builder)
    {
        builder.ToTable("Instructions", "Assessments");

        builder
            .HasKey(instructions => instructions.Id);

        builder
            .Property(instructions => instructions.Category)
            .HasConversion(
                category => category.Value,
                value => UserCategory.FromValue(value));
    }
}