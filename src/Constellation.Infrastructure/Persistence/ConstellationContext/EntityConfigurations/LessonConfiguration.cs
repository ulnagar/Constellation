using Constellation.Core.Models.SciencePracs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<SciencePracLesson>
    {
        public void Configure(EntityTypeBuilder<SciencePracLesson> builder)
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

    public class LessonRollConfiguration : IEntityTypeConfiguration<SciencePracRoll>
    {
        public void Configure(EntityTypeBuilder<SciencePracRoll> builder)
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

    public class LessonRollStudentAttendanceConfiguration : IEntityTypeConfiguration<SciencePracRoll.LessonRollStudentAttendance>
    {
        public void Configure(EntityTypeBuilder<SciencePracRoll.LessonRollStudentAttendance> builder)
        {
            builder.HasKey(roll => roll.Id);

            builder.HasOne(roll => roll.Student)
                .WithMany(student => student.LessonsAttended)
                .HasForeignKey(roll => roll.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
