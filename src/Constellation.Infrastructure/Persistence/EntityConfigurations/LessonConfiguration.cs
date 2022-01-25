using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.HasKey(lesson => lesson.Id);

            builder.HasMany(lesson => lesson.Offerings)
                .WithMany(offering => offering.Lessons);

            builder.HasMany(lesson => lesson.Rolls)
                .WithOne(delivery => delivery.Lesson)
                .HasForeignKey(delivery => delivery.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class LessonRollConfiguration : IEntityTypeConfiguration<LessonRoll>
    {
        public void Configure(EntityTypeBuilder<LessonRoll> builder)
        {
            builder.HasKey(roll => roll.Id);

            builder.HasOne(roll => roll.School)
                .WithMany()
                .HasForeignKey(roll => roll.SchoolCode)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(roll => roll.Attendance)
                .WithOne(attendance => attendance.LessonRoll)
                .HasForeignKey(attendance => attendance.LessonRollId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class LessonRollStudentAttendanceConfiguration : IEntityTypeConfiguration<LessonRoll.LessonRollStudentAttendance>
    {
        public void Configure(EntityTypeBuilder<LessonRoll.LessonRollStudentAttendance> builder)
        {
            builder.HasKey(roll => roll.Id);

            builder.HasOne(roll => roll.Student)
                .WithMany(student => student.LessonsAttended)
                .HasForeignKey(roll => roll.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
