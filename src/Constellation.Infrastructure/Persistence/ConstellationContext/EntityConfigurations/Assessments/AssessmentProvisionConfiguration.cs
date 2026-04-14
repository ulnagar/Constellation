namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Assessments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AssessmentProvisionConfiguration : IEntityTypeConfiguration<AssessmentProvision>
{
    public void Configure(EntityTypeBuilder<AssessmentProvision> builder)
    {
        builder.ToTable("AssessmentProvisions", "Assessments");

        builder
            .HasKey(provision => new { provision.AssessmentStudentId, provision.ProvisionId });

        builder
            .Property(provision => provision.Code)
            .HasConversion(
                code => code.Value,
                value => ProvisionCode.FromValue(value));
    }
}