namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalStudentConfiguration : IEntityTypeConfiguration<EdvalStudent>
{
    public void Configure(EntityTypeBuilder<EdvalStudent> builder)
    {
        builder.ToTable("Student", "Edval");

        builder
            .HasKey(entity => entity.StudentId);
    }
}