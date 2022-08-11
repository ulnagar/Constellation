namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.OwnsOne(student => student.Gender);

        builder.HasMany(student => student.Cohorts).WithMany(cohort => cohort.Students);

        builder.HasOne(student => student.School).WithMany(school => school.Students).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(student => student.Photo).WithOne(photo => photo.Student).HasForeignKey<StudentPhoto>(photo => photo.StudentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(student => student.Family).WithMany(family => family.Students).OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(student => student.Enrolments).WithOne(enrolment => enrolment.Student).OnDelete(DeleteBehavior.NoAction);
    }
}
