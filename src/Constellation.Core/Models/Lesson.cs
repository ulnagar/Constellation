namespace Constellation.Core.Models;

using Constellation.Core.Enums;


// New lessons are entered linked with course, and then created in backend for group of offerings.
// When a lesson is created, all rolls for that lesson are pre-generated.
// If a student enrols or unenrols from a offering, all related unsubmitted rolls are updated accordingly (add student for new enrolled class, remove student for old enrolled class).

public class Lesson
{
    public Lesson()
    {
        Id = Guid.NewGuid();

        // At creation time, build all applicable rolls and attach
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public List<CourseOffering> Offerings { get; set; } = new();
    public List<LessonRoll> Rolls { get; set; } = new();
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
    public virtual Lesson? Lesson { get; set; }
    public string SchoolCode { get; set; } = string.Empty;
    public virtual School? School { get; set; }
    public int? SchoolContactId { get; set; }
    public virtual SchoolContact? SchoolContact { get; set; }
    public ICollection<LessonRollStudentAttendance> Attendance { get; set; }
    public DateTime? LessonDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public LessonStatus Status { get; set; }

    public class LessonRollStudentAttendance
    {
        public LessonRollStudentAttendance()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid LessonRollId { get; set; }
        public virtual LessonRoll? LessonRoll { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public virtual Student? Student { get; set; }
        public bool Present { get; set; }
    }
}
