namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder.HasOne(enrolment => enrolment.Student).WithMany(student => student.Enrolments).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(enrolment => enrolment.Class).WithMany(student => student.Enrolments).OnDelete(DeleteBehavior.NoAction);
    }
}
