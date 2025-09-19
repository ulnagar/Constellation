namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Tutorials;

using Constellation.Core.Models.WorkFlow.Identifiers;
using Converters;
using Core.Models.Students;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests", "Tutorials");

        builder
            .HasKey(request => request.Id);

        builder
            .Property(request => request.Id)
            .HasConversion(
                id => id.Value,
                value => RequestId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(request => request.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .ComplexProperty(request => request.Student)
            .IsRequired();

        builder
            .ComplexProperty(request => request.Student)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(request => request.Student)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(request => request.Student)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(request => request.Type)
            .HasConversion(
                type => type.Value,
                value => TutorialType.FromValue(value));

        builder
            .Property(request => request.Status)
            .HasConversion(
                status => status.Value,
                value => RequestStatus.FromValue(value));

        builder
            .Property(request => request.PeriodIds)
            .HasConversion(new JsonColumnConverter<IReadOnlyList<PeriodId>>());

        builder
            .HasMany(request => request.Notes)
            .WithOne()
            .HasForeignKey(note => note.RequestId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Navigation(request => request.Notes)
            .AutoInclude();
    }
}