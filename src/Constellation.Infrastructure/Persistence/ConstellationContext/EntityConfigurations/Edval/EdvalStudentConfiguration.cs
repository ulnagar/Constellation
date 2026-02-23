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

        builder
            .Property(entry => entry.PreferredName)
            .IsRequired(false);

        builder
            .Property(entry => entry.RollGroup)
            .IsRequired(false);

        builder
            .Property(entry => entry.House)
            .IsRequired(false);

        builder
            .Property(entry => entry.StudentReference)
            .IsRequired(false);

        builder
            .Property(entry => entry.PhoneNumber)
            .IsRequired(false);

        builder
            .Property(entry => entry.UniqueId)
            .IsRequired(false);

        builder
            .Property(entry => entry.EmailAddress)
            .IsRequired(false);

        builder
            .Property(entry => entry.Gender)
            .IsRequired(false);
    }
}