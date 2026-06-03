namespace Constellation.Core.Models.StudentOnboarding;

using Core.Enums;
using Enums;
using Identifiers;
using Models.Identifiers;
using Primitives;
using Shared;

public sealed class Application : AggregateRoot, IAuditableEntity
{
    private readonly List<Parent> _parents = [];

    private Application() { }

    private Application(
        Applicant applicant,
        Program program,
        string year,
        Grade grade,
        SchoolCode? schoolCode = null,
        string? schoolName = null)
    {
        Id = new();

        ApplicantId = applicant.Id;
        Applicant = applicant;
        Program = program;
        Year = year;
        Grade = grade;

        if (schoolCode is not null)
        {
            SchoolCode = schoolCode;
            SchoolName = schoolName;
        }

        Phase = ApplicationPhase.DataEntry;
        Status = ApplicationStatus.Pending;
        Deadline = DateOnly.MaxValue;
    }

    public ApplicationId Id { get; private set; }
    public ApplicantId ApplicantId { get; private set; } 
    public Applicant Applicant { get; private set; }

    public IReadOnlyList<Parent> Parents => _parents.AsReadOnly();

    public Program Program { get; private set; }
    public string Year { get; private set; }
    public Grade Grade { get; private set; }
    public SchoolCode? SchoolCode { get; private set; }
    public string? SchoolName { get; private set; }

    public ApplicationPhase Phase { get; private set; }
    public ApplicationStatus Status { get; private set; }
    public DateOnly Deadline { get; private set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<Application> Create(
        Applicant applicant,
        Program program,
        string year,
        Grade grade,
        School? school)
    {
        return new Application(
            applicant,
            program,
            year,
            grade,
            school?.Code,
            school?.Name);
    }
}