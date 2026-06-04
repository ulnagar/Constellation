namespace Constellation.Core.Models.StudentOnboarding;

using Core.Enums;
using Enums;
using Errors;
using Identifiers;
using Models.Identifiers;
using Policy;
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


        State = ApplicationState.NewApplication;
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

    public ApplicationState State { get; private set; }
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

    // Status transitions within the current phase
    public Result Accept()
    {
        Result<ApplicationState> newState = ApplicationState.Of(State.Phase, ApplicationStatus.Accepted);

        if (newState.IsFailure)
            return newState;

        return TransitionTo(newState.Value);
    }

    public Result Decline()
    {
        Result<ApplicationState> newState = ApplicationState.Of(State.Phase, ApplicationStatus.Declined);

        if (newState.IsFailure)
            return newState;

        return TransitionTo(newState.Value);
    }

    public Result Lapse()
    {
        Result<ApplicationState> newState = ApplicationState.Of(State.Phase, ApplicationStatus.Lapsed);

        if (newState.IsFailure)
            return newState;

        return TransitionTo(newState.Value);
    }

    // Advance to the next phase (only valid from an Accepted state)
    public Result Advance()
    {
        ApplicationState? next = ApplicationTransitions.ValidTransitionsFrom(State)
            .SingleOrDefault(s => s.Status == ApplicationStatus.Pending);

        if (next is null)
            return Result.Failure(ApplicationErrors.TransitionBlocked(State));

        TransitionTo(next);

        return Result.Success();
    }

    private Result TransitionTo(ApplicationState target)
    {
        if (!ApplicationTransitions.IsValid(State, target))
            return Result.Failure(ApplicationErrors.TransitionInvalid(State, target));

        State = target;

        return Result.Success();
    }
}