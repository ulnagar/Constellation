namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;

public sealed class TutorialRoll : Entity, IAuditableEntity
{
    private readonly List<TutorialRollStudent> _students = new();

    public TutorialRoll(
        Guid Id,
        DateTime sessionDate)
        : base(Id)
    {
        SessionDate = sessionDate;
    }

    public Guid TutorialId { get; set; }
    public DateTime SessionDate { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public IReadOnlyCollection<TutorialRollStudent> Students => _students;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }

    public void AddStudent(string studentId)
    {
        if (_students.All(student => student.StudentId != studentId))
        {
            var student = new TutorialRollStudent
            {
                StudentId = studentId
            };

            _students.Add(student);
        }
    }

    public void Cancel()
    {
        IsDeleted = true;
    }
}
