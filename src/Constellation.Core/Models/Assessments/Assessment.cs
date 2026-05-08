namespace Constellation.Core.Models.Assessments;

using Auth;
using Core.Enums;
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
    private readonly List<AssessmentInstruction> _instructions = [];

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

        RaiseDomainEvent(new AssessmentCreatedDomainEvent(new(), Id));
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

    public IReadOnlyList<AssessmentInstruction> Instructions => _instructions.AsReadOnly();
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

    public void RemoveDownload(AssessmentDownload download) => 
        _downloads.Remove(download);

    public Result AddStudent(Student student, List<Provision> provisions)
    {
        AssessmentStudent? existingEntry = _students.FirstOrDefault(entry => entry.StudentId == student.Id);

        if (existingEntry is not null && !existingEntry.IsDeleted)
            return Result.Failure(AssessmentErrors.StudentAlreadyExists(student.Id));

        if (existingEntry is not null)
        {
            existingEntry.Reinstate(provisions);
            return Result.Success();
        }

        AssessmentStudent studentEntry = new(Id, student);

        foreach (Provision provision in provisions)
            studentEntry.AddProvision(provision);

        _students.Add(studentEntry);

        return Result.Success();
    }

    public void RemoveStudent(StudentId studentId)
    {
        AssessmentStudent? entry = _students.FirstOrDefault(entry => entry.StudentId == studentId);

        if (entry is not null)
            entry.Delete();
    }

    public Result AddStudentProvision(StudentId studentId, Provision provision)
    {
        AssessmentStudent? existingEntry = _students.FirstOrDefault(entry => entry.StudentId == studentId);

        if (existingEntry is null || existingEntry.IsDeleted)
            return Result.Failure(AssessmentErrors.NoLinkedStudent(studentId));

        existingEntry.AddProvision(provision);
     
        return Result.Success();
    }

    public Result<SubmissionId> AddStudentSubmission(StudentId studentId, AppUser user)
    {
        AssessmentStudent? assessmentStudent = _students.FirstOrDefault(s => s.StudentId == studentId);
        
        if (assessmentStudent is null)
        {
            // Student not found in this assessment, cannot add submission
            return Result.Failure<SubmissionId>(AssessmentErrors.NoLinkedStudent(studentId));
        }
        
        SubmissionId submissionId = assessmentStudent.AddSubmission(user);

        RaiseDomainEvent(new AssessmentSubmissionReceivedDomainEvent(new(), Id, studentId, submissionId));

        return submissionId;
    }

    public void AddInstructions(AssessmentInstruction instruction)
    {
        AssessmentInstruction? existingInstructions = _instructions.FirstOrDefault(entry => entry.Category == instruction.Category);

        if (existingInstructions is not null)
            _instructions.Remove(existingInstructions);

        _instructions.Add(instruction);
    }

    public void RemoveInstructions(AssessmentInstruction instruction)
    {
        AssessmentInstruction? existingInstructions = _instructions.FirstOrDefault(entry => entry.Id == instruction.Id);

        if (existingInstructions is not null)
            _instructions.Remove(existingInstructions);
    }
}