using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    // New lessons are entered linked with course, and then created in backend for group of offerings.

    // When a lesson is created, all rolls for that lesson are pre-generated.

    // If a student enrols or unenrols from a offering, all related unsubmitted rolls are updated accordingly (add student for new enrolled class, remove student for old enrolled class).

    public class Lesson
    {
        public Lesson()
        {
            Id = Guid.NewGuid();
            Offerings = new List<CourseOffering>();
            Rolls = new List<LessonRoll>();

            // At creation time, build all applicable rolls and attach
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public ICollection<CourseOffering> Offerings { get; set; }
        public ICollection<LessonRoll> Rolls { get; set; }
        public bool DoNotGenerateRolls { get; set; }
    }

    public class LessonRoll
    {
        public LessonRoll()
        {
            Id = Guid.NewGuid();
            Attendance = new List<LessonRollStudentAttendance>();
        }

        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string SchoolCode { get; set; }
        public School School { get; set; }
        public int? SchoolContactId { get; set; }
        public SchoolContact SchoolContact { get; set; }
        public ICollection<LessonRollStudentAttendance> Attendance { get; set; }
        public DateTime? LessonDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string Comment { get; set; }
        public LessonStatus Status { get; set; }

        public class LessonRollStudentAttendance
        {
            public LessonRollStudentAttendance()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; set; }
            public Guid LessonRollId { get; set; }
            public LessonRoll LessonRoll { get; set; }
            public string StudentId { get; set; }
            public Student Student { get; set; }
            public bool Present { get; set; }
        }
    }
}
