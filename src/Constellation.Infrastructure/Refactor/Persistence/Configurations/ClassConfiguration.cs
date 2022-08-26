namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Constellation.Infrastructure.Refactor.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.OwnsOne(@class => @class.ClassType);

        builder.HasOne(@class => @class.Faculty).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(@class => @class.Grade).WithMany().OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(@class => @class.Enrolments).WithOne(enrolment => enrolment.Class).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(@class => @class.Sessions).WithOne(session => session.Class).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(@class => @class.Resources).WithOne(resource => resource.Class).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(@class => @class.Teachers).WithOne(assignment => assignment.Class).OnDelete(DeleteBehavior.NoAction);

        builder.Property(@class => @class.StartDate).HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder.Property(@class => @class.EndDate).HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}

public class TutorialClassConfiguration : IEntityTypeConfiguration<TutorialClass>
{
    public void Configure(EntityTypeBuilder<TutorialClass> builder)
    {
        builder.HasOne(tutorial => tutorial.AssociatedClass).WithMany().OnDelete(DeleteBehavior.NoAction);
    }
}
