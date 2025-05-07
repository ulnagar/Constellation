namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Edval;

using Core.Models.Edval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class EdvalTimetableConfiguration : IEntityTypeConfiguration<EdvalTimetable>
{
    public void Configure(EntityTypeBuilder<EdvalTimetable> builder)
    {
        builder.ToTable("Timetable", "Edval");

        builder
            .Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Key", 0);
    }
}