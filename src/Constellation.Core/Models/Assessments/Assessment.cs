namespace Constellation.Core.Models.Assessments;

using Auth;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using Students;
using Students.Identifiers;
using Subjects;
using Subjects.Identifiers;

public sealed class Assessment : AggregateRoot
{
    private readonly List<AssessmentDownload> _downloads = [];
    private readonly List<AssessmentStudent> _students = [];

    /// <summary>
    /// Required for EF Core
    /// </summary>
    private Assessment() { }

    public Assessment(
        string name,
        Course course,
        DateTimeOffset dueDate,
        DateTimeOffset availableFrom,
        DateTimeOffset availableTo)
    {
        Id = new();

        Name = name;
        CourseId = course.Id;
        Course = course.ToString();
        Grade = course.Grade;

        DueDate = dueDate;
        AvailableFrom = availableFrom;
        AvailableTo = availableTo;
    }

    public AssessmentId Id { get; init; }

    public string Name { get; private set; }
    public CourseId CourseId { get; private set; }
    public string Course { get; private set; }
    public Grade Grade { get; private set; }

    public DateTimeOffset DueDate { get; private set; }
    public DateTimeOffset AvailableTo { get; private set; }
    public DateTimeOffset AvailableFrom { get; private set; }

    public int CanvasId { get; private set; }
    public int AllowedAttempts { get; private set; }

    public IReadOnlyList<AssessmentDownload> Downloads => _downloads.AsReadOnly();
    public IReadOnlyList<AssessmentStudent> Students => _students.AsReadOnly();

    public void AddCanvasDetails(
        int canvasId,
        int allowedAttempts)
    {
        CanvasId = canvasId;
        AllowedAttempts = allowedAttempts;
    }

    public void Update(
        string name,
        Course course,
        DateTimeOffset dueDate,
        DateTimeOffset availableFrom,
        DateTimeOffset availableTo)
    {
        Name = name;
        CourseId = course.Id;
        Course = course.ToString();
        Grade = course.Grade;

        DueDate = dueDate;
        AvailableFrom = availableFrom;
        AvailableTo = availableTo;
    }

    public void AddDownload(AssessmentDownload download) => 
        _downloads.Add(download);

    public Result AddStudent(Student student, List<Provision> provisions)
    {
        if (_students.Any(s => s.StudentId == student.Id))
            return Result.Failure(AssessmentErrors.StudentAlreadyExists(student.Id));

        AssessmentStudent studentEntry = new(Id, student);

        foreach (Provision provision in provisions)
            studentEntry.AddProvision(provision);

        _students.Add(studentEntry);

        return Result.Success();
    }

    public Result AddStudentSubmission(StudentId studentId, AppUser user)
    {
        AssessmentStudent? assessmentStudent = _students.FirstOrDefault(s => s.StudentId == studentId);
        
        if (assessmentStudent is null)
        {
            // Student not found in this assessment, cannot add submission

            return Result.Failure(AssessmentErrors.NoLinkedStudent(studentId));
        }
        
        SubmissionId submissionId = assessmentStudent.AddSubmission(user);

        RaiseDomainEvent(new AssessmentSubmissionReceivedDomainEvent(new(), Id, submissionId));

        return Result.Success();
    }
}