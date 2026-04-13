namespace Constellation.Core.Models.Assessments;

using Core.ValueObjects;
using Enums;
using Identifiers;
using Primitives;
using Students;
using Students.Identifiers;

public sealed class Assessment : AggregateRoot
{
    private readonly List<AssessmentDownload> _downloads = [];
    private readonly List<StudentSubmission> _submissions = [];

    private Assessment()
    {
        Id = new();
    }

    public AssessmentId Id { get; init; }

    public string Name { get; private set; }
    public string Course { get; private set; }
    public Grade Grade { get; private set; }

    public int CanvasId { get; private set; }
    public DateTime CanvasDueDate { get; private set; }
    public DateTime? CanvasLockDate { get; private set; } // "Until" in Canvas
    public DateTime? CanvasUnlockDate { get; private set; } // "Available From" in Canvas
    public int AllowedAttempts { get; private set; }

    public IReadOnlyList<AssessmentDownload> Downloads => _downloads.AsReadOnly();
    public IReadOnlyList<StudentSubmission> Submissions => _submissions.AsReadOnly();

    public void AddDownload(AssessmentDownload download) => 
        _downloads.Add(download);
}

public sealed class AssessmentDownload
{
    private AssessmentDownload()
    {
        Id = new();
    }

    public AssessmentDownloadId Id { get; init; }
    public AssessmentId AssessmentId { get; private set; }
    public string Name { get; private set; }
    public DateOnly AvailableFrom { get; private set; }
    public DateOnly AvailableTo { get; private set; }
    public bool IsRestricted { get; private set; }
}

public sealed class StudentSubmission
{
    public StudentSubmission(
        Student student)
    {
        Id = new();

        StudentId = student.Id;
        Student = student.Name;
        StudentGrade = student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram;
        SchoolName = student.CurrentEnrolment?.SchoolName ?? string.Empty;
    }

    public SubmissionId Id { get; init; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade StudentGrade { get; private set; }
    public string SchoolName { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }
    public string SubmittedBy { get; private set; }
    public EmailAddress SubmittedByEmail { get; private set; }
}