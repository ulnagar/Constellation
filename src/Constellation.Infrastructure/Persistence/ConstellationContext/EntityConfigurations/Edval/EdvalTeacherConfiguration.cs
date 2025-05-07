namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalTeacherConfiguration : IEntityTypeConfiguration<EdvalTeacher>
{
    public void Configure(EntityTypeBuilder<EdvalTeacher> builder)
    {
        builder.ToTable("Teacher", "Edval");

        builder
            .HasKey(entity => entity.UniqueId);
    }
}